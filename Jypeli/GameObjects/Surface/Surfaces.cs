using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Jypeli
{
    /// <summary>
    /// Kentän reunat.
    /// </summary>
    public struct Surfaces : IEnumerable<Surface>
    {
        internal Surface l, r, t, b;

        /// <summary>
        /// Vasen reuna.
        /// </summary>
        public Surface Left
        {
            get { return l; }
        }

        /// <summary>
        /// Oikea reuna.
        /// </summary>
        public Surface Right
        {
            get { return r; }
        }

        /// <summary>
        /// Yläreuna.
        /// </summary>
        public Surface Top
        {
            get { return t; }
        }

        /// <summary>
        /// Alareuna.
        /// </summary>
        public Surface Bottom
        {
            get { return b; }
        }

        /// <summary>
        /// Enumeraattori pinnoille.
        /// Mahdollistaa foreach-lauseen käytön.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Surface> GetEnumerator()
        {
            if ( Left != null ) yield return Left;
            if ( Right != null ) yield return Right;
            if ( Top != null ) yield return Top;
            if ( Bottom != null ) yield return Bottom;
        }

        /// <summary>
        /// Enumeraattori pinnoille.
        /// Mahdollistaa foreach-lauseen käytön.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if ( Left != null ) yield return Left;
            if ( Right != null ) yield return Right;
            if ( Top != null ) yield return Top;
            if ( Bottom != null ) yield return Bottom;
        }

        /// <summary>
        /// Valikoiva enumeraattori pinnoille.
        /// </summary>
        /// <param name="directions">Mukaan laskettavien reunojen suunnat</param>
        /// <returns></returns>
        public IEnumerable<Surface> Get( params Direction[] directions )
        {
            if ( directions.Contains(Direction.Left) && Left != null ) yield return Left;
            if ( directions.Contains(Direction.Right) && Right != null ) yield return Right;
            if ( directions.Contains(Direction.Up) && Top != null ) yield return Top;
            if ( directions.Contains(Direction.Down) && Bottom != null ) yield return Bottom;
        }
    }
}
