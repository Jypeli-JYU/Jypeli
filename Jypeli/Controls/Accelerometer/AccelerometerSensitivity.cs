namespace Jypeli
{
    /// <summary>
    /// Herkkyys jolla kallistus/ele halutaan tunnistaa.
    /// </summary>
    public enum AccelerometerSensitivity : int
    {
        /// <summary>
        /// Kallistus/ele tunnistetaan nopeasti.
        /// </summary>
        Realtime = 1,

        /// <summary>
        /// Kallistus/ele tunnistetaan melko nopeasti.
        /// </summary>
        High = 20,

        /// <summary>
        /// Kallistus/ele tunnistetaan melko myöhään.
        /// </summary>
        Medium = 50,

        /// <summary>
        /// Kallistus/ele tunnistetaan myöhään.
        /// </summary>
        Low = 70,
    }
}
