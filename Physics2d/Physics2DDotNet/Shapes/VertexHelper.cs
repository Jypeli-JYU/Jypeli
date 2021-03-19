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

    public class VertexInfo
    {
        Vector2D centroid;
        Scalar inertia;
        Scalar area;
        public VertexInfo(Vector2D centroid, Scalar inertia, Scalar area)
        {
            this.centroid = centroid;
            this.inertia = inertia;
            this.area = area;
        }
        public Vector2D Centroid
        {
            get { return centroid; }
        }
        public Scalar Inertia
        {
            get { return inertia; }
        }
        public Scalar Area
        {
            get { return area; }
        }
    }

    public static class VertexHelper
    {
        public static Vector2D[] ApplyMatrix(ref Matrix2x3 matrix, Vector2D[] vertexes)
        {
            return OperationHelper.ArrayRefOp<Matrix2x3, Vector2D, Vector2D>(ref matrix, vertexes, Vector2D.Transform);
        }
        public static Vector2D[][] ApplyMatrixToRange(ref Matrix2x3 matrix, Vector2D[][] polygons)
        {
            if (polygons == null) { throw new ArgumentNullException("polygons"); }
            Vector2D[][] result = new Vector2D[polygons.Length][];
            for (int index = 0; index < polygons.Length; ++index)
            {
                result[index] = ApplyMatrix(ref matrix, polygons[index]);
            }
            return result;
        }

        /// <summary>
        /// Takes a 2D Boolean array with a true value representing a collidable pixel and converts 
        /// it to an array of vertex that surrounds that bitmap. The bitmap should be a single piece 
        /// if there are many pieces it will only return the geometry of the first piece it comes 
        /// across. Make sure there are at least 3 pixels in a piece otherwise an exception will be 
        /// thrown (it wont be a polygon). 
        /// </summary>
        /// <param name="bitmap">a bitmap to be converted. true means its collidable.</param>
        /// <returns>A Vector2D[] representing the bitmap.</returns>
        public static Vector2D[] CreateFromBitmap(bool[,] bitmap)
        {
            return CreateFromBitmap(new ArrayBitmap(bitmap));
        }
        public static Vector2D[] CreateFromBitmap(IBitmap bitmap)
        {
            if (bitmap == null) { throw new ArgumentNullException("bitmap"); }
            if (bitmap.Width < 2 || bitmap.Height < 2) { throw new ArgumentOutOfRangeException("bitmap"); }
            return BitmapHelper.CreateFromBitmap(bitmap);
        }
        public static Vector2D[][] CreateRangeFromBitmap(bool[,] bitmap)
        {
            return CreateRangeFromBitmap(new ArrayBitmap(bitmap));
        }
        public static Vector2D[][] CreateRangeFromBitmap(IBitmap bitmap)
        {
            if (bitmap == null) { throw new ArgumentNullException("bitmap"); }
            if (bitmap.Width < 2 || bitmap.Height < 2) { throw new ArgumentOutOfRangeException("bitmap"); }
            return BitmapHelper.CreateManyFromBitmap(bitmap);
        }
        /// <summary>
        /// creates vertexes that describe a Rectangle.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height">The length of the Rectangle</param>
        /// <returns>array of vectors the describe a rectangle</returns>
        public static Vector2D[] CreateRectangle(Scalar width, Scalar height)
        {
            if (width <= 0) { throw new ArgumentOutOfRangeException("width", "must be greater then 0"); }
            if (height <= 0) { throw new ArgumentOutOfRangeException("height", "must be greater then 0"); }
            Scalar wd2 = width * .5f;
            Scalar ld2 = height * .5f;
            return new Vector2D[4]
            {
                new Vector2D(wd2, ld2),
                new Vector2D(-wd2, ld2),
                new Vector2D(-wd2, -ld2),
                new Vector2D(wd2, -ld2)
            };
        }
        public static Vector2D[] CreateCircle(Scalar radius, int vertexCount)
        {
            if (radius <= 0) { throw new ArgumentOutOfRangeException("radius", "Must be greater then zero."); }
            if (vertexCount < 3) { throw new ArgumentOutOfRangeException("vertexCount", "Must be equal or greater then 3"); }
            Vector2D[] result = new Vector2D[vertexCount];
            Scalar angleIncrement = MathHelper.TwoPi / vertexCount;
            for (int index = 0; index < vertexCount; ++index)
            {
                Scalar angle = angleIncrement * index;
                Vector2D.FromLengthAndAngle(ref radius, ref angle, out result[index]);
            }
            return result;
        }
        /// <summary>
        /// Calculates the moment of inertia for a polygon
        /// </summary>
        /// <param name="vertexes"></param>
        /// <returns>the moment of inertia</returns>
        public static Scalar GetInertia(Vector2D[] vertexes)
        {
            return BoundingPolygon.GetInertia(vertexes);
        }
        public static Scalar GetInertiaOfRange(Vector2D[][] polygons)
        {
            if (polygons == null) { throw new ArgumentNullException("polygons"); }
            if (polygons.Length == 0) { throw new ArgumentOutOfRangeException("polygons"); }
            Scalar denom = 0;
            Scalar numer = 0;
            Scalar a, b, c, d;
            Vector2D v1, v2;
            for (int polyIndex = 0; polyIndex < polygons.Length; ++polyIndex)
            {
                Vector2D[] vertexes = polygons[polyIndex];
                if (vertexes == null) { throw new ArgumentNullException("polygons"); }
                if (vertexes.Length == 0) { throw new ArgumentOutOfRangeException("polygons"); }
                if (vertexes.Length == 1) { break; }
                v1 = vertexes[vertexes.Length - 1];
                for (int index = 0; index < vertexes.Length; index++, v1 = v2)
                {
                    v2 = vertexes[index];
                    Vector2D.Dot(ref v2, ref v2, out a);
                    Vector2D.Dot(ref v2, ref v1, out b);
                    Vector2D.Dot(ref v1, ref v1, out c);
                    Vector2D.ZCross(ref v1, ref v2, out d);
                    d = Math.Abs(d);
                    numer += d;
                    denom += (a + b + c) * d;
                }
            }
            if (numer == 0) { return 1; }
            return denom / (numer * 6);
        }

        /// <summary>
        /// INCOMPLETE! TODO: FINISH
        /// </summary>
        /// <param name="vertexes"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector2D[] GetIntersection(Vector2D[] vertexes, Scalar radius)
        {
            Scalar[] distances = new Scalar[vertexes.Length];
            for (int index = 0; index < vertexes.Length; ++index)
            {
                distances[index] = vertexes[index].Magnitude - radius;
            }
            Scalar lastDistance = distances[distances.Length - 1];
            Vector2D lastVertex = vertexes[vertexes.Length - 1];
            Vector2D vertex;
            Scalar distance;
            List<Vector2D> result = new List<Vector2D>(vertexes.Length + 1);
            for (int index = 0; index < vertexes.Length; ++index, lastVertex = vertex, lastDistance = distance)
            {
                vertex = vertexes[index];
                distance = distances[index];
                if (Math.Abs(Math.Sign(distance) - Math.Sign(lastDistance)) == 2)
                {
                    Scalar lastABS = Math.Abs(lastDistance);
                    Scalar total = (lastABS + Math.Abs(distance));
                    Scalar percent = lastABS / total;
                    Vector2D intersection;
                    Vector2D.Lerp(ref lastVertex, ref vertex, ref percent, out intersection);
                    result.Add(intersection);
                }
                if (distance >= 0)
                {
                    result.Add(vertex);
                }
            }
            return result.ToArray();
        }

        public static VertexInfo GetVertexInfo(Vector2D[] vertexes)
        {
            if (vertexes == null) { throw new ArgumentNullException("vertexes"); }
            if (vertexes.Length < 3) { throw new ArgumentOutOfRangeException("vertexes"); }
            Scalar area = 0;
            Scalar denom = 0;
            Scalar numer = 0;
            Scalar a, b, c, d;
            Vector2D centroid = Vector2D.Zero;
            Vector2D v1 = vertexes[vertexes.Length - 1];
            Vector2D v2;
            for (int index = 0; index < vertexes.Length; index++, v1 = v2)
            {
                v2 = vertexes[index];
                Vector2D.Dot(ref v2, ref v2, out a);
                Vector2D.Dot(ref v2, ref v1, out b);
                Vector2D.Dot(ref v1, ref v1, out c);
                Vector2D.ZCross(ref v1, ref v2, out d);
                area += d;
                centroid.X += ((v1.X + v2.X) * d);
                centroid.Y += ((v1.Y + v2.Y) * d);
                d = Math.Abs(d);
                numer += d;
                denom += (a + b + c) * d;
            }
            area = Math.Abs(area * .5f);
            d = 1 / (area * 6);
            centroid.X *= d;
            centroid.Y *= d;
            return new VertexInfo(centroid, denom / (numer * 6), area);
        }
        public static VertexInfo GetVertexInfoOfRange(Vector2D[][] polygons)
        {
            if (polygons == null) { throw new ArgumentNullException("polygons"); }
            if (polygons.Length == 0) { throw new ArgumentOutOfRangeException("polygons"); }
            Scalar a, b, c, d;
            Scalar denom = 0;
            Scalar numer = 0;
            Scalar areaTotal = 0;
            Vector2D centroid = Vector2D.Zero;
            for (int index1 = 0; index1 < polygons.Length; ++index1)
            {
                Vector2D[] vertexes = polygons[index1];
                if (vertexes == null) { throw new ArgumentNullException("polygons"); }
                if (vertexes.Length < 3) { throw new ArgumentOutOfRangeException("polygons", "There must be at least 3 vertexes"); }
                Scalar area = 0;
                Vector2D v1 = vertexes[vertexes.Length - 1];
                Vector2D v2;
                for (int index = 0; index < vertexes.Length; ++index, v1 = v2)
                {
                    v2 = vertexes[index];
                    Vector2D.Dot(ref v2, ref v2, out a);
                    Vector2D.Dot(ref v2, ref v1, out b);
                    Vector2D.Dot(ref v1, ref v1, out c);
                    Vector2D.ZCross(ref v1, ref v2, out d);
                    area += d;
                    centroid.X += ((v1.X + v2.X) * d);
                    centroid.Y += ((v1.Y + v2.Y) * d);
                    d = Math.Abs(d);
                    numer += d;
                    denom += (a + b + c) * d;
                }
                areaTotal += Math.Abs(area);
            }
            areaTotal *= .5f;
            d = 1 / (areaTotal * 6);
            centroid.X *= d;
            centroid.Y *= d;
            return new VertexInfo(
                centroid,
                (numer == 0) ? (1) : (denom / (numer * 6)),
                areaTotal);
        }

        internal static void CalculateNormals(Vector2D[] vertexes, Vector2D[] normals, int offset)
        {
            Vector2D[] edges = new Vector2D[vertexes.Length];
            Vector2D last = vertexes[0];
            Vector2D current;
            Vector2D temp;
            for (int index = vertexes.Length - 1; index > -1; --index, last = current)
            {
                current = vertexes[index];
                Vector2D.Subtract(ref current, ref last, out temp);
                Vector2D.Normalize(ref temp, out temp);
                Vector2D.GetRightHandNormal(ref temp, out edges[index]);
            }
            last = edges[vertexes.Length - 1];
            for (int index = 0; index < vertexes.Length; ++index, last = current)
            {
                current = edges[index];
                Vector2D.Add(ref current, ref last, out temp);
                Vector2D.Normalize(ref temp, out normals[index + offset]);
            }
        }
      
        
        public static Vector2D[] GetVertexNormals(Vector2D[] vertexes)
        {
            if (vertexes == null) { throw new ArgumentNullException("vertexes"); }
            Vector2D[] result = new Vector2D[vertexes.Length];
            CalculateNormals(vertexes, result, 0);
            return result;
        }
        public static Vector2D[][] GetVertexNormalsOfRange(Vector2D[][] polygons)
        {
            if (polygons == null) { throw new ArgumentNullException("polygons"); }
            Vector2D[][] result = new Vector2D[polygons.Length][];
            for (int index = 0; index < polygons.Length; ++index)
            {
                result[index] = GetVertexNormals(polygons[index]);
            }
            return result;
        }



        public static Vector2D[] GetIntersection(Vector2D[] vertexes, Line line)
        {
            Scalar[] distances = new Scalar[vertexes.Length];
            for (int index = 0; index < vertexes.Length; ++index)
            {
                line.GetDistance(ref vertexes[index], out distances[index]);
            }
            Scalar lastDistance = distances[distances.Length - 1];
            Vector2D lastVertex = vertexes[vertexes.Length - 1];
            Vector2D vertex;
            Scalar distance;
            List<Vector2D> result = new List<Vector2D>(vertexes.Length + 1);
            for (int index = 0; index < vertexes.Length; ++index, lastVertex = vertex, lastDistance = distance)
            {
                vertex = vertexes[index];
                distance = distances[index];
                if (Math.Abs(Math.Sign(distance) - Math.Sign(lastDistance)) == 2)
                {
                    Scalar lastABS = Math.Abs(lastDistance);
                    Scalar total = (lastABS + Math.Abs(distance));
                    Scalar percent = lastABS / total;
                    Vector2D intersection;
                    Vector2D.Lerp(ref lastVertex, ref vertex, ref percent, out intersection);
                    result.Add(intersection);
                }
                if (distance >= 0)
                {
                    result.Add(vertex);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// makes sure the distance between 2 vertexes is under the length passed, by adding vertexes between them.
        /// </summary>
        /// <param name="vertexes">the original vertexes.</param>
        /// <param name="maxLength">the maximum distance allowed between 2 vertexes</param>
        /// <returns>The new vertexes.</returns>
        public static Vector2D[] Subdivide(Vector2D[] vertexes, Scalar maxLength)
        {
            return Subdivide(vertexes, maxLength, true);
        }
        /// <summary>
        /// makes sure the distance between 2 vertexes is under the length passed, by adding vertexes between them.
        /// </summary>
        /// <param name="vertexes">the original vertexes.</param>
        /// <param name="maxLength">the maximum distance allowed between 2 vertexes</param>
        /// <param name="loop">if it should check the distance between the first and last vertex.</param>
        /// <returns>The new vertexes.</returns>
        public static Vector2D[] Subdivide(Vector2D[] vertexes, Scalar maxLength, bool loop)
        {
            if (vertexes == null) { throw new ArgumentNullException("vertexes"); }
            if (vertexes.Length < 2) { throw new ArgumentOutOfRangeException("vertexes"); }
            if (maxLength <= 0) { throw new ArgumentOutOfRangeException("maxLength", "must be greater then zero"); }
            LinkedList<Vector2D> list = new LinkedList<Vector2D>(vertexes);
            LinkedListNode<Vector2D> prevNode, node;
            if (loop)
            {
                prevNode = list.Last;
                node = list.First;
            }
            else
            {
                prevNode = list.First;
                node = prevNode.Next;
            }
            for (; node != null;
                prevNode = node,
                node = node.Next)
            {
                Vector2D edge = node.Value - prevNode.Value;
                Scalar mag = edge.Magnitude;
                if (mag > maxLength)
                {
                    int count = (int)Math.Ceiling(mag / maxLength);
                    mag = 1f / count;
                    Vector2D.Multiply(ref edge, ref mag, out edge);
                    for (int pos = 1; pos < count; ++pos)
                    {
                        prevNode = list.AddAfter(prevNode, edge + prevNode.Value);
                    }
                }
            }
            Vector2D[] result = new Vector2D[list.Count];
            list.CopyTo(result, 0);
            return result;
        }
        /// <summary>
        /// Reduces a Polygon's number of vertexes.
        /// </summary>
        /// <param name="vertexes">The Polygon to reduce.</param>
        /// <returns>The reduced vertexes.</returns>
        public static Vector2D[] Reduce(Vector2D[] vertexes)
        {
            return Reduce(vertexes, 0);
        }
        /// <summary>
        /// Reduces a Polygon's number of vertexes.
        /// </summary>
        /// <param name="vertexes">The Polygon to reduce.</param>
        /// <param name="areaTolerance">
        /// The amount the removal of a vertex is allowed to change the area of the polygon.
        /// (Setting this value to 0 will reverse what the Subdivide method does) 
        /// </param>
        /// <returns>The reduced vertexes.</returns>
        public static Vector2D[] Reduce(Vector2D[] vertexes, Scalar areaTolerance)
        {
            if (vertexes == null) { throw new ArgumentNullException("vertexes"); }
            if (vertexes.Length < 2) { throw new ArgumentOutOfRangeException("vertexes"); }
            if (areaTolerance < 0) { throw new ArgumentOutOfRangeException("areaTolerance", "must be equal to or greater then zero."); }
            List<Vector2D> result = new List<Vector2D>(vertexes.Length);
            Vector2D v1, v2, v3;
            Scalar old1, old2, new1;
            v1 = vertexes[vertexes.Length - 2];
            v2 = vertexes[vertexes.Length - 1];
            areaTolerance *= 2;
            for (int index = 0; index < vertexes.Length; ++index, v2 = v3)
            {
                if (index == vertexes.Length - 1)
                {
                    if (result.Count == 0) { throw new ArgumentOutOfRangeException("areaTolerance", "The Tolerance is too high!"); }
                    v3 = result[0];
                }
                else { v3 = vertexes[index]; }
                Vector2D.ZCross(ref v1, ref v2, out old1);
                Vector2D.ZCross(ref v2, ref v3, out old2);
                Vector2D.ZCross(ref v1, ref v3, out new1);
                if (Math.Abs(new1 - (old1 + old2)) > areaTolerance)
                {
                    result.Add(v2);
                    v1 = v2;
                }
            }
            return result.ToArray();
        }
        /// <summary>
        /// Calculates the area of a polygon.
        /// </summary>
        /// <param name="vertexes">The vertexes of the polygon.</param>
        /// <returns>The area.</returns>
        public static Scalar GetArea(Vector2D[] vertexes)
        {
            Scalar result;
            BoundingPolygon.GetArea(vertexes, out result);
            return result;
        }
        /// <summary>
        /// Calculates the Centroid of a polygon.
        /// </summary>
        /// <param name="vertexes">The vertexes of the polygon.</param>
        /// <returns>The Centroid of a polygon.</returns>
        /// <remarks>
        /// This is Also known as Center of Gravity/Mass.
        /// </remarks>
        public static Vector2D GetCentroid(Vector2D[] vertexes)
        {
            Vector2D result;
            BoundingPolygon.GetCentroid(vertexes, out result);
            return result;
        }
        /// <summary>
        /// repositions the polygon so the Centroid is the origin.
        /// </summary>
        /// <param name="vertexes">The vertexes of the polygon.</param>
        /// <returns>The vertexes of the polygon with the Centroid as the Origin.</returns>
        public static Vector2D[] CenterVertexes(Vector2D[] vertexes)
        {
            Vector2D centroid;
            BoundingPolygon.GetCentroid(vertexes, out centroid);
            return OperationHelper.ArrayRefOp<Vector2D, Vector2D, Vector2D>(vertexes, ref centroid, Vector2D.Subtract);
        }
        
        public static Vector2D[][] GetIntersectionOfRange(Vector2D[][] polygons, Line line)
        {
            List<Vector2D[]> submerged = new List<Vector2D[]>(polygons.Length);
            for (int index = 0; index < polygons.Length; ++index)
            {
                Vector2D[] vertexes = GetIntersection(polygons[index], line);
                if (vertexes.Length >= 3) { submerged.Add(vertexes); }
            }
            return submerged.ToArray();
        }
        public static Scalar GetAreaOfRange(Vector2D[][] polygons)
        {
            if (polygons == null) { throw new ArgumentNullException("polygons"); }
            if (polygons.Length == 0) { throw new ArgumentOutOfRangeException("polygons"); }
            Scalar result = 0;
            Scalar temp;
            Vector2D[] polygon;
            for (int index = 0; index < polygons.Length; ++index)
            {
                polygon = polygons[index];
                if (polygon == null) { throw new ArgumentNullException("polygons"); }
                BoundingPolygon.GetArea(polygon, out temp);
                result += temp;
            }
            return result;
        }
        public static Vector2D GetCentroidOfRange(Vector2D[][] polygons)
        {
            if (polygons == null) { throw new ArgumentNullException("polygons"); }
            if (polygons.Length == 0) { throw new ArgumentOutOfRangeException("polygons"); }


            Scalar temp, area, areaTotal;
            Vector2D v1, v2;
            Vector2D[] vertexes;
            areaTotal = 0;
            Vector2D result = Vector2D.Zero;
            for (int index1 = 0; index1 < polygons.Length; ++index1)
            {
                vertexes = polygons[index1];
                if (vertexes == null) { throw new ArgumentNullException("polygons"); }
                if (vertexes.Length < 3) { throw new ArgumentOutOfRangeException("polygons", "There must be at least 3 vertexes"); }
                v1 = vertexes[vertexes.Length - 1];
                area = 0;
                for (int index = 0; index < vertexes.Length; ++index, v1 = v2)
                {
                    v2 = vertexes[index];
                    Vector2D.ZCross(ref v1, ref v2, out temp);
                    area += temp;
                    result.X += ((v1.X + v2.X) * temp);
                    result.Y += ((v1.Y + v2.Y) * temp);
                }
                areaTotal += Math.Abs(area);
            }
            temp = 1 / (areaTotal * 3);
            result.X *= temp;
            result.Y *= temp;
            return result;
        }
        public static Vector2D[][] CenterVertexesRange(Vector2D[][] polygons)
        {
            if (polygons == null) { throw new ArgumentNullException("polygons"); }
            Vector2D centroid = GetCentroidOfRange(polygons);
            Vector2D[][] result = new Vector2D[polygons.Length][];
            for (int index = 0; index < polygons.Length; ++index)
            {
                result[index] = OperationHelper.ArrayRefOp<Vector2D, Vector2D, Vector2D>(polygons[index], ref centroid, Vector2D.Subtract);
            }
            return result;
        }
        public static Vector2D[][] ReduceRange(Vector2D[][] polygons)
        {
            return ReduceRange(polygons, 0);
        }
        public static Vector2D[][] ReduceRange(Vector2D[][] polygons, Scalar areaTolerance)
        {
            if (polygons == null) { throw new ArgumentNullException("polygons"); }
            Vector2D[][] result = new Vector2D[polygons.Length][];
            for (int index = 0; index < polygons.Length; ++index)
            {
                result[index] = Reduce(polygons[index], areaTolerance);
            }
            return result;
        }
        public static Vector2D[][] SubdivideRange(Vector2D[][] polygons, Scalar maxLength)
        {
            return SubdivideRange(polygons, maxLength, true);
        }
        public static Vector2D[][] SubdivideRange(Vector2D[][] polygons, Scalar maxLength, bool loop)
        {
            if (polygons == null) { throw new ArgumentNullException("polygons"); }
            Vector2D[][] result = new Vector2D[polygons.Length][];
            for (int index = 0; index < polygons.Length; ++index)
            {
                result[index] = Subdivide(polygons[index], maxLength, loop);
            }
            return result;
        }

    }
}