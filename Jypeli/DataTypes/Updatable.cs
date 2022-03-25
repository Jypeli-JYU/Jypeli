namespace Jypeli
{
    /// <summary>
    /// Rajapinta päivittyville olioille.
    /// </summary>
    public interface Updatable
    {
        /// <summary>
        /// Ajetaanko oliolle päivitystä
        /// </summary>
        bool IsUpdated { get; }

        /// <summary>
        /// Päivitysfunktio
        /// </summary>
        /// <param name="time">Kulunut aika edellisestä päivityksestä</param>
        void Update(Time time);
    }
}
