using System;
using System.Collections.Generic;
using System.Text;
using Jypeli;

namespace Jypeli.SimplePhysics
{
    public class ImpulseCollection
    {
        List<Impulse> impulses = new List<Impulse>();

        public void Add(Impulse impulse)
        {
            impulses.Add( impulse );
        }

        public Vector GetNextForce()
        {
            Vector force = Vector.Zero;

            for ( int i = 0; i < impulses.Count; i++ )
            {
                if ( impulses[i].IsExpired )
                    impulses.RemoveAt( i-- );
                else
                    force += impulses[i].GetNextForce();
            }

            return force;
        }
    }
}