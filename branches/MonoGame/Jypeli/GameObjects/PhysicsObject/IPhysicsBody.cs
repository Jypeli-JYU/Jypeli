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

        void MakeStatic();
        void ApplyImpulse( Vector impulse );
    }
}
