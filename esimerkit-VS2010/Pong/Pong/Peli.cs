using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

public class Pong : PhysicsGame
{
    Vector nopeusYlos = new Vector( 0, 200 );
    Vector nopeusAlas = new Vector( 0, -200 );

    PhysicsObject pallo;
    PhysicsObject maila1;
    PhysicsObject maila2;

    PhysicsObject vasenReuna;
    PhysicsObject oikeaReuna;

    IntMeter pelaajan1Pisteet;
    IntMeter pelaajan2Pisteet;

    public Pong()
    {
        SetWindowSize( 800, 600 );
        IsMouseVisible = true;
    }

    public override void Begin()
    {
        LuoKentta();
        AsetaOhjaimet();
        LisaaLaskurit();
        AloitaPeli();
    }

    void LuoKentta()
    {
        Level.Width = Screen.Width;
        Level.Height = Screen.Height;

        pallo = new PhysicsObject( 40.0, 40.0 );
        pallo.Shape = Shape.Circle;
        pallo.X = -200.0;
        pallo.Y = 0.0;
        pallo.Restitution = 1.0;
        pallo.KineticFriction = 0.0;
        pallo.MomentOfInertia = Double.PositiveInfinity;
        Add( pallo );
        AddCollisionHandler(pallo, KasittelePallonTormays);

        maila1 = LuoMaila( Level.Left + 20.0, 0.0 );
        maila2 = LuoMaila( Level.Right - 20.0, 0.0 );

        vasenReuna = Level.CreateLeftBorder();
        vasenReuna.Restitution = 1.0;
        vasenReuna.KineticFriction = 0.0;

        oikeaReuna = Level.CreateRightBorder();
        oikeaReuna.Restitution = 1.0;
        oikeaReuna.KineticFriction = 0.0;

        PhysicsObject ylaReuna = Level.CreateTopBorder();
        ylaReuna.Restitution = 1.0;
        ylaReuna.KineticFriction = 0.0;

        PhysicsObject alaReuna = Level.CreateBottomBorder();
        alaReuna.Restitution = 1.0;
        alaReuna.KineticFriction = 0.0;

        Level.BackgroundColor = Color.Black;

        Camera.ZoomToLevel();
    }

    PhysicsObject LuoMaila( double x, double y )
    {
        PhysicsObject maila = PhysicsObject.CreateStaticObject( 20.0, 100.0 );
        maila.Shape = Shape.Rectangle;
        maila.X = x;
        maila.Y = y;
        //maila.Restitution = 1.0;
        //maila.KineticFriction = 0.0;
        Add( maila );
        return maila;
    }

    void LisaaLaskurit()
    {
        pelaajan1Pisteet = LuoPisteLaskuri(Screen.Left + 100.0, Screen.Top - 100.0);
        pelaajan2Pisteet = LuoPisteLaskuri(Screen.Right - 100.0, Screen.Top - 100.0);
    }

    IntMeter LuoPisteLaskuri(double x, double y)
    {
        IntMeter laskuri = new IntMeter(0);
        laskuri.MaxValue = 10;
        
        Label naytto = new Label();
        naytto.BindTo(laskuri);
        naytto.X = x;
        naytto.Y = y;
        naytto.TextColor = Color.White;
        naytto.BorderColor = Level.BackgroundColor;
        naytto.Color = Level.BackgroundColor;
        Add(naytto);

        return laskuri;
    }

    void KasittelePallonTormays( PhysicsObject pallo, PhysicsObject kohde )
    {
        if ( kohde == oikeaReuna )
        {
            //MessageDisplay.Add( "Piste 1. pelaajalle" );
            pelaajan1Pisteet.Value += 1;
        }
        else if ( kohde == vasenReuna )
        {
            //MessageDisplay.Add( "Piste 2. pelaajalle" );
            pelaajan2Pisteet.Value += 1;
        }
    }

    void AloitaPeli()
    {
        Vector impulssi = new Vector( RandomGen.NextDouble( 100, 500 ), RandomGen.NextDouble( 100, 500 ) );
        pallo.Hit( impulssi );
    }

    void AsetaOhjaimet()
    {
        Keyboard.Listen( Key.W, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta mailaa ylös", maila1, nopeusYlos );
        Keyboard.Listen( Key.W, ButtonState.Released, AsetaNopeus, null, maila1, Vector.Zero );
        Keyboard.Listen( Key.S, ButtonState.Down, AsetaNopeus, "Pelaaja 1: Liikuta mailaa alas", maila1, nopeusAlas );
        Keyboard.Listen( Key.S, ButtonState.Released, AsetaNopeus, null, maila1, Vector.Zero );

        Keyboard.Listen( Key.Up, ButtonState.Down, AsetaNopeus, "Pelaaja 2: Liikuta mailaa ylös", maila2, nopeusYlos );
        Keyboard.Listen( Key.Up, ButtonState.Released, AsetaNopeus, null, maila2, Vector.Zero );
        Keyboard.Listen( Key.Down, ButtonState.Down, AsetaNopeus, "Pelaaja 2: Liikuta mailaa alas", maila2, nopeusAlas );
        Keyboard.Listen( Key.Down, ButtonState.Released, AsetaNopeus, null, maila2, Vector.Zero );

        //Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        //Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        /*ControllerOne.Listen(Button.DPadUp, ButtonState.Down, AsetaNopeus, "Liikuta mailaa ylös", maila1, nopeusYlos);
        ControllerOne.Listen(Button.DPadUp, ButtonState.Released, AsetaNopeus, null, maila1, Vector.Zero);
        ControllerOne.Listen(Button.DPadDown, ButtonState.Down, AsetaNopeus, "Liikuta mailaa alas", maila1, nopeusAlas);
        ControllerOne.Listen(Button.DPadDown, ButtonState.Released, AsetaNopeus, null, maila1, Vector.Zero);

        ControllerTwo.Listen(Button.DPadUp, ButtonState.Down, AsetaNopeus, "Liikuta mailaa ylös", maila2, nopeusYlos);
        ControllerTwo.Listen(Button.DPadUp, ButtonState.Released, AsetaNopeus, null, maila2, Vector.Zero);
        ControllerTwo.Listen(Button.DPadDown, ButtonState.Down, AsetaNopeus, "Liikuta mailaa alas", maila2, nopeusAlas);
        ControllerTwo.Listen(Button.DPadDown, ButtonState.Released, AsetaNopeus, null, maila2, Vector.Zero);

        ControllerOne.Listen(Button.Back, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        ControllerTwo.Listen(Button.Back, ButtonState.Pressed, ConfirmExit, "Lopeta peli");*/
    }

    void AsetaNopeus( PhysicsObject maila, Vector nopeus )
    {
        if ( ( nopeus.Y < 0 ) && ( maila.Bottom < Level.Bottom ) )
        {
            maila.Velocity = Vector.Zero;
            return;
        }
        if ( ( nopeus.Y > 0 ) && ( maila.Top > Level.Top ) )
        {
            maila.Velocity = Vector.Zero;
            return;
        }

        maila.Velocity = nopeus;
    }
}
