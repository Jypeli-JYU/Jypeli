namespace Jypeli
{
    /// <summary>
    /// Suunta/ele joka tunnistetaan.
    /// </summary>
    public enum AccelerometerDirection
    {
        /// <summary>
        /// kallistetaan mihin tahansa suuntaan.
        /// </summary>
        Any,

        /// <summary>
        /// Kallistetaan vasemalle.
        /// </summary>
        Left,

        /// <summary>
        /// Kallistetaan oikealle.
        /// </summary>
        Right,

        /// <summary>
        /// Kallistetaan yl�s.
        /// </summary>
        Up,

        /// <summary>
        /// Kallistetaan alas.
        /// </summary>
        Down,

        /// <summary>
        /// Puhelimen ravistusele.
        /// </summary>
        Shake,

        /// <summary>
        /// Puhelimen "nopea liike"-ele, esim. n�p�ytys tai t�r�hdys.
        /// </summary>
        Tap
    }
}
