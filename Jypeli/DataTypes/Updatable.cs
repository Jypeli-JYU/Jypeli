namespace Jypeli
{
    /// <summary>
    /// Rajapinta päivittyville olioille.
    /// </summary>
    public interface Updatable
    {
        bool IsUpdated { get; }
        void Update( Time time );
    }
}
