namespace Jypeli
{
    /// <summary>
    /// Analoginen ohjain. Tämä voi olla joko painike, jota voi painaa
    /// eri voimakkuuksilla (padiohjaimen liipainäppäin), ohjaustikku
    /// tai puhelimen kiihtyvyysanturi
    /// </summary>
    public enum AnalogControl
    {
        /// <summary>
        /// Ohjaimen tavallisemmin käytettävä analogitikku. Padissa, jossa on kaksi tikkua, käytetään vasenta.
        /// </summary>
        DefaultStick,

        /// <summary>
        /// Ohjaimen vasen analogitikku.
        /// </summary>
        LeftStick,

        /// <summary>
        /// Ohjaimen oikea analogitikku.
        /// </summary>
        RightStick,

        /// <summary>
        /// Ohjaimen vasen liipasin.
        /// </summary>
        LeftTrigger,

        /// <summary>
        /// Ohjaimen oikea liipasin.
        /// </summary>
        RightTrigger,

        /// <summary>
        /// Puhelimen kiihtyvyysanturi
        /// </summary>
        Accelerometer,
    }
}
