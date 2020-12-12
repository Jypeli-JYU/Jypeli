﻿using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Tasohyppelypeli : PhysicsGame
{
    private const double NOPEUS = 200;
    private const double HYPPYNOPEUS = 750;
    private const int RUUDUN_KOKO = 40;

    private PlatformCharacter pelaaja1;

    private Image pelaajanKuva = LoadImage("norsu.png");
    private Image tahtiKuva = LoadImage("tahti.png");

    private SoundEffect maaliAani = LoadSoundEffect("maali.wav");

    private int tahtia = 0;
    EasyHighScore e = new EasyHighScore();
    IntMeter pistelaskuri = new IntMeter(0);

    Label l;
    public override void Begin()
    {
        A();
    }

    void A()
    {
        Gravity = new Vector(0, -1000);

        LuoKentta();
        LisaaNappaimet();

        pistelaskuri.MaxValue = 1;
        pistelaskuri.UpperLimit += kaikki;

        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;

        l = new Label("Vakiofontti");
        l.Font = new Font(25);
        //l.Font.SetFontSize(50);
        //l.TextScale = new Vector(0.5, 0.5);
        l.Y = 170;
        Add(l);

        Label l2 = new Label("Vakiofontti");
        l2.Font = new Font(25, true);
        l2.Y = 150;
        Add(l2);

        MessageDisplay.Add("Kissa");

        Explosion e = new Explosion(200);
        //Add(e);
    }


    private void kaikki()
    {

        //e.EnterAndShow(tahtia);
        //e.HighScoreWindow.Closed += delegate
        //{
        //    DoNextUpdate(() =>
        //    {
        //        ClearAll();
        //        Begin();
        //    });
        //    
        //};
    }

    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta1.txt");
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaTahti);
        kentta.SetTileMethod('N', LisaaPelaaja);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.White, Color.SkyBlue);
    }

    private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.Green;
        Add(taso);
    }

    private void LisaaTahti(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tahti = PhysicsObject.CreateStaticObject(leveys, korkeus);
        tahti.IgnoresCollisionResponse = true;
        tahti.Position = paikka;
        tahti.Image = tahtiKuva;
        tahti.Tag = "tahti";
        Add(tahti);

        tahtia++;
    }

    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(leveys, korkeus);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 4.0;
        pelaaja1.Image = pelaajanKuva;
        AddCollisionHandler(pelaaja1, "tahti", TormaaTahteen);
        Add(pelaaja1);

        AssaultRifle a = new AssaultRifle(50, 10);
        pelaaja1.Weapon = a;

    }

    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -NOPEUS);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, NOPEUS);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);

        Keyboard.Listen(Key.Space, ButtonState.Pressed, Alusta, "Pelaaja hyppää");
        Keyboard.Listen(Key.P, ButtonState.Pressed, Soita, "Pelaaja hyppää");
        Keyboard.Listen(Key.O, ButtonState.Pressed, Pause, "Pelaaja hyppää");

        Keyboard.Listen(Key.W, ButtonState.Down, delegate { l.TextScale += l.TextScale * 0.1; }, null);
        Keyboard.Listen(Key.S, ButtonState.Down, delegate { l.TextScale -= l.TextScale * 0.1; }, null);

        ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");

        ControllerOne.Listen(Button.DPadLeft, ButtonState.Down, Liikuta, "Pelaaja liikkuu vasemmalle", pelaaja1, -NOPEUS);
        ControllerOne.Listen(Button.DPadRight, ButtonState.Down, Liikuta, "Pelaaja liikkuu oikealle", pelaaja1, NOPEUS);
        ControllerOne.Listen(Button.A, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");

        Mouse.Listen(MouseButton.Left, ButtonState.Pressed, LaitaPalikka, null);

        Mouse.ListenMovement(0.1, Tahtaa, "Tähtää aseella");
        Mouse.Listen(MouseButton.Left, ButtonState.Down, AmmuAseella, "ammu", pelaaja1.Weapon);
    }

    private void Pause()
    {
        MediaPlayer.Pause();
        Timer.SingleShot(1, () => MediaPlayer.Resume());
    }

    private void Soita()
    {
        MediaPlayer.Play("maali");
        MediaPlayer.IsRepeating = true;
    }

    private void Alusta()
    {
        for (int i = 0; i < 100; i++)
        {
            PhysicsObject p = new PhysicsObject(10, 10);
            p.Position = RandomGen.NextVector(-10, 10);
            Add(p);
        }

        //ClearAll();
        //A();

        //MessageDisplay = new MessageDisplay();
        //MessageDisplay.BackgroundColor = Color.LightGray;
        //Add(MessageDisplay);
    }

    private void LaitaPalikka()
    {
        PhysicsObject p = new PhysicsObject(5, 5);
        p.Position = Mouse.PositionOnWorld;
        Add(p);
    }

    void Tahtaa(AnalogState hiirenLiike)
    {
        Vector suunta = (Mouse.PositionOnWorld - pelaaja1.Weapon.AbsolutePosition).Normalize();
        pelaaja1.Weapon.Angle = suunta.Angle;
    }

    void AmmuAseella(Weapon ase)
    {
        PhysicsObject ammus = ase.Shoot();

        if (ammus != null)
        {
            //ammus.Size *= 3;
            //ammus.Image = ...
            //ammus.MaximumLifetime = TimeSpan.FromSeconds(2.0);
        }
    }

    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    private void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }

    private void TormaaTahteen(PhysicsObject hahmo, PhysicsObject tahti)
    {
        maaliAani.Play();
        MessageDisplay.Add("Keräsit tähden!");
        tahti.Destroy();
        pistelaskuri.Value++;
    }
}
