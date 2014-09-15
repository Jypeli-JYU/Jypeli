﻿using System;
using System.Collections.Generic;
using Jypeli;

public class Peli : Game
{
    Image norsunkuva;
    List<Vector> norsut = new List<Vector>();

    public Peli()
        : base()
    {
        Phone.DisplayOrientation = DisplayOrientation.Landscape;
    }

    public override void Begin()
    {
        //Camera.ZoomFactor = 1;
        //var img = ResourceContent.Load<Microsoft.Xna.Framework.Graphics.Texture2D>( "CannonBall" );
        //norsunkuva = new Image( img );
		//MediaPlayer.Play("AbracaZebra");
        //var sound = ResourceContent.Load<Microsoft.Xna.Framework.Audio.SoundEffect>( "laser" );
        //sound.Play();
        //norsunkuva = LoadImage( "norsu" );

        //Camera.ZoomToLevel();


        GameObject p1 = new GameObject( 200.0, 200.0, Shape.Circle );
        p1.X = 0.0;
        p1.Y = Level.Bottom + 100.0;
        Add( p1 );

        GameObject p2 = new GameObject( 100.0, 100.0, Shape.Circle );
        p2.X = 0.0;
        p2.Y = Level.Bottom + 250.0;
        Add( p2 );

        GameObject p3 = new GameObject( 60.0, 60.0, Shape.Circle );
        p3.X = 0.0;
        p3.Y = Level.Bottom + 330.0;
        Add( p3 );

        //TouchPanel.Listen( ButtonState.Pressed, LisaaNorsu, "Lisää norsu" );
        TouchPanel.Listen( ButtonState.Down, KeskitaNaytto, "Keskitä näyttö" );
    }

    void KeskitaNaytto( Touch touch )
    {
        Camera.Move( touch.PositionOnScreen / 100 );

    }

    void LisaaNorsu( Touch touch )
    {
		//PlaySound( "CannonFire" );
        norsut.Add( touch.PositionOnWorld );
    }

    void PoistaNorsut()
    {
        norsut.Clear();
    }

    protected override void Paint( Canvas canvas )
    {
        canvas.BrushColor = Color.White;
        canvas.DrawLine( canvas.TopLeft, canvas.TopRight );
        canvas.DrawLine( canvas.TopRight, canvas.BottomRight );
        canvas.DrawLine( canvas.BottomRight, canvas.BottomLeft );
        canvas.DrawLine( canvas.BottomLeft, canvas.TopLeft );

        canvas.BrushColor = Color.LightGray;
        canvas.DrawLine( canvas.Left + 50, canvas.Top - 50, canvas.Right - 50, canvas.Top - 50 );
        canvas.DrawLine( canvas.Right - 50, canvas.Top - 50, canvas.Right - 50, canvas.Bottom + 50 );
        canvas.DrawLine( canvas.Right - 50, canvas.Bottom + 50, canvas.Left + 50, canvas.Bottom + 50 );
        canvas.DrawLine( canvas.Left + 50, canvas.Bottom + 50, canvas.Left + 50, canvas.Top - 50 );

        canvas.BrushColor = Color.Teal;
        canvas.DrawLine( canvas.TopLeft, canvas.BottomRight );
        canvas.DrawLine( canvas.TopRight, canvas.BottomLeft );

        canvas.BrushColor = Color.Magenta;
        canvas.DrawLine( 0, -20, 0, 20 );
        canvas.DrawLine( -10, 10, 0, 20 );
        canvas.DrawLine( 0, 20, 10, 10 );

        double radius = 1;
        if ( Mouse.GetButtonState( MouseButton.Left ) == ButtonState.Down ) radius += 10;
        if ( Mouse.GetButtonState( MouseButton.Right ) == ButtonState.Down ) radius += 20;

        for ( int i = 0; i < 10; i++ )
        {
            canvas.BrushColor = RandomGen.NextColor();
            Vector from = Mouse.PositionOnWorld + RandomGen.NextVector( 3, radius );
            Vector to = Mouse.PositionOnWorld + RandomGen.NextVector( 3, radius );
            canvas.DrawLine( from, to );
        }

        foreach ( Vector piste in norsut )
        {
            canvas.DrawImage( piste, norsunkuva );
        }

        base.Paint( canvas );
    }
}