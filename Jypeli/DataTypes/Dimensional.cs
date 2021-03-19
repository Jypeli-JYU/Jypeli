namespace Jypeli
{
    /// <summary>
    /// Olio jolla on reunat.
    /// </summary>
    public interface Dimensional
    {
        /// <summary>
        /// Vasen reuna.
        /// </summary>
        double Left { get; }

        /// <summary>
        /// Oikea reuna.
        /// </summary>
        double Right { get; }

        /// <summary>
        /// Yläreuna.
        /// </summary>
        double Top { get; }

        /// <summary>
        /// Alareuna.
        /// </summary>
        double Bottom { get; }

        /// <summary>
        /// Koko.
        /// </summary>
        Vector Size { get; }

        /// <summary>
        /// Leveys.
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Korkeus.
        /// </summary>
        double Height { get; }
    }

    /// <summary>
    /// Olio jolla on reunat joita voi muuttaa.
    /// </summary>
    public interface DimensionalRW : Dimensional
    {
        /// <summary>
        /// Vasen reuna.
        /// </summary>
        new double Left { get; set; }

        /// <summary>
        /// Oikea reuna.
        /// </summary>
        new double Right { get; set; }

        /// <summary>
        /// Yläreuna.
        /// </summary>
        new double Top { get; set; }

        /// <summary>
        /// Alareuna.
        /// </summary>
        new double Bottom { get; set; }

        /// <summary>
        /// Koko.
        /// </summary>
        new Vector Size { get; set; }

        /// <summary>
        /// Leveys.
        /// </summary>
        new double Width { get; set; }

        /// <summary>
        /// Korkeus.
        /// </summary>
        new double Height { get; set; }
    }
}
