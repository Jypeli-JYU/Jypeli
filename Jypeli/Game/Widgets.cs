namespace Jypeli
{
    public partial class Game
    {
        /// <summary>
        /// Viestinäyttö, johon voi laittaa viestejä.
        /// </summary>
        /// <value>Viestinäyttö.</value>
        public MessageDisplay MessageDisplay { get; set; }

        private void AddMessageDisplay(bool force = false)
        {
            if (MessageDisplay == null)
            {
                MessageDisplay = new MessageDisplay();
                MessageDisplay.BackgroundColor = Color.LightGray;
            }
            else
                MessageDisplay.Clear();

            if ( !MessageDisplay.IsAddedToGame || force)
                Add( MessageDisplay );
        }

        private bool IsJypeliWidget<T>( T obj ) where T : IGameObject
        {
            return ReferenceEquals(obj, MessageDisplay);
        }
    }
}
