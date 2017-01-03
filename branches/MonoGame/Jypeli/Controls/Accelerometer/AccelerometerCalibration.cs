namespace Jypeli
{
    /// <summary>
    /// Kalibrointi puhelimen kallistuksen nollakohdalle.
    /// (Asento miss� puhelinta ei ole kallistettu yht��n)
    /// </summary>
    public enum AccelerometerCalibration
    {
        /// <summary>
        /// Puhelin on vaakatasossa n�ytt� yl�sp�in.
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
