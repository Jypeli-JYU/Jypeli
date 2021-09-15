using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Fysiikkapeli : PhysicsGame
{
    /*
    HUOM!
    Tämä projektimalli käyttää hyvin kokeellista versiota Jypelistä.
    Kaikki ominaisuudet eivät välttämättä toimi, tai ne voivat olla puuttellisia.
    
    Jos kuitenkin käytät tätä, olisi hyvä jos ilmoitat löytämistäsi vioista.
    */
    public override void Begin()
    {
        // Kirjoita ohjelmakoodisi tähän

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }
}

