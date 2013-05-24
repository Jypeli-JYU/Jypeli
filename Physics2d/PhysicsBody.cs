using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AdvanceMath;
using Jypeli;
using Physics2DDotNet;
using Physics2DDotNet.Shapes;

namespace Jypeli.Physics
{
    public class PhysicsBody : IPhysicsBody
    {
        Physics2DDotNet.Body innerBody;
        Shape _shape;
        Vector _size;

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

        /// <summary>
        /// Olion paikka koordinaatistossa. Käsittää sekä X- että Y-koordinaatin.
        /// </summary>
        [Save]
        public Vector Position
        {
            get
            {
                Vector2D v = innerBody.State.Position.Linear;
                return new Vector( v.X, v.Y );
            }
            set { innerBody.State.Position.Linear = new Vector2D( value.X, value.Y ); }
        }

        /// <summary>
        /// Olion koko (x on leveys, y on korkeus).
        /// </summary>
        public Vector Size
        {
            get
            {
                return _size;
            }
            set
            {
                innerBody.Shape = CreatePhysicsShape( _shape, value );
                _size = value;
            }
        }

        public double Mass
        {
            get
            {
                return innerBody.Mass.Mass;
            }
            set
            {
                innerBody.Mass = Body.GetMassInfo( value, innerBody.Shape );
            }
        }

        public double MassInv
        {
            get
            {
                return innerBody.Mass.MassInv;
            }
            set
            {
                Mass = 1 / value;
            }
        }

        public double Restitution
        {
            get
            {
                return innerBody.Coefficients.Restitution;
            }
            set
            {
                innerBody.Coefficients.Restitution = value;
            }
        }
        
        public Vector Velocity
        {
            get
            {
                return (Vector)innerBody.State.Velocity.Linear;
            }
            set
            {
                innerBody.State.Velocity.Linear = (Vector2D)value;
            }
        }

        public Vector Acceleration
        {
            get
            {
                return (Vector)innerBody.State.Acceleration.Linear;
            }
            set
            {
                innerBody.State.Acceleration.Linear = (Vector2D)value;
            }
        }

        public PhysicsBody( double width, double height, Shape shape )
        {
            Coefficients c = new Coefficients( DefaultCoefficients.Restitution, DefaultCoefficients.StaticFriction, DefaultCoefficients.DynamicFriction );
            innerBody = new Body( new PhysicsState( ALVector2D.Zero ), physicsShape, DefaultMass, c, new Lifespan() );
        }

        /// <summary>
        /// Tekee oliosta staattisen. Staattinen olio ei liiku muiden olioiden törmäyksistä,
        /// vaan ainoastaan muuttamalla suoraan sen paikkaa tai nopeutta.
        /// </summary>
        public void MakeStatic()
        {
            Mass = double.PositiveInfinity;
            //CanRotate = false;
            //IgnoresGravity = true;
        }

        public void ApplyImpulse( Vector impulse )
        {
            innerBody.ApplyImpulse( (Vector2D)impulse );
        }

        internal static IShape CreatePhysicsShape( Shape shape, Vector size )
        {
            return CreatePhysicsShape( shape, size, GetDefaultParameters( size.X, size.Y ) );
        }

        private static CollisionShapeParameters GetDefaultParameters( double width, double height )
        {
            CollisionShapeParameters p;
            p.MaxVertexDistance = Math.Min( width, height ) / 3;
            p.DistanceGridSpacing = Math.Min( width, height ) / 2;
            return p;
        }

        internal void SetShape( Shape shape, CollisionShapeParameters parameters )
        {
            _shape = shape;
            innerBody.Shape = CreatePhysicsShape( shape, Size, parameters );
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
