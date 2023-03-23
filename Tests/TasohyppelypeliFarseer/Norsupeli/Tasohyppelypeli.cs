using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Tasohyppelypeli : PhysicsGame
{
    public override void Begin()
    {
        /*
        var palkki = new PhysicsObject(10, 500);
        //palkki.MakeStatic();
        palkki.X = 200;
        palkki.Mass = 10000;
        Add(palkki);

        Keyboard.Listen(Key.Space, ButtonState.Pressed, () =>
        {
            var p = new PhysicsObject(10, 10);
            p.Velocity = new Vector(10000, 0);
            Add(p);
        }, null);
        */
        Gravity = new Vector(0, -100);
        Level.BackgroundColor = Color.Black;
        Level.CreateBorders();
    }

    protected override void Update(Time time)
    {
        for (int i = 0; i < 20; i++)
        {
            var p = new GameObject(20, 20);
            p.Position = Level.GetRandomPosition();
            //p.Shape = Shape.Circle;
            p.Color = RandomGen.NextDarkColor();
            Add(p);
        }
        base.Update(time);
    }
}
