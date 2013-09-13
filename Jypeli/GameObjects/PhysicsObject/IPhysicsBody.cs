using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli.Physics
{
    public interface IPhysicsBody
    {
        IPhysicsObject Owner { get; }

        Vector Size { get; set; }
        Shape Shape { get; set; }
        
        double Mass { get; set; }
        double MassInv { get; set; }
        double MomentOfInertia { get; set; }
        
        double Restitution { get; set; }
        double StaticFriction { get; set; }
        double KineticFriction { get; set; }
        
        Vector Position { get; set; }
        Vector Velocity { get; set; }
        Vector Acceleration { get; set; }

        double Angle { get; set; }
        double AngularVelocity { get; set; }
        double AngularAcceleration { get; set; }

        double LinearDamping { get; set; }
        double AngularDamping { get; set; }

        bool IgnoresCollisionResponse { get; set; }
        bool IgnoresGravity { get; set; }
        bool IgnoresPhysicsLogics { get; set; }

        void MakeStatic();

        void ApplyForce( Vector force );
        void ApplyImpulse( Vector impulse );
        void ApplyTorque( double torque );

        void Stop();
        void StopAxial( Vector axis );
        void StopAngular();

        event CollisionHandler<IPhysicsBody, IPhysicsBody> Collided;
    }
}
