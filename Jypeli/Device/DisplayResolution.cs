using System;

namespace Jypeli
{
    /// <summary>
    /// Mobiililaitteiden resoluutiovaihtoehdot
    /// </summary>
    public class DisplayResolution : IEquatable<DisplayResolution>
    {
        /// <summary>
        /// Pieni tarkkuus (WVGA, 400 x 240).
        /// WP7-yhteensopivuustila, ei varsinaisesti paranna suorituskykyä.
        /// </summary>
        public static readonly DisplayResolution Small = new DisplayResolution(400, 240);

        /// <summary>
        /// Suuri tarkkuus (WVGA, 800 x 480).
        /// Oletus WP8:lla.
        /// </summary>
        public static readonly DisplayResolution Large = new DisplayResolution(800, 480);

        /// <summary>
        /// HD720-tarkkuus (720p, 1280 x 720).
        /// Ei toimi kaikilla puhelimilla.
        /// </summary>
        public static readonly DisplayResolution HD720 = new DisplayResolution(1280, 720);

        /// <summary>
        /// HD1080-tarkkuus (1080p, 1920 x 1080).
        /// Ei toimi kaikilla puhelimilla.
        /// </summary>
        public static readonly DisplayResolution HD1080 = new DisplayResolution(1920, 1080);


        /// <summary>
        /// Näytön leveys pikseleinä.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Näytön korkeus pikseleinä.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Näytön kuvasuhde.
        /// </summary>
        public double AspectRatio
        {
            get { return Width / Height; }
        }

        /// <summary>
        /// Näytön pikselien määrä.
        /// </summary>
        public int PixelCount
        {
            get { return Width * Height; }
        }

        /// <summary>
        /// Alustaa uuden näyttöresoluution.
        /// </summary>
        /// <param name="width">Leveys</param>
        /// <param name="height">Korkeus</param>
        public DisplayResolution(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height must be positive integers");

            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Hajautuskoodi
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (Width << 16) | Height;
        }

        /// <summary>
        /// Onko ruutujen koot samat
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DisplayResolution);
        }

        /// <summary>
        /// Onko ruutujen koot samat
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DisplayResolution other)
        {
            if (other == null)
                return false;

            return other.Width == this.Width && other.Height == this.Height;
        }

        /// <summary>
        /// Onko ruutujen koot samat
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(DisplayResolution a, DisplayResolution b)
        {
            if (ReferenceEquals(a, null))
                return ReferenceEquals(b, null);
            if (ReferenceEquals(b, null))
                return false;

            return a.Width == b.Width && a.Height == b.Height;
        }

        /// <summary>
        /// Onko ruutujen koot eri
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(DisplayResolution a, DisplayResolution b)
        {
            if (ReferenceEquals(a, null))
                return !ReferenceEquals(b, null);
            if (ReferenceEquals(b, null))
                return true;

            return a.Width != b.Width || a.Height != b.Height;
        }
    }
}
