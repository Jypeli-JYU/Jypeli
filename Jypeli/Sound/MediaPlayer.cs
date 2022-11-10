using System;

namespace Jypeli
{
    /// <summary>
    /// Mediasoitin, jolla voi soittaa musiikkikappaleita.
    /// </summary>
    /// TODO: Tekee nyt käytännössä samat asiat kuin Sound-luokka. Voisiko yhdistää?
    public class MediaPlayer
    {
        private Sound sound;
        private double volumeHolder = 0;

        /// <summary>
        /// Voiko musiikkia soittaa.
        /// Windows Phonella rajoituksena on että käyttäjän musiikkia ei saa keskeyttää.
        /// Ei tarvitse tarkistaa itse ennen Play-kutsua, Jypeli tekee sen puolesta.
        /// </summary>
        public bool CanPlay
        {
            get
            {
                return Game.AudioEnabled;
            }
        }

        /// <summary>
        /// Äänenvoimakkuus välillä 0.0 - 1.0.
        /// </summary>
        public double Volume
        {
            get { return sound.Volume; }
            set { sound.Volume = value; }
        }

        /// <summary>
        /// Onko mediasoitin hiljaisella vai ei.
        /// </summary>
        public bool IsMuted
        {
            get { return Volume == 0; }
            set
            {
                if (value && !IsMuted)
                {
                    volumeHolder = Volume;
                    Volume = 0;
                }
                else if (!value && IsMuted)
                {
                    Volume = volumeHolder;
                }
            }
        }

        /// <summary>
        /// Toistetaanko kappaleita alusta.
        /// </summary>
        public bool IsRepeating
        {
            get { return sound.IsLooped; }
            set { sound.IsLooped = value; }
        }

        /// <summary>
        /// Soitetaanko tällä hetkellä mitään.
        /// </summary>
        public bool IsPlaying
        {
            get { return sound?.State == SoundState.Playing; }
        }

        internal MediaPlayer()
        {
        }

        /// <summary>
        /// Soittaa kappaleen.
        /// </summary>
        /// <param name="songName">Kappaleen nimi.</param>
        public void Play(string songName)
        {
            if (!CanPlay)
                return;

            sound?.Stop();

            sound = Game.LoadSoundEffect(songName).CreateSound();

            sound.Play();
        }

        /// <summary>
        /// Soittaa kappaleen.
        /// </summary>
        /// <param name="sound">Soitettava ääni.</param>
        public void Play(SoundEffect sound)
        {
            if (!CanPlay)
                return;

            this.sound?.Stop();

            this.sound = sound.CreateSound();

            this.sound.Play();
        }

        /// <summary>
        /// Pysäyttää soiton.
        /// </summary>
        public void Pause()
        {
            if (!CanPlay || sound == null)
                return;
            sound.Pause();
        }

        /// <summary>
        /// Jatkaa pysäytettyä kappaletta.
        /// </summary>
        public void Resume()
        {
            if (!CanPlay || sound == null)
                return;
            sound.Resume();
        }

        /// <summary>
        /// Keskeyttää soiton.
        /// </summary>
        public void Stop()
        {
            if (!CanPlay || sound == null)
                return;
            sound.Stop();
        }
    }
}
