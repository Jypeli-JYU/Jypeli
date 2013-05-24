using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli.Physics;

namespace Jypeli
{
    public partial class Game
    {
        internal IPhysicsClient PhysicsClient;

        private void InitPhysics()
        {
            PhysicsClient = PhysicsInterface.GetClient();
        }
    }
}
