using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    public partial class Game
    {
        public Keyboard Keyboard { get; private set; }
        public Mouse Mouse { get; private set; }
        public TouchPanel TouchPanel { get; private set; }

        private void InitControls()
        {
            Keyboard = new Keyboard();
            Mouse = new Mouse();
            TouchPanel = new TouchPanel();
        }

        private void UpdateControls()
        {
            Keyboard.Update();
            Mouse.Update();
            TouchPanel.Update();
        }
    }
}
