using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli.Physics
{
    public interface IPhysicsClient
    {
        IPhysicsBody CreateBody( IPhysicsObject owner, double width, double height, Shape shape );
    }
}
