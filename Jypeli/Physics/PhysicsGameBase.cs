using System;
using System.Collections.Generic;
using System.ComponentModel;
using Jypeli.Physics;

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

        public IPhysicsEngine Engine { get; private set; }

        private SynchronousList<IAxleJoint> Joints = new SynchronousList<IAxleJoint>();
        protected Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>> collisionHandlers =
            new Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>>();
        protected Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>> protectedCollisionHandlers =
            new Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>>();
        
        private Vector gravity = Vector.Zero;

        /// <summary>
        /// Painovoima. Voimavektori, joka vaikuttaa kaikkiin ei-staattisiin kappaleisiin.
        /// </summary>
        public Vector Gravity
        {
            get
            {
                return gravity;
            }
            set
            {
                gravity = value;
                Engine.Gravity = value;
            }
        }

        /// <summary>
        /// Käynnissä olevan fysiikkapelin pääolio.
        /// </summary>
        public static new PhysicsGameBase Instance
        {
            get
            {
                if ( Game.Instance == null ) throw new InvalidOperationException( "Game class is not initialized" );
                if ( !( Game.Instance is PhysicsGameBase ) ) throw new InvalidOperationException( "Game class is not PhysicsGame" );

                return (PhysicsGameBase)( Game.Instance );
            }
        }

        /// <summary>
        /// Onko fysiikan laskenta käytössä vai ei.
        /// </summary>
        public bool PhysicsEnabled { get; set; }

        /// <summary>
        /// Alustaa uuden fysiikkapelin.
        /// </summary>
        public PhysicsGameBase( IPhysicsEngine engine )
            : base()
        {
            this.Engine = engine;
            PhysicsEnabled = true;

            Joints.ItemAdded += OnJointAdded;
            Joints.ItemRemoved += OnJointRemoved;
        }

        void OnJointAdded( IAxleJoint j )
        {
            if ( !j.Object1.IsAddedToGame )
            {
                j.SetEngine( Engine );
                j.Object1.AddedToGame += j.AddToEngine;
            }
            else if ( j.Object2 != null && !j.Object2.IsAddedToGame )
            {
                j.SetEngine( Engine );
                j.Object2.AddedToGame += j.AddToEngine;
            }
            else
            {
                Engine.AddJoint( j );
            }
        }

        void OnJointRemoved( IAxleJoint j )
        {
            Engine.RemoveJoint( j );
        }

        protected override void OnObjectAdded( IGameObject obj )
        {
            if ( obj is PhysicsObject )
            {
                PhysicsObject po = (PhysicsObject)obj;
                Engine.AddBody( po.Body );
            }

            base.OnObjectAdded( obj );
        }

        protected override void OnObjectRemoved( IGameObject obj )
        {
            if ( obj is PhysicsObject )
            {
                PhysicsObject po = (PhysicsObject)obj;
                Engine.RemoveBody( po.Body );
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
            if(!FarseerGame)
                ClearPhysics(); // Farseerilla tämä poistaa jo kappaleet moottorilta ja ne poistettaisiin uudestaan myöhemmin Layerien tyhjennyksen yhteydessä, joka taas aiheuttaa nullpointerin.
            RemoveCollisionHandlers();
            base.ClearAll();
        }

        /// <summary>
        /// Nollaa fysiikkamoottorin.
        /// </summary>
        private void ClearPhysics()
        {
            Engine.Clear();
        }

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
                Engine.Update( dt );
            }

            base.Update( time );

            // Updating joints must be after base.Update so that the bodies 
            // are added to the engine before the joints
            Joints.Update( time );
        }

        /// <summary>
        /// Lisää liitoksen peliin.
        /// </summary>
        public void Add( IAxleJoint j )
        {
            Joints.Add( j );
        }

        /// <summary>
        /// Poistaa liitoksen pelistä.
        /// </summary>
        /// <param name="j"></param>
        internal void Remove( IAxleJoint j )
        {
            Joints.Remove( j );
        }

        #region Collision handlers

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun olio <code>obj</code> törmää johonkin toiseen olioon.
        /// </summary>
        /// <typeparam name="O">Törmäävän olion tyyppi.</typeparam>
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
        /// <typeparam name="O">Törmäävän olion tyyppi.</typeparam>
        /// <typeparam name="T">Kohdeolion tyyppi.</typeparam>
        /// <param name="obj">Törmäävä olio</param>
        /// <param name="handler">Törmäyksen käsittelevä aliohjelma.</param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public void AddProtectedCollisionHandler<O, T>( O obj, CollisionHandler<O, T> handler )
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

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun
        /// olio <code>obj</code> törmää tiettyyn toiseen olioon <code>target</code>.
        /// </summary>
        /// <param name="obj">Törmäävä olio.</param>
        /// <param name="target">Olio johon törmätään.</param>
        /// <param name="handler">Metodi, joka käsittelee törmäyksen (ei parametreja).</param>
        public void AddCollisionHandlerByRef<O, T>( O obj, T target, CollisionHandler<O, T> handler )
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
                        handler( (O)collider, (T)collidee );
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
        public void AddCollisionHandlerByTag<O, T>( O obj, object tag, CollisionHandler<O, T> handler )
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
        /// olio <code>obj</code> törmää toiseen olioon.
        /// </summary>
        /// <param name="obj">Törmäävä olio.</param>
        /// <param name="target">Törmättävän olion viite tai tagi.</param>
        /// <param name="handler">Metodi, joka käsittelee törmäyksen (ei parametreja).</param>
        public void AddCollisionHandler<O, T>( O obj, object target, CollisionHandler<O, T> handler )
            where O : IPhysicsObject
            where T : IPhysicsObject
        {
            if ( target is T )
                AddCollisionHandlerByRef( obj, (T)target, handler );
            else
                AddCollisionHandlerByTag( obj, target, handler );
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
        [EditorBrowsable( EditorBrowsableState.Never )]
        public void RemoveProtectedCollisionHandlers( PhysicsObject obj = null, PhysicsObject target = null, object tag = null, Delegate handler = null )
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
