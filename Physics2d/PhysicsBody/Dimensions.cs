#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using Physics2DDotNet.Shapes;
using AdvanceMath;
using Physics2DDotNet;
using System.Diagnostics;
namespace Jypeli
{
    public partial class PhysicsBody
    {
        Shape _shape;
        Vector _size;

        /// <summary>
        /// Fysiikkamoottorin käyttämä tietorakenne.
        /// </summary>
        public Body Body { get; private set; }

        /// <summary>
        /// Olion paikka koordinaatistossa. Käsittää sekä X- että Y-koordinaatin.
        /// </summary>
        [Save]
        public Vector Position
        {
            get
            {
                Vector2D v = Body.State.Position.Linear;
                return new Vector( v.X, v.Y );
            }
            set { Body.State.Position.Linear = new Vector2D( value.X, value.Y ); }
        }

        /// <summary>
        /// Olion koko (x on leveys, y on korkeus).
        /// </summary>
        [Save]
        public Vector Size
        {
            get
            {
                return _size;
            }
            set
            {
                Body.Shape = CreatePhysicsShape( _shape, value );
                _size = value;
            }
        }

        /// <summary>
        /// Kulma, jossa olio on. Oliota voi pyörittää kulmaa vaihtamalla.
        /// </summary>
        [Save]
        public double Angle
        {
            get { return Body.State.Position.Angular; }
            set { Body.State.Position.Angular = value; }
        }

        /// <summary>
        /// Olion muoto.
        /// </summary>
        public Shape Shape
        {
            get { return _shape; }
            set
            {
                SetShape( value, GetDefaultParameters( Size.X, Size.Y ) );
            }
        }

        internal void SetShape( Shape shape, CollisionShapeParameters parameters )
        {
            _shape = shape;
            Body.Shape = CreatePhysicsShape( shape, Size, parameters );
        }

        internal static IShape CreatePhysicsShape( Shape shape, Vector size )
        {
            return CreatePhysicsShape( shape, size, GetDefaultParameters( size.X, size.Y ) );
        }

        /// <summary>
        /// Creates a shape to be used in the Physics Body. A physics shape is scaled to the
        /// size of the object. In addition, it has more vertices and some additional info
        /// that is used in collision detection.
        /// </summary>
        internal static IShape CreatePhysicsShape( Shape shape, Vector size, CollisionShapeParameters parameters )
        {
            if ( shape is RaySegment )
            {
                RaySegment raySegment = (RaySegment)shape;
                Physics2DDotNet.Shapes.RaySegment singleSegment = new Physics2DDotNet.Shapes.RaySegment(
                    new Vector2D( raySegment.Origin.X, raySegment.Origin.Y ),
                    new Vector2D( raySegment.Direction.X, raySegment.Direction.Y ),
                    raySegment.Length );
                return new RaySegmentsShape( singleSegment );
            }
            else if ( shape is Ellipse )
            {
                Debug.Assert( shape.IsUnitSize );

                double smaller = Math.Min( size.X, size.Y );
                double bigger = Math.Max( size.X, size.Y );
                // Average between width and height.
                double r = smaller / 2 + ( bigger - smaller ) / 2;
                int vertexCount = (int)Math.Ceiling( ( 2 * Math.PI * r ) / parameters.MaxVertexDistance );

                if ( Math.Abs( size.X - size.Y ) <= double.Epsilon )
                {
                    // We get more accurate results by using the circleshape.
                    // in addition, the circleshape does not need a DistanceGrid
                    // object (which is slow to initialize) because calculations
                    // for a circleshape are much simpler.
                    return new CircleShape( r, vertexCount );
                }
                else
                {
                    Vector2D[] vertexes = new Vector2D[vertexCount];

                    double a = 0.5 * size.X;
                    double b = 0.5 * size.Y;

                    for ( int i = 0; i < vertexCount; i++ )
                    {
                        double t = ( i * 2 * Math.PI ) / vertexCount;
                        double x = a * Math.Cos( t );
                        double y = b * Math.Sin( t );
                        vertexes[i] = new Vector2D( x, y );
                    }

                    return new PolygonShape( vertexes, parameters.DistanceGridSpacing );
                }
            }
            else
            {
                Vector2D[] originalVertexes = new Vector2D[shape.Cache.OutlineVertices.Length];
                for ( int i = 0; i < shape.Cache.OutlineVertices.Length; i++ )
                {
                    Vector v = shape.Cache.OutlineVertices[i];
                    if ( shape.IsUnitSize )
                    {
                        v.X *= size.X;
                        v.Y *= size.Y;
                    }
                    originalVertexes[i] = new Vector2D( v.X, v.Y );
                }

                Vector2D[] polyVertexes = VertexHelper.Subdivide( originalVertexes, parameters.MaxVertexDistance );

                return new PolygonShape( polyVertexes, parameters.DistanceGridSpacing );
            }
        }
    }
}
