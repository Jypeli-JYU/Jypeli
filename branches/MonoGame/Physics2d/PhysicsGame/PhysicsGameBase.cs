using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Physics2DDotNet;
using Physics2DDotNet.Joints;

namespace Jypeli
{
    /// <summary>
    /// Kantaluokka fysiikkapeleille.
    /// </summary>
    public abstract partial class PhysicsGameBase : Game
    {
        protected struct CollisionRecord
        {
            public IPhysicsObject obj;
            public IPhysicsObject target;
            public object targetTag;
            public Delegate handler;

            public CollisionRecord( IPhysicsObject obj, IPhysicsObject target, object targetTag, Delegate handler )
            {
                this.obj = obj;
                this.target = target;
                this.targetTag = targetTag;
                this.handler = handler;
            }
        }

        protected PhysicsEngine phsEngine;
        private SynchronousList<Joint> Joints = new SynchronousList<Joint>();
        protected Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>> collisionHandlers =
            new Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>>();
        protected Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>> protectedCollisionHandlers =
            new Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>>();

        /// <summary>
        /// Onko fysiikan laskenta käytössä vai ei.
        /// </summary>
        public bool PhysicsEnabled { get; set; }

        /// <summary>
        /// Alustaa uuden fysiikkapelin.
        /// </summary>
        public PhysicsGameBase()
            : base()
        {
            PhysicsEnabled = true;
            phsEngine = new PhysicsEngine();

            Joints.ItemAdded += OnJointAdded;
            Joints.ItemRemoved += OnJointRemoved;
        }

        void OnJointAdded( Joint j )
        {
            j.Lifetime.IsExpired = false;
            phsEngine.AddJoint( j );
        }

        void OnJointRemoved( Joint j )
        {
            j.Lifetime.IsExpired = true;
        }

        protected override void OnObjectAdded( IGameObject obj )
        {
            if ( obj is PhysicsObject )
            {
                AddToEngine( (PhysicsObject)obj );
            }

            base.OnObjectAdded( obj );
        }

        protected override void OnObjectRemoved( IGameObject obj )
        {
            if ( obj is PhysicsObject )
            {
                RemoveFromEngine( (PhysicsObject)obj );
            }

            base.OnObjectRemoved( obj );
        }

        /// <summary>
        /// Pysäyttää kaiken liikkeen.
        /// </summary>
        public void StopAll()
        {
            foreach ( var layer in Layers )
            {
                foreach ( var obj in layer.Objects )
                {
                    if ( obj is PhysicsObject )
                        ( (PhysicsObject)obj ).Stop();
                }
            }
        }

        /// <summary>
        /// Nollaa kaiken (kontrollit, näyttöobjektit, ajastimet ja fysiikkamoottorin).
        /// </summary>
        public override void ClearAll()
        {
            ClearPhysics();
            base.ClearAll();
        }

        /// <summary>
        /// Nollaa fysiikkamoottorin.
        /// </summary>
        private void ClearPhysics()
        {
            phsEngine.Clear();
        }

        private Body GetBody( PhysicsObject obj )
        {
            return ( (PhysicsBody)obj.Body ).Body;
        }

        private void AddToEngine( PhysicsObject po )
        {
            var b = GetBody( po );
            if ( b.Engine != null )
                return;

            b.Lifetime.IsExpired = false;
            phsEngine.AddBody( b );
        }

        private void RemoveFromEngine( PhysicsObject po )
        {
            var b = GetBody( po );
            if ( b.Engine == null )
                return;

            b.Lifetime.IsExpired = true;
        }

#if joints
        /// <summary>
        /// Lisää liitoksen peliin.
        /// </summary>
        public void Add( Physics2DDotNet.Joints.Joint j )
        {
            Joints.Add( j );
        }

        /// <summary>
        /// Poistaa liitoksen pelistä.
        /// </summary>
        /// <param name="j"></param>
        internal void Remove( Physics2DDotNet.Joints.Joint j )
        {
            Joints.Remove( j );
        }

        /// <summary>
        /// Poistaa liitoksen pelistä.
        /// </summary>
        /// <param name="j"></param>
        internal void Remove( AxleJoint j )
        {
            Joints.Remove( j.innerJoint );
        }

