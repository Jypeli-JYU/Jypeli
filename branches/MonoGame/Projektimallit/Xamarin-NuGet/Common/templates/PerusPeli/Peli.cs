﻿using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace ${ProjectName}
{
    public class ${ProjectName} : Game
    {
        public override void Begin()
        {
            // Kirjoita ohjelmakoodisi tähän

            PhoneBackButton.Listen( ConfirmExit, "Lopeta peli" );
            Keyboard.Listen( Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli" );
        }
    }
}
