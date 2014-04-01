using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Controls;
using Jypeli.Widgets;
using Jypeli.Assets;

public class Peli : PhysicsGame
{
    public override void Begin()
    {
        Level.CreateBorders();

        for (int i = 0; i < 10; i++)
        {
            PhysicsObject olio = new PhysicsObject( RandomGen.NextDouble( 10, 40 ), RandomGen.NextDouble( 10, 40 ) );
            olio.Angle = RandomGen.NextAngle();
            olio.Position = RandomGen.NextVector( 100, 150 );
            Add( olio );
        }

        MediaPlayer.Play( "AbracaZebra" );
        Keyboard.Listen( Key.Space, ButtonState.Pressed, Pum, null );
    }

    void Pum()
    {
        Explosion rajahdys = new Explosion( 150 );
        Add( rajahdys );
    }
}
