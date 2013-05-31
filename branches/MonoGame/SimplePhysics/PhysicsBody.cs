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
        private double _restitution = 1.0;
        private double _kineticFriction = 0.0;

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

        public void ApplyImpulse( Vector impulse )
        {
            Velocity += impulse * _massInv;
        }
    }
}