        /// <summary>
        /// Lisää liitoksen peliin.
        /// </summary>
        public void Add( AxleJoint j )
        {
            bool obj1ok = j.Object1.IsAddedToGame;
            bool obj2ok = j.Object2 == null || j.Object2.IsAddedToGame;

            if ( obj1ok && obj2ok )
            {
                Add( j.innerJoint );
            }
            else
            {
                if ( !obj1ok ) j.Object1.AddedToGame += j.DelayedAddJoint;
                if ( !obj2ok ) j.Object2.AddedToGame += j.DelayedAddJoint;
            }
        }
#endif

        /// <summary>
        /// Ajetaan kun pelin tilannetta päivitetään. Päivittämisen voi toteuttaa perityssä luokassa
        /// toteuttamalla tämän metodin. Perityn luokan metodissa tulee kutsua kantaluokan metodia.
        /// </summary>
        /// <param name="time"></param>
        protected override void Update( Time time )
        {
            double dt = time.SinceLastUpdate.TotalSeconds;

            if ( PhysicsEnabled )
            {
                phsEngine.Update( dt );
            }

            base.Update( time );

            // Updating joints must be after base.Update so that the bodies 
            // are added to the engine before the joints
            Joints.Update( time );
        }

        #region Collision handlers

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun olio <code>obj</code> törmää johonkin toiseen olioon.
        /// </summary>
        /// <typeparam name="T">Kohdeolion tyyppi.</typeparam>
        /// <param name="obj">Törmäävä olio</param>
        /// <param name="handler">Törmäyksen käsittelevä aliohjelma.</param>
        public void AddCollisionHandler<O, T>( O obj, CollisionHandler<O, T> handler )
            where O : IPhysicsObject
            where T : IPhysicsObject
        {
            if ( obj == null ) throw new NullReferenceException( "Colliding object must not be null" );
            if ( handler == null ) throw new NullReferenceException( "Handler must not be null" );

            CollisionHandler<IPhysicsObject, IPhysicsObject> targetHandler =
                delegate( IPhysicsObject collider, IPhysicsObject collidee )
                {
                    if ( collidee is T )
                        handler( (O)collider, (T)collidee );
                };

            obj.Collided += targetHandler;
            collisionHandlers.Add( new CollisionRecord( obj, null, null, handler ), targetHandler );
        }

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun olio <code>obj</code> törmää johonkin toiseen olioon.
        /// Jypelin sisäiseen käyttöön!
        /// </summary>
        /// <typeparam name="T">Kohdeolion tyyppi.</typeparam>
        /// <param name="obj">Törmäävä olio</param>
        /// <param name="handler">Törmäyksen käsittelevä aliohjelma.</param>
        internal void AddProtectedCollisionHandler<O, T>( O obj, CollisionHandler<O, T> handler )
            where O : IPhysicsObject
            where T : IPhysicsObject
        {
            if ( obj == null ) throw new NullReferenceException( "Colliding object must not be null" );
            if ( handler == null ) throw new NullReferenceException( "Handler must not be null" );

            CollisionHandler<IPhysicsObject, IPhysicsObject> targetHandler =
                delegate( IPhysicsObject collider, IPhysicsObject collidee )
                {
                    if ( collidee is T )
                        handler( (O)collider, (T)collidee );
                };

            obj.Collided += targetHandler;
            protectedCollisionHandlers.Add( new CollisionRecord( obj, null, null, handler ), targetHandler );
        }

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun yleinen fysiikkaolio <code>obj</code>
        /// törmää johonkin toiseen yleiseen fysiikkaolioon.
        /// </summary>
        /// <param name="obj">Törmäävä olio</param>
        /// <param name="handler">Törmäyksen käsittelevä aliohjelma.</param>
        public void AddCollisionHandler( IPhysicsObject obj, CollisionHandler<IPhysicsObject, IPhysicsObject> handler )
        {
            AddCollisionHandler<IPhysicsObject, IPhysicsObject>( obj, handler );
        }

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun fysiikkaolio <code>obj</code> törmää johonkin toiseen fysiikkaolioon.
        /// </summary>
        /// <param name="obj">Törmäävä olio</param>
        /// <param name="handler">Törmäyksen käsittelevä aliohjelma.</param>
        public void AddCollisionHandler( PhysicsObject obj, CollisionHandler<PhysicsObject, PhysicsObject> handler )
        {
            AddCollisionHandler<PhysicsObject, PhysicsObject>( obj, handler );
        }

#if structs
        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun fysiikkaolio <code>obj</code> törmää johonkin fysiikkarakenteeseen.
        /// </summary>
        /// <param name="obj">Törmäävä olio</param>
        /// <param name="handler">Törmäyksen käsittelevä aliohjelma.</param>
        public void AddCollisionHandler( PhysicsObject obj, CollisionHandler<PhysicsObject, PhysicsStructure> handler )
        {
            AddCollisionHandler<PhysicsObject, PhysicsStructure>( obj, handler );
        }

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun fysiikkarakenne <code>o</code> törmää johonkin fysiikkaolioon.
        /// </summary>
        /// <param name="obj">Törmäävä fysiikkarakenne</param>
        /// <param name="handler">Törmäyksen käsittelevä aliohjelma.</param>
        public void AddCollisionHandler( PhysicsStructure obj, CollisionHandler<PhysicsStructure, PhysicsObject> handler )
        {
            AddCollisionHandler<PhysicsStructure, PhysicsObject>( obj, handler );
        }

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun fysiikkarakenne <code>o</code> törmää toiseen fysiikkarakenteeseen.
        /// </summary>
        /// <param name="obj">Törmäävä fysiikkarakenne</param>
        /// <param name="handler">Törmäyksen käsittelevä aliohjelma.</param>
        public void AddCollisionHandler( PhysicsStructure obj, CollisionHandler<PhysicsStructure, PhysicsStructure> handler )
        {
            AddCollisionHandler<PhysicsStructure, PhysicsStructure>( obj, handler );
        }
#endif

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun
        /// olio <code>obj</code> törmää tiettyyn toiseen olioon <code>target</code>.
        /// </summary>
        /// <param name="obj">Törmäävä olio.</param>
        /// <param name="target">Olio johon törmätään.</param>
        /// <param name="handler">Metodi, joka käsittelee törmäyksen (ei parametreja).</param>
        public void AddCollisionHandler<O, T>( O obj, T target, CollisionHandler<PhysicsObject, T> handler )
            where O : IPhysicsObject
            where T : IPhysicsObject
        {
            if ( obj == null ) throw new NullReferenceException( "Colliding object must not be null" );
            if ( target == null ) throw new NullReferenceException( "Collision target must not be null" );
            if ( handler == null ) throw new NullReferenceException( "Handler must not be null" );

            CollisionHandler<IPhysicsObject, IPhysicsObject> targetHandler =
                delegate( IPhysicsObject collider, IPhysicsObject collidee )
                {
                    if ( object.ReferenceEquals( collidee, target ) )
                        handler( (PhysicsObject)collider, (T)collidee );
                };

            obj.Collided += targetHandler;
            collisionHandlers.Add( new CollisionRecord( obj, target, null, handler ), targetHandler );
        }

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun 
        /// olio <code>obj</code> törmää toiseen olioon, jolla on tietty tagi <code>tag</code>.
        /// </summary>
        /// <param name="obj">Törmäävä olio.</param>
        /// <param name="tag">Törmättävän olion tagi.</param>
        /// <param name="handler">Metodi, joka käsittelee törmäyksen (ei parametreja).</param>
        public void AddCollisionHandler<O, T>( O obj, object tag, CollisionHandler<O, T> handler )
            where O : IPhysicsObject
            where T : IPhysicsObject
        {
            if ( obj == null ) throw new NullReferenceException( "Colliding object must not be null" );
            if ( tag == null ) throw new NullReferenceException( "Tag must not be null" );
            if ( handler == null ) throw new NullReferenceException( "Handler must not be null" );

            CollisionHandler<IPhysicsObject, IPhysicsObject> targetHandler =
                delegate( IPhysicsObject collider, IPhysicsObject collidee )
                {
                    if ( collidee is T && StringHelpers.StringEquals( collidee.Tag, tag ) )
                        handler( (O)collider, (T)collidee );
                };

            obj.Collided += targetHandler;
            collisionHandlers.Add( new CollisionRecord( obj, null, tag, handler ), targetHandler );
        }

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun 
        /// yleinen fysiikkaolio <code>obj</code> törmää toiseen yleiseen fysiikkaolioon, jolla on tietty tagi <code>tag</code>.
        /// </summary>
        /// <param name="obj">Törmäävä olio.</param>
        /// <param name="tag">Törmättävän olion tagi.</param>
        /// <param name="handler">Metodi, joka käsittelee törmäyksen (ei parametreja).</param>
        public void AddCollisionHandler( IPhysicsObject obj, object tag, CollisionHandler<IPhysicsObject, IPhysicsObject> handler )
        {
            AddCollisionHandler<IPhysicsObject, IPhysicsObject>( obj, tag, handler );
        }

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun 
        /// fysiikkaolio <code>obj</code> törmää toiseen fysiikkaolioon, jolla on tietty tagi <code>tag</code>.
        /// </summary>
        /// <param name="obj">Törmäävä olio.</param>
        /// <param name="tag">Törmättävän olion tagi.</param>
        /// <param name="handler">Metodi, joka käsittelee törmäyksen (ei parametreja).</param>
        public void AddCollisionHandler( PhysicsObject obj, object tag, CollisionHandler<PhysicsObject, PhysicsObject> handler )
        {
            AddCollisionHandler<PhysicsObject, PhysicsObject>( obj, tag, handler );
        }
        
