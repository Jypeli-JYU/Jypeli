using System;

namespace Jypeli
{
    public class Grid
    {
        public Color Color { get; set; }
        public Vector CellSize { get; set; }

        public Grid()
        {
            Color = Color.Green;
            CellSize = new Vector( 10, 10 );
        }

        public Vector SnapToLines( Vector v )
        {
            Vector result;
            result.X = Math.Round( v.X / this.CellSize.X ) * this.CellSize.X;
            result.Y = Math.Round( v.Y / this.CellSize.Y ) * this.CellSize.Y;
            return result;
        }
    }
}
