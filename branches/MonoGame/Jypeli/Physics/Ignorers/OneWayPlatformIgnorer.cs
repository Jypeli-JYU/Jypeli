﻿using AdvanceMath;
using Jypeli.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli.Physics
{
    /// <summary>
    /// Allows us to have platforms that are one way.
    /// </summary>
    public class OneWayPlatformIgnorer : Ignorer
    {
        double depthAllowed;

        public OneWayPlatformIgnorer(double depthAllowed)
        {
            this.depthAllowed = depthAllowed;
        }

        public override bool BothNeeded
        {
            get
            {
                return false;
            }
        }

        public override bool CanCollide(IPhysicsBody thisBody, IPhysicsBody otherBody, Ignorer other)
        {
            if (otherBody.IgnoresPhysicsLogics)
                // || otherBody.IsBroadPhaseOnly)
            {
                return true;
            }

            return thisBody.Position.Y - depthAllowed > otherBody.Position.Y;
        }
    }
}
