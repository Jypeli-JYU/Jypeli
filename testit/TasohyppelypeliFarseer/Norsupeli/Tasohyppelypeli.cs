using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Tasohyppelypeli : PhysicsGame
{

    PhysicsObject p1;
    PhysicsObject p2;

    public override void Begin()
    {
        Gravity = new Vector(0, -0);

        p1 = new PhysicsObject(5, 50);
        Add(p1);

        p2 = new PhysicsObject(100, 50);
        p2.Body.Velocity = new Vector(-120, 10);
        p2.Position = new Vector(400, 0);
        Add(p2);
    }

    protected override void Update(Time time)
    {
        base.Update(time);
    }
}
