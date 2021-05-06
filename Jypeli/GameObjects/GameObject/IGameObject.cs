using System;
using System.Collections.Generic;
//using Jypeli.GameObjects;

namespace Jypeli
{
    /// <summary>
    /// Yhteinen rajapinta kaikille peliolioille.
    /// </summary>
    public interface IGameObject : Destroyable, DimensionalRW, PositionalRW, Updatable, Tagged
    {
        Layer Layer { get; }
        IGameObject Parent { get; set; }
        Brain Brain { get; set; }

        //SynchronousList<GameObject> Objects { get; }
        int ObjectCount { get; }

        bool IsVisible { get; set; }
        bool IsAddedToGame { get; }
        bool IgnoresLighting { get; set; }

        TimeSpan CreationTime { get; }
        TimeSpan Lifetime { get; }
        TimeSpan MaximumLifetime { get; set; }
        
        Angle Angle { get; set; }

        Vector RelativePositionToMainParent { get; set; }
        Angle RelativeAngleToMainParent { get; set; }
        Vector RelativePosition { get; set; }
        Angle RelativeAngle { get; set; }
        Vector UnitX { get; }
        Vector UnitY { get; }
        Vector RelativeUnitX { get; }
        Vector RelativeUnitY { get; }
        
        Animation Animation { get; set; }
        Image Image { get; set; }
        Color Color { get; set; }

        Shape Shape { get; set; }
        Vector TextureWrapSize { get; set; }
        bool TextureFillsShape { get; set; }
        bool RotateImage { get; set; }

        event Action AddedToGame;
        event Action Removed;

        bool IsInside( Vector point );

        void Add( IGameObject childObject );
        void Remove( IGameObject childObject );

        void Move( Vector movement );
        void MoveTo( Vector location, double speed, Action doWhenArrived );
        void StopMoveTo();

        IEnumerable<T> GetChildObjects<T>() where T : IGameObject;
        IEnumerable<T> GetChildObjects<T>( Predicate<T> predicate ) where T : IGameObject;
    }

    /// <summary>
    /// Jypelin sisäiset metodit ja propertyt joihin käyttäjän ei tarvitse
    /// päästä käsiksi kuuluvat tähän luokkaan. Kaikki oliot jotka toteuttavat
    /// IGameObject-rajapinnan toteuttavat myös IGameObjectInternal-rajapinnan.
    /// Ota tämä huomioon jos aiot tehdä oman olion joka toteuttaa suoraan
    /// IGameObject(Internal)-rajapinnan.
    /// <example>
    /// void UpdateObject(IGameObject obj)
    /// {
    ///    ((IGameObjectInternal)obj).Update();
    /// }
    /// </example>
    /// </summary>
    public interface IGameObjectInternal : IGameObject
    {
        new Layer Layer { get; set; }
        List<Listener> AssociatedListeners { get; }

        new bool IsAddedToGame { get; set; }

        void OnAddedToGame();
        void OnRemoved();
    }
}
