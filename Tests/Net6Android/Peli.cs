using System;
using System.Collections.Generic;
using Android.Content.Res;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class Tasohyppelypeli : PhysicsGame
{

    PhysicsObject p1;
    PhysicsObject p2;

    public override void Begin()
    {
        Level.BackgroundColor = Color.DarkGray;
        //p1 = new PhysicsObject(5, 50);
        //Add(p1);
        //
        //p2 = new PhysicsObject(100, 50);
        //p2.Body.Velocity = new Jypeli.Vector(-120, 10);
        //p2.Position = new Jypeli.Vector(400, 0);
        //Add(p2);

        GameObject g1 = new GameObject(20, 20);
        Add(g1);

        GameObject g2 = new GameObject(20, 20);
        g2.Position = new Jypeli.Vector(50, 50);
        g2.Color = Color.Orange;
        g2.Shape = Shape.Star;
        g1.Add(g2);

        GameObject g3 = new GameObject(20, 20);
        g3.Position = new Jypeli.Vector(-50, -50);
        g3.Image = LoadImage("norsu.png");
        g2.Add(g3);

        Label l = new Label();
        Add(l);


        Label l2 = new Label();
        l2.Y = 50;
        Add(l2);

        //ExplosionSystem ex = new ExplosionSystem(LoadImage("norsu"), 2000);
        //Add(ex);



        //Layers[0].Grid = new Grid();
        //Layers[0].Grid.CellSize = new Vector(5, 5);

        //Surfaces ss = Level.CreateBorders();
        //AddCollisionHandler<PhysicsObject, PhysicsObject>(p1, delegate { MessageDisplay.Add("kissa"); });

        //ControllerOne.ListenAnalog(AnalogControl.RightStick, 0, (a) => l.Text = a.StateVector.ToString(), null);
        //ControllerOne.ListenAnalog(AnalogControl.LeftStick, 0, (a) => l2.Text = a.StateVector.ToString(), null);

        //ControllerOne.Listen(Button.A, ButtonState.Down, () => ControllerOne.Vibrate(1, 0, 0, 0, 1), null);

        //ControllerOne.ListenAnalog(AnalogControl.LeftStick, 0, (a) => g1.Position = a.StateVector * 200, null);
        //ControllerOne.ListenAnalog(AnalogControl.RightStick, 0, (a) => g1.Angle = a.StateVector.Angle, null);

        Keyboard.ListenArrows(ButtonState.Down, (v) => Camera.Position += v, null);
        //Keyboard.Listen(Key.Space, ButtonState.Down, () => {
        //    //g1.Image = LoadImage("norsu.png");
        //    ex.AddEffect(Vector.Zero, 10);
        //}, null);
    }

    protected override void Paint(Canvas canvas)
    {
        base.Paint(canvas);
    }

    protected override void Update(Time time)
    {
        base.Update(time);
    }
}
