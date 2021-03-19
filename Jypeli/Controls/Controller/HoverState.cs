namespace Jypeli
{
    /// <summary>
    /// Tilatieto olion päällä olemisesta
    /// </summary>
    public enum HoverState
    {
        /// <summary>
        /// Ei olion päällä.
        /// </summary>
        Off,

        /// <summary>
        /// Siirtymässä olion päälle.
        /// </summary>
        Enter,

        /// <summary>
        /// Olion päällä.
        /// </summary>
        On,

        /// <summary>
        /// Poistumassa olion päältä.
        /// </summary>
        Exit
    }
}
