using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static DisplayOrientation PortraitInverse = new DisplayOrientation( 0, 1 );


        private readonly int xmul;
        private readonly int ymul;

        private DisplayOrientation(int xmul, int ymul)
        {
            this.xmul = xmul;
            this.ymul = ymul;
        }

        public override int GetHashCode()
        {
            return xmul * 2 + ymul;
        }

        public override bool Equals( object obj )
        {
            return this.Equals( obj as DisplayOrientation );
        }

        public bool Equals( DisplayOrientation other )
        {
            if ( other == null )
                return false;

            return other.xmul == this.xmul && other.ymul == this.ymul;
        }

        public static bool operator ==( DisplayOrientation a, DisplayOrientation b )
        {
            if ( ReferenceEquals( a, null ) )
                return ReferenceEquals( b, null );
            if ( ReferenceEquals( b, null ) )
                return false;

            return a.xmul == b.xmul && a.ymul == b.ymul;
        }

        public static bool operator !=( DisplayOrientation a, DisplayOrientation b )
        {
            if ( ReferenceEquals( a, null ) )
                return !ReferenceEquals( b, null );
            if ( ReferenceEquals( b, null ) )
                return true;

            return a.xmul != b.xmul || a.ymul != b.ymul;
        }
    }
}
