namespace Jypeli
{
    /// <summary>
    /// Rajapinta olioille, joilla on Tag-ominaisuus.
    /// </summary>
    public interface Tagged
    {
        /// <summary>
        /// Olion tagi, voi olla mitä tahansa
        /// </summary>
        object Tag { get; set; }
    }
}
