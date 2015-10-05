using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        PhysicsStructure ParentStructure { get; }

        //Body Body { get; }

        Ignorer CollisionIgnorer { get; set; }
        int CollisionIgnoreGroup { get; set; }

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
        /// Tapahtuu, kun t�rm�t��n toiseen fysiikkaolioon.
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
    }

    /// <summary>
    /// Jypelin sis�iset metodit ja propertyt joihin k�ytt�j�n ei tarvitse
    /// p��st� k�siksi kuuluvat t�h�n luokkaan. Kaikki oliot jotka toteuttavat
    /// IPhysicsObject-rajapinnan toteuttavat my�s IPhysicsObjectInternal-rajapinnan.
    /// Ota t�m� huomioon jos aiot tehd� oman olion joka toteuttaa suoraan
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
    /// T�rm�ystapahtumiin liitett�v�n metodin tyyppi. T�rm�yksen k�sittelev�ll�
    /// metodilla ei ole paluuarvoa ja se ottaa yhden <code>Collision</code>-tyyppisen
    /// parametrin.
    /// </summary>
    public delegate void CollisionHandler<O, T>( O collidingObject, T otherObject );

    /// <summary>
    /// T�rm�ystapahtumiin liitett�v�n metodin tyyppi. T�rm�yksen k�sittelev�ll�
    /// metodilla ei ole paluuarvoa ja se ottaa yhden <code>Collision</code>-tyyppisen
    /// parametrin.
    /// </summary>
    public delegate void AdvancedCollisionHandler<O, T>( O collidingObject, T otherObject, Collision collision );
}
