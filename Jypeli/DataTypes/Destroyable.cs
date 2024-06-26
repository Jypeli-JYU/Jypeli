﻿using System;

namespace Jypeli
{
    /// <summary>
    /// Rajapinta olioille, jotka ovat tuhottavissa.
    /// </summary>
    public interface Destroyable
    {
        /// <summary>
        /// Onko kappale tuhottu
        /// </summary>
        bool IsDestroyed { get; }

        /// <summary>
        /// Tapahtuu kun kappale tuhotaan
        /// </summary>
        event Action Destroyed;

        /// <summary>
        /// Tuhoaa kappaleen
        /// </summary>
        void Destroy();
    }

    /// <summary>
    /// Rajapinta olioille, joiden tuhoaminen kestää seuraavaan päivitykseen.
    /// </summary>
    public interface DelayedDestroyable : Destroyable // TODO: Tästä rajapinnasta voitaneen hankkiutua eroon?
    {
        /// <summary>
        /// Onko kappale tällä hetkellä tuhoutumassa
        /// </summary>
        bool IsDestroying { get; }
    }
}
