using System;
//using Physics2DDotNet;
//using Physics2DDotNet.Ignorers;
using Jypeli.Physics;

namespace Jypeli
{
    /// <summary>
    /// Yhteinen rajapinta kaikille fysiikkaolioille.
    /// </summary>
    public interface IPhysicsObject : IGameObject, DelayedDestroyable
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        PhysicsStructure ParentStructure { get; }

        //Body Body { get; }

        Ignorer CollisionIgnorer { get; set; }
        int CollisionIgnoreGroup { get; set; }

        /// <summary>
        /// Funktio, jolla voidaan välttää törmäys toisiin kappaleisiin.
        /// Palauta <c>true</c>, jos kappaleiden pitäisi törmätä, <c>false</c> jos pitäisi mennä läpi.
        /// </summary>
        /// <remarks>
        /// HUOM! Käytä tätä vain ja ainoastaan törmäyksestä päättämiseen, älä käytä tätä yleisenä
        /// törmäystapahtuman käsittelijänä. Törmäystapahtumien käsittelyyn tulee käyttää <c>AddCollisiosionHandler</c>in kautta laitettuja funktioita.
        /// </remarks>
        Func<IPhysicsObject, IPhysicsObject, bool> CollisionIgnoreFunc { get; set; }

        double Mass { get; set; }
        double MomentOfInertia { get; set; }
        bool CanRotate { get; set; }

        double Restitution { get; set; }
        double StaticFriction { get; set; }
        double KineticFriction { get; set; }

        Vector Velocity { get; set; }
        Vector Acceleration { get; set; }
        double AngularVelocity { get; set; }
        double AngularAcceleration { get; set; }
        
        double LinearDamping { get; set; }
        double AngularDamping { get; set; }
        
        bool IgnoresExplosions { get; set; }
        bool IgnoresCollisionResponse { get; set; }
        bool IgnoresGravity { get; set; }
        bool IgnoresPhysicsLogics { get; set; }

        /// <summary>
        /// Tapahtuu, kun törmätään toiseen fysiikkaolioon.
        /// </summary>
        event CollisionHandler<IPhysicsObject, IPhysicsObject> Collided;

        void Push( Vector force );
        void Push( Vector force, TimeSpan time );
        void Hit( Vector impulse );
        void ApplyTorque( double torque );

        void Stop();
        void StopAxial( Vector axis );
        void StopHorizontal();
        void StopVertical();
        void StopAngular();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Jypelin sisäiset metodit ja propertyt joihin käyttäjän ei tarvitse
    /// päästä käsiksi kuuluvat tähän luokkaan. Kaikki oliot jotka toteuttavat
    /// IPhysicsObject-rajapinnan toteuttavat myös IPhysicsObjectInternal-rajapinnan.
    /// Ota tämä huomioon jos aiot tehdä oman olion joka toteuttaa suoraan
    /// IPhysicsObject(Internal)-rajapinnan.
    /// <example>
    /// void UpdateObject(IPhysics obj)
    /// {
    ///    ((IPhysicsObjectInternal)obj).Update();
    /// }
    /// </example>
    /// </summary>
    public interface IPhysicsObjectInternal : IGameObjectInternal, IPhysicsObject
    {
    }

    /// <summary>
    /// Törmäystapahtumiin liitettävän metodin tyyppi. Törmäyksen käsittelevällä
    /// metodilla ei ole paluuarvoa ja se ottaa yhden <code>Collision</code>-tyyppisen
    /// parametrin.
    /// </summary>
    public delegate void CollisionHandler<O, T>( O collidingObject, T otherObject );

    /// <summary>
    /// Törmäystapahtumiin liitettävän metodin tyyppi. Törmäyksen käsittelevällä
    /// metodilla ei ole paluuarvoa ja se ottaa yhden <code>Collision</code>-tyyppisen
    /// parametrin.
    /// </summary>
    public delegate void AdvancedCollisionHandler<O, T>( O collidingObject, T otherObject, Collision collision );
}
