using System;
using FarseerPhysics.Dynamics;
using AdvanceMath;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Jypeli.Farseer;

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
            get => Body._xf.P * FSConvert.SimToDisplay;
            set
            {
                Vector2 temp = new Vector2((float)value.X, (float)value.Y) * FSConvert.DisplayToSim;
                //Debug.Assert(!float.IsNaN(value.X) && !float.IsNaN(value.Y));
                Body.SetTransform(ref temp, (float)Angle);
            }
        }

        /// <summary>
        /// Olion koko (x on leveys, y on korkeus).
        /// </summary>
        [Save]
        public Vector Size
        {
            get
            {
                return _size * FSConvert.SimToDisplay;
            }
            set
            {
                Body.Shape = CreatePhysicsShape( _shape, value * FSConvert.DisplayToSim);
                _size = value * FSConvert.DisplayToSim;
            }
        }

        /// <summary>
        /// Kulma, jossa olio on. Oliota voi pyörittää kulmaa vaihtamalla.
        /// </summary>
        [Save]
        public double Angle
        {
            get { return Body.Rotation; }
            set { Body.Rotation = (float)value; }
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
            //Body.Shape = CreatePhysicsShape( shape, Size, parameters );
        }

        internal static Shape CreatePhysicsShape( Shape shape, Vector size )
        {
            return CreatePhysicsShape( shape, size, GetDefaultParameters( size.X, size.Y ) );
        }

        /// <summary>
        /// Creates a shape to be used in the Physics Body. A physics shape is scaled to the
        /// size of the object. In addition, it has more vertices and some additional info
        /// that is used in collision detection.
        /// </summary>
        internal static Shape CreatePhysicsShape( Shape shape, Vector size, CollisionShapeParameters parameters )
        {
            if ( shape is RaySegment )
            {
                RaySegment raySegment = (RaySegment)shape;
                return null;
                
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
                    return null;//new CircleShape( r, vertexCount );
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

                    return null;//new PolygonShape( vertexes, parameters.DistanceGridSpacing );
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

                //Vector2D[] polyVertexes = VertexHelper.Subdivide( originalVertexes, parameters.MaxVertexDistance );

                return null;//new PolygonShape( polyVertexes, parameters.DistanceGridSpacing );
            }
        }
    }
}
