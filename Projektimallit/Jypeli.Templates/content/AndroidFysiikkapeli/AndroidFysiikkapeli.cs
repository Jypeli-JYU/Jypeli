using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace AndroidFysiikkapeli;

/// @author Omanimi
/// @version Päivämäärä
/// <summary>
/// 
/// </summary>
public class AndroidFysiikkapeli : PhysicsGame
{
    public override void Begin()
    {
        // Kirjoita ohjelmakoodisi tähän

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }
}

