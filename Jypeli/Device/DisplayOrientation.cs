using System;

namespace Jypeli
{
    /// <summary>
    /// Näytön asemointi.
    /// </summary>
    public class DisplayOrientation : IEquatable<DisplayOrientation>
    {
        /// <summary>
        /// Vaakasuuntainen, vasemmalle käännetty.
        /// </summary>
        public static DisplayOrientation LandscapeLeft = new DisplayOrientation( 1, 0 );

        /// <summary>
        /// Vaakasuuntainen, oikealle käännetty.
        /// </summary>
        public static DisplayOrientation LandscapeRight = new DisplayOrientation( -1, 0 );

        /// <summary>
        /// Vaakasuuntainen.
        /// </summary>
        public static DisplayOrientation Landscape = LandscapeLeft;

        /// <summary>
        /// Pystysuuntainen.
        /// </summary>
        public static DisplayOrientation Portrait = new DisplayOrientation( 0, 1 );

        /// <summary>
        /// Pystysuuntainen, ylösalaisin käännetty.
        /// </summary>
        public static DisplayOrientation PortraitInverse = new DisplayOrientation( 0, -1 );

        /// <summary>
        /// X-kerroin: 1 jos vaakasuora vasemmalle, -1 jos vaakasuora oikealle, 0 jos pystysuora.
        /// </summary>
        internal readonly int Xmul;

        /// <summary>
        /// Y-kerroin: 1 jos pystysuora, -1 jos ylösalaisin, 0 jos vaakasuora.
        /// </summary>
        internal readonly int Ymul;

        internal DisplayOrientation(int xmul, int ymul)
        {
            this.Xmul = xmul;
            this.Ymul = ymul;
        }

        /// <summary>
        /// Hajautuskoodi
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Xmul * 2 + Ymul;
        }

        /// <summary>
        /// Onko näytön asemointi sama
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals( object obj )
        {
            return this.Equals( obj as DisplayOrientation );
        }

        /// <summary>
        /// Onko näytön asemointi sama
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals( DisplayOrientation other )
        {
            if ( other == null )
                return false;

            return other.Xmul == this.Xmul && other.Ymul == this.Ymul;
        }

        /// <summary>
        /// Onko näytön asemoinnit samat
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==( DisplayOrientation a, DisplayOrientation b )
        {
            if ( ReferenceEquals( a, null ) )
                return ReferenceEquals( b, null );
            if ( ReferenceEquals( b, null ) )
                return false;

            return a.Xmul == b.Xmul && a.Ymul == b.Ymul;
        }

        /// <summary>
        /// Onko näytön asemoinnit eri
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=( DisplayOrientation a, DisplayOrientation b )
        {
            if ( ReferenceEquals( a, null ) )
                return !ReferenceEquals( b, null );
            if ( ReferenceEquals( b, null ) )
                return true;

            return a.Xmul != b.Xmul || a.Ymul != b.Ymul;
        }
    }
}
