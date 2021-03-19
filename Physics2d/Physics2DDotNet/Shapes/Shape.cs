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
using AdvanceMath;
using AdvanceMath.Geometry2D;

namespace Physics2DDotNet.Shapes
{
    public interface IShape
    {
        object Tag { get;set;}
        Vector2D[] Vertexes { get;}
        /// <summary>
        /// These are the normals for the original vertexes.
        /// </summary>
        Vector2D[] VertexNormals { get;}
        Scalar Inertia { get;}
        bool CanGetIntersection { get;}
        bool CanGetCustomIntersection { get;}

        void CalcBoundingRectangle(ref Matrix2x3 matrix, out BoundingRectangle rectangle);
        bool TryGetIntersection(Vector2D point, out IntersectionInfo info);
        bool TryGetCustomIntersection(Body self, Body other, out object customIntersectionInfo);
    }
    public interface IRaySegmentsCollidable : IShape
    {
        bool TryGetRayCollision(Body thisBody, Body raysBody, RaySegmentsShape raySegments, out RaySegmentIntersectionInfo info);
    }
    public interface IHasArea : IShape
    {
        Scalar Area { get;}
        Vector2D Centroid { get;}
        void GetDistance(ref Vector2D point, out Scalar result);
    }
    public interface IGlobalFluidAffectable : IHasArea
    {
        DragInfo GetFluidInfo(Vector2D tangent);
    }
    public delegate Vector2D GetTangentCallback(Vector2D centroid);
    public interface ILineFluidAffectable : IGlobalFluidAffectable
    {
        FluidInfo GetFluidInfo(GetTangentCallback callback, Line line);
    }
    public interface IExplosionAffectable : IGlobalFluidAffectable
    {
        DragInfo GetExplosionInfo(Matrix2x3 matrix, Scalar radius, GetTangentCallback callback);
    }
}