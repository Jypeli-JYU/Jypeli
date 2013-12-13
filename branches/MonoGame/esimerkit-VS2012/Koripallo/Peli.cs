﻿using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Koripallo : PhysicsGame
{
    #region Muuttujat

    int korinSuunta;
    bool saakoHeittaa;
    bool onHeittamassa = false;

    /// <summary>
    /// Tähän lisätään viimeisimmät hiiren/kosketuspaneelin liikkeet heiton aikana,
    /// jotta heiton voima ei tulisi liian herkästi viimeisestä liikkeestä.
    /// 
    /// Taulukon kokoa muuttamalla voi valita kuinka pitkältä ajalta heitto luetaan.
    /// </summary>
    Vector[] viimeisetLiikkeet = new Vector[8];

    /// <summary>
    /// Kuinka nopeasti heitto lähtee suhteessa hiiren tai kosketuspaneelin liikkeeseen.
    /// </summary>
    const double HeittoNopeus = 8;

    int liikeIndeksi = 0;

    PhysicsObject pallo;
    PhysicsObject levy;
    PhysicsObject koriVasen;
    PhysicsObject koriOikea;
    PhysicsObject koriKeskus;
    PhysicsObject lattia;

    GameObject korinVerkko;
    GameObject pelaaja;

    Vector heittoVoima;
    IntMeter kenttaLaskuri;
    DoubleMeter voimaMittari;

    SoundEffect pomppuAani = LoadSoundEffect( "hit" );

    Timer liikkuvuusAjastin;
    Timer KentanVaihtoAjastin;

    Image[] pelaajanKuvat = LoadImages( "heittaja1", "heittaja2", "heittaja3" );
    Image pallonKuva = LoadImage( "koripallo" );
    Image verkonKuva = LoadImage( "kori" );
    Image taustaKuva = LoadImage( "tausta" );

    #endregion

    void liikutaKoria()
    {
        if ( ( levy.Top >= Level.Top ) || ( korinVerkko.Bottom <= Level.Bottom ) )
        {
            korinSuunta = ( ( -1 ) * korinSuunta );
        }

        levy.Y += korinSuunta;
        koriKeskus.Y += korinSuunta;
        koriOikea.Y += korinSuunta;
        koriVasen.Y += korinSuunta;
        korinVerkko.Y += korinSuunta;
    }

    void asetaPallo()
    {
        pallo.IgnoresGravity = true;
        pallo.AngularVelocity = 0.0;
        pallo.Velocity = Vector.Zero;
        pallo.X = pelaaja.X;
        pallo.Y = pelaaja.Top;
        saakoHeittaa = true;
    }

    void luoPelaaja()
    {
        pelaaja = new GameObject( 200, 200 );
        Animation heitto = new Animation( pelaajanKuvat );
        heitto.FPS = 20;
        pelaaja.Animation = heitto;
        pelaaja.X = ( 0.80 * Level.Left );
        pelaaja.Y = Level.Bottom + 100;
        Add( pelaaja );
    }

    void luoPallo()
    {
        pallo = new PhysicsObject( 50.0, 50.0, Shape.Circle );
        pallo.Mass = 12.0;
        pallo.Restitution = 0.8;
        pallo.KineticFriction = 0.6;
        pallo.AngularDamping = 0.99;
        pallo.Image = pallonKuva;
        asetaPallo();

        AddCollisionHandler( pallo, pallonTormayksenKasittely );
        Add( pallo );
    }

    PhysicsObject teeKorinReunat( Vector v )
    {
        PhysicsObject reunaPallo = PhysicsObject.CreateStaticObject( v.X, v.Y );
        reunaPallo.Shape = Shape.Circle;
        return reunaPallo;
    }

    void luoKori()
    {
        levy = PhysicsObject.CreateStaticObject( 13.0, 250.0 );
        levy.Shape = Shape.Rectangle;
        levy.Y = 0.5 * Level.Top;
        levy.X = Level.Right - 20.0;
        Add( levy );

        Vector pallonKoko = new Vector( 5.0, 5.0 );
        koriVasen = teeKorinReunat( pallonKoko );
        koriOikea = teeKorinReunat( pallonKoko );
        koriKeskus = teeKorinReunat( pallonKoko );

        koriOikea.Color = Color.Red;
        koriOikea.Y = levy.Bottom + 20.0;
        koriOikea.X = levy.Left - 15.0;
        koriVasen.Color = Color.Red;
        koriVasen.Y = levy.Bottom + 20.0;
        koriVasen.X = levy.Left - 90.0;
        koriKeskus.X = ( koriOikea.X + koriVasen.X ) / 2;
        koriKeskus.Y = ( ( koriOikea.Y + koriVasen.Y ) / 2 ) - 15.0;
        koriKeskus.IgnoresCollisionResponse = true;
        koriKeskus.IsVisible = false;

        korinVerkko = new GameObject( 90.0, 115.0 );
        korinVerkko.Shape = Shape.Rectangle;
        korinVerkko.Image = verkonKuva;
        korinVerkko.X = ( levy.Left - 45.0 );
        korinVerkko.Y = ( koriOikea.Top - 58 );

        Add( koriOikea );
        Add( koriVasen );
        Add( koriKeskus );
        Add( korinVerkko );
    }

    void luoKenttaNaytto( int kentanNro )
    {
        kenttaLaskuri = new IntMeter( kentanNro );

        Widget kenttaRuutu = new Widget( 100, 50 );
        kenttaRuutu.Color = Color.Transparent;
        kenttaRuutu.BorderColor = Color.White;
        kenttaRuutu.Layout = new VerticalLayout();
        kenttaRuutu.X = Screen.LeftSafe + 75;
        kenttaRuutu.Y = Screen.TopSafe - 40;
        Add( kenttaRuutu );

        Label kenttaTeksti = new Label( "Taso" );
        kenttaTeksti.TextColor = Color.White;
        kenttaRuutu.Add( kenttaTeksti );

        Label kenttaNaytto = new Label();
        kenttaNaytto.BindTo( kenttaLaskuri );
        kenttaNaytto.TextColor = Color.White;
        kenttaRuutu.Add( kenttaNaytto );
    }

    void luoVoimaMittari()
    {
        voimaMittari = new DoubleMeter( 0 );
        voimaMittari.MaxValue = 30000;
        BarGauge voimaPalkki = new BarGauge( 10, 0.2 * Screen.HeightSafe );
        voimaPalkki.BorderColor = Color.Black;
        voimaPalkki.BindTo( voimaMittari );
        voimaPalkki.X = ( 0.95 * Screen.LeftSafe );
        voimaPalkki.Y = ( 0.05 * Screen.Height );
        Add( voimaPalkki );
    }

    void luoKentta( double leveys, double korkeus, int kentanNro )
    {
        Level.Width = leveys;
        Level.Height = korkeus;

        Level.Background.Image = taustaKuva;
        Level.Background.FitToLevel();
        Gravity = new Vector( 0, -1000.0 );
        Mouse.IsCursorVisible = true;

        lattia = Level.CreateBottomBorder();
        lattia.Restitution = 0.6;
        lattia.KineticFriction = 0.2;
        Level.CreateTopBorder();
        Level.CreateLeftBorder();
        Level.CreateRightBorder();

        luoPelaaja();
        luoPallo();
        luoKori();
        luoKenttaNaytto( kentanNro );
        luoVoimaMittari();

        if ( kentanNro >= 2 )
        {
            while ( true )
            {
                korinSuunta = RandomGen.NextInt( -1, 2 );
                if ( korinSuunta != 0 ) break;
            }
            korinSuunta = korinSuunta * kentanNro;
            liikkuvuusAjastin.Start();
        }

        Camera.ZoomToLevel();

#if WINDOWS_PHONE
        Camera.Follow(pallo);
        Camera.StayInLevel = true;
#endif
    }

    void seuraavaKentta()
    {
        kenttaLaskuri.Value++;
        ClearAll();

#if WINDOWS_PHONE
        // Ei kasvateta kenttää puhelinversiossa.
        luoKentta(Level.Width, Level.Height, kenttaLaskuri.Value);
#else
        luoKentta( 1.1 * Level.Width, 1.1 * Level.Height, kenttaLaskuri.Value );
#endif

        asetaNapit();
    }

    void pallonTormayksenKasittely( PhysicsObject pallo, PhysicsObject kohde )
    {
        if ( ( kohde != pelaaja ) && ( kohde != koriKeskus ) )
        {
            pomppuAani.Play();
        }

        if ( !KentanVaihtoAjastin.Enabled && ( kohde == koriKeskus ) && ( pallo.Velocity.Y <= 0 ) )
        {
            pallo.Velocity = new Vector( 0, 0.1 * pallo.Velocity.Y );
            liikkuvuusAjastin.Stop();
            KentanVaihtoAjastin.Start( 1 );
        }
    }

    void laskeVoima()
    {
        if ( saakoHeittaa )
        {
            Vector hiirenPaikka = Mouse.PositionOnWorld;
            heittoVoima.X += ( hiirenPaikka.X - pallo.X );
            heittoVoima.Y += ( hiirenPaikka.Y - pallo.Y );
            voimaMittari.Value = heittoVoima.Magnitude;
        }
    }

    void laskeVoimaPadilla()
    {
        if (saakoHeittaa)
        {
            heittoVoima.X += (ControllerOne.LeftThumbDirection.X * 250);
            heittoVoima.Y += (ControllerOne.LeftThumbDirection.Y * 250);
            voimaMittari.Value = heittoVoima.Magnitude;
        }
    }

    void heitaPallo()
    {
        ControllerOne.Vibrate( 0.2, 0.2, 0, 0, 0.5 );
        pelaaja.Animation.Start( 1 );
        saakoHeittaa = false;
        pallo.IgnoresGravity = false;
        pallo.Hit( heittoVoima );
        heittoVoima = Vector.Zero;
        voimaMittari.Reset();
    }

    void pyoritaPalloa( double kulmanopeus )
    {
        if ( saakoHeittaa )
        {
            pallo.AngularVelocity += kulmanopeus;
        }
    }

    void pyoritaPalloaPadilla()
    {
        if (saakoHeittaa)
        {
            pallo.AngularVelocity += ControllerOne.RightThumbDirection.X;
        }
    }

    bool OnPallonPaalla( Vector paikka )
    {
        return Vector.Distance( paikka, pallo.Position ) < ( pallo.Width / 2 );
    }

    void HeitaHiirella( AnalogState state )
    {
        //MessageDisplay.RealTime = true;
        //MessageDisplay.Add( String.Format( "({0}, {1})", Mouse.PositionOnScreen.X, Mouse.PositionOnScreen.Y ) );

        if ( onHeittamassa )
        {
            pallo.Position = Mouse.PositionOnWorld;
            LisaaLiike( state.MouseMovement );
        }
    }

    void AloitaHeitto()
    {
        pallo.Stop();
        pallo.IgnoresGravity = true;
        onHeittamassa = true;
        for ( int i = 0; i < viimeisetLiikkeet.Length; i++ )
            viimeisetLiikkeet[i] = Vector.Zero;
    }

    void AloitaHeittoHiirella()
    {
        if ( OnPallonPaalla( Mouse.PositionOnWorld ) )
        {
            AloitaHeitto();
        }
    }

    void LopetaHeitto()
    {
        if ( onHeittamassa )
        {
            onHeittamassa = false;
            pallo.IgnoresGravity = false;
            double xSumma = 0, ySumma = 0;
            for ( int i = 0; i < viimeisetLiikkeet.Length; i++ )
            {
                xSumma += viimeisetLiikkeet[i].X;
                ySumma += viimeisetLiikkeet[i].Y;
            }

            pallo.Velocity = new Vector( xSumma, ySumma ) * HeittoNopeus;
        }
    }

    void AloitaHeittoKosketuksella( Touch kosketus )
    {
        if ( OnPallonPaalla( kosketus.PositionOnWorld ) )
        {
            AloitaHeitto();
        }
    }

    void LopetaHeittoKosketuksella( Touch kosketus )
    {
        LopetaHeitto();
    }

    void LisaaLiike( Vector liike )
    {
        if ( liikeIndeksi == viimeisetLiikkeet.Length )
            liikeIndeksi = 0;
        viimeisetLiikkeet[liikeIndeksi++] = liike;
    }

    void HeitaKosketuksella( Touch kosketus )
    {
        if ( onHeittamassa )
        {
            pallo.Position = kosketus.PositionOnWorld;
            LisaaLiike( kosketus.MovementOnWorld );
        }
    }

    void asetaNapit()
    {
        Mouse.Listen( MouseButton.Left, ButtonState.Pressed, AloitaHeittoHiirella, null );
        Mouse.Listen( MouseButton.Left, ButtonState.Released, LopetaHeitto, null );
        Mouse.Listen( MouseButton.Right, ButtonState.Pressed, asetaPallo, "Palauta pallo alkutilanteeseen" );
        Mouse.ListenMovement( 0.1, HeitaHiirella, null );

        TouchPanel.Listen( ButtonState.Pressed, AloitaHeittoKosketuksella, null );
        TouchPanel.Listen( ButtonState.Released, LopetaHeittoKosketuksella, null );
        TouchPanel.Listen( ButtonState.Down, HeitaKosketuksella, null );
        //PhoneBackButton.Listen(ConfirmExit,"Lopettaa pelin");

        Keyboard.Listen( Key.Space, ButtonState.Down, laskeVoima, "Pidä pohjassa heiton voimakkuuden säätämiseen." );
        Keyboard.Listen( Key.Space, ButtonState.Released, heitaPallo, null );
        Keyboard.Listen( Key.Left, ButtonState.Down, pyoritaPalloa, "Anna pallolle alakierrettä.", 0.2 );
        Keyboard.Listen( Key.Right, ButtonState.Down, pyoritaPalloa, "Anna pallolle yläkierrettä.", -0.2 );

        Keyboard.Listen( Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet" );
        //Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Poistu");

        ControllerOne.Listen( Button.RightTrigger, ButtonState.Down, laskeVoimaPadilla, "Pidä pohjassa heiton voimakkuuden säätämiseen." );
        ControllerOne.Listen( Button.RightTrigger, ButtonState.Released, heitaPallo, null );
        ControllerOne.Listen( Button.RightStick, ButtonState.Up, pyoritaPalloaPadilla, "Anna pallolle kierrettä." );
        ControllerOne.Listen( Button.A, ButtonState.Pressed, asetaPallo, "Palauta pallo alkutilanteeseen" );

        ControllerOne.Listen( Button.Start, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet" );
        ControllerOne.Listen( Button.Back, ButtonState.Pressed, Exit, "Poistu" );
    }

    public Koripallo()
    {
        SetWindowSize( 1024, 768 );
        //SetWindowPosition( 0, 0 );
    }
    
    public override void Begin()
    {
        Phone.DisplayOrientation = DisplayOrientation.Portrait;

#if WINDOWS_PHONE
        Level.Width = 350;
        Level.Height = 650;
#endif
        liikkuvuusAjastin = new Timer();
        liikkuvuusAjastin.Interval = 0.003;
        liikkuvuusAjastin.Timeout += liikutaKoria;

        KentanVaihtoAjastin = new Timer();
        KentanVaihtoAjastin.Interval = 1.5;
        KentanVaihtoAjastin.Timeout += seuraavaKentta;

        luoKentta( Level.Width, Level.Height, 1 );
        asetaNapit();
    }
}
