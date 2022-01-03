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
        /// 0 on keskellä, negatiivinen menee vasemmalle ja positiivinen oikealle.
        /// </summary>
        /// <remarks>
        /// Toistaiseksi tämän toimiminen vaatii monoäänen.
        /// Stereoäänellä tämä ei tee mitään.
        /// </remarks>
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
        /// Äänenkorkeus, -1.0 on oktaavin alempana, 1.0 oktaavin ylempänä.
        /// </summary>
        public double Pitch
        {
            get { return PitchFromAL(OpenAL.GetPitch(soundeffect.handle)); }
            set { OpenAL.SetPitch(soundeffect.handle,PitchToAL(value)); }
        }

        /*
         * OpenAL haluaa korkeuden olevan välillä 0...inf.
         * Vakiona korkeus on 1.
         * Korkeuden puolittaminen tarkoittaa yhtä oktaavia alaspäin.
         * Kertominen kahdella taas nostaa yhdellä oktaavilla.
         * 
         * Jypeli historiallisistä syistä (XNA) taas käyttää eri järjestelmää,
         * ja näiden välille piti tehdä pieni muunnos.
         */

        private static double PitchToAL(double value)
        {
            return Math.Pow(2, value);
        }

        private static double PitchFromAL(double value)
        {
            return Math.Log2(value);
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
