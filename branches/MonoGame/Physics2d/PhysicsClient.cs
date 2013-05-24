using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

namespace Jypeli.Physics
{
    public class PhysicsClient : IPhysicsClient
    {
        public IPhysicsBody CreateBody( double width, double height, Shape shape )
        {
            return new PhysicsBody( width, height, shape );
        }
    }
}
