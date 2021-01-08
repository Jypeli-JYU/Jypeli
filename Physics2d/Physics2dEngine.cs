using System;
using AdvanceMath;
using Jypeli.Physics;
using Physics2DDotNet;
using Physics2DDotNet.PhysicsLogics;
using Physics2DDotNet.Solvers;

namespace Jypeli.Physics2d
{
    public class Physics2dEngine : IPhysicsEngine
    {
        PhysicsEngine phsEngine;
        GravityField gravityField = null;
        Vector _gravity = Vector.Zero;

        public Vector Gravity
        {
            get
            {
                return _gravity;
            }
            set
            {
                if ( gravityField != null ) gravityField.Lifetime.IsExpired = true;

                if ( value != Vector.Zero )
                {
                    gravityField = new GravityField( new Vector2D( value.X, value.Y ), new Lifespan() );
                    phsEngine.AddLogic( gravityField );
                }

                _gravity = value;
            }
        }

        public Physics2dEngine()
        {
            phsEngine = new PhysicsEngine();
            phsEngine.BroadPhase = new Physics2DDotNet.Detectors.SelectiveSweepDetector();
            //phsEngine.BroadPhase = new Physics2DDotNet.Detectors.SpatialHashDetector();

            SequentialImpulsesSolver phsSolver = new SequentialImpulsesSolver();
            phsSolver.Iterations = 12;
            phsSolver.SplitImpulse = true;
            //phsSolver.BiasFactor = 0.7;
            phsSolver.BiasFactor = 0.0;
            //phsSolver.AllowedPenetration = 0.1;
            phsSolver.AllowedPenetration = 0.01;
            phsEngine.Solver = (CollisionSolver)phsSolver;
        }

        public IPhysicsBody CreateBody( IPhysicsObject owner, double width, double height, Shape shape )
        {
            return new PhysicsBody( width, height, shape ) { Owner = owner };
        }

        public IAxleJoint CreateJoint( IPhysicsObject obj1, IPhysicsObject obj2, Vector pivot )
        {
            return new AxleJoint( obj1 as PhysicsObject, obj2 as PhysicsObject, pivot );
        }

        public IAxleJoint CreateJoint( IPhysicsObject obj1, Vector pivot )
        {
            return new AxleJoint( obj1 as PhysicsObject, pivot );
        }

        public void AddBody( IPhysicsBody body )
        {
            if ( !( body is PhysicsBody ) )
                throw new ArgumentException( "This body does not belong to this physics engine." );

            Body b = ( (PhysicsBody)body ).Body;

            if ( b.Engine != null )
                return;

            b.Lifetime.IsExpired = false;
            phsEngine.AddBody( b );
        }

        public void RemoveBody( IPhysicsBody body )
        {
            if ( !( body is PhysicsBody ) )
                throw new ArgumentException( "This body does not belong to this physics engine." );

            Body b = ( (PhysicsBody)body ).Body;

            // 24. 10. mitä varten tämä tarkistus on? - Rami
            //if ( b.Engine != null )
            //    return;

            b.Lifetime.IsExpired = true;
        }

        public void AddJoint( IAxleJoint joint )
        {
            if ( !( joint is AxleJoint ) )
                throw new ArgumentException( "Joint does not belong to this physics engine." );

            var j = ( (AxleJoint)joint ).innerJoint;
            
            j.Lifetime.IsExpired = false;
            phsEngine.AddJoint( j );
        }

        public void RemoveJoint( IAxleJoint joint )
        {
            if ( !( joint is AxleJoint ) )
                throw new ArgumentException( "Joint does not belong to this physics engine." );

            var j = ( (AxleJoint)joint ).innerJoint;

            j.Lifetime.IsExpired = true;
        }

        public void Clear()
        {
            phsEngine.Clear();
        }

        public void Update( double dt )
        {
            phsEngine.Update( dt );
        }

        /// <summary>
        /// Ei käytössä Physics2d moottorissa.
        /// </summary>
        public IAxleJoint ConnectBodies(PhysicsObject physObj1, PhysicsObject physObj2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ei käytössä Physics2d moottorissa.
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IAxleJoint CreateJoint(IPhysicsObject obj1, IPhysicsObject obj2, JointTypes type)
        {
            throw new NotImplementedException();
        }
    }
}
