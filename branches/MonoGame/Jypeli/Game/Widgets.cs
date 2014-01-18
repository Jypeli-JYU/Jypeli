using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    public partial class Game
    {
        /// <summary>
        /// Viestinäyttö, johon voi laittaa viestejä.
        /// </summary>
        /// <value>Viestinäyttö.</value>
        public MessageDisplay MessageDisplay { get; set; }

        private void addMessageDisplay()
        {
            if ( MessageDisplay == null )
            {
                MessageDisplay = new MessageDisplay();
                MessageDisplay.BackgroundColor = Color.LightGray;
            }

            if ( !MessageDisplay.IsAddedToGame )
                Add( MessageDisplay );
        }

        private bool IsJypeliWidget<T>( T obj ) where T : IGameObject
        {
            return object.ReferenceEquals( obj, this.MessageDisplay );
        }
    }
}
