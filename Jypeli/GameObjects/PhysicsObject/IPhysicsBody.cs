using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Jypeli.Physics
{
    public interface IPhysicsBody
    {
        IPhysicsObject Owner { get; }

        Vector Size { get; set; }
        Shape Shape { get; set; }
        List<List<Vector2>> Vertices { get; }
        
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
        bool CanRotate { get; set; }

        void MakeStatic();
        void MakeOneWay();
        void MakeOneWay(Vector direction);

        void ApplyForce( Vector force );
        void ApplyImpulse( Vector impulse );
        void ApplyTorque( double torque );

        void Stop();
        void StopAxial( Vector axis );
        void StopAngular();

        void SetCollisionIgnorer( Ignorer ignorer );

        event CollisionHandler<IPhysicsBody, IPhysicsBody> Collided;
        event AdvancedCollisionHandler<IPhysicsBody, IPhysicsBody> Colliding;
    }
}
