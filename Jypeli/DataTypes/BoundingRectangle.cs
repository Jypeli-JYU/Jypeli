using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

namespace Jypeli
{
    public struct BoundingRectangle
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;

        public double Left { get { return X - Width / 2; } }
        public double Right { get { return X + Width / 2; } }
        public double Bottom { get { return Y - Height / 2; } }
        public double Top { get { return Y + Height / 2; } }

        public Vector Position
        {
            get { return new Vector( X, Y ); }
            set { X = value.X; Y = value.Y; }
        }

        public Vector Size
        {
            get { return new Vector( Width, Height ); }
            set { Width = value.X; Height = value.Y; }
        }

        public Vector TopLeft
        {
            get { return new Vector( X - Width / 2, Y + Height / 2 ); }
        }

        public Vector BottomRight
        {
            get { return new Vector( X + Width / 2, Y - Height / 2 ); }
        }

        public double DiagonalLength
        {
            get { return Math.Sqrt( Width * Width + Height * Height ); }
        }

        public BoundingRectangle(double x, double y, double w, double h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public BoundingRectangle( Vector topLeft, Vector bottomRight )
        {
            Width = bottomRight.X - topLeft.X;
            Height = topLeft.Y - bottomRight.Y;
            X = topLeft.X + Width / 2;
            Y = bottomRight.Y + Height / 2;
        }

        public bool IsInside( Vector point )
        {
            return point.X >= Left && point.X <= Right && point.Y >= Bottom && point.Y <= Top;
        }

		public static bool Intersects( BoundingRectangle a, BoundingRectangle b )
        {
            bool xcond = a.Right >= b.Left && a.Left <= b.Right || b.Right >= a.Left && b.Left <= a.Right;
            bool ycond = a.Top >= b.Bottom && a.Bottom <= b.Top || b.Top >= a.Bottom && b.Bottom <= a.Top;
            return xcond && ycond;
        }

        public static BoundingRectangle GetIntersection( BoundingRectangle a, BoundingRectangle b )
        {
            double left = Math.Max( a.Left, b.Left );
            double top = Math.Min( a.Top, b.Top );
            double right = Math.Min( a.Right, b.Right );
            double bot = Math.Max( a.Bottom, b.Bottom );

            double iw = right - left;
            double ih = top - bot;

            return new BoundingRectangle( left + iw / 2, bot + ih / 2, iw, ih );
        }

        public static Direction GetIntersectionDirection( BoundingRectangle rect, BoundingRectangle intersection )
        {
            double dx = rect.X - intersection.X;
            double dy = rect.Y - intersection.Y;
            double adx = Math.Abs( dx );
            double ady = Math.Abs( dy );

            if ( adx > ady )
            {
                if ( dx < 0 ) return Direction.Left;
                return Direction.Right;
            }

            if ( dy < 0 ) return Direction.Down;
            return Direction.Up;
        }
    }
}
