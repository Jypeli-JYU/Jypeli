#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;

namespace Jypeli
{
    /// <summary>
    /// Ajastin, joka voidaan asettaa laukaisemaan tapahtumia tietyin väliajoin.
    /// </summary>
    public sealed class Timer
    {
        #region Events

        /// <summary>
        /// Tapahtuu väliajoin.
        /// </summary>
        public event Action Timeout;

        #endregion

        private static SynchronousList<Timer> timers = new SynchronousList<Timer>();

        private bool _enabled = false;

        private TimeSpan timeToTrigger = TimeSpan.MaxValue;
        private TimeSpan trigInterval = new TimeSpan( 0, 0, 1 );
        private TimeSpan savedTrigger = TimeSpan.Zero;
        
        private TimeSpan timeToCount = TimeSpan.MaxValue;
        private TimeSpan countInterval = TimeSpan.FromMilliseconds( 10 );
        private TimeSpan savedCount = TimeSpan.Zero;

        #region Properties

        /// <summary>
        /// Ajastin päällä/pois päältä.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if ( value == _enabled ) return;
                if ( value )
                    Start();
                else
                    Stop();
            }
        }

        /// <summary>
        /// Ajastin ei pysähdy vaikka peli pysäytettäisiin.
        /// </summary>
        public bool IgnorePause { get; set; }

        /// <summary>
        /// Aika sekunneissa, jonka välein <c>TimeOut</c> tapahtuu.
        /// </summary>
        public double Interval
        {
            get { return trigInterval.TotalSeconds; }
            set
            {
                if ( value <= 0 ) throw new ArgumentException( "Interval cannot be zero or negative!" );

                try
                {
                    trigInterval = TimeSpan.FromSeconds(value);
                }
                catch (OverflowException)
                {
                    // Workaround for overflow when converting TimeSpan.MaxValue to seconds and back again
                    trigInterval = TimeSpan.MaxValue;
                }

                if ( Enabled )
                    timeToTrigger = trigInterval;
            }
        }

        /// <summary>
        /// Menossa oleva hetki nollasta väliaikaan (<c>Interval</c>).
        /// </summary>
        public double CurrentTime
        {
            get { return SecondCounter.Value; }
            set { SecondCounter.Value = value; }
        }

        // /// <summary>
        // /// Ajastimen käynnistysaika.
        // /// </summary>
        //public TimeSpan StartTime { get; private set; }

        /// <summary>
        /// Sekuntilaskuri. Voidaan sitoa näyttöihin.
        /// </summary>
        public DoubleMeter SecondCounter { get; private set; }

        /// <summary>
        /// Kuinka monta sekuntia sekuntilaskuri laskee yhden sekunnin aikana.
        /// Oletus on 1. Arvolla 2 laskuri laskee tuplanopeudella, arvolla -1 taaksepäin jne.
        /// </summary>
        public double SecondCounterStep { get; set; }

        /// <summary>
        /// Määrää, kuinka monta kertaa tapahtuma suoritetaan.
        /// Kun tapahtumaa on suoritettu tarpeeksi, <c>Enabled</c> saa automaattisesti
        /// arvon <c>false</c>, jolloin ajastin pysähtyy.
        /// Kun laskuri nollataan, myös Times palautuu oletusarvoonsa.
        /// Huomaa, että <c>TimesLimited</c> tulee olla <c>true</c>, että arvo otetaan huomioon.
        /// </summary>
        /// <see cref="TimesLimited"/>
        public IntMeter Times { get; private set; }

        /// <summary>
        /// Ajastimen suorituskertojen rajoitus päälle/pois.
        /// </summary>
        /// <see cref="Times"/>
        public bool TimesLimited
        {
            get { return Times.MinValue == 0; }
            set
            {
                Times.MinValue = value ? 0 : 1;
            }
        }

        /// <summary>
        /// Vapaasti asetettava muuttuja.
        /// Arvo ei muutu, jos sitä ei muuteta.
        /// </summary>
        public object Tag { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Alustaa uuden ajastinluokan.
        /// </summary>
        public Timer()
        {
            SecondCounter = new DoubleMeter( 0 );
            Times = new IntMeter( 1 );
            Times.MinValue = 1;
            Times.MaxValue = 1;
            Enabled = false;
        }

        /// <summary>
        /// Alustaa uuden ajastimen ja asettaa sille ajan sekunneissa, jonka 
        /// välein <c>TimeOut</c> tapahtuu.
        /// </summary>
        /// <param name="interval">Aika sekunneissa.</param>
        public Timer(double interval) : this()
        {
            Interval = interval;
        }

        /// <summary>
        /// Alustaa uuden ajastimen ja asettaa sille tapahtuma-aikavälin sekä
        /// aliohjelman, jota kutsutaan <c>TimeOut</c>:in yhteydessä.
        /// </summary>
        /// <param name="interval">Aika sekunneissa, jonka välein aliohjelmaa kutsutaan.</param>
        /// <param name="onTimeout">Kutsuttava aliohjelma.</param>
        public Timer(double interval, Action onTimeout) : this(interval)
        {
            Timeout += onTimeout;
        }

        /// <summary>
        /// Luo ja käynnistää uuden ajastimen tietyllä tapahtuma-aikavälillä sekä
        /// aliohjelmalla, jota kutsutaan <c>TimeOut</c>:in yhteydessä.
        /// Palauttaa luodun ajastimen.
        /// </summary>
        /// <param name="interval">Aika sekunneissa, jonka välein aliohjelmaa kutsutaan.</param>
        /// <param name="onTimeout">Kutsuttava aliohjelma.</param>
        /// <returns>Ajastimen.</returns>
        public static Timer CreateAndStart(double interval, Action onTimeout)
        {
            Timer t = new Timer(interval, onTimeout);
            t.Start();
            return t;
        }

        /// <summary>
        /// Kutsuu aliohjelmaa <c>onTimeout</c> annetun ajan kuluttua.
        /// Ajastin luodaan automaattisesti.
        /// </summary>
        /// <param name="seconds">Aika sekunteina.</param>
        /// <param name="onTimeout">Kutsuttava aliohjelma.</param>
        public static void SingleShot( double seconds, Action onTimeout )
        {
            Timer t = new Timer();
            t.Interval = seconds;
            t.Timeout += onTimeout;
            t.Start( 1 );
        }

        /// <summary>
        /// Rajoittaa aliohjelman toimintaa niin, että se voidaan suorittaa vain tietyin väliajoin.
        /// </summary>
        /// <param name="action">Toiminta</param>
        /// <param name="seconds">Kuinka monta sekuntia täytyy odottaa ennen seuraavaa suoritusta</param>
        /// <returns>Rajoitettu aliohjelma</returns>
        public static Action Limit( Action action, double seconds )
        {
            Timer limiter = new Timer();
            bool allowInvoke = true;

            limiter.Interval = seconds;
            limiter.LimitTimes( 1 );
            limiter.Timeout += () => allowInvoke = true;

            return delegate
            {
                if ( allowInvoke )
                {
                    allowInvoke = false;
                    action();
                    limiter.Start();
                }
            };
        }
        
        /// <summary>
        /// Käynnistää ajastimen.
        /// </summary>
        public void Start()
        {
            _enabled = true;
            timeToTrigger = trigInterval;
            timeToCount = countInterval;
            timers.Add( this );
        }

        /// <summary> 
        /// Käynnistää ajastimen, rajoittaa suorituskerrat. 
        /// </summary> 
        /// <param name="times">Kuinka monta kertaa tulee ajastintapahtuma.</param> 
        public void Start( int times )
        {
            _enabled = true;
            LimitTimes( times );
            timeToTrigger = trigInterval - savedTrigger;
            timeToCount = countInterval - savedCount;
            timers.Add( this );
        }

        /// <summary>
        /// Pysäyttää ajastimen tallentaen sen tilan.
        /// </summary>
        public void Pause()
        {
            _enabled = false;
            savedTrigger = trigInterval - timeToTrigger;
            savedCount = countInterval - timeToCount;
            timers.Remove( this );
        }

        /// <summary>
        /// Pysäyttää ajastimen ja nollaa sen tilan.
        /// </summary>
        public void Stop()
        {
            _enabled = false;
            savedTrigger = TimeSpan.Zero;
            savedCount = TimeSpan.Zero;
            timers.Remove( this );
        }

        /// <summary>
        /// Nollaa ajastimen tilan.
        /// Myös suorituskerrat nollataan.
        /// </summary>
        public void Reset()
        {
            SecondCounter.Reset();
            Times.Reset();
            timeToTrigger = trigInterval;
        }

        private void LimitTimes( int numTimes )
        {
            TimesLimited = true;
            Times.DefaultValue = numTimes;
            Times.MaxValue = numTimes;
            Times.Value = numTimes;
        }

        /// <summary>
        /// Poistaa kaikki ajastimet.
        /// </summary>
        internal static void ClearAll()
        {
            timers.ForEach(t => { t.Enabled = false; });
            timers.Clear();
        }

        private static void UpdateTimer( Timer timer, TimeSpan dt )
        {
            if ( !timer.Enabled )
                return;

            timer.timeToCount -= dt;

            while ( timer.timeToCount.TotalSeconds <= 0 )
            {
                // Count second counter
                timer.timeToCount += timer.countInterval;
                timer.SecondCounter.Value += timer.countInterval.TotalSeconds;
            }

            if ( timer.Timeout != null || timer.TimesLimited )
            {
                timer.timeToTrigger -= dt;

                while (timer.timeToTrigger.TotalSeconds <= 0)
                {
                    // Trigger timeouts
                    timer.timeToTrigger += timer.trigInterval; // TODO: MessageDisplay.MessageTime = TimeSpan.MaxValue; Aiheuttaa tässä kohdassa overflown
                    if (timer._enabled && timer.Timeout is not null)
                    {
                        if (timer.TimesLimited)
                        {
                            if (--timer.Times.Value <= 0)
                            {
                                // The timer has executed its maximum times, stop it
                                timer.Stop();
                            }
                        }
                        timer.Timeout();
                    }
                }
            }
        }

        internal static void UpdateAll( Time time )
        {
            timers.Update( time );
            timers.ForEach( UpdateTimer, time.SinceLastUpdate );
        }

        internal static void UpdateAll( Time time, Predicate<Timer> isUpdated )
        {
            timers.Update( time );

            foreach ( var timer in timers )
            {
                if ( isUpdated( timer ) )
                    UpdateTimer( timer, time.SinceLastUpdate );
            }
        }

        #endregion
    }
}
