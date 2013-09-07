using System;
using System.Collections.Generic;
using System.Text;
using Jypeli;

namespace Jypeli.Physics
{
    [Save]
    public class PhysicsBody : IPhysicsBody
    {
        [Save] private double _mass = 1.0;
        [Save] private double _massInv = 1.0;
        [Save] private double _restitution = 1.0;
        [Save] private double _staticFriction = 0.0;
        [Save] private double _kineticFriction = 0.0;

        public event CollisionHandler<IPhysicsBody, IPhysicsBody> Collided;

        [Save]
        public IPhysicsObject Owner { get; internal set; }

        [Save]
        public Shape Shape { get; private set; }

        public double Mass
        {
            get { return _mass; }
            set
            {
                _mass = value;
                _massInv = 1 / value;
            }
        }

        public double MassInv
        {
            get { return _massInv; }
            set
            {
                _massInv = value;
                _mass = 1 / value;
            }
        }

        public double MomentOfInertia
        {
            get { return double.PositiveInfinity; }
            set { }
        }

        public double Restitution
        {
            get { return _restitution; }
            set { _restitution = value; }
        }

        public double KineticFriction
        {
            get { return _kineticFriction; }
            set { _kineticFriction = value; }
        }

        public double StaticFriction
        {
            get { return _staticFriction; }
            set { _staticFriction = value; }
        }

        public double AngularDamping
        {
            get { return 0; }
            set { }
        }

        public double LinearDamping
        {
            get { return 0; }
            set { }
        }

        [Save]
        public Vector Position { get; set; }

        [Save]
        public Vector Velocity { get; set; }

        [Save]
        public Vector Acceleration { get; set; }

        [Save]
        public Vector Size { get; set; }

        public double Angle
        {
            get { return 0; }
            set { }
        }

        public double AngularVelocity
        {
            get { return 0; }
            set { }
        }

        public double AngularAcceleration
        {
            get { return 0; }
            set { }
        }

        public bool IgnoresCollisionResponse { get; set; }

        public bool IgnoresGravity { get; set; }

        public bool IgnoresPhysicsLogics { get; set; }

        public PhysicsBody( double width, double height, Shape shape )
        {
            Shape = shape;
            Size = new Vector( width, height );
        }

        public void MakeStatic()
        {
            _mass = double.PositiveInfinity;
            _massInv = 0;
        }

        public void ApplyForce( Vector force )
        {
            Acceleration += force * _massInv;
        }

        public void ApplyImpulse( Vector impulse )
        {
            Velocity += impulse * _massInv;
        }

        public void ApplyTorque( double torque )
        {
            // No angular motion implemented (yet?)
        }

        public void Stop()
        {
            Acceleration = Vector.Zero;
            Velocity = Vector.Zero;
        }

        public void StopAngular()
        {
            // No angular motion to stop :)
        }

        public void StopAxial( Vector axis )
        {
            Acceleration = Acceleration.Project( axis.LeftNormal );
            Velocity = Velocity.Project( axis.LeftNormal );
        }
    }
}
