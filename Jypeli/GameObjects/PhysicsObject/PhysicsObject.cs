using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli.Physics;

namespace Jypeli
{
    [Save]
    public partial class PhysicsObject : GameObject, IPhysicsObjectInternal
    {
        [Save]
        private double _storedMomentOfInertia = 1;

        [Save]
        public IPhysicsBody Body { get; private set; }

        public int CollisionIgnoreGroup
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override Vector Position
        {
            get { return Body.Position; }
            set { Body.Position = value; }
        }

        public double Mass
        {
            get { return Body.Mass; }
            set
            {
                Body.Mass = value;
                _storedMomentOfInertia = Body.MomentOfInertia;
            }
        }

        public double MomentOfInertia
        {
            get { return Body.MomentOfInertia; }
            set
            {
                Body.MomentOfInertia = value;
                _storedMomentOfInertia = Body.MomentOfInertia;
            }
        }

        /// <summary>
        /// Jos <c>false</c>, olio ei voi pyöriä.
        /// </summary>
        public bool CanRotate
        {
            get { return !double.IsPositiveInfinity( MomentOfInertia ); }
            set
            {
                if ( !value )
                {
                    _storedMomentOfInertia = Body.MomentOfInertia;
                    Body.MomentOfInertia = double.PositiveInfinity;
                }
                else
                {
                    Body.MomentOfInertia = _storedMomentOfInertia;
                }
            }
        }

        public double Restitution
        {
            get { return Body.Restitution; }
            set { Body.Restitution = value; }
        }

        public double StaticFriction
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double KineticFriction
        {
            get
            {
                return Body.KineticFriction;
            }
            set
            {
                Body.KineticFriction = value;
            }
        }

        public Vector Velocity
        {
            get { return Body.Velocity; }
            set { Body.Velocity = value; }
        }

        public Vector Acceleration
        {
            get { return Body.Acceleration; }
            set { Body.Acceleration = value; }
        }

        public double AngularVelocity
        {
            get { return Body.AngularVelocity; }
            set { Body.AngularVelocity = value; }
        }

        public double AngularAcceleration
        {
            get { return Body.AngularAcceleration; }
            set { Body.AngularAcceleration = value; }
        }

        public double LinearDamping
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double AngularDamping
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IgnoresExplosions
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IgnoresCollisionResponse
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IgnoresGravity
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IgnoresPhysicsLogics
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private BoundingRectangle _bRect = new BoundingRectangle();

        public BoundingRectangle BoundingRectangle
        {
            get
            {
                _bRect.Position = this.Position;
                _bRect.Size = this.Size;
                return _bRect;
            }
        }

        public event CollisionHandler<IPhysicsObject, IPhysicsObject> Collided;

        public PhysicsObject( double width, double height, Shape shape )
            : base( width, height, shape )
        {
            Body = Game.Instance.PhysicsClient.CreateBody( width, height, shape );
        }

        public PhysicsObject( double width, double height )
            : this( width, height, Shape.Rectangle )
        {
        }

        public static PhysicsObject CreateStaticObject( double width, double height, Shape shape )
        {
            var obj = new PhysicsObject( width, height, shape );
            obj.MakeStatic();
            return obj;
        }

        public static PhysicsObject CreateStaticObject( double width, double height )
        {
            var obj = new PhysicsObject( width, height );
            obj.MakeStatic();
            return obj;
        }

        public void MakeStatic()
        {
            Body.MakeStatic();
        }

        public void Push( Vector force )
        {
            throw new NotImplementedException();
        }

        public void Push( Vector force, TimeSpan time )
        {
            throw new NotImplementedException();
        }

        public void Hit( Vector impulse )
        {
            Body.ApplyImpulse( impulse );
        }

        public void ApplyTorque( double torque )
        {
            throw new NotImplementedException();
        }

        public void StopAxial( Vector axis )
        {
            throw new NotImplementedException();
        }

        public void StopHorizontal()
        {
            throw new NotImplementedException();
        }

        public void StopVertical()
        {
            throw new NotImplementedException();
        }

        public void StopAngular()
        {
            throw new NotImplementedException();
        }

        public bool IsDestroying
        {
            get { throw new NotImplementedException(); }
        }

        public event Action Destroying;
    }
}
