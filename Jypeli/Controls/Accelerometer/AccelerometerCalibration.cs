namespace Jypeli
{
    /// <summary>
    /// Kalibrointi puhelimen kallistuksen nollakohdalle.
    /// (Asento missä puhelinta ei ole kallistettu yhtään)
    /// </summary>
    public enum AccelerometerCalibration
    {
        /// <summary>
        /// Puhelin on vaakatasossa näyttö ylöspäin.
        /// </summary>
        ZeroAngle,

        /// <summary>
        /// Puhelin on 45-asteen kulmassa.
        /// </summary>
        HalfRightAngle,

        /// <summary>
        /// Puhelin on pystysuorassa.
        /// </summary>
        RightAngle,
    }
}
