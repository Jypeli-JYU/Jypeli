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
using System.Collections.Generic;

#if WINDOWS_STOREAPP
using ImageConverter = Jypeli.ListHelpers.Converter<Jypeli.Image, Jypeli.Image>;
#else
using ImageConverter = System.Converter<Jypeli.Image, Jypeli.Image>;
#endif

namespace Jypeli
{
    /// <summary>
    /// Sarja kuvia, jotka vaihtuvat halutulla nopeudella. 
    /// Yksi animaatio koostuu yhdestä tai useammasta kuvasta.
    /// </summary>
    /// <remarks>
    /// Eri peliolioille kannattaa yleensä tehdä oma animaatio-olio,
    /// jotta niiden animaatiot voivat edetä eri tahtiin. Jos animaatioilla
    /// on kuitenkin samat kuvat, kannattaa samaa kuvataulukkoa käyttää
    /// molemmille.
    /// <example>
    /// Image[] kuvat = LoadImages("kuva1", "kuva2", "kuva3");
    /// 
    /// void LuoKentta()
    /// {
    ///     // ...
    ///     o1.Animation = new animation(kuvat);
    ///     o2.Animation = new Animation(kuvat);
    /// }
    /// </example>
    /// </remarks>
    public class Animation : IEnumerable<Image>
    {
        private double secondsPerFrame;
        private TimeSpan startTime;
        private int currentIndex;
        private int repeatCount = -1; // When -1, animation repeats forever.

        // NOTES:
        //
        // Animation frames should be immutable, because that is what
        // the Layer class assumes. So, let's not provide
        // any methods to modify the images of an existing animation.
        //
        // This is not a big deal, as there rarely is any need
        // to modify animations at runtime and constructing new
        // animation objects is quite easy anyway.

        internal Image[] frames = null;

        /// <summary>
        /// Onko animaatio käynnissä.
        /// </summary>
        public bool IsPlaying { get; set; }

        /// <summary>
        /// Ruutujen määrä.
        /// </summary>
        public int FrameCount
        {
            get { return frames.Length; }
        }

        int lastRepeat = 0;

        /// <summary>
        /// Aktiivisen animaatioruudun indeksi (alkaa nollasta).
        /// </summary>
        public int CurrentFrameIndex
        {
            get
            {
                if ( !IsPlaying ) return currentIndex;

                double secondsNow = Game.Time.SinceStartOfGame.TotalSeconds;
                double secondsAdvanced = secondsNow - startTime.TotalSeconds;
                int currentRepeat = (int)( secondsAdvanced / ( FrameCount * secondsPerFrame ) );

                if ( currentRepeat > lastRepeat )
                {
                    OnPlayed();
                    lastRepeat = currentRepeat;
                }
                    
                if ( repeatCount >= 0 && currentRepeat >= repeatCount )
                    Stop();

                return ( (int)( secondsAdvanced * FPS ) ) % FrameCount;
            }
        }

        /// <summary>
        /// Animaation päivitysnopeus ruutuina sekunnissa (frames per second).
        /// </summary>
        public double FPS
        {
            get { return 1 / secondsPerFrame; }
            set
            {
                if ( value == 0 )
                    throw new ArgumentException( "FPS can not be zero" );
                secondsPerFrame = 1 / value;
            }
        }

        /// <summary>
        /// Tällä hetkellä näkyvä ruutu animaatiosta.
        /// </summary>
        public Image CurrentFrame
        {
            get { return frames[CurrentFrameIndex]; }
        }

        /// <summary>
        /// Jos <c>true</c>, animaatio ei pysähtyessä palaa ensimmäiseen
        /// ruutuun.
        /// </summary>
        public bool StopOnLastFrame { get; set; }

        /// <summary>
        /// Animaation leveys. Nolla, jos animaatiossa ei ole yhtään ruutua.
        /// </summary>
        public double Width
        {
            get { return FrameCount > 0 ? frames[0].Width : 0; }
        }

        /// <summary>
        /// Animaation korkeus. Nolla, jos animaatiossa ei ole yhtään ruutua.
        /// </summary>
        public double Height
        {
            get { return FrameCount > 0 ? frames[0].Height : 0; }
        }

        /// <summary>
        /// Animaation koko. Vector.Zero, jos animaatiossa ei ole yhtään ruutua.
        /// </summary>
        public Vector Size
        {
            get { return new Vector( Width, Height ); }
        }

        /// <summary>
        /// Tapahtuma, joka tapahtuu kun animaatio on suoritettu.
        /// </summary>
        public event Action Played;

        private void OnPlayed()
        {
            if ( Played != null ) Played();
        }

        /// <summary>
        /// Luo uuden animaation.
        /// </summary>
        /// <param name="frames">Animaation ruudut.</param>
        public Animation( params Image[] frames )
        {
            if ( frames.Length == 0 )
                throw new ArgumentException( "Animation must have at least one frame." );
            FPS = 25;
            currentIndex = 0;
            startTime = Game.Time.SinceStartOfGame;
            this.frames = frames;
        }

