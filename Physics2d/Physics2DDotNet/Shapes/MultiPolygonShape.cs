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
using System.Collections.Generic;

using AdvanceMath;
using AdvanceMath.Geometry2D;


namespace Physics2DDotNet.Shapes
{
    /// <summary>
    /// A shape that contains multiple polygons.
    /// </summary>
    public sealed class MultiPolygonShape : IShape, IRaySegmentsCollidable, ILineFluidAffectable, IExplosionAffectable
    {
        #region static
        private static Vector2D[] ConcatVertexes(Vector2D[][] polygons)
        {

            int totalLength = 0;
            Vector2D[] polygon;
            for (int index = 0; index < polygons.Length; ++index)
            {
                polygon = polygons[index];
                if (polygon == null) { throw new ArgumentNullException("polygons"); }
                if (polygon.Length < 3) { throw new ArgumentException("too few", "polygons"); }
                totalLength += polygon.Length;
            }
            Vector2D[] result = new Vector2D[totalLength];
            int offset = 0;
            for (int index = 0; index < polygons.Length; ++index)
            {
                polygon = polygons[index];
                polygon.CopyTo(result, offset);
                offset += polygon.Length;
            }
            return result;
        }
        #endregion
        #region fields
        private DistanceGrid grid;
        private Vector2D[][] polygons;
        private Vector2D[] vertexNormals;
        private Vector2D[] vertexes;
        private Vector2D centroid;
        private Scalar area;
        private Scalar inertia;
        private object tag;
        #endregion
        #region constructors
        public MultiPolygonShape(Vector2D[][] polygons, Scalar gridSpacing)
        {
            if (gridSpacing <= 0) { throw new ArgumentOutOfRangeException("gridSpacing"); }
            if (polygons == null) { throw new ArgumentNullException("polygons"); }
            if (polygons.Length == 0) { throw new ArgumentOutOfRangeException("polygons"); }
            this.polygons = polygons;
            this.vertexes = ConcatVertexes(polygons);
            this.vertexNormals = CalculateNormals();
            this.grid = new DistanceGrid(this, gridSpacing);
            VertexInfo info = VertexHelper.GetVertexInfoOfRange(polygons);
            this.inertia = info.Inertia;
            this.centroid = info.Centroid;
            this.area = info.Area;
        }
        public MultiPolygonShape(Vector2D[][] polygons, Scalar gridSpacing, Scalar inertia)
            : this(polygons, gridSpacing)
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
        public Vector2D[] Vertexes { get { return vertexes; } }
        public Vector2D[] VertexNormals { get { return vertexNormals; } }
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

        public Vector2D[][] Polygons
        {
            get { return polygons; }
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
        private Vector2D[] CalculateNormals()
        {
            Vector2D[] result = new Vector2D[Vertexes.Length];
            int offset = 0;
            for (int index = 0; index < polygons.Length; ++index)
            {
                Vector2D[] polygon = polygons[index];
                VertexHelper.CalculateNormals(polygon, result, offset);
                offset += polygon.Length;
            }
            return result;
        }

        public void CalcBoundingRectangle(ref Matrix2x3 matrix, out BoundingRectangle rectangle)
        {
            BoundingRectangle.FromVectors(ref matrix, Vertexes, out rectangle);
        }

        public bool TryGetIntersection(Vector2D point, out IntersectionInfo info)
        {
            return grid.TryGetIntersection(point, out info);
        }

        public void GetDistance(ref Vector2D point, out Scalar result)
        {
            result = Scalar.MaxValue;
            Scalar temp;
            for (int index = 0; index < polygons.Length; ++index)
            {
                BoundingPolygon.GetDistance(polygons[index], ref point, out temp);
                if (temp < result)
                {
                    result = temp;
                }
            }
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
            Scalar temp;
            Vector2D[][] polygons = this.Polygons;
            for (int index = 0; index < segments.Length; ++index)
            {
                result[index] = -1;
            }
            Matrix2x3 matrix = raysBody.Matrices.ToBody * thisBody.Matrices.ToWorld;
            for (int polyIndex = 0; polyIndex < polygons.Length; ++polyIndex)
            {
                Vector2D[] unTrans = polygons[polyIndex];
                Vector2D[] polygon = new Vector2D[unTrans.Length];
                for (int index = 0; index < unTrans.Length; ++index)
                {
                    Vector2D.Transform(ref matrix, ref unTrans[index], out polygon[index]);
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
                        if (temp >= 0 && temp <= segment.Length)
                        {
                            if (result[index] == -1 || temp < result[index])
                            {
                                result[index] = temp;
                            }
                            intersects = true;
                        }
                    }
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
            Vector2D dragCenter;
            Scalar dragArea;
            ShapeHelper.GetFluidInfo(polygons, tangent, out dragCenter, out dragArea);
            return new DragInfo(dragCenter, dragArea);
        }
        FluidInfo ILineFluidAffectable.GetFluidInfo(GetTangentCallback callback, Line line)
        {
            if (polygons.Length == 1)
            {
                return ShapeHelper.GetFluidInfo(Vertexes, callback, line);
            }
            List<Vector2D[]> submerged = new List<Vector2D[]>(polygons.Length);
            for (int index = 0; index < polygons.Length; ++index)
            {
                Vector2D[] vertexes = VertexHelper.GetIntersection(polygons[index], line);
                if (vertexes.Length >= 3) { submerged.Add(vertexes); }
            }
            if (submerged.Count == 0) { return null; }
            Vector2D[][] newPolygons = submerged.ToArray();
            Vector2D centroid = VertexHelper.GetCentroidOfRange(newPolygons);
            Scalar area = VertexHelper.GetAreaOfRange(newPolygons);
            Vector2D tangent = callback(centroid);
            Vector2D dragCenter;
            Scalar dragArea;
            ShapeHelper.GetFluidInfo(newPolygons, tangent, out dragCenter, out dragArea);
            return new FluidInfo(dragCenter, dragArea, centroid, area);
        }

        DragInfo IExplosionAffectable.GetExplosionInfo(Matrix2x3 matrix, Scalar radius, GetTangentCallback callback)
        {
            //TODO: do this right!
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