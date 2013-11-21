using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Peli : Game
{
    public override void Begin()
    {
        Keyboard.Listen( Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli" );
    }
}
