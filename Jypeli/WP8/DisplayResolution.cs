namespace Jypeli.WP8
{
    /// <summary>
    /// Puhelimen näytön tarkkuus.
    /// </summary>
    public static class DisplayResolution
    {
        /// <summary>
        /// Pieni tarkkuus (WVGA, 400 x 240).
        /// WP7-yhteensopivuustila, ei varsinaisesti paranna suorituskykyä.
        /// </summary>
        public static Jypeli.DisplayResolution Small = Jypeli.DisplayResolution.Small;

        /// <summary>
        /// Suuri tarkkuus (WVGA, 800 x 480).
        /// Oletus WP8:lla.
        /// </summary>
        public static Jypeli.DisplayResolution Large = Jypeli.DisplayResolution.Large;

        /// <summary>
        /// HD720-tarkkuus (720p, 1280 x 720).
        /// Ei toimi kaikilla puhelimilla.
        /// </summary>
        public static Jypeli.DisplayResolution HD720 = Jypeli.DisplayResolution.HD720;

        /// <summary>
        /// HD1080-tarkkuus (1080p, 1920 x 1080).
        /// Ei toimi kaikilla puhelimilla.
        /// </summary>
        public static Jypeli.DisplayResolution HD1080 = Jypeli.DisplayResolution.HD1080;
    }
}

namespace Jypeli.WP7
{
    /// <summary>
    /// Puhelimen näytön tarkkuus.
    /// </summary>
    public static class DisplayResolution
    {
        /// <summary>
        /// Pieni tarkkuus (WVGA, 400 x 240).
        /// WP7-yhteensopivuustila, ei varsinaisesti paranna suorituskykyä.
        /// </summary>
        public static Jypeli.DisplayResolution Small = Jypeli.DisplayResolution.Small;

        /// <summary>
        /// Suuri tarkkuus (WVGA, 800 x 480).
        /// Oletus WP8:lla.
        /// </summary>
        public static Jypeli.DisplayResolution Large = Jypeli.DisplayResolution.Large;
    }
}
