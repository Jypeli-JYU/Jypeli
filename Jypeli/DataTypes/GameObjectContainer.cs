using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    /// <summary>
    /// Rajapinta luokalle joka sisältää peliolioita.
    /// </summary>
    public interface GameObjectContainer
    {
        /// <summary>
        /// Lisää peliolion.
        /// </summary>
        /// <param name="obj">Olio</param>
        void Add( GameObject obj );

        /// <summary>
        /// Poistaa peliolion tuhoamatta sitä.
        /// </summary>
        /// <param name="obj">Olio</param>
        void Remove( GameObject obj );
    }
}
