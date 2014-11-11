using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

namespace Jypeli.Physics
{
    public class PhysicsClient : IPhysicsClient
    {
        public IPhysicsBody CreateBody( IPhysicsObject owner, double width, double height, Shape shape )
        {
            return new PhysicsBody( width, height, shape ) { Owner = owner };
        }

        public IAxleJoint CreateJoint( IPhysicsObject obj1, IPhysicsObject obj2 )
        {
            return new AxleJoint( obj1 as PhysicsObject, obj2 as PhysicsObject );
        }

        public IAxleJoint CreateJoint( IPhysicsObject obj1, Vector pivot )
        {
            return new AxleJoint( obj1 as PhysicsObject, pivot );
        }

        public void AddJoint( IAxleJoint joint )
        {
            ( (PhysicsGameBase)Game.Instance ).Add( (AxleJoint)joint );
        }

        public void RemoveJoint( IAxleJoint joint )
        {
            ( (PhysicsGameBase)Game.Instance ).Remove( (AxleJoint)joint );
        }
    }
}
