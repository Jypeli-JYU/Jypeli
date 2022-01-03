using System;
using Jypeli.Audio.OpenAL;

namespace Jypeli
{
    public enum SoundState
    {
        Playing,

    }

    /// <summary>
    /// Yleinen äänen toistamiseen käytettävä luokka.
    /// Tällä ei ole kovin suuria eroja <c>SoundEffect</c>-luokan kanssa.
    /// </summary>
    public class Sound // TODO: Mitä kannattaisi tehdä tämän ja SoundEffectin suhteen. Näiden molempien olemassaolo on hiemna outoa.
    {
        private SoundEffect soundeffect;
        /// <summary>
        /// Jos <c>true</c>, ääntä soitetaan toistuvasti.
        /// </summary>
        public bool IsLooped 
        {
            get { return OpenAL.GetLooping(soundeffect.handle); }
            set { OpenAL.SetLooping(soundeffect.handle, value); }
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
            get { return OpenAL.GetPan(soundeffect.handle); }
            set { OpenAL.SetPan(soundeffect.handle, value); }
        }

        /// <summary>
        /// Äänenvoimakkuus välillä 0.0 - 1.0.
        /// </summary>
        public double Volume
        {
            get { return OpenAL.GetVolume(soundeffect.handle); }
            set { OpenAL.SetVolume(soundeffect.handle, value); }
        }

        /// <summary>
        /// Äänenkorkeus välillä -1.0 - 1.0.
        /// </summary>
        /// <remarks>
        /// -1.0 on oktaavin alempana, 1.0 oktaavin ylempänä.
        /// </remarks>
        public double Pitch
        {
            get { return OpenAL.GetPitch(soundeffect.handle); }
            set { OpenAL.SetPitch(soundeffect.handle, value); }
        }

        /// <summary>
        /// Äänen tila.
        /// </summary>
        /// <returns></returns>
        public SoundState State
        {
            get => SoundState.Playing;
        }

        internal Sound(SoundEffect s)
        {
            soundeffect = s;
        }

        /// <summary>
        /// Toistaa äänen
        /// </summary>
        /// <param name="retries"></param>
        public void Play(int retries = 3)
        {
            try
            {
                OpenAL.Play(soundeffect.handle);
            }
            catch (NullReferenceException)
            {
                Console.Error.WriteLine("Null reference exception trying to play a sound, disabling audio");
                Game.DisableAudio();
            }
            catch (InvalidOperationException)
            {
                // Workaround: Sometimes on Android an InvalidOperationException is thrown when playing a sound
                // Trying again seems to work; if not, no sound is better than crashing the game
                if (retries > 0)
                    Play(retries - 1);
            }
        }

        /// <summary>
        /// Jatkaa äänen toistamista
        /// </summary>
        public void Resume()
        {
            OpenAL.Play(soundeffect.handle);
        }

        /// <summary>
        /// Pysäyttää äänen toistamisen
        /// </summary>
        public void Stop()
        {
            OpenAL.Stop(soundeffect.handle);
        }

        /// <summary>
        /// Keskeyttää äänen toistamisen
        /// </summary>
        public void Pause()
        {
            OpenAL.Pause(soundeffect.handle);
        }
    }
}
