namespace Jypeli
{
    /// <summary>
    /// Rajapinta luokalle joka sisältää peliolioita.
    /// </summary>
    public interface GameObjectContainer
    {
        /// <summary>
        /// Lisää peliolion.
        /// </summary>
        /// <param name="obj">Olio</param>
        void Add( IGameObject obj );

        /// <summary>
        /// Poistaa peliolion tuhoamatta sitä.
        /// </summary>
        /// <param name="obj">Olio</param>
        void Remove( IGameObject obj );
    }
}
