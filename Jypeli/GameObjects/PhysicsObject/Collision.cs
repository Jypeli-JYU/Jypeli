using System;
using System.Collections.Generic;

namespace Jypeli.Physics
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Obsolete("Ei käytössä")]
    public class Collision
    {
        public class ContactPoint
        {
            public Vector Position { get; set; }
            public Vector Normal { get; set; }

            public ContactPoint(Vector pos, Vector n)
            {
                Position = pos;
                Normal = n;
            }
        }

        public IPhysicsBody Object1 { get; private set; }
        public IPhysicsBody Object2 { get; private set; }
        public IEnumerable<ContactPoint> Contacts { get; private set; }

        public Collision(IPhysicsBody o1, IPhysicsBody o2, IEnumerable<ContactPoint> contacts)
        {
            Object1 = o1;
            Object2 = o2;
            Contacts = contacts;
        }
    }
}
