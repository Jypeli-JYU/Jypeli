namespace Jypeli
{
    /// <summary>
    /// Rajapinta olioille, joilla on Tag-ominaisuus.
    /// </summary>
    public interface Tagged
    {
        object Tag { get; set; }
    }
}