        /// <summary>
        /// Poistaa kaikki ehdot täyttävät törmäyksenkäsittelijät.
        /// </summary>
        /// <param name="obj">Törmäävä olio. null jos ei väliä.</param>
        /// <param name="target">Törmäyksen kohde. null jos ei väliä.</param>
        /// <param name="tag">Törmäyksen kohteen tagi. null jos ei väliä.</param>
        /// <param name="handler">Törmäyksenkäsittelijä. null jos ei väliä.</param>
        public void RemoveCollisionHandlers( PhysicsObject obj = null, PhysicsObject target = null, object tag = null, Delegate handler = null )
        {
            Predicate<CollisionRecord> pred = rec =>
                ( obj == null || object.ReferenceEquals( obj, rec.obj ) ) &&
                ( target == null || target == rec.target ) &&
                ( tag == null || StringHelpers.StringEquals( tag, rec.targetTag ) ) &&
                ( handler == null || object.ReferenceEquals( handler, rec.handler ) );

            List<CollisionRecord> remove = new List<CollisionRecord>();

            foreach (var key in collisionHandlers.Keys)
            {
                if (pred(key)) remove.Add(key);
            }

            foreach (var key in remove)
            {
                key.obj.Collided -= collisionHandlers[key];
                collisionHandlers.Remove(key);
            }
        }

