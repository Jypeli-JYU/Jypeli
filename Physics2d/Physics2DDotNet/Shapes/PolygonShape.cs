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
    /// Use this to Represent a Polygon in the engine
    /// </summary>
    public sealed class PolygonShape : 
        IShape, IRaySegmentsCollidable, ILineFluidAffectable , IExplosionAffectable 
    {
        #region fields
        private DistanceGrid grid;
        private Vector2D[] vertexes;
        private Vector2D[] vertexNormals;
        private Vector2D centroid;
        private Scalar area;
        private Scalar inertia;
        private object tag;
        #endregion
        #region constructors
        /// <summary>
        /// Creates a new Polygon Instance.
        /// </summary>
        /// <param name="vertexes">the vertexes that make up the shape of the Polygon</param>
        /// <param name="gridSpacing">
        /// How large a grid cell is. Usualy you will want at least 2 cells between major vertexes.
        /// The smaller this is the better precision you get, but higher cost in memory. 
        /// The larger the less precision and if it's to high collision detection may fail completely.
        public PolygonShape(Vector2D[] vertexes, Scalar gridSpacing)
        {
            if (vertexes == null) { throw new ArgumentNullException("vertexes"); }
            if (vertexes.Length < 3) { throw new ArgumentException("too few", "vertexes"); }
            if (gridSpacing <= 0) { throw new ArgumentOutOfRangeException("gridSpacing"); }
            this.vertexes = vertexes;
            this.grid = new DistanceGrid(this, gridSpacing);
            this.vertexNormals = VertexHelper.GetVertexNormals(this.vertexes);
            VertexInfo info = VertexHelper.GetVertexInfo(vertexes);
            this.inertia = info.Inertia;
            this.centroid = info.Centroid;
            this.area = info.Area;
        }
        /// <summary>
        /// Creates a new Polygon Instance.
        /// </summary>
        /// <param name="vertexes">the vertexes that make up the shape of the Polygon</param>
        /// <param name="gridSpacing">
        /// How large a grid cell is. Usualy you will want at least 2 cells between major vertexes.
        /// The smaller this is the better precision you get, but higher cost in memory. 
        /// The larger the less precision and if it's to high collision detection may fail completely.
        /// </param>
        /// <param name="momentOfInertiaMultiplier">
        /// How hard it is to turn the shape. Depending on the construtor in the 
        /// Body this will be multiplied with the mass to determine the moment of inertia.
        /// </param>
        public PolygonShape(Vector2D[] vertexes, Scalar gridSpacing, Scalar inertia):this(vertexes,gridSpacing)
        {
            this.inertia = inertia;
        }
        #endregion
        #region properties
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }
        public Vector2D[] Vertexes {    get { return vertexes; } }
        public Vector2D[] VertexNormals   {   get { return vertexNormals; }  }
        public Scalar Inertia
        {
            get { return inertia; }
        }
        public Vector2D Centroid
        {
            get { return centroid; }
        }
        public Scalar Area
        {
            get { return area; }
        }
        public  bool CanGetIntersection
        {
            get { return true; }
        }
        public  bool CanGetCustomIntersection
        {
            get { return false; }
        }
        #endregion
        #region methods
        public  void CalcBoundingRectangle(ref Matrix2x3 matrix, out BoundingRectangle rectangle)
        {
            BoundingRectangle.FromVectors(ref matrix, Vertexes, out rectangle);
        }
        public  void GetDistance(ref Vector2D point, out Scalar result)
        {
            BoundingPolygon.GetDistance(Vertexes, ref point, out result);
        }
        public  bool TryGetIntersection(Vector2D point, out IntersectionInfo info)
        {
            return grid.TryGetIntersection(point, out info);
        }
        public  bool TryGetCustomIntersection(Body self, Body other, out object customIntersectionInfo)
        {
            throw new NotSupportedException();
        }
        DragInfo IGlobalFluidAffectable.GetFluidInfo(Vector2D tangent)
        {
            Scalar min, max;
            ShapeHelper.GetProjectedBounds(this.Vertexes, tangent, out min, out max);
            Scalar avg = (max + min) / 2;
            return new DragInfo(tangent * avg, max - min);
        }
        bool IRaySegmentsCollidable.TryGetRayCollision(Body thisBody, Body raysBody, RaySegmentsShape raySegments, out RaySegmentIntersectionInfo info)
        {
            bool intersects = false;
            Scalar temp;
            RaySegment[] segments = raySegments.Segments;
            Scalar[] result = new Scalar[segments.Length];
            Matrix2x3 matrix = raysBody.Matrices.ToBody * thisBody.Matrices.ToWorld;
            Vector2D[] polygon = new Vector2D[Vertexes.Length];
            for (int index = 0; index < polygon.Length; ++index)
            {
                Vector2D.Transform(ref matrix, ref Vertexes[index], out polygon[index]);
            }
            BoundingRectangle rect;
            BoundingRectangle.FromVectors(polygon, out rect);
            BoundingPolygon poly = new BoundingPolygon(polygon);

            for (int index = 0; index < segments.Length; ++index)
            {
                RaySegment segment = segments[index];

                rect.Intersects(ref segment.RayInstance, out temp);
                if (temp >= 0 && temp <= segment.Length)
                {
                    
                    poly.Intersects(ref segment.RayInstance, out temp);
                    if (temp < 0 || temp > segment.Length)
                    {
                        result[index] = -1;
                    }
                    else
                    {
                        result[index] = temp;
                        intersects = true;
                    }
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
            Scalar min,max;
            ShapeHelper.GetProjectedBounds(inter, tangent, out min, out max);
            Scalar avg = (max + min) / 2;
            return new DragInfo(tangent * avg, max - min);
        }
        #endregion


    }
}