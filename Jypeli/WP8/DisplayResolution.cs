using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli
{
    /// <summary>
    /// Puhelimen näytön tarkkuus.
    /// </summary>
    public enum DisplayResolution
    {
        /// <summary>
        /// Pieni tarkkuus (WVGA, 400 x 240).
        /// WP7-yhteensopivuustila, ei varsinaisesti paranna suorituskykyä.
        /// </summary>
        Small,

        /// <summary>
        /// Suuri tarkkuus (WVGA, 800 x 480).
        /// Oletus WP8:lla.
        /// </summary>
        Large,

        /// <summary>
        /// HD720-tarkkuus (720p, 1280 x 720).
        /// Ei toimi kaikilla puhelimilla.
        /// </summary>
        HD720,

        /// <summary>
        /// HD1080-tarkkuus (1080p, 1920 x 1080).
        /// Ei toimi kaikilla puhelimilla.
        /// </summary>
        HD1080,
    }
}

// Aliases for different phone series below

namespace Jypeli.WP7
{
    /// <summary>
    /// Puhelimen näytön tarkkuus.
    /// </summary>
    public static class DisplayResolution
    {
        /// <summary>
        /// Pieni tarkkuus (WVGA, 400 x 240).
        /// </summary>
        public static Jypeli.DisplayResolution Small
        {
            get { return Jypeli.DisplayResolution.Small; }
        }

        /// <summary>
        /// Suuri tarkkuus (WVGA, 800 x 480).
        /// </summary>
        public static Jypeli.DisplayResolution Large
        {
            get { return Jypeli.DisplayResolution.Large; }
        }
    }
}

namespace Jypeli.WP8
{
    /// <summary>
    /// Puhelimen näytön tarkkuus.
    /// </summary>
    public static class DisplayResolution
    {
        /// <summary>
        /// Pieni tarkkuus (WVGA, 400 x 240).
        /// </summary>
        public static Jypeli.DisplayResolution Small
        {
            get { return Jypeli.DisplayResolution.Small; }
        }

        /// <summary>
        /// Suuri tarkkuus (WVGA, 800 x 480).
        /// </summary>
        public static Jypeli.DisplayResolution Large
        {
            get { return Jypeli.DisplayResolution.Large; }
        }

        /// <summary>
        /// HD720-tarkkuus (720p, 1280 x 720).
        /// Ei toimi kaikilla puhelimilla.
        /// </summary>
        public static Jypeli.DisplayResolution HD720
        {
            get { return Jypeli.DisplayResolution.HD720; }
        }

        /// <summary>
        /// HD1080-tarkkuus (1080p, 1920 x 1080).
        /// Ei toimi kaikilla puhelimilla.
        /// </summary>
        public static Jypeli.DisplayResolution HD1080
        {
            get { return Jypeli.DisplayResolution.HD1080; }
        }
    }
}
