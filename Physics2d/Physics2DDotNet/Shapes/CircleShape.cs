#region MIT License
/*
 * Copyright (c) 2005-2008 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion




#if UseDouble
using Scalar = System.Double;
#else
using Scalar = System.Single;
#endif

using System;

using AdvanceMath;
using AdvanceMath.Geometry2D;


namespace Physics2DDotNet.Shapes
{
    /// <summary>
    /// A Circle
    /// </summary>
    public sealed class CircleShape : IShape, IRaySegmentsCollidable, ILineFluidAffectable, IExplosionAffectable
    {
        #region fields
        private Scalar radius;
        private object tag;
        private Scalar inertia;
        private Vector2D[] vertexNormals;
        private Vector2D[] vertexes;
        #endregion
        #region constructors

        /// <summary>
        /// Creates a new Circle Instance.
        /// </summary>
        /// <param name="radius">how large the circle is.</param>
        /// <param name="vertexCount">
        /// The number or vertex that will be generated along the perimeter of the circle. 
        /// This is for collision detection.
        /// </param>
        public CircleShape(Scalar radius, int vertexCount)
            : this(radius, vertexCount, MassInfo.InertiaOfSolidCylinder(radius))
        { }
        /// <summary>
        /// Creates a new Circle Instance.
        /// </summary>
        /// <param name="radius">how large the circle is.</param>
        /// <param name="vertexCount">
        /// The number or vertex that will be generated along the perimeter of the circle. 
        /// This is for collision detection.
        /// </param>
        /// <param name="inertia">
        /// How hard it is to turn the shape. Depending on the construtor in the 
        /// Body this will be multiplied with the mass to determine the moment of inertia.
        /// </param>
        public CircleShape(Scalar radius, int vertexCount, Scalar inertia)
        {
            if (radius <= 0) { throw new ArgumentOutOfRangeException("radius", "must be larger then zero"); }
            if (vertexCount < 3) { throw new ArgumentOutOfRangeException("vertexCount", "Must be equal or greater then 3"); }
            this.vertexes = VertexHelper.CreateCircle(radius, vertexCount);
            this.vertexNormals = VertexHelper.CreateCircle(1, vertexCount);
            this.inertia = inertia;
            this.radius = radius;
        }
        #endregion
        #region properties
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }
        public Vector2D[] Vertexes { get { return vertexes; } }
        public Vector2D[] VertexNormals { get { return vertexNormals; } }
        public Scalar Inertia
        {
            get { return inertia; }
        }
        public Vector2D Centroid
        {
            get { return Vector2D.Zero; }
        }
        public Scalar Area
        {
            get { return radius * radius * MathHelper.Pi; }
        }
        /// <summary>
        /// the distance from the position where the circle ends.
        /// </summary>
        public Scalar Radius
        {
            get { return radius; }
        }
        public bool CanGetIntersection
        {
            get { return true; }
        }
        public bool CanGetCustomIntersection
        {
            get { return false; }
        }
        #endregion
        #region methods
        public void CalcBoundingRectangle(ref Matrix2x3 matrix, out BoundingRectangle rectangle)
        {
            BoundingRectangle.FromCircle(ref matrix, ref radius, out rectangle);
        }
        public void GetDistance(ref Vector2D point, out Scalar result)
        {
            Vector2D.GetMagnitude(ref point, out result);
            result -= radius;
        }
        public bool TryGetIntersection(Vector2D point, out IntersectionInfo info)
        {
            info.Position = point;
            Vector2D normal;
            Vector2D.Normalize(ref point, out info.Distance, out normal);
            info.rawNormal = normal;
            info.normalized = true;
            info.Distance -= radius;
            return info.Distance <= 0;
        }
        public bool TryGetCustomIntersection(Body self, Body other, out object customIntersectionInfo)
        {
            throw new NotSupportedException();
        }
        bool IRaySegmentsCollidable.TryGetRayCollision(Body thisBody, Body raysBody, RaySegmentsShape raySegments, out RaySegmentIntersectionInfo info)
        {
            bool intersects = false;
            RaySegment[] segments = raySegments.Segments;
            Scalar[] result = new Scalar[segments.Length];
            BoundingCircle bounding = new BoundingCircle(Vector2D.Zero, this.radius);
            Matrix2x3 vertexM = thisBody.Matrices.ToBody * raysBody.Matrices.ToWorld;

            for (int index = 0; index < segments.Length; ++index)
            {
                RaySegment segment = segments[index];
                Ray ray2;

                Vector2D.Transform(ref vertexM, ref segment.RayInstance.Origin, out ray2.Origin);
                Vector2D.TransformNormal(ref vertexM, ref segment.RayInstance.Direction, out ray2.Direction);

                Scalar temp;
                bounding.Intersects(ref ray2, out temp);
                if (temp >= 0 && temp <= segment.Length)
                {
                    intersects = true;
                    result[index] = temp;
                    continue;
                }
                else
                {
                    result[index] = -1;
                }
            }
            if (intersects)
            {
                info = new RaySegmentIntersectionInfo(result);
            }
            else
            {
                info = null;
            }
            return intersects;
        }
        DragInfo IGlobalFluidAffectable.GetFluidInfo(Vector2D tangent)
        {
            return new DragInfo(Vector2D.Zero, radius * 2);
        }
        FluidInfo ILineFluidAffectable.GetFluidInfo(GetTangentCallback callback, Line line)
        {
            return ShapeHelper.GetFluidInfo(Vertexes, callback, line);
        }
        DragInfo IExplosionAffectable.GetExplosionInfo(Matrix2x3 matrix, Scalar radius, GetTangentCallback callback)
        {
            //TODO: do this right!
            Vector2D[] vertexes2 = new Vector2D[Vertexes.Length];
            for (int index = 0; index < vertexes2.Length; ++index)
            {
                vertexes2[index] = matrix * Vertexes[index];
            }
            Vector2D[] inter = VertexHelper.GetIntersection(vertexes2, radius);
            if (inter.Length < 3) { return null; }
            Vector2D centroid = VertexHelper.GetCentroid(inter);
            Vector2D tangent = callback(centroid);
            Scalar min, max;
            ShapeHelper.GetProjectedBounds(inter, tangent, out min, out max);
            Scalar avg = (max + min) / 2;
            return new DragInfo(tangent * avg, max - min);
        }
        #endregion
    }
}