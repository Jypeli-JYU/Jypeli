using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli.Physics
{
    public interface IPhysicsClient
    {
        IPhysicsBody CreateBody( IPhysicsObject owner, double width, double height, Shape shape );
        IAxleJoint CreateJoint( IPhysicsObject obj1, IPhysicsObject obj2 );
        IAxleJoint CreateJoint( IPhysicsObject obj1, Vector pivot );

        // Temporary, until PhysicsGame class is moved to Jypeli
        void AddJoint( IAxleJoint joint );
        void RemoveJoint( IAxleJoint joint );
    }
}
