using System;
using Jypeli;

namespace Jypeli.SimplePhysics
{
	public sealed class SingleImpulse : Impulse
    {
        Vector force;

        public bool IsExpired { get; private set; }

        public SingleImpulse( Vector force )
        {
            this.force = force;
        }
 
        public Vector GetNextForce()
        {
            if ( IsExpired )
                return Vector.Zero;

			IsExpired = true;
			return force;
        }
    }
}

