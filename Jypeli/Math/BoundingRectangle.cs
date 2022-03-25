using System;

namespace Jypeli
{
    /// <summary>
    /// Suorakaide
    /// </summary>
    public struct BoundingRectangle
    {
        /// <summary>
        /// Suorakaiteen keskipisteen X
        /// </summary>
        public double X;
        /// <summary>
        /// Suorakaiteen keskipisteen Y
        /// </summary>
        public double Y;
        /// <summary>
        /// Suorakaiteen leveyse
        /// </summary>
        public double Width;
        /// <summary>
        /// Suorakaiteen korkeus
        /// </summary>
        public double Height;

        /// <summary>
        /// Suorakaiteen vasemman reunan X
        /// </summary>
        public double Left { get { return X - Width / 2; } }
        /// <summary>
        /// Suorakaiteen oikean reunan X
        /// </summary>
        public double Right { get { return X + Width / 2; } }
        /// <summary>
        /// Suorakaiteen alareunen Y
        /// </summary>
        public double Bottom { get { return Y - Height / 2; } }
        /// <summary>
        /// Suorakaiteen yläreunan Y
        /// </summary>
        public double Top { get { return Y + Height / 2; } }

        /// <summary>
        /// Suorakaiteen keskipiste
        /// </summary>
        public Vector Position
        {
            get { return new Vector(X, Y); }
            set { X = value.X; Y = value.Y; }
        }


        /// <summary>
        /// Suorakaiteen koko
        /// </summary>
        public Vector Size
        {
            get { return new Vector(Width, Height); }
            set { Width = value.X; Height = value.Y; }
        }

        /// <summary>
        /// Suorakaiteen vasemman ylänurkan koordinaatti
        /// </summary>
        public Vector TopLeft
        {
            get { return new Vector(X - Width / 2, Y + Height / 2); }
        }

        /// <summary>
        /// Suorakaiteen oikean alanurkan koordinaatti
        /// </summary>
        public Vector BottomRight
        {
            get { return new Vector(X + Width / 2, Y - Height / 2); }
        }

        /// <summary>
        /// Suorakaiteen vasemman alanurkan koordinaatti
        /// </summary>
        public Vector BottomLeft
        {
            get { return new Vector(X - Width / 2, Y - Height / 2); }
        }

        /// <summary>
        /// Suorakaiteen oikean ylönurkan koordinaatti
        /// </summary>
        public Vector TopRight
        {
            get { return new Vector(X + Width / 2, Y + Height / 2); }
        }

        /// <summary>
        /// Suorakaiteen lävistäjän pituus
        /// </summary>
        public double DiagonalLength
        {
            get { return Math.Sqrt(Width * Width + Height * Height); }
        }


        /// <summary>
        /// Alustetaan suorakaide keskipisteen ja koon perusteella
        /// </summary>
        /// <param name="x">keskipisteen x</param>
        /// <param name="y">keskipisteen y</param>
        /// <param name="w">leveys</param>
        /// <param name="h">korkeus</param>
        public BoundingRectangle(double x, double y, double w, double h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        /// <summary>
        /// Alustetaan suorakaiden nurkkapisteiden avulla
        /// </summary>
        /// <param name="topLeft">vasen ylänurkka</param>
        /// <param name="bottomRight">oikea alanurkka</param>
        public BoundingRectangle(Vector topLeft, Vector bottomRight)
        {
            Width = bottomRight.X - topLeft.X;
            Height = topLeft.Y - bottomRight.Y;
            X = topLeft.X + Width / 2;
            Y = bottomRight.Y + Height / 2;
        }


        /// <summary>
        /// Tutkitaan onko piste suorakaiteen sisällä
        /// </summary>
        /// <param name="point">tutkittavan pisteen koordinaatti</param>
        /// <returns>true jos sisällä, muuten false</returns>
        public bool IsInside(Vector point)
        {
            return point.X >= Left && point.X <= Right && point.Y >= Bottom && point.Y <= Top;
        }

        /// <summary>
        /// Leikkaavatko suorakaiteet toisiaan
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
		public static bool Intersects(BoundingRectangle a, BoundingRectangle b)
        {
            bool xcond = a.Right >= b.Left && a.Left <= b.Right || b.Right >= a.Left && b.Left <= a.Right;
            bool ycond = a.Top >= b.Bottom && a.Bottom <= b.Top || b.Top >= a.Bottom && b.Bottom <= a.Top;
            return xcond && ycond;
        }

        /// <summary>
        /// Suorakaiteiden leikkaus
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Leikkaus</returns>
        public static BoundingRectangle GetIntersection(BoundingRectangle a, BoundingRectangle b)
        {
            double left = Math.Max(a.Left, b.Left);
            double top = Math.Min(a.Top, b.Top);
            double right = Math.Min(a.Right, b.Right);
            double bot = Math.Max(a.Bottom, b.Bottom);

            double iw = right - left;
            double ih = top - bot;

            return new BoundingRectangle(left + iw / 2, bot + ih / 2, iw, ih);
        }

        /// <summary>
        /// Mihin suuntaan leikkaus on suhteessa suorakaiteen keskipistettä
        /// </summary>
        /// <param name="rect">Suorakulmio</param>
        /// <param name="intersection">Leikkaus</param>
        /// <returns>Suunta</returns>
        public static Direction GetIntersectionDirection(BoundingRectangle rect, BoundingRectangle intersection)
        {
            double dx = rect.X - intersection.X;
            double dy = rect.Y - intersection.Y;
            double adx = Math.Abs(dx);
            double ady = Math.Abs(dy);

            if (adx > ady)
            {
                if (dx < 0)
                    return Direction.Left;
                return Direction.Right;
            }

            if (dy < 0)
                return Direction.Down;
            return Direction.Up;
        }
    }
}
