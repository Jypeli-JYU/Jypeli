using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class platform : PhysicsGame
{
    // Vakiot
    double movementSpeed = 400.0;
    double jumpSpeed = 550.0;
    int grav = -1600;

    PlatformCharacter player;

    public override void Begin()
    {
        Gravity = new Vector(0, grav);

        CreateLevel();
        Camera.ZoomToLevel();
        AddPlayer();
        AddControls();
    }

    // Kenttä
    void CreateLevel()
    {
        // Reunat ja kamera
        Level.Height = Level.Width * Screen.Height / Screen.Width;
        Level.CreateBorders(false);
        Level.Background.Color = new Color(73, 73, 73);
        Level.Background.FitToLevel();


        // Maataso
        Surface ground = new Surface(Level.Width, 20);
        ground.Y = -158;
        Add(ground);

        // Ramppi Surfacella tehtynä
        Surface ramp = new Surface(185, 20);
        ramp.X = 220;
        ramp.Y = -100;
        ramp.Angle = Angle.FromDegrees(37);
        Add(ramp);

        // Kolmioramppi
        PhysicsObject ramp2 = new PhysicsObject(250, 100);
        ramp2.X = 280;
        ramp2.Y = -100;
        ramp2.Shape = Shape.Triangle;
        ramp2.MakeStatic();
        //Add(ramp2);

        // Kukkulataso
        Surface hill = new Surface(Level.Right - ramp.Right, 20);
        hill.X = hill.Width / 2 + ramp.Right;
        hill.Y = ramp.Top - hill.Height / 2;
        Add(hill);
    }

    // Kontrollit
    void AddControls()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Show help");
        Keyboard.Listen(Key.F1, ButtonState.Released, MessageDisplay.Clear, null);
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Quit game");

        Keyboard.Listen(Key.Left, ButtonState.Down, MovePlayer, "Move left", player, -movementSpeed);
        Keyboard.Listen(Key.Right, ButtonState.Down, MovePlayer, "Move right", player, movementSpeed);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, JumpPlayer, "Jump", player, jumpSpeed);
    }

    // Pelaaja
    void AddPlayer()
    {
        player = new PlatformCharacter(25, 40); // Vaihda esim. (25, 10) ja (25, 100) 
        player.Jump(0);
        player.X = -400; player.Y = 0;
        Add(player);
    }

    // Liikuminen
    void MovePlayer(PlatformCharacter player, double movementSpeed)
    {
        player.Walk(movementSpeed);
    }

    // Hyppääminen
    void JumpPlayer(PlatformCharacter player, double jumpSpeed)
    {
        player.Jump(jumpSpeed);
    }
}
