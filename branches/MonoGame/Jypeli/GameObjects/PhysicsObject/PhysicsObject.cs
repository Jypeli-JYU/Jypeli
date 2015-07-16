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

        /// <summary>
        /// Olion massa (paino).
        /// Mitä enemmän massaa, sitä enemmän voimaa tarvitaan saamaan olio liikkeelle / pysähtymään.
        /// </summary>
        /// <value>The mass.</value>
        public double Mass
        {
            get { return Body.Mass; }
            set
            {
                Body.Mass = value;
                _storedMomentOfInertia = Body.MomentOfInertia;
            }
        }

        /// <summary>
        /// Olion hitausmomentti eli massa/paino kääntyessä.
        /// Mitä suurempi, sitä hitaampi olio on kääntymään / sitä enemmän vääntöä tarvitaan.
        /// Äärettömällä hitausmomentilla olio ei käänny lainkaan (paitsi suoraan kulmaa muuttamalla).
        /// </summary>
        /// <value>The moment of inertia.</value>
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

        /// <summary>
        /// Kimmoisuuskerroin (0 = ei kimmoisa, 1 = täysin kimmoisa, yli 1 = saa energiaa tyhjästä)
        /// </summary>
        /// <value>The restitution.</value>
        public double Restitution
        {
            get { return Body.Restitution; }
            set { Body.Restitution = value; }
        }

        /// <summary>
        /// Lepokitka (hidastaa liikkeelle lähtiessä).
        /// Ks. <see cref="KineticFriction"/> (liikekitka)
        /// </summary>
        /// <value>The static friction.</value>
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

        /// <summary>
        /// Liikekitka (hidastaa kun olio on jo liikkeessä).
        /// Ks. <see cref="StaticFriction"/> (lepokitka)
        /// </summary>
        /// <value>The kinetic friction.</value>
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

        /// <summary>
        /// Nopeus.
        /// </summary>
        /// <value>The velocity.</value>
        public Vector Velocity
        {
            get { return Body.Velocity; }
            set { Body.Velocity = value; }
        }

        /// <summary>
        /// Kiihtyvyys.
        /// </summary>
        /// <value>The acceleration.</value>
        public Vector Acceleration
        {
            get { return Body.Acceleration; }
            set { Body.Acceleration = value; }
        }

        /// <summary>
        /// Kulmanopeus.
        /// </summary>
        /// <value>The angular velocity.</value>
        public double AngularVelocity
        {
            get { return Body.AngularVelocity; }
            set { Body.AngularVelocity = value; }
        }

        /// <summary>
        /// Kulmakiihtyvyys.
        /// </summary>
        /// <value>The angular acceleration.</value>
        public double AngularAcceleration
        {
            get { return Body.AngularAcceleration; }
            set { Body.AngularAcceleration = value; }
        }

        /// <summary>
        /// Nopeuskerroin.
        /// Pienempi arvo kuin 1 (esim. 0.998) toimii kuten kitka / ilmanvastus.
        /// </summary>
        /// <value>The linear damping.</value>
        public double LinearDamping
        {
            get { return Body.LinearDamping; }
            set { Body.LinearDamping = value; }
        }

        /// <summary>
        /// Kulmanopeuskerroin.
        /// Pienempi arvo kuin 1 (esim. 0.998) toimii kuten kitka / ilmanvastus.
        /// </summary>
        /// <value>The angular damping.</value>
        public double AngularDamping
        {
            get { return Body.AngularDamping; }
            set { Body.AngularDamping = value; }
        }

        /// <summary>
        /// Jättääkö törmäykset huomiotta.
        /// </summary>
        /// <value><c>true</c> if ignores collision response; otherwise, <c>false</c>.</value>
        public bool IgnoresCollisionResponse
        {
            get { return Body.IgnoresCollisionResponse; }
            set { Body.IgnoresCollisionResponse = value; }
        }

        /// <summary>
        /// Jättääkö painovoiman huomiotta.
        /// </summary>
        /// <value><c>true</c> if ignores gravity; otherwise, <c>false</c>.</value>
        public bool IgnoresGravity
        {
            get { return Body.IgnoresGravity; }
            set { Body.IgnoresGravity = value; }
        }

        /// <summary>
        /// Jättääkö fysiikkakentät (esim. painovoiman) huomiotta.
        /// </summary>
        /// <value><c>true</c> if ignores physics logics; otherwise, <c>false</c>.</value>
        public bool IgnoresPhysicsLogics
        {
            get { return Body.IgnoresPhysicsLogics; }
            set { Body.IgnoresPhysicsLogics = value; }
        }

        /// <summary>
        /// Rakenneolio, johon tämä olio kuuluu.
        /// </summary>
        public PhysicsStructure ParentStructure { get; internal set; }

        [Save]
        public bool IgnoresExplosions { get; set; }

        private BoundingRectangle _bRect = new BoundingRectangle();

        /// <summary>
        /// Olion sisältävä laatikko törmäyskäsittelyä varten.
        /// </summary>
        /// <value>The bounding rectangle.</value>
        public BoundingRectangle BoundingRectangle
        {
            get
            {
                _bRect.Position = this.Position;
                _bRect.Size = this.Size;
                return _bRect;
            }
        }

        /// <summary>
        /// Tapahtuu, kun törmätään toiseen fysiikkaolioon.
        /// </summary>
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
                if ( otherObject.ParentStructure != null ) Collided( this, otherObject.ParentStructure );
                Collided( this, otherObject );
            }
            
            Brain.OnCollision( otherObject );
        }

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="shape">Muoto (esim. Shape.Circle).</param>
        public PhysicsObject( double width, double height, Shape shape )
            : base( width, height, shape )
        {
            Initialize( width, height, shape );
        }

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public PhysicsObject( double width, double height )
            : this( width, height, Shape.Rectangle )
        {
        }

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön.
        /// </summary>
        /// <param name="animation">Animaatio tai kuva.</param>
		public PhysicsObject(Animation animation)
			: base( animation )
		{
            Initialize( animation.Width, animation.Height, Shape.Rectangle );
		}

        private void Initialize( double width, double height, Shape shape )
		{
            Body = PhysicsGameBase.Instance.Engine.CreateBody( this, width, height, shape );
			Body.Collided += this.OnCollided;
		}

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön ja tekee siitä staattisen (liikkumattoman).
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus</param> 
        /// <param name="shape">Muoto (esim. Shape.Circle).</param>
        public static PhysicsObject CreateStaticObject( double width, double height, Shape shape )
        {
            var obj = new PhysicsObject( width, height, shape );
            obj.MakeStatic();
            return obj;
        }

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön ja tekee siitä staattisen (liikkumattoman).
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus</param> 
        public static PhysicsObject CreateStaticObject( double width, double height )
        {
            var obj = new PhysicsObject( width, height );
            obj.MakeStatic();
            return obj;
        }

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön.
        /// </summary>
        /// <param name="animation">Animaatio tai kuva.</param>
        public static PhysicsObject CreateStaticObject( Animation animation )
        {
            var obj = new PhysicsObject( animation );
            obj.MakeStatic();
            return obj;
        }

        /// <summary>
        /// Tekee oliosta staattisen eli liikkumattoman.
        /// </summary>
        public void MakeStatic()
        {
            Body.MakeStatic();
        }

        /// <summary>
        /// Onko olio tuhoutumassa.
        /// </summary>
        /// <value><c>true</c> if this instance is destroying; otherwise, <c>false</c>.</value>
        public bool IsDestroying
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Tarkistaa, jätetäänkö törmäämättä toiseen olioon.
        /// </summary>
        /// <returns><c>true</c>, jos ei törmätä, <c>false</c> jos törmätään.</returns>
        /// <param name="target">Olio johon törmäystä tutkitaan.</param>
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
        /// Tapahtuu kun olio on tuhoutumassa.
        /// </summary>
        public event Action Destroying;
    }
}
