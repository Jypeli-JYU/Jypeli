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
    static class ShapeHelper
    {
        sealed class StubComparer : System.Collections.Generic.IComparer<SAPNode>
        {
            public int Compare(SAPNode left, SAPNode right)
            {
                if (left.value < right.value) { return -1; }
                if (left.value > right.value) { return 1; }
                return ((left == right) ? (0) : ((left.begin) ? (-1) : (1)));
            }
        }
        sealed class SAPNode
        {
            public bool begin;
            public Scalar value;
            public SAPNode(Scalar value, bool begin)
            {
                this.value = value;
                this.begin = begin;
            }
        }
        static StubComparer comparer = new StubComparer();

        public static FluidInfo GetFluidInfo(Vector2D[] vertexes2, GetTangentCallback callback, Line line)
        {
            Vector2D centroid;
            Scalar area;
            Vector2D dragCenter;
            Scalar dragArea;
            Vector2D[] vertexes = VertexHelper.GetIntersection(vertexes2, line);
            if (vertexes.Length < 3) { return null; }
            centroid = VertexHelper.GetCentroid(vertexes);
            area = VertexHelper.GetArea(vertexes);
            Vector2D tangent = callback(centroid);
            Scalar min, max;
            ShapeHelper.GetProjectedBounds(vertexes, tangent, out min, out max);
            Scalar avg = (max + min) / 2;
            dragCenter = tangent * avg;
            dragArea = max - min;
            return new FluidInfo(dragCenter, dragArea, centroid, area);
        }

        public static void GetFluidInfo(Vector2D[][] polygons, Vector2D tangent, out Vector2D dragCenter, out Scalar dragArea)
        {
            Scalar min, max;
            Scalar avg;
            if (polygons.Length == 1)
            {
                ShapeHelper.GetProjectedBounds(
                    polygons[0],
                    tangent, out min, out max);
                avg = (max + min) / 2;
                dragCenter = tangent * avg;
                dragArea = max - min;
                return;
            }
            SAPNode[] sapNodes = new SAPNode[polygons.Length * 2];
            for (int index = 0; index < polygons.Length; ++index)
            {
                ShapeHelper.GetProjectedBounds(polygons[index], tangent, out min, out max);
                sapNodes[index * 2] = new SAPNode(min, true);
                sapNodes[(index * 2) + 1] = new SAPNode(max, false);
            }
            Array.Sort<SAPNode>(sapNodes, comparer);
            int depth = 0;
            Scalar result = 0;
            Scalar start = 0;
            for (int index = 0; index < sapNodes.Length; ++index)
            {
                SAPNode node = sapNodes[index];
                if (node.begin)
                {
                    if (depth == 0)
                    {
                        start = node.value;
                    }
                    depth++;
                }
                else
                {
                    depth--;
                    if (depth == 0)
                    {
                        result += node.value - start;
                    }
                }
            }
            avg = (sapNodes[0].value + sapNodes[sapNodes.Length - 1].value) / 2;
            dragCenter = tangent * avg;
            dragArea = result;
        }


        public static void GetProjectedBounds(Vector2D[] vertexes, Vector2D tangent, out Scalar min, out Scalar max)
        {
            Scalar value;
            Vector2D.Dot(ref vertexes[0], ref tangent, out value);
            min = value;
            max = value;
            for (int index = 1; index < vertexes.Length; ++index)
            {
                Vector2D.Dot(ref vertexes[index], ref tangent, out value);
                if (value > max)
                {
                    max = value;
                }
                else if (value < min)
                {
                    min = value;
                }
            }
        }
    }

}