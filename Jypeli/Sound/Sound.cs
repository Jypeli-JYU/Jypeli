using System;

namespace Jypeli
{
    /// <summary>
    /// Yleinen äänen toistamiseen käytettävä luokka.
    /// Tällä ei ole kovin suuria eroja <c>SoundEffect</c>-luokan kanssa.
    /// </summary>
    public class Sound
    {
        SoundEffectInstance effectInstance;

        static double Clamp( double value, double min, double max )
        {
            return ( value < min ) ? ( min ) : ( ( value > max ) ? ( max ) : ( value ) );
        }

        /// <summary>
        /// Jos <c>true</c>, ääntä soitetaan toistuvasti.
        /// </summary>
        public bool IsLooped
        {
            get { return effectInstance.IsLooped; }
            set { effectInstance.IsLooped = value; }
        }
        
        /// <summary>
        /// Äänen kuuluminen vasemmasta ja oikeasta kaiuttimesta.
        /// Arvot vaihtelevat välillä -1.0 - 1.0 seuraavasti:
        /// -1.0 -> täysin vasemmalla
        /// 0.0 -> keskellä
        /// 1.0 -> täysin oikealla
        /// </summary>
        public double Pan
        {
            get { return effectInstance.Pan; }
            set { effectInstance.Pan = (float)Clamp( value, -1.0, 1.0 ); }
        }

        /// <summary>
        /// Äänenvoimakkuus välillä 0.0 - 1.0.
        /// </summary>
        public double Volume
        {
            get { return effectInstance.Volume; }
            set { effectInstance.Volume = (float)Clamp( value, 0.0, 1.0 ); }
        }

        /// <summary>
        /// Äänenkorkeus välillä -1.0 - 1.0.
        /// </summary>
        /// <remarks>
        /// -1.0 on oktaavin alempana, 1.0 oktaavin ylempänä.
        /// </remarks>
        public double Pitch
        {
            get { return effectInstance.Pitch; }
            set { effectInstance.Pitch = (float)Clamp( value, -1.0, 1.0 ); }
        }

        /// <summary>
        /// Äänen tila.
        /// </summary>
        /// <returns></returns>
        public SoundState State
        {
            get => effectInstance.State;
        }

        internal Sound( SoundEffectInstance s )
        {
            effectInstance = s;
        }

        /// <summary>
        /// Toistaa äänen
        /// </summary>
        /// <param name="retries"></param>
        public void Play( int retries = 3 )
        {
            try
            {
                effectInstance.Play();
            }
            catch ( NullReferenceException )
            {
                Console.Error.WriteLine( "Null reference exception trying to play a sound, disabling audio" );
                Game.DisableAudio();
            }
            catch ( InvalidOperationException )
            {
                // Workaround: Sometimes on Android an InvalidOperationException is thrown when playing a sound
                // Trying again seems to work; if not, no sound is better than crashing the game
                if ( retries > 0 )
                    Play( retries - 1 );
            }
        }

        /// <summary>
        /// Jatkaa äänen toistamista
        /// </summary>
        public void Resume()
        {
            effectInstance.Resume();
        }

        /// <summary>
        /// Pysäyttää äänen toistamisen
        /// </summary>
        public void Stop()
        {
            effectInstance.Stop();
        }

        /// <summary>
        /// Keskeyttää äänen toistamisen
        /// </summary>
        public void Pause()
        {
            effectInstance.Pause();
        }
    }

    public class SoundState
    {
    }
}
