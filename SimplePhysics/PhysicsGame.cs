using System;
using System.Collections.Generic;
using System.Text;
using Jypeli;
using Jypeli.Physics;

namespace Jypeli
{
    public partial class PhysicsGame : Game
    {
        class Contact
        {
            public PhysicsObject obj1;
            public PhysicsObject obj2;
            public Vector normal;
            public int deadcycles;

            public bool Active
            {
                get { return deadcycles == 0; }
            }

            public Contact( PhysicsObject obj1, PhysicsObject obj2, Vector n )
            {
                this.obj1 = obj1;
                this.obj2 = obj2;
                this.normal = n;
                this.deadcycles = 0;
            }

            public void UpdateContact()
            {
                BoundingRectangle intsect = BoundingRectangle.GetIntersection( obj1.BoundingRectangle, obj2.BoundingRectangle );
                Vector n = ( intsect.Position - obj1.Position ).Normalize();
                deadcycles = 0;
            }

            public void UpdateNoContact()
            {
                this.deadcycles++;
            }
        }

        List<PhysicsObject> physObjects = new List<PhysicsObject>();
        List<Contact> contacts = new List<Contact>();

        /// <summary>
        /// Painovoima.
        /// </summary>
        public Vector Gravity { get; set; }

        /// <summary>
        /// Luo uuden fysiikkapelin.
        /// </summary>
        public PhysicsGame()
            : base()
        {
        }

        /// <summary>
        /// Lisää olion peliin.
        /// </summary>
        /// <param name="o">Olio</param>
        /// <param name="layer">Kerros jolle lisätään</param>
        public override void Add( IGameObject o, int layer )
        {
            if ( o is PhysicsObject )
            {
                physObjects.Add( (PhysicsObject)o );
            }

            base.Add( o, layer );
        }

        /// <summary>
        /// Jypeli kutsuu tätä metodia joka päivityksellä.
        /// </summary>
        /// <param name="time">Olio joka kertoo ajan viime päivityksestä ja pelin alusta</param>
        protected override void Update( Time time )
        {
            const double maxdt = 0.01;
            double dt = time.SinceLastUpdate.TotalSeconds;

            while ( dt > maxdt )
            {
                Integrate( maxdt );
                dt -= maxdt;
            }

            Cleanup();
            Integrate( dt );

            base.Update( time );
        }

        private void Cleanup()
        {
            for ( int i = physObjects.Count - 1; i >= 0; i-- )
            {
                if ( physObjects[i].IsDestroyed )
                    physObjects.RemoveAt( i );
            }

            for ( int i = contacts.Count - 1; i >= 0; i-- )
            {
                if ( contacts[i].obj1 == null || contacts[i].obj2 == null
                    || contacts[i].obj1.IsDestroyed || contacts[i].obj2.IsDestroyed
                    || contacts[i].deadcycles > 1000 )
                {
                    //MessageDisplay.Add( "Contact removed", RandomGen.NextColor( contacts[i] ) );
                    contacts.RemoveAt( i );
                }
            }
        }

        private void Integrate( double dt )
        {
            if ( dt <= 0 ) return;

            Vector temp;

            for ( int i = 0; i < physObjects.Count; i++ )
            {
                PhysicsBody body = (PhysicsBody)physObjects[i].Body;
                if ( !body.IgnoresGravity && !body.IgnoresPhysicsLogics )
                    body.Velocity += Gravity * body.MassInv * dt;

                temp = body.Acceleration;
                NullifyContactForces( physObjects[i], ref temp );
                body.Acceleration = temp;
                body.Velocity += temp * dt;

                temp = body.Velocity;
                NullifyContactForces( physObjects[i], ref temp );
                body.Velocity = temp;
                body.Position += temp * dt;
                
                // Acceleration is for this update only
                body.Acceleration = Vector.Zero;
            }

            SolveCollisions( dt );
        }

        private bool HasContact( PhysicsObject obj1, PhysicsObject obj2 )
        {
            Contact contact = GetContact( obj1, obj2 );
            return contact != null && contact.Active;
        }

        private Contact GetContact( PhysicsObject obj1, PhysicsObject obj2 )
        {
            return contacts.Find<Contact>( c => c.obj1 == obj1 && c.obj2 == obj2 || c.obj1 == obj2 && c.obj2 == obj1 );
        }

