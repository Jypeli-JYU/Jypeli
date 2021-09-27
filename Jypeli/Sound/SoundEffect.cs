using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;

using XnaSoundEffect = Microsoft.Xna.Framework.Audio.SoundEffect;
using System.Collections.Generic;
using System.IO;

namespace Jypeli
{
    /// <summary>
    /// Ääniefekti. Yhdestä efektistä voi luoda <c>CreateSound</c>-metodilla monta ääntä (<c>Sound</c>),
    /// jotka voivat soida yhtäaikaa. Ääntä ei tarvitse kuitenkaan luoda itse, jos vain kutsuu
    /// <c>Play</c>-metodia.
    /// </summary>
    public class SoundEffect
    {
        List<Sound> Instances = new List<Sound>();

        private string assetName;
        XnaSoundEffect xnaEffect;
        Timer posTimer;

        /// <summary>
        /// Tapahtuu kun ääniefekti on toistettu loppuun.
        /// </summary>
        public event Action Finished;

        /// <summary>
        /// Ääniefektin kesto sekunteina.
        /// </summary>
        public TimeSpan Duration { get { DoLoad(); return xnaEffect.Duration; } }

        /// <summary>
        /// Paikka äänessä sekunteina (missä kohtaa toistoa ollaan). Ei voi asettaa.
        /// </summary>
        public DoubleMeter Position { get; set; }

        /// <summary>
        /// Toistetaanko ääntä parhaillaan.
        /// </summary>
        public bool IsPlaying { get; private set; }

        private static string[] soundExtensions = { ".wav", ".mp3", ".xnb" }; 

        private void DoLoad()
        {
            if (assetName is null)
                return;

            if (xnaEffect == null)
            {
                Debug.Assert(assetName != null);
                xnaEffect = FromContent(assetName);
            }

            Position.MaxValue = xnaEffect.Duration.TotalSeconds;
        }


        private XnaSoundEffect FromContent(string assetname)
        {
            assetName = Game.FileExtensionCheck(assetName, soundExtensions);
            FileStream fs = new FileStream(assetName, FileMode.Open);
            XnaSoundEffect sound = XnaSoundEffect.FromStream(fs);
            fs.Close();
            return sound;

        }
        internal SoundEffect(string assetName)
        {
            this.assetName = assetName;
            this.xnaEffect = null;
            InitPosition();
        }

        internal SoundEffect( XnaSoundEffect effect )
        {
            this.Position = new DoubleMeter(0, 0, 0);
            this.assetName = null;
            xnaEffect = effect;
            InitPosition();
        }

        internal SoundEffect(Stream stream)
        {
            this.Position = new DoubleMeter(0, 0, 0);
            this.assetName = null;
            try
            {
                xnaEffect = XnaSoundEffect.FromStream(stream);
            }
            catch (NoAudioHardwareException)
            {
                Game.Instance.OnNoAudioHardwareException();
            }
            InitPosition();
            
        }

        private void InitPosition()
        {
            Position = new DoubleMeter(0, 0, 0);
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
            if(Finished != null) Finished();
        }

        /// <summary>
        /// Luo Sound-tyyppisen olion. Oliolla on ominaisuuksia kuten voimakkuus ja korkeus
        /// joita voidaan muuttaa soiton aikana.
        /// </summary>
        /// <returns></returns>
        public Sound CreateSound()
        {
            if ( !Game.AudioEnabled )
                return null;

			try
			{
            	DoLoad();
            	return new Sound( xnaEffect.CreateInstance() );
			}
			catch (NoAudioHardwareException)
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
            DoLoad();
            Sound sound = CreateSound();
            if (sound == null) return false;

            StartPlaying( sound );
            return true;
        }

        /// <summary>
        /// Soittaa äänen.
        /// </summary>
        /// <param name="volume">Äänenvoimakkuus 0.0 - 1.0</param>
        /// <param name="pitch">Äänen taajuusmuutos. -1.0 = oktaavi alaspäin, 1.0 = oktaavi ylöspäin, 0.0 = normaali.</param>
        /// <param name="pan">Balanssi eli kummasta kaiuttimesta ääni kuuluu enemmän. -1.0 = kokonaan vasemmasta, 1.0 = kokonaan oikeasta, 0.0 = yhtä paljon kummastakin </param>
        /// <returns></returns>
        public bool Play( double volume, double pitch, double pan )
        {
            DoLoad();
            Sound sound = CreateSound();
            if (sound == null) return false;

            sound.Volume = volume;
            sound.Pitch = pitch;
            sound.Pan = pan;

            StartPlaying( sound );
            return true;
        }

        private void StartPlaying( Sound sound )
        {
			try
			{
	            sound.Play();
	            Instances.Add( sound );
	            Position.Reset();
	            posTimer.Start();
	            IsPlaying = true;
			}
			catch (InstancePlayLimitException)
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

        /// <summary>
        /// Äänenvoimakkuuden taso 0.0 - 1.0
        /// </summary>
        public static double MasterVolume 
        {
            set { XnaSoundEffect.MasterVolume = (float)value; }
            get { return XnaSoundEffect.MasterVolume; }
        }
    }
}
