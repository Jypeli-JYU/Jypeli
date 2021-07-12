using System;
using System.Collections.Generic;

namespace Jypeli
{
    /// <summary>
    /// Luo tien. Tie koostuu useasta pienemmästä "pätkästä".
    /// </summary>
    public class RoadMap
    {
        private Vector[] wayPoints;
        private Angle[] angles = null;

        /// <summary>
        /// Tien oletusleveys.
        /// </summary>
        public double DefaultWidth { get; set; }

        /// <summary>
        /// Tien oletuskitka.
        /// </summary>
        public double DefaultFriction { get; set; }

        /// <summary>
        /// Tienpätkät.
        /// </summary>
        public GameObject[] Segments { get; private set; }

        /// <summary>
        /// Funktio, joka luo yksittäisen tienpätkän.
        /// </summary>
        /// Funktion tulisi olla muotoa
        /// <example>
        /// PhysicsObject CreateSegment( double width, double height, Shape shape )
        /// </example>
        /// Funktion tulisi sijoittaa saamansa parametrit uudelle oliolle. Lisäksi
        /// funktion tarvitsee lisätä luomansa olio peliin.
        public Func<double, double, Shape, PhysicsObject> CreateSegmentFunction { get; set; }


        /// <summary>
        /// Luo uuden RoadMapin.
        /// </summary>
        /// <param name="wayPoints">Lista reittipisteistä.</param>
        public RoadMap(IList<Vector> wayPoints)
        {
            this.DefaultWidth = 10.0;
            this.DefaultFriction = 1.0;
            this.CreateSegmentFunction = CreateSegment;
            this.wayPoints = new Vector[wayPoints.Count];
            wayPoints.CopyTo(this.wayPoints, 0);
            this.Segments = new GameObject[wayPoints.Count - 1];
        }

        /// <summary>
        /// Etenemissuunta pisteen kohdalla.
        /// </summary>
        /// <param name="wayPointIndex">Pisteen indeksi (alkaen nollasta).</param>
        public Angle GetAngle(int wayPointIndex)
        {
            // TODO: calculate this on the fly so that this check becomes unnecessary.
            if (angles == null)
                throw new Exception("Call Insert() first");
            return angles[wayPointIndex];
        }

        /// <summary>
        /// Luo tien kentälle.
        /// </summary>
        public void Insert()
        {
            if (wayPoints.Length < 2)
            {
                throw new ArgumentException("Must have at least 2 points");
            }

            angles = new Angle[wayPoints.Length];

            Vector first = wayPoints[0];
            Vector second = wayPoints[1];
            Vector toBeginning = (first - second).Normalize();
            Vector beginning = first + toBeginning;

            Vector previousLeft, previousRight;
            CalculatePoints(beginning, first, second, out previousLeft, out previousRight);
            angles[0] = (second - first).Angle;

            Vector secondToLast = wayPoints[wayPoints.Length - 2];
            Vector last = wayPoints[wayPoints.Length - 1];
            Vector toVeryLast = (last - secondToLast).Normalize();
            Vector veryLast = last + toVeryLast;

            for (int i = 1; i < wayPoints.Length; i++)
            {
                Vector previous = wayPoints[i - 1];
                Vector current = wayPoints[i];
                Vector next;

                Vector toPrevious = (previous - current).Normalize();
                Vector toNext;

                if (i == wayPoints.Length - 1)
                {
                    next = veryLast;
                    toNext = toVeryLast;
                }
                else
                {
                    next = wayPoints[i + 1];
                    toNext = (next - current).Normalize();
                }

                Vector left, right;
                CalculatePoints(previous, current, next, out left, out right);
                angles[i] = (next - previous).Angle;

                Vector center = previous + toNext / 2;

                Vector[] vertices = new Vector[4];
                vertices[0] = previousLeft - center;
                vertices[1] = previousRight - center;
                vertices[2] = right - center;
                vertices[3] = left - center;

                IndexTriangle[] triangles = new IndexTriangle[]
                {
                    new IndexTriangle(0, 3, 1),
                    new IndexTriangle(1, 3, 2)
                };

                ShapeCache cache = new ShapeCache(vertices, triangles);
                Polygon shape = new Polygon(cache, false);

                PhysicsObject segment = CreateSegmentFunction(100, 100, shape);
                segment.Position = center;
                Segments[i - 1] = segment;

                previousLeft = left;
                previousRight = right;
            }
        }

        private PhysicsObject CreateSegment(double width, double height, Shape shape)
        {
            PhysicsObject o = PhysicsObject.CreateStaticObject(width, height, shape);
            o.Color = Color.Gray;
            o.IgnoresCollisionResponse = true;
            o.KineticFriction = DefaultFriction;

            // TopDownPhysicsGame does not exist in MonoJypeli
            //if (Game.Instance is TopDownPhysicsGame)
            //{
            //    TopDownPhysicsGame g = (TopDownPhysicsGame)Game.Instance;
            //    g.AddSurface(o);
            //}
            //else
            //{
                Game.Instance.Add(o);
            //}

            return o;
        }

        private void CalculatePoints(Vector previous, Vector current, Vector next, out Vector left, out Vector right)
        {
            Vector toNext = (next - current).Normalize();
            Vector toPrevious = (previous - current).Normalize();
            Vector direction = (next - previous).Normalize();

            Vector perpendicular = new Vector(direction.Y, -direction.X);
            Vector toLeft = -perpendicular;
            Vector toRight = perpendicular;

            left = current + toLeft * DefaultWidth / 2;
            right = current + toRight * DefaultWidth / 2;
        }

        /// <summary>
        /// Onko annettu piste radan sisällä
        /// </summary>
        /// <param name="point">Piste</param>
        /// <returns></returns>
        public bool IsInside(Vector point)
        {
            return ListHelpers.ArrayFind(Segments, seg => seg.IsInside(point)) != null;
        }
    }
}
