namespace Jypeli
{
    /// <summary>
    /// Olio jolla on paikka.
    /// </summary>
    public interface Positional
    {
        /// <summary>
        /// Paikka.
        /// </summary>
        Vector Position { get; }

        /// <summary>
        /// Paikan X-koordinaatti.
        /// </summary>
        double X { get; }

        /// <summary>
        /// Paikan Y-koordinaatti.
        /// </summary>
        double Y { get; }
    }

    /// <summary>
    /// Olio jolla on paikka jota voi muuttaa.
    /// </summary>
    public interface PositionalRW : Positional
    {
        /// <summary>
        /// Paikka.
        /// </summary>
        new Vector Position { get; set; }

        /// <summary>
        /// Paikan X-koordinaatti.
        /// </summary>
        new double X { get; }

        /// <summary>
        /// Paikan Y-koordinaatti.
        /// </summary>
        new double Y { get; }
    }
}