        private void AddContact( PhysicsObject obj1, PhysicsObject obj2, Vector n )
        {
            Contact contact = GetContact( obj1, obj2 );

            if ( contact != null )
            {
                //MessageDisplay.Add( "Contact enabled", RandomGen.NextColor( contact ) );
                contact.UpdateContact();
            }
            else
            {
                contact = new Contact( obj1, obj2, n );
                //MessageDisplay.Add( "New contact!", RandomGen.NextColor( contact ) );
                contacts.Add( contact );
            }
        }

        private void NullifyContactForces( PhysicsObject obj, ref Vector vector )
        {
            foreach ( var contact in contacts.FindAll( c => c.obj1 == obj ) )
            {
                if ( !contact.Active )
                    continue;

                var target = contact.obj2;
                double par = vector.ScalarProjection( contact.normal );
                double perp = vector.ScalarProjection( contact.normal.LeftNormal );

                vector = perp * contact.normal.LeftNormal + ( par < 0 ? par : 0 ) * contact.normal;
            }

            foreach ( var contact in contacts.FindAll( c => c.obj2 == obj ) )
            {
                if ( !contact.Active )
                    continue;

                var target = contact.obj1;
                double par = vector.ScalarProjection( contact.normal );
                double perp = vector.ScalarProjection( contact.normal.LeftNormal );

                vector = perp * contact.normal.LeftNormal + ( par < 0 ? par : 0 ) * contact.normal;
            }
        }

        private bool IsOverlapping( PhysicsObject obj1, PhysicsObject obj2 )
        {
            // Just a simple rectangle check for now
            return BoundingRectangle.Intersects( obj1.BoundingRectangle, obj2.BoundingRectangle );
        }

        private void SolveCollisions( double dt )
        {
            for ( int i = 0; i < physObjects.Count; i++ )
            {
                var iBody = (PhysicsBody)( physObjects[i].Body );
                var iStatic = double.IsInfinity( iBody.Mass );

                for ( int j = 0; j < i; j++ )
                {
                    var jBody = (PhysicsBody)( physObjects[j].Body );
                    var jStatic = double.IsInfinity( jBody.Mass );
                    Vector n;

                    if ( iStatic && jStatic )
                        continue;

                    Contact contact = GetContact( physObjects[i], physObjects[j] );

                    if ( IsOverlapping( physObjects[i], physObjects[j] ) )
                    {
                        if ( contact != null && contact.Active )
                        {
                            // Already responded to this collision
                            contact.UpdateContact();
                            continue;
                        }

                        // Collision (perfectly elastic)
                        BoundingRectangle intsect = BoundingRectangle.GetIntersection( physObjects[i].BoundingRectangle, physObjects[j].BoundingRectangle );
                        double rest = physObjects[i].Restitution * physObjects[j].Restitution;

                        if ( iStatic || jStatic )
                        {
                            int index = iStatic ? j : i;
                            n = ( intsect.Position - physObjects[index].Position ).Normalize();
                            double v_n = physObjects[index].Velocity.ScalarProjection( n );

                            if ( v_n > 0 )
                            {
                                // Approaching the wall
                                if ( !physObjects[i].IgnoresCollisionResponse && !physObjects[j].IgnoresCollisionResponse )
                                {
                                    // Collision response
                                    Vector p = n.LeftNormal;
                                    double v_p = physObjects[index].Velocity.ScalarProjection( p );
                                    physObjects[index].Velocity = rest * ( v_p * p - v_n * n );
                                }
                            }
                        }
                        else
                        {
                            // TODO: Restitution
                            Vector p_i = physObjects[i].Mass * physObjects[i].Velocity;
                            Vector p_j = physObjects[j].Mass * physObjects[j].Velocity;
                            Vector p_sum = p_i + p_j;
                            Vector p_diff = p_j - p_i;
                            n = ( physObjects[j].AbsolutePosition - physObjects[i].AbsolutePosition ).Normalize();

                            Vector v_j = ( p_sum - p_diff ) / 2;
                            Vector v_i = p_sum - v_j;

                            if ( !physObjects[i].IgnoresCollisionResponse && !physObjects[j].IgnoresCollisionResponse )
                            {
                                // Collision response
                                physObjects[i].Velocity = v_i;
                                physObjects[j].Velocity = v_j;
                            }
                        }

                        // Collision event
                        iBody.OnCollided( jBody );
                        jBody.OnCollided( iBody );
                        AddContact( physObjects[i], physObjects[j], n );
                    }
                    else
                    {
                        // No contact between these objects
                        if ( contact != null )
                        {
                            /*if ( contact.Active )
                                MessageDisplay.Add( "Contact disabled", RandomGen.NextColor( contact ) );*/

                            contact.UpdateNoContact();
                        }
                    }
                }
            }
        }
    }
}
