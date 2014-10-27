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
            public PhysicsBody body1;
            public PhysicsBody body2;
            public Vector normal;
            public int deadcycles;

            public bool Active
            {
                get { return deadcycles == 0; }
            }

            public Contact( PhysicsBody body1, PhysicsBody body2, Vector n )
            {
                this.body1 = body1;
                this.body2 = body2;
                this.normal = n;
                this.deadcycles = 0;
            }

            public void UpdateContact()
            {
                BoundingRectangle intsect = BoundingRectangle.GetIntersection( body1.GetBoundingRect(), body2.GetBoundingRect() );
                Vector n = ( intsect.Position - body1.Position ).Normalize();
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
            PhysicsObject po = o as PhysicsObject;

            if ( po != null )
            {
                if ( po.Body == null )
                    throw new ArgumentException( "Physics object has no body." );

                if ( !(po.Body is PhysicsBody) )
                    throw new ArgumentException( "Physics object has unrecognizable body type." );
                
                physObjects.Add( po );
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
                if ( contacts[i].body1 == null || contacts[i].body2 == null
                    || contacts[i].body1.Owner.IsDestroyed || contacts[i].body2.Owner.IsDestroyed
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
                NullifyContactForces( body, ref temp );
                body.Acceleration = temp;
                body.Velocity += temp * dt;

                temp = body.Velocity;
                NullifyContactForces( body, ref temp );
                body.Velocity = temp;
                body.Position += temp * dt;
                
                // Acceleration is for this update only
                body.Acceleration = Vector.Zero;
            }

            SolveCollisions( dt );
        }

        private bool HasContact( PhysicsBody body1, PhysicsBody body2 )
        {
            Contact contact = GetContact( body1, body2 );
            return contact != null && contact.Active;
        }

        private Contact GetContact( PhysicsBody body1, PhysicsBody body2 )
        {
            return contacts.Find<Contact>( c => c.body1 == body1 && c.body2 == body2 || c.body1 == body2 && c.body2 == body1 );
        }

        private void AddContact( PhysicsBody body1, PhysicsBody body2, Vector n )
        {
            Contact contact = GetContact( body1, body2 );

            if ( contact != null )
            {
                //MessageDisplay.Add( "Contact enabled", RandomGen.NextColor( contact ) );
                contact.UpdateContact();
            }
            else
            {
                contact = new Contact( body1, body2, n );
                //MessageDisplay.Add( "New contact!", RandomGen.NextColor( contact ) );
                contacts.Add( contact );
            }
        }

        private void NullifyContactForces( PhysicsBody body, ref Vector vector )
        {
            foreach ( var contact in contacts.FindAll( c => c.body1 == body ) )
            {
                if ( !contact.Active )
                    continue;

                var target = contact.body2;
                double par = vector.ScalarProjection( contact.normal );
                double perp = vector.ScalarProjection( contact.normal.LeftNormal );

                vector = perp * contact.normal.LeftNormal + ( par < 0 ? par : 0 ) * contact.normal;
            }

            foreach ( var contact in contacts.FindAll( c => c.body2 == body ) )
            {
                if ( !contact.Active )
                    continue;

                var target = contact.body1;
                double par = vector.ScalarProjection( contact.normal );
                double perp = vector.ScalarProjection( contact.normal.LeftNormal );

                vector = perp * contact.normal.LeftNormal + ( par < 0 ? par : 0 ) * contact.normal;
            }
        }

        private bool IsOverlapping( PhysicsBody body1, PhysicsBody body2 )
        {
            // Just a simple rectangle check for now
            return BoundingRectangle.Intersects( body1.GetBoundingRect(), body2.GetBoundingRect() );
        }

        private bool CanCollide( PhysicsObject obj1, PhysicsObject obj2 )
        {
            if ( obj1.IgnoresCollisionResponse || obj2.IgnoresCollisionResponse )
                return false;

            if ( obj1.CollisionIgnorer != null )
            {
                if ( !obj1.CollisionIgnorer.CanCollide( obj1.Body, obj2.Body, obj2.CollisionIgnorer ) )
                    return false;

                if ( !obj1.CollisionIgnorer.BothNeeded )
                    return true;
            }

            if ( obj2.CollisionIgnorer != null )
            {
                if ( !obj2.CollisionIgnorer.CanCollide( obj2.Body, obj1.Body, obj1.CollisionIgnorer ) )
                    return false;
            }

            return true;
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
                        // No collision checking between two static objects
                        continue;

                    if ( iBody.Position == jBody.Position )
                        // No collision checking if there's no collision normal. Usually this happens only if
                        // two objects are manually placed in the same coordinates.
                        continue;

                    Contact contact = GetContact( iBody, jBody );

                    if ( IsOverlapping( iBody, jBody ) )
                    {
                        if ( contact != null && contact.Active )
                        {
                            // Already responded to this collision
                            contact.UpdateContact();
                            continue;
                        }

                        // Collision (perfectly elastic)
                        BoundingRectangle intsect = BoundingRectangle.GetIntersection( iBody.GetBoundingRect(), jBody.GetBoundingRect() );
                        double rest = physObjects[i].Restitution * physObjects[j].Restitution;

                        if ( iStatic || jStatic )
                        {
                            int index = iStatic ? j : i;
                            n = ( intsect.Position - physObjects[index].Position ).Normalize();
                            double v_n = physObjects[index].Velocity.ScalarProjection( n );

                            if ( v_n > 0 )
                            {
                                // Approaching the wall
                                if ( CanCollide( physObjects[i], physObjects[j] ) )
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

                            if ( CanCollide( physObjects[i], physObjects[j] ) )
                            {
                                // Collision response
                                physObjects[i].Velocity = v_i;
                                physObjects[j].Velocity = v_j;
                            }
                        }

                        // Collision event
                        iBody.OnCollided( jBody );
                        jBody.OnCollided( iBody );
                        AddContact( iBody, jBody, n );
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
