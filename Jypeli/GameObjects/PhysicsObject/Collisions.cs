using Jypeli.Physics;
using System;

namespace Jypeli
{
    public partial class PhysicsObject
    {
        private Ignorer _collisionIgnorer = null;

        /// <inheritdoc/>
        public Func<IPhysicsObject, IPhysicsObject, bool> CollisionIgnoreFunc { get; set; }

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

                if ( groupIgnorer == null )
                    groupIgnorer = new JypeliGroupIgnorer();
                groupIgnorer.LegacyGroup = value;
                CollisionIgnorer = groupIgnorer;

            }
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
        /// Tapahtuu, kun törmätään toiseen fysiikkaolioon.
        /// </summary>
        public event CollisionHandler<IPhysicsObject, IPhysicsObject> Collided;

        /// <summary>
        /// Fysiikkamoottori kutsuu kun törmäys tapahtuu
        /// </summary>
        /// <param name="thisBody"></param>
        /// <param name="otherBody"></param>
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
        /// Tarkistaa, jätetäänkö törmäämättä toiseen olioon.
        /// Ts. tarkistaa, onko joko tällä oliolla tai toisella oliolla esim.
        /// IgnoresCollisionResponse tai keskenään sama CollisionIgnoreGroup.
        /// </summary>
        /// <returns><c>true</c>, jos ei törmätä, <c>false</c> jos törmätään.</returns>
        /// <param name="target">Olio johon törmäystä tutkitaan.</param>
        public bool IgnoresCollisionWith( PhysicsObject target )
        {
            if (this == target)
                return true;
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

            if ( groupIgnorer == null )
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
        /// Tekee oliosta läpimentävän alhaalta ylöspäin (tasohyppelytaso).
        /// Huom. toimii yhdessä CollisionIgnoreGroupien kanssa ainoastaan jos käytössä on Farseer fysiikkamoottori!
        /// </summary>
        public void MakeOneWay()
        {
            if (Game.Instance.FarseerGame)
                Body.MakeOneWay();
            else
                CollisionIgnorer = new OneWayPlatformIgnorer(Height);
        }

        /// <summary>
        /// Tekee oliosta läpimentävän annettuun suuntaan.
        /// Tämä toimii vain Farseer-moottorin kanssa!
        /// </summary>
        /// <param name="dir">Suunta johon päästään läpi</param>
        public void MakeOneWay(Vector dir)
        {
            if (Game.Instance.FarseerGame)
                Body.MakeOneWay(dir);
            else
                throw new NotImplementedException("Tämä on käytössä vain Farseer-moottorin kanssa.");
        }
    }
}