        /// <summary>
        /// Luo uuden animaation kuvasta.
        /// </summary>
        /// <param name="image">Kuva.</param>
        public static implicit operator Animation(Image image)
        {
            if ( image == null ) return null;
            return new Animation( image );
        }

        /// <summary>
        /// Luo kopion jo tunnetusta animaatiosta.
        /// </summary>
        /// <param name="src">Kopioitava animaatio.</param>
        public Animation( Animation src )
        {
            FPS = src.FPS;
            IsPlaying = src.IsPlaying;
            startTime = src.startTime;
            currentIndex = src.currentIndex;
            repeatCount = src.repeatCount;
            frames = new Image[src.FrameCount];

            // Copy only the references to images.
            for ( int i = 0; i < src.FrameCount; i++ )
                frames[i] = src.frames[i];
        }

        /// <summary>
        /// Käyttää haluttua metodia kaikkiin animaation ruutuihin.
        /// </summary>
        /// <param name="anim">Animaatio</param>
        /// <param name="method">Metodi, joka ottaa parametriksi kuvan ja palauttaa kuvan</param>
        /// <returns>Uusi animaatio</returns>
        public static Animation Apply( Animation anim, ImageConverter method )
        {
            Animation applied = new Animation( anim );

            for ( int i = 0; i < anim.frames.Length; i++ )
            {
                applied.frames[i] = method(anim.frames[i]);
            }

            return applied;
        }

        /// <summary>
        /// Peilaa animaation X-suunnasssa.
        /// </summary>
        /// <param name="anim">Animaatio</param>
        /// <returns>Peilattu animaatio</returns>
        public static Animation Mirror( Animation anim )
        {
            return Apply( anim, Image.Mirror );
        }

        /// <summary>
        /// Peilaa animaation Y-suunnasssa.
        /// </summary>
        /// <param name="anim">Animaatio</param>
        /// <returns>Peilattu animaatio</returns>
        public static Animation Flip( Animation anim )
        {
            return Apply( anim, Image.Flip );
        }

        /// <summary>
        /// Palauttaa animaation, joka toistuu lopusta alkuun.
        /// </summary>
        /// <param name="anim">Animaatio</param>
        /// <returns>Käännetty animaatio</returns>
        public static Animation Reverse( Animation anim )
        {
            Animation reversed = new Animation( anim );

            for ( int i = 0; i < anim.frames.Length / 2; i++ )
            {
                reversed.frames[i] = anim.frames[anim.frames.Length - 1 - i];
                reversed.frames[anim.frames.Length - 1 - i] = anim.frames[i];
            }

            return reversed;
        }

        /// <summary>
        /// Käynnistää animaation alusta.
        /// </summary>
        public void Start()
        {
            Start( -1 );
        }

        /// <summary>
        /// Käynnistää animaation alusta.
        /// </summary>
        /// <param name="repeatCount">Kuinka monta kertaa animaatio suoritetaan.</param>
        public void Start( int repeatCount )
        {
            this.repeatCount = repeatCount;
            startTime = Game.Time.SinceStartOfGame;
            IsPlaying = true;
            lastRepeat = 0;
        }

        /// <summary>
        /// Keskeyttää animaation toiston.
        /// </summary>
        public void Pause()
        {
            IsPlaying = false;
        }

        /// <summary>
        /// Jatkaa animaatiota siitä, mihin viimeksi jäätiin.
        /// </summary>
        public void Resume()
        {
            IsPlaying = true;
        }

        /// <summary>
        /// Pysäyttää animaation asettaen sen ensimmäiseen ruutuun.
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            currentIndex = 0;
            if ( StopOnLastFrame )
                currentIndex = FrameCount - 1;
            repeatCount = -1;
        }

        /// <summary>
        /// Etenee animaatiossa halutun määrän ruutuja. Käytä negatiivisia arvoja, jos haluat
        /// kulkea taaksepäin.
        /// </summary>
        /// <param name="numberOfFrames">Edettävä määrä ruutuja.</param>
        public void Step( int numberOfFrames )
        {
            IsPlaying = false;
            currentIndex = currentIndex + numberOfFrames;

            if ( currentIndex >= FrameCount )
            {
                // Animation has reached its final frame
                if ( repeatCount > 0 ) repeatCount--;

                if ( repeatCount == 0 )
                    Stop();
                else
                {
                    // Play it again, Sam
                    currentIndex %= FrameCount;
                }

                OnPlayed();
            }
        }

        /// <summary>
        /// Etenee animaatiossa yhden ruudun eteenpäin.
        /// </summary>
        public void Step()
        {
            Step( 1 );
        }

        #region IEnumerable<Image> Members

        /// <summary>
        /// Animaation kuvien iteraattori
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Image> GetEnumerator()
        {
            foreach ( Image frame in frames )
                yield return frame;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return frames.GetEnumerator();
        }

        #endregion
    }
}
