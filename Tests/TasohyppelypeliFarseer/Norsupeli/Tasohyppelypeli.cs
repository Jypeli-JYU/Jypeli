using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System.Numerics;

public class Tasohyppelypeli : PhysicsGame
{

    PhysicsObject p1;
    PhysicsObject p2;

    public override void Begin()
    {
        Gravity = new Vector2(0, -0);

        p1 = new PhysicsObject(5, 50);
        Add(p1);

        p2 = new PhysicsObject(100, 50);
        p2.Body.Velocity = new Jypeli.Vector(-120, 10);
        p2.Position = new Jypeli.Vector(400, 0);
        Add(p2);

        Surfaces ss = Level.CreateBorders();
        AddCollisionHandler<PhysicsObject, PhysicsObject>(p1, delegate { MessageDisplay.Add("kissa"); });
    }

    protected override void Update(Time time)
    {
        base.Update(time);
    }
}
