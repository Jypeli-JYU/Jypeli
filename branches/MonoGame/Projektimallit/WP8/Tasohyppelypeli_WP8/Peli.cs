using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Peli : PhysicsGame
{
    const double nopeus = 200;
    const double hyppyNopeus = 750;
    const int RUUDUN_KOKO = 40;

    PlatformCharacter pelaaja1;

    Image pelaajanKuva = LoadImage( "norsu" );
    Image tahtiKuva = LoadImage( "tahti" );

    public override void Begin()
    {
        Gravity = new Vector( 0, -1000 );

        LuoKentta();
        LisaaKontrollit();

        Camera.Follow( pelaaja1 );
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
    }

    void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset( "kentta1" );
        kentta.SetTileMethod( '#', LisaaTaso );
        kentta.SetTileMethod( '*', LisaaTahti );
        kentta.SetTileMethod( 'N', LisaaPelaaja );
        kentta.Execute( RUUDUN_KOKO, RUUDUN_KOKO );
        Level.CreateBorders();
        Level.Background.CreateGradient( Color.White, Color.SkyBlue );
    }

    void LisaaTaso( Vector paikka, double leveys, double korkeus )
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject( leveys, korkeus );
        taso.Position = paikka;
        taso.Color = Color.Green;
        Add( taso );
    }

    void LisaaTahti( Vector paikka, double leveys, double korkeus )
    {
        PhysicsObject tahti = PhysicsObject.CreateStaticObject( leveys, korkeus );
        tahti.IgnoresCollisionResponse = true;
        tahti.Position = paikka;
        tahti.Image = tahtiKuva;
        tahti.Tag = "tahti";
        Add( tahti );
    }

    void LisaaPelaaja( Vector paikka, double leveys, double korkeus )
    {
        pelaaja1 = new PlatformCharacter( leveys, korkeus );
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 4.0;
        pelaaja1.Image = pelaajanKuva;
        AddCollisionHandler(pelaaja1, "tahti", TormaaTahteen);
        Add( pelaaja1 );
    }

    void LisaaKontrollit()
    {
        Widget vasenReuna = new Widget( Screen.Width / 3, Screen.Height );
        vasenReuna.Left = Screen.Left;
        vasenReuna.IsVisible = false;
        Add( vasenReuna );

        Widget oikeaReuna = new Widget( Screen.Width / 3, Screen.Height );
        oikeaReuna.Right = Screen.Right;
        oikeaReuna.IsVisible = false;
        Add( oikeaReuna );

        Widget ylaReuna = new Widget( Screen.Width, Screen.Height / 3 );
        ylaReuna.Top = Screen.Top;
        ylaReuna.IsVisible = false;
        Add( ylaReuna );

        TouchPanel.ListenOn( vasenReuna, ButtonState.Down, Liikuta, "Liikuta pelaajaa", pelaaja1, -nopeus );
        TouchPanel.ListenOn( oikeaReuna, ButtonState.Down, Liikuta, "Liikuta pelaajaa", pelaaja1, nopeus );
        TouchPanel.ListenOn( ylaReuna, ButtonState.Down, Hyppaa, "Hyppää", pelaaja1, hyppyNopeus );

        PhoneBackButton.Listen( ConfirmExit, "Lopeta peli" );
    }

    void Liikuta( Touch kosketus, PlatformCharacter hahmo, double nopeus )
    {
        hahmo.Walk( nopeus );
    }

    void Hyppaa( Touch kosketus, PlatformCharacter hahmo, double nopeus )
    {
        hahmo.Jump( nopeus );
    }

    void TormaaTahteen(PhysicsObject hahmo, PhysicsObject tahti)
    {
        MessageDisplay.Add("Keräsit tähden!");
        tahti.Destroy();
    }
}
