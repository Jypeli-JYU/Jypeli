using System;

namespace Jypeli
{
    /// <summary>
    /// Rajapinta olioille, jotka ovat tuhottavissa.
    /// </summary>
    public interface Destroyable
    {
        bool IsDestroyed { get; }
        event Action Destroyed;
        void Destroy();
    }

    /// <summary>
    /// Rajapinta olioille, joiden tuhoaminen kestää seuraavaan päivitykseen.
    /// </summary>
    public interface DelayedDestroyable : Destroyable
    {
        bool IsDestroying { get; }
        event Action Destroying;
    }
}
