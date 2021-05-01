using System;
using Microsoft.Xna.Framework.Content;
using XnaSong = Microsoft.Xna.Framework.Media.Song;
using Microsoft.Xna.Framework.Audio;

namespace Jypeli
{
    /// <summary>
    /// Mediasoitin, jolla voi soittaa musiikkikappaleita.
    /// </summary>
    /// TODO: Tekee nyt käytännössä samat asiat kuin Sound-luokka. Voisiko yhdistää?
    public class MediaPlayer
    {
        private ContentManager content;
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
#if WINDOWS_PHONE || ANDROID
                //Microsoft.Xna.Framework.FrameworkDispatcher.Update();
                return Game.AudioEnabled && Microsoft.Xna.Framework.Media.MediaPlayer.GameHasControl;
#else
                return Game.AudioEnabled;
#endif
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
            if (!CanPlay) return;
            if (sound != null)
            {
                sound.Stop();
            }

            try
            {
                sound = Game.LoadSoundEffect(songName).CreateSound();
            }
            catch (ContentLoadException e)
            {
                throw new Exception(
                    "Could not play the song \"" + songName + "\".\n" +
                    "Please check that you added the song to the Content project and typed the name correctly.", e);
            }
            sound.Play();
        }

#if WINDOWS
        /// <summary>
        /// Soittaa kappaleen tiedostosta.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi.</param>
        public void PlayFromFile( string fileName )
        {
            if ( !CanPlay ) return;

            // Use reflection to get the internal constructor of Song class
            // The public constructor does not take spaces well!
            var ctor = typeof( Song ).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] { typeof( string ), typeof( string ), typeof( int ) }, null );

            Song song = (Song)ctor.Invoke( new object[] { "name", fileName, 0 } );
            Microsoft.Xna.Framework.Media.MediaPlayer.Play( song );
        }
#endif

        /// <summary>
        /// Pysäyttää soiton.
        /// </summary>
        public void Pause()
        {
            if (!CanPlay || sound == null) return;
            sound.Pause();
        }

        /// <summary>
        /// Jatkaa pysäytettyä kappaletta.
        /// </summary>
        public void Resume()
        {
            if (!CanPlay || sound == null) return;
            sound.Resume();
        }

        /// <summary>
        /// Keskeyttää soiton.
        /// </summary>
        public void Stop()
        {
            if (!CanPlay || sound == null) return;
            sound.Stop();
        }
    }
}
