﻿using Microsoft.Xna.Framework.Audio;
using System;

namespace Jypeli
{
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

        internal Sound( SoundEffectInstance s )
        {
            effectInstance = s;
        }

        public void Play()
        {
            try
            {
                effectInstance.Play();
            }
#if !WINDOWS_STOREAPP
            catch (NullReferenceException)
            {
                Console.Error.WriteLine("Null reference exception trying to play a sound, disabling audio");
                Game.DisableAudio();
            }
#endif
            finally
            {
            }
        }

        public void Resume()
        {
            effectInstance.Resume();
        }

        public void Stop()
        {
            effectInstance.Stop();
        }

        public void Pause()
        {
            effectInstance.Pause();
        }
    }
}
