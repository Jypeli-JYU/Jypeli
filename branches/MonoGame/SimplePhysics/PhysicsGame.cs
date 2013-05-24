using System;
using System.Collections.Generic;
using System.Text;
using Jypeli;

namespace Jypeli
{
    public class PhysicsGame : Game
    {
        List<PhysicsObject> physObjects = new List<PhysicsObject>();

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

                    if ( BoundingRectangle.Intersects( physObjects[i].BoundingRectangle, physObjects[j].BoundingRectangle ) )
                    {
                        // Collision (perfectly elastic)
                        BoundingRectangle intsect = BoundingRectangle.GetIntersection( physObjects[i].BoundingRectangle, physObjects[j].BoundingRectangle );

                        if ( iStatic || jStatic )
                        {
                            int index = iStatic ? j : i;

                            // ds is the distance travelled during dt
                            Vector ds = ( physObjects[index].Acceleration * dt + physObjects[index].Velocity ) * dt;
                            Vector n = ( intsect.Position - physObjects[index].Position ).Normalize();
                            Vector p = n.RightNormal;
                                                        
                            // n = normal, p = parallel
                            double pos_n = physObjects[index].Position.ScalarProjection( n );
                            double pos_p = physObjects[index].Position.ScalarProjection( p );
                            double contact_n = intsect.Position.ScalarProjection( n );
                            double contact_p = intsect.Position.ScalarProjection( p );
                            double ds_n = ds.ScalarProjection( n );
                            double ds_p = ds.ScalarProjection( p );

                            // ds2 is the part after the collision
                            double ds2_n = contact_n - pos_n;
                            double ds1_n = ds_n - ds2_n;
                            physObjects[index].Position += ds_p * p + ( ds1_n - ds2_n ) * n;

                            double v_n = physObjects[index].Velocity.ScalarProjection( n );
                            double v_p = physObjects[index].Velocity.ScalarProjection( p );
                            physObjects[index].Velocity = v_p * p - v_n * n;
                        }
                        else
                        {
                            Vector p_i = physObjects[i].Mass * physObjects[i].Velocity;
                            Vector p_j = physObjects[j].Mass * physObjects[j].Velocity;
                            Vector p_sum = p_i + p_j;
                            Vector p_diff = p_j - p_i;

                            Vector v_j = ( p_sum - p_diff ) / 2;
                            Vector v_i = p_sum - v_j;

                            physObjects[i].Velocity = v_i;
                            physObjects[j].Velocity = v_j;
                        }
                    }
                }
            }
        }
    }
}
