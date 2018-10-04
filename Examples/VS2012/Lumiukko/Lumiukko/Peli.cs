using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Controls;
using Jypeli.Widgets;
using Jypeli.Assets;

public class Peli : PhysicsGame
{
    public override void Begin()
    {
        PhysicsStructure ukko = new PhysicsStructure();

        var pallo1 = new PhysicsObject( 60, 60, Shape.Circle );
        ukko.Add( pallo1 );

        var pallo2 = new PhysicsObject( 40, 40, Shape.Circle );
        pallo2.Bottom = pallo1.Top;
        ukko.Add( pallo2 );

        Gravity = Vector.UnitY * -200;
        Level.CreateBorders();
        Camera.ZoomToLevel();

        Mouse.Listen( MouseButton.Left, ButtonState.Pressed, ValitseOlio, null );
        Mouse.Listen( MouseButton.Left, ButtonState.Down, Liikuta, null );
        Mouse.Listen( MouseButton.Left, ButtonState.Released, IrroitaOlio, null );

        Add( ukko );
    }

    PhysicsObject obj = null;

    void ValitseOlio()
    {
        obj = GetObjectAt( Mouse.PositionOnWorld ) as PhysicsObject;
    }

    void Liikuta()
    {
        if ( obj == null )
            return;

        Vector d = Mouse.PositionOnWorld - obj.AbsolutePosition;
        if ( d == Vector.Zero )
            return;

        obj.Velocity = d;
    }

    void IrroitaOlio()
    {
        obj = null;
    }
}
