using System;

#if JYPELI
using Microsoft.Xna.Framework;
#endif


namespace Jypeli
{
    /// <summary>
    /// Sisältää tiedon ajasta, joka on kulunut pelin alusta ja viime päivityksestä.
    /// </summary>
    public struct Time
    {
        public static readonly Time Zero = new Time();

        private TimeSpan _upd;
        private TimeSpan _start;

        /// <summary>
        /// Aika joka on kulunut viime päivityksestä.
        /// </summary>
        public TimeSpan SinceLastUpdate
        {
            get { return _upd; }
        }

        /// <summary>
        /// Aika joka on kulunut pelin alusta.
        /// </summary>
        public TimeSpan SinceStartOfGame
        {
            get { return _start; }
        }

        /// <summary>
        /// Rakentaa ajan kahdesta TimeSpan-oliosta.
        /// </summary>
        /// <param name="fromUpdate">Päivityksestä kulunut aika</param>
        /// <param name="fromStart">Pelin alusta kulunut aika</param>
        internal Time( TimeSpan fromUpdate, TimeSpan fromStart )
        {
            _upd = fromUpdate;
            _start = fromStart;
        }

#if JYPELI

        /// <summary>
        /// Rakentaa ajan XNA:n GameTimestä.
        /// </summary>
        /// <param name="gameTime">XNA:n vastaava olio</param>
        internal Time( GameTime gameTime )
            : this( gameTime.ElapsedGameTime, gameTime.TotalGameTime )
        {
        }

        internal void Advance( GameTime gameTime )
        {
            _upd = gameTime.ElapsedGameTime;
            _start += gameTime.ElapsedGameTime;
        }
#endif

    }
}
