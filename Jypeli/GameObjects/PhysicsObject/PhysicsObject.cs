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
        private double _storedMomentOfInertia = 1;

        private Ignorer _collisionIgnorer = null;

        /// <summary>
        /// Olio, jolla voi välttää oliota osumasta tiettyihin muihin olioihin.
        /// </summary>
        public virtual Ignorer CollisionIgnorer
        {
            get { return _collisionIgnorer; }
            set
            {
                _collisionIgnorer = value;
                Body.SetCollisionIgnorer( value );
            }
        }

        /// <summary>
        /// Törmäysryhmä.
        /// Oliot jotka ovat samassa törmäysryhmässä menevät toistensa läpi.
        /// Jos ryhmä on nolla tai negatiivinen, sillä ei ole vaikutusta.
        /// </summary>
        public virtual int CollisionIgnoreGroup
        {
            get
            {
                var groupIgnorer = CollisionIgnorer as JypeliGroupIgnorer;
                return groupIgnorer != null ? groupIgnorer.LegacyGroup : 0;
            }
            set
            {
                var groupIgnorer = CollisionIgnorer as JypeliGroupIgnorer;

                if (groupIgnorer == null)
                    CollisionIgnorer = groupIgnorer = new JypeliGroupIgnorer();

                groupIgnorer.LegacyGroup = value;
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

            if ( thisObject != this || otherObject == null || this.IsDestroyed || otherObject.IsDestroyed ) return;

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
            if ( Game.Instance.PhysicsClient == null )
                throw new InvalidOperationException( "No physics engine attached to the game. Try checking your references and that you're using the correct game class." );

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

        /// <summary>
        /// Lisää uuden törmäyksenvälttelyryhmän.
        /// </summary>
        /// <param name="group">Ryhmän numero (indeksi).</param>
        public void AddCollisionIgnoreGroup( int group )
        {
            var groupIgnorer = CollisionIgnorer as JypeliGroupIgnorer;

            if (groupIgnorer == null)
                CollisionIgnorer = groupIgnorer = new JypeliGroupIgnorer();

            groupIgnorer.AddGroup( group );
        }

        /// <summary>
        /// Poistaa annetun törmäyksenvälttelyryhmän.
        /// </summary>
        /// <param name="group">Ryhmän numero (indeksi).</param>
        public void RemoveCollisionIgnoreGroup( int group )
        {
            var groupIgnorer = CollisionIgnorer as JypeliGroupIgnorer;

            if ( groupIgnorer == null )
                return;

            groupIgnorer.RemoveGroup( group );
        }

        /// <summary>
        /// Poistaa kaikki törmäysryhmät, jolloin olio saa törmäillä vapaasti.
        /// </summary>
        public void ClearCollisionIgnoreGroups()
        {
            CollisionIgnorer = new JypeliGroupIgnorer();
        }

        /// <summary>
        /// Onko olio tuhoutumassa.
        /// </summary>
        public event Action Destroying;
    }
}
