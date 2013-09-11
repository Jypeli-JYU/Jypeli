using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Jypeli.Controls;
//using Jypeli.GameObjects;

namespace Jypeli
{
    /// <summary>
    /// Yhteinen rajapinta kaikille peliolioille.
    /// </summary>
    public interface IGameObject : Tagged, Destroyable, Updatable
    {
        Layer Layer { get; }
        IGameObject Parent { get; set; }
        Brain Brain { get; set; }

        SynchronousList<GameObject> Objects { get; }

        bool IsVisible { get; set; }
        bool IsAddedToGame { get; }

        TimeSpan CreationTime { get; }
        TimeSpan Lifetime { get; }
        TimeSpan MaximumLifetime { get; set; }
        
        Vector Size { get; set; }
        double Width { get; set; }
        double Height { get; set; }

        Vector Position { get; set; }
        Angle Angle { get; set; }
        double X { get; set; }
        double Y { get; set; }
        double Left { get; set; }
        double Top { get; set; }
        double Right { get; set; }
        double Bottom { get; set; }

        Vector AbsolutePosition { get; set; }
        Angle AbsoluteAngle { get; set; }
        
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

        void Add( GameObject childObject );
        void Remove( GameObject childObject );

        void Move( Vector movement );
        void MoveTo( Vector location, double speed, Action doWhenArrived );
        void StopMoveTo();
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
        Layer Layer { set; }
        List<Listener> AssociatedListeners { get; }

        void Update( Time time );

        bool IsAddedToGame { set; }

        void OnAddedToGame();
        void OnRemoved();
    }
}
