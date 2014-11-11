using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    /// <summary>
    /// Fysiikkapeli (SimplePhysics)
    /// </summary>
    public partial class SimplePhysicsEngine
    {
        private struct CollisionRecord
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

        private Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>> collisionHandlers =
            new Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>>();
        private Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>> protectedCollisionHandlers =
            new Dictionary<CollisionRecord, CollisionHandler<IPhysicsObject, IPhysicsObject>>();

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

#if structures
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

#if structures
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
        /// Määrää, mihin aliohjelmaan siirrytään kun 
        /// fysiikkaolio <code>obj</code> törmää fysiikkarakenteeseen, jolla on tietty tagi <code>tag</code>.
        /// </summary>
        /// <param name="obj">Törmäävä olio.</param>
        /// <param name="tag">Törmättävän olion tagi.</param>
        /// <param name="handler">Metodi, joka käsittelee törmäyksen (ei parametreja).</param>
        public void AddCollisionHandler( PhysicsObject obj, object tag, CollisionHandler<PhysicsObject, PhysicsStructure> handler )
        {
            AddCollisionHandler<PhysicsObject, PhysicsStructure>( obj, tag, handler );
        }

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun 
        /// fysiikkarakenne <code>obj</code> törmää fysiikkaolioon, jolla on tietty tagi <code>tag</code>.
        /// </summary>
        /// <param name="obj">Törmäävä rakenne.</param>
        /// <param name="tag">Törmättävän olion tagi.</param>
        /// <param name="handler">Metodi, joka käsittelee törmäyksen (ei parametreja).</param>
        public void AddCollisionHandler( PhysicsStructure obj, object tag, CollisionHandler<PhysicsStructure, PhysicsObject> handler )
        {
            AddCollisionHandler<PhysicsStructure, PhysicsObject>( obj, tag, handler );
        }

        /// <summary>
        /// Määrää, mihin aliohjelmaan siirrytään kun 
        /// fysiikkarakenne <code>obj</code> törmää toiseen fysiikarakenteeseen, jolla on tietty tagi <code>tag</code>.
        /// </summary>
        /// <param name="obj">Törmäävä rakenne.</param>
        /// <param name="tag">Törmättävän rakenteen tagi.</param>
        /// <param name="handler">Metodi, joka käsittelee törmäyksen (ei parametreja).</param>
        public void AddCollisionHandler( PhysicsStructure obj, object tag, CollisionHandler<PhysicsStructure, PhysicsStructure> handler )
        {
            AddCollisionHandler<PhysicsStructure, PhysicsStructure>( obj, tag, handler );
        }
#endif

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

            foreach ( var key in collisionHandlers.Keys )
            {
                if ( pred( key ) ) remove.Add( key );
            }

            foreach ( var key in remove )
            {
                key.obj.Collided -= collisionHandlers[key];
                collisionHandlers.Remove( key );
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
    }
}
