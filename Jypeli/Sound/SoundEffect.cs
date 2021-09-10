using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Jypeli.Audio.OpenAL;

namespace Jypeli
{
    /// <summary>
    /// Ääniefekti. Yhdestä efektistä voi luoda <c>CreateSound</c>-metodilla monta ääntä (<c>Sound</c>),
    /// jotka voivat soida yhtäaikaa. Ääntä ei tarvitse kuitenkaan luoda itse, jos vain kutsuu
    /// <c>Play</c>-metodia.
    /// </summary>
    public class SoundEffect
    {
        private List<Sound> Instances = new List<Sound>();

        private string assetName;
        private Timer posTimer;

        internal uint handle;

        /// <summary>
        /// Tapahtuu kun ääniefekti on toistettu loppuun.
        /// </summary>
        public event Action Finished;

        /// <summary>
        /// Ääniefektin kesto sekunteina.
        /// </summary>
        public TimeSpan Duration { get { return TimeSpan.FromSeconds(OpenAL.GetDuration(handle)); } }

        /// <summary>
        /// Paikka äänessä sekunteina (missä kohtaa toistoa ollaan). Ei voi asettaa.
        /// </summary>
        public DoubleMeter Position { get; set; }

        /// <summary>
        /// Toistetaanko ääntä parhaillaan.
        /// </summary>
        public bool IsPlaying { get; private set; }

        private static string[] soundExtensions = { ".wav" };

        internal SoundEffect(string assetName)
        {
            this.assetName = assetName;
            handle = OpenAL.LoadSound(assetName);
            InitPosition();
        }

        internal SoundEffect(string assetName, Stream stream)
        {
            handle = OpenAL.LoadSound(stream);
            InitPosition();
        }

        internal SoundEffect()
        { }

        private void InitPosition()
        {
            Position = new DoubleMeter(0, 0, OpenAL.GetDuration(handle));
            posTimer = new Timer();
            posTimer.Interval = 0.01;
            posTimer.Timeout += new Action(IncrementPosition);
            Position.UpperLimit += EffectPlayed;
        }

        private void IncrementPosition()
        {
            Position.Value += posTimer.Interval;
        }

        private void EffectPlayed()
        {
            posTimer.Stop();
            Position.Reset();
            Instances.Clear();
            IsPlaying = false;
            if (Finished != null)
                Finished();
        }

        /// <summary>
        /// Luo Sound-tyyppisen olion. Oliolla on ominaisuuksia kuten voimakkuus ja korkeus
        /// joita voidaan muuttaa soiton aikana.
        /// </summary>
        /// <returns></returns>
        public Sound CreateSound()
        {
            try
            {
                return new Sound(Clone());
            }
            catch
            {
                Game.Instance.OnNoAudioHardwareException();
                return null;
            }
        }

        /// <summary>
        /// Soittaa äänen.
        /// </summary>
        /// <returns></returns>
        public bool Play()
        {
            Sound sound = CreateSound();
            if (sound == null)
                return false;

            StartPlaying(sound);
            return true;
        }

        /// <summary>
        /// Soittaa äänen.
        /// </summary>
        /// <param name="volume">Äänenvoimakkuus 0.0 - 1.0</param>
        /// <param name="pitch">Äänen taajuusmuutos. -1.0 = oktaavi alaspäin, 1.0 = oktaavi ylöspäin, 0.0 = normaali.</param>
        /// <param name="pan">Balanssi eli kummasta kaiuttimesta ääni kuuluu enemmän. -1.0 = kokonaan vasemmasta, 1.0 = kokonaan oikeasta, 0.0 = yhtä paljon kummastakin </param>
        /// <returns></returns>
        public bool Play(double volume, double pitch, double pan)
        {
            Sound sound = CreateSound();
            if (sound == null)
                return false;

            sound.Volume = volume;
            sound.Pitch = pitch;
            sound.Pan = pan;

            StartPlaying(sound);
            return true;
        }

        private void StartPlaying(Sound sound)
        {
            try
            {
                sound.Play();
                Instances.Add(sound);
                Position.Reset();
                posTimer.Start();
                IsPlaying = true;
            }
            catch
            {
                // Too many sounds are playing at once
                // Just ignore this for now...
            }
        }

        /// <summary>
        /// Pysäyttää äänen toistamisen.
        /// </summary>
        public void Stop()
        {
            foreach (var sound in Instances)
            {
                sound.Stop();
            }

            EffectPlayed();
        }

        internal SoundEffect Clone()
        {
            var s = new SoundEffect();
            s.handle = OpenAL.Duplicate(handle);
            s.assetName = this.assetName;
            return s;
        }

        /// <summary>
        /// Äänenvoimakkuuden taso 0.0 - 1.0
        /// </summary>
        public static double MasterVolume
        {
            get; set;
        }
    }

}
