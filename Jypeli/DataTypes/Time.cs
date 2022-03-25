using System;


namespace Jypeli
{
    /// <summary>
    /// Sisältää tiedon ajasta, joka on kulunut pelin alusta ja viime päivityksestä.
    /// </summary>
    public struct Time
    {
        /// <summary>
        /// Nolla-aika
        /// </summary>
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
        internal Time(TimeSpan fromUpdate, TimeSpan fromStart)
        {
            _upd = fromUpdate;
            _start = fromStart;
        }

        internal void Advance(double dt)
        {
            _upd = TimeSpan.FromSeconds(dt);
            _start += _upd;
        }
    }
}