        /// <summary>
        /// Poistaa kaikki ehdot täyttävät törmäyksenkäsittelijät.
        /// Jypelin sisäiseen käyttöön!
        /// </summary>
        /// <param name="obj">Törmäävä olio. null jos ei väliä.</param>
        /// <param name="target">Törmäyksen kohde. null jos ei väliä.</param>
        /// <param name="tag">Törmäyksen kohteen tagi. null jos ei väliä.</param>
        /// <param name="handler">Törmäyksenkäsittelijä. null jos ei väliä.</param>
        internal void RemoveProtectedCollisionHandlers( PhysicsObject obj = null, PhysicsObject target = null, object tag = null, Delegate handler = null )
        {
            Predicate<CollisionRecord> pred = rec =>
                ( obj == null || object.ReferenceEquals( obj, rec.obj ) ) &&
                ( target == null || target == rec.target ) &&
                ( tag == null || tag.Equals( rec.targetTag ) ) &&
                ( handler == null || object.ReferenceEquals( handler, rec.handler ) );

            List<CollisionRecord> remove = new List<CollisionRecord>();

            foreach ( var key in collisionHandlers.Keys )
            {
                if ( pred( key ) ) remove.Add( key );
            }

            foreach ( var key in remove )
            {
                key.obj.Collided -= protectedCollisionHandlers[key];
                protectedCollisionHandlers.Remove( key );
            }
        }

        #endregion
    }
}
