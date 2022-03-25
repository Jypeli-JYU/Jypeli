using Silk.NET.Input;

namespace Jypeli
{
    /// <summary>
    /// Hiiren erilaiset kursorivaihtoehdot
    /// </summary>
    public enum MouseCursor
    {
        /// <summary>
        /// Oletuskursori
        /// </summary>
        Default = StandardCursor.Default,
        /// <summary>
        /// Nuoli
        /// </summary>
        Arrow = StandardCursor.Arrow,
        /// <summary>
        /// Pystypalkki
        /// </summary>
        IBeam = StandardCursor.IBeam,
        /// <summary>
        /// Tähtäin
        /// </summary>
        Crosshair = StandardCursor.Crosshair,
        /// <summary>
        /// Käsi
        /// </summary>
        Hand = StandardCursor.Hand,
        /// <summary>
        /// Pystysuuntaiset nuolet
        /// </summary>
        HResize = StandardCursor.HResize,
        /// <summary>
        /// Sivusuuntaiset nuolet
        /// </summary>
        VResize = StandardCursor.VResize
    }
}
