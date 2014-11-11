using System;
using System.Collections.Generic;
using System.Text;
using Jypeli;
using Jypeli.Physics;

namespace Jypeli
{
    public partial class SimplePhysicsEngine : IPhysicsEngine
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

        List<PhysicsBody> bodies = new List<PhysicsBody>();
        List<Contact> contacts = new List<Contact>();

        /// <summary>
        /// Painovoima.
        /// </summary>
        public Vector Gravity { get; set; }

        /// <summary>
        /// Luo uuden fysiikkamoottorin.
        /// </summary>
        public SimplePhysicsEngine()
            : base()
        {
        }

        public IPhysicsBody CreateBody( IPhysicsObject owner, double width, double height, Shape shape )
        {
            return new PhysicsBody( width, height, shape ) { Owner = owner };
        }

        public IAxleJoint CreateJoint( IPhysicsObject obj1, IPhysicsObject obj2 )
        {
            throw new NotImplementedException( "Joints are not implemented in SimplePhysics yet." );
        }

        public IAxleJoint CreateJoint( IPhysicsObject obj1, Vector pivot )
        {
            throw new NotImplementedException( "Joints are not implemented in SimplePhysics yet." );
        }

        /// <summary>
        /// Lisää kappaleen moottoriin.
        /// </summary>
        /// <param name="body">Kappale</param>
        public void AddBody( IPhysicsBody body )
        {
            if ( !(body is PhysicsBody) )
                throw new ArgumentException( "Physics object has unrecognizable body type." );
                
            bodies.Add( (PhysicsBody)body );
        }

        /// <summary>
        /// Lisää kappaleen moottoriin.
        /// </summary>
        /// <param name="body">Kappale</param>
        public void RemoveBody( IPhysicsBody body )
        {
            if ( !( body is PhysicsBody ) )
                throw new ArgumentException( "Physics object has unrecognizable body type." );

            bodies.Remove( (PhysicsBody)body );
        }

        public void AddJoint( IAxleJoint joint )
        {
            throw new NotImplementedException( "Joints are not implemented in SimplePhysics yet." );
        }

        public void RemoveJoint( IAxleJoint joint )
        {
        }

        /// <summary>
        /// Tyhjentää kaiken.
        /// </summary>
        public void Clear()
        {
            contacts.Clear();
            bodies.Clear();
        }

        /// <summary>
        /// Päivittää moottorin tilan.
        /// </summary>
        /// <param name="dt">Aika viime päivityksestä</param>
        public void Update( double dt )
        {
            const double maxdt = 0.01;

            while ( dt > maxdt )
            {
                Integrate( maxdt );
                dt -= maxdt;
            }

            Cleanup();
            Integrate( dt );
        }

        private void Cleanup()
        {
            for ( int i = bodies.Count - 1; i >= 0; i-- )
            {
                if ( bodies[i].IsDestroyed )
                    bodies.RemoveAt( i );
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

            for ( int i = 0; i < bodies.Count; i++ )
            {
                PhysicsBody body = (PhysicsBody)bodies[i];
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

        private bool CanCollide( PhysicsBody obj1, PhysicsBody obj2 )
        {
            if ( obj1.IgnoresCollisionResponse || obj2.IgnoresCollisionResponse )
                return false;

            if ( obj1.CollisionIgnorer != null )
            {
                if ( !obj1.CollisionIgnorer.CanCollide( obj1, obj2, obj2.CollisionIgnorer ) )
                    return false;

                if ( !obj1.CollisionIgnorer.BothNeeded )
                    return true;
            }

            if ( obj2.CollisionIgnorer != null )
            {
                if ( !obj2.CollisionIgnorer.CanCollide( obj2, obj1, obj1.CollisionIgnorer ) )
                    return false;
            }

            return true;
        }

        private void SolveCollisions( double dt )
        {
            for ( int i = 0; i < bodies.Count; i++ )
            {
                var iBody = (PhysicsBody)( bodies[i] );
                var iStatic = double.IsInfinity( iBody.Mass );

                for ( int j = 0; j < i; j++ )
                {
                    var jBody = (PhysicsBody)( bodies[j] );
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
                        double rest = bodies[i].Restitution * bodies[j].Restitution;

                        if ( iStatic || jStatic )
                        {
                            int index = iStatic ? j : i;
                            n = ( intsect.Position - bodies[index].Position ).Normalize();
                            double v_n = bodies[index].Velocity.ScalarProjection( n );

                            if ( v_n > 0 )
                            {
                                // Approaching the wall
                                if ( CanCollide( bodies[i], bodies[j] ) )
                                {
                                    // Collision response
                                    Vector p = n.LeftNormal;
                                    double v_p = bodies[index].Velocity.ScalarProjection( p );
                                    bodies[index].Velocity = rest * ( v_p * p - v_n * n );
                                }
                            }
                        }
                        else
                        {
                            // TODO: Restitution
                            Vector p_i = bodies[i].Mass * bodies[i].Velocity;
                            Vector p_j = bodies[j].Mass * bodies[j].Velocity;
                            Vector p_sum = p_i + p_j;
                            Vector p_diff = p_j - p_i;
                            n = ( bodies[j].Position - bodies[i].Position ).Normalize();

                            Vector v_j = ( p_sum - p_diff ) / 2;
                            Vector v_i = p_sum - v_j;

                            if ( CanCollide( bodies[i], bodies[j] ) )
                            {
                                // Collision response
                                bodies[i].Velocity = v_i;
                                bodies[j].Velocity = v_j;
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
