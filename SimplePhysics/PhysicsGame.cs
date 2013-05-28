using System;
using System.Collections.Generic;
using System.Text;
using Jypeli;

namespace Jypeli
{
    public partial class PhysicsGame : Game
    {
        List<PhysicsObject> physObjects = new List<PhysicsObject>();
        Dictionary<PhysicsObject, List<PhysicsObject>> contacts = new Dictionary<PhysicsObject, List<PhysicsObject>>();

        public PhysicsGame()
            : base()
        {
        }

        public override void Add( IGameObject o, int layer )
        {
            if ( o is PhysicsObject )
            {
                physObjects.Add( (PhysicsObject)o );
            }

            base.Add( o, layer );
        }

        protected override void Update( Time time )
        {
            const double maxdt = 0.01;
            double dt = time.SinceLastUpdate.TotalSeconds;

            while ( dt > maxdt )
            {
                Integrate( maxdt );
                dt -= maxdt;
            }
            
            Integrate( dt );

            base.Update( time );
        }

        private void Integrate( double dt )
        {
            if ( dt <= 0 ) return;
                        
            for ( int i = 0; i < physObjects.Count; i++ )
            {
                physObjects[i].Velocity += physObjects[i].Acceleration * dt;
                physObjects[i].Position += physObjects[i].Velocity * dt;
            }

            SolveCollisions( dt );
        }

        private bool HasContact( PhysicsObject obj1, PhysicsObject obj2 )
        {
            if ( contacts.ContainsKey( obj1 ) && contacts[obj1].Contains( obj2 ) )
                return true;

            if ( contacts.ContainsKey( obj2 ) && contacts[obj2].Contains( obj1 ) )
                return true;

            return false;
        }

        private void AddContact( PhysicsObject obj1, PhysicsObject obj2 )
        {
            if ( contacts.ContainsKey( obj1 ) )
            {
                contacts[obj1].Add( obj2 );
                return;
            }

            if ( contacts.ContainsKey( obj2 ) )
            {
                contacts[obj2].Add( obj1 );
                return;
            }

            var newList = new List<PhysicsObject>();
            newList.Add( obj2 );
            contacts.Add( obj1, newList );
        }

        private void RemoveContact( PhysicsObject obj1, PhysicsObject obj2 )
        {
            if ( contacts.ContainsKey( obj1 ) && contacts[obj1].Contains( obj2 ) )
                contacts[obj1].Remove( obj2 );

            if ( contacts.ContainsKey( obj2 ) && contacts[obj2].Contains( obj1 ) )
                contacts[obj2].Remove( obj1 );
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
                bool iStatic = double.IsInfinity( physObjects[i].Mass );

                for ( int j = 0; j < i; j++ )
                {
                    bool jStatic = double.IsInfinity( physObjects[j].Mass );

                    if ( iStatic && jStatic )
                        continue;

                    if ( IsOverlapping( physObjects[i], physObjects[j] ) )
                    {
                        if ( HasContact( physObjects[i], physObjects[j] ) )
                        {
                            // Already responded to this collision
                            continue;
                        }

                        // Collision (perfectly elastic)
                        BoundingRectangle intsect = BoundingRectangle.GetIntersection( physObjects[i].BoundingRectangle, physObjects[j].BoundingRectangle );
                        double rest = physObjects[i].Restitution * physObjects[j].Restitution;

                        if ( iStatic || jStatic )
                        {
                            int index = iStatic ? j : i;
                            Vector n = ( intsect.Position - physObjects[index].Position ).Normalize();
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
                        physObjects[i].OnCollided( physObjects[j] );
                        physObjects[j].OnCollided( physObjects[i] );
                        AddContact( physObjects[i], physObjects[j] );
                    }
                    else
                    {
                        // No contact between these objects
                        RemoveContact( physObjects[i], physObjects[j] );
                    }
                }
            }
        }
    }
}
