using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class EmptyTest : PhysicsGame
{
    public override void Begin()
    {
        PhysicsObject p = new PhysicsObject(50, 50);
        Add(p);

        List<Vector> path = new List<Vector>();
        path.Add(new Vector(0, 0));
        path.Add(new Vector(100, 100));
        path.Add(new Vector(-100, 100));
        path.Add(new Vector(0, -100));

        PathFollowerBrain pfb = new PathFollowerBrain(path);
        pfb.Speed = 500;
        p.Brain = pfb;
        pfb.Loop = true;
        pfb.Active = true;
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit,
"Lopeta peli");
    }
}