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
    /// Represents a Single point.
    /// </summary>
    public sealed class ParticleShape : IShape
    {
        #region static
        /// <summary>
        /// All particles are the same! so use this one!
        /// </summary>
        public static readonly ParticleShape Default = new ParticleShape();
        #endregion
        #region fields
        private object tag;
        private Vector2D[] vertexes; 
        #endregion
        #region constructors
        /// <summary>
        /// Creates a new Particle Instance.
        /// </summary>
        public ParticleShape()
        {
            this.vertexes = new Vector2D[1] { Vector2D.Zero };
        }
        #endregion
        #region Properties
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }
        public Vector2D[] Vertexes { get { return vertexes; } }
        public Vector2D[] VertexNormals { get { return null; } }
        public Scalar Inertia
        {
            get { return 1; }
        }
        public bool CanGetIntersection
        {
            get { return false; }
        }
        public bool CanGetCustomIntersection
        {
            get { return false; }
        }
        #endregion
        #region Methods
        public void CalcBoundingRectangle(ref Matrix2x3 matrix, out BoundingRectangle rectangle)
        {
            rectangle.Max.X = matrix.m02;
            rectangle.Max.Y = matrix.m12;
            rectangle.Min = rectangle.Max;
        }
        public bool TryGetIntersection(Vector2D point, out IntersectionInfo info)
        {
            throw new NotSupportedException();
        }
        public bool TryGetCustomIntersection(Body self, Body other, out object customIntersectionInfo)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}