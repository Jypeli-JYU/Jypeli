using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli
{
    /// <summary>
    /// Puhelimen näytön asemointi.
    /// </summary>
    public enum DisplayOrientation
    {
        /// <summary>
        /// Vaakasuuntainen.
        /// </summary>
        Landscape,

        /// <summary>
        /// Vaakasuuntainen, vasemmalle käännetty.
        /// </summary>
        LandscapeLeft,

        /// <summary>
        /// Vaakasuuntainen, oikealle käännetty.
        /// </summary>
        LandscapeRight,

        /// <summary>
        /// Pystysuuntainen.
        /// </summary>
        Portrait,

        /// <summary>
        /// Pystysuuntainen, ylösalaisin käännetty.
        /// </summary>
        PortraitInverse,
    }
}
