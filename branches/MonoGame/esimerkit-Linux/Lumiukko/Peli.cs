using Jypeli;
//using Jypeli.Controls;

public class Peli : Game
{
    public override void Begin()
    {
        /*Camera.ZoomToLevel();

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
        Add( p3 );*/

        Keyboard.Listen( Key.Left, ButtonState.Down, Camera.Move, null, -Vector.UnitX );
        Keyboard.Listen( Key.Right, ButtonState.Down, Camera.Move, null, Vector.UnitX );
        Keyboard.Listen( Key.Up, ButtonState.Down, Camera.Move, null, Vector.UnitY );
        Keyboard.Listen( Key.Down, ButtonState.Down, Camera.Move, null, -Vector.UnitY );
        Keyboard.Listen( Key.Period, ButtonState.Down, Camera.Zoom, null, 1.1 );
        Keyboard.Listen( Key.Comma, ButtonState.Down, Camera.Zoom, null, 0.9 );
        Keyboard.Listen( Key.Escape, ButtonState.Pressed, Exit, "Poistu" );
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

        base.Paint( canvas );
    }
}
