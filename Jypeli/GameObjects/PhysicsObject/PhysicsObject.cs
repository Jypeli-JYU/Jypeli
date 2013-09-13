using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli.Physics;

namespace Jypeli
{
    [Save]
    public partial class PhysicsObject : GameObject, IPhysicsObjectInternal
    {
        [Save]
        private int _ignoreGroup = 0;

        [Save]
        private double _storedMomentOfInertia = 1;

        /// <summary>
        /// Olio, jolla voi välttää oliota osumasta tiettyihin muihin olioihin.
        /// </summary>
        public virtual Ignorer CollisionIgnorer { get; set; }

        /// <summary>
        /// Törmäysryhmä.
        /// Oliot jotka ovat samassa törmäysryhmässä menevät toistensa läpi.
        /// Jos ryhmä on nolla tai negatiivinen, sillä ei ole vaikutusta.
        /// </summary>
        public virtual int CollisionIgnoreGroup
        {
            get { return _ignoreGroup; }
            set
            {
                _ignoreGroup = value;
                
                if ( _ignoreGroup == 0 )
                {
                    this.CollisionIgnorer = null;
                }
                else
                {
                    var ignorer = new GroupIgnorer();
                    ignorer.Groups.Add( value );
                    this.CollisionIgnorer = ignorer;
                }
            }
        }

        public double Mass
        {
            get { return Body.Mass; }
            set
            {
                Body.Mass = value;
                _storedMomentOfInertia = Body.MomentOfInertia;
            }
        }

        public double MomentOfInertia
        {
            get { return Body.MomentOfInertia; }
            set
            {
                Body.MomentOfInertia = value;
                _storedMomentOfInertia = Body.MomentOfInertia;
            }
        }

        /// <summary>
        /// Jos <c>false</c>, olio ei voi pyöriä.
        /// </summary>
        public bool CanRotate
        {
            get { return !double.IsPositiveInfinity( MomentOfInertia ); }
            set
            {
                if ( !value )
                {
                    _storedMomentOfInertia = Body.MomentOfInertia;
                    Body.MomentOfInertia = double.PositiveInfinity;
                }
                else
                {
                    Body.MomentOfInertia = _storedMomentOfInertia;
                }
            }
        }

        public double Restitution
        {
            get { return Body.Restitution; }
            set { Body.Restitution = value; }
        }

        public double StaticFriction
        {
            get
            {
                return Body.StaticFriction;
            }
            set
            {
                Body.StaticFriction = value;
            }
        }

        public double KineticFriction
        {
            get
            {
                return Body.KineticFriction;
            }
            set
            {
                Body.KineticFriction = value;
            }
        }

        public Vector Velocity
        {
            get { return Body.Velocity; }
            set { Body.Velocity = value; }
        }

        public Vector Acceleration
        {
            get { return Body.Acceleration; }
            set { Body.Acceleration = value; }
        }

        public double AngularVelocity
        {
            get { return Body.AngularVelocity; }
            set { Body.AngularVelocity = value; }
        }

        public double AngularAcceleration
        {
            get { return Body.AngularAcceleration; }
            set { Body.AngularAcceleration = value; }
        }

        public double LinearDamping
        {
            get { return Body.LinearDamping; }
            set { Body.LinearDamping = value; }
        }

        public double AngularDamping
        {
            get { return Body.AngularDamping; }
            set { Body.AngularDamping = value; }
        }
        
        public bool IgnoresCollisionResponse
        {
            get { return Body.IgnoresCollisionResponse; }
            set { Body.IgnoresCollisionResponse = value; }
        }

        public bool IgnoresGravity
        {
            get { return Body.IgnoresGravity; }
            set { Body.IgnoresGravity = value; }
        }

        public bool IgnoresPhysicsLogics
        {
            get { return Body.IgnoresPhysicsLogics; }
            set { Body.IgnoresPhysicsLogics = value; }
        }

        [Save]
        public bool IgnoresExplosions { get; set; }

        private BoundingRectangle _bRect = new BoundingRectangle();

        public BoundingRectangle BoundingRectangle
        {
            get
            {
                _bRect.Position = this.Position;
                _bRect.Size = this.Size;
                return _bRect;
            }
        }

        public event CollisionHandler<IPhysicsObject, IPhysicsObject> Collided;

        /// <summary>
        /// Fysiikkamoottori kutsuu kun törmäys tapahtuu
        /// </summary>
        /// <param name="otherObject"></param>
        public void OnCollided( IPhysicsBody thisBody, IPhysicsBody otherBody )
        {
            var thisObject = thisBody.Owner;
            var otherObject = otherBody.Owner;

            if ( thisObject != this || otherObject == null || this.IsDestroyed || otherObject.IsDestroyed == null ) return;

            if ( Collided != null )
            {
                //if ( otherObject.ParentStructure != null ) Collided( this, otherObject.ParentStructure );
                Collided( this, otherObject );
            }
            
            Brain.OnCollision( otherObject );
        }

        public PhysicsObject( double width, double height, Shape shape )
            : base( width, height, shape )
        {
            Body = Game.Instance.PhysicsClient.CreateBody( this, width, height, shape );
            Body.Collided += this.OnCollided;
        }

        public PhysicsObject( double width, double height )
            : this( width, height, Shape.Rectangle )
        {
        }

        public static PhysicsObject CreateStaticObject( double width, double height, Shape shape )
        {
            var obj = new PhysicsObject( width, height, shape );
            obj.MakeStatic();
            return obj;
        }

        public static PhysicsObject CreateStaticObject( double width, double height )
        {
            var obj = new PhysicsObject( width, height );
            obj.MakeStatic();
            return obj;
        }

        public void MakeStatic()
        {
            Body.MakeStatic();
        }

        public bool IsDestroying
        {
            get { throw new NotImplementedException(); }
        }

        public bool IgnoresCollisionWith( PhysicsObject target )
        {
            if ( this.IgnoresCollisionResponse || target.IgnoresCollisionResponse )
                return true;
            if ( this.CollisionIgnorer == null || target.CollisionIgnorer == null )
                return false;

            return !this.CollisionIgnorer.CanCollide( this.Body, target.Body, target.CollisionIgnorer );
        }

        public event Action Destroying;
    }
}
