using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli.Physics
{
    public interface IPhysicsBody
    {
        Shape Shape { get; }
        
        double Mass { get; set; }
        double MassInv { get; set; }
        double MomentOfInertia { get; set; }
        
        double Restitution { get; set; }
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
        void ApplyImpulse( Vector impulse );
    }
}
