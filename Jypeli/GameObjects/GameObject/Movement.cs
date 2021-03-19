#region MIT License
/*
 * Copyright (c) 2009 University of Jyv�skyl�, Department of Mathematical
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
 * Authors: Tero J�ntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using Jypeli.GameObjects;

namespace Jypeli
{
    public partial class GameObject
    {
        private SynchronousList<Oscillator> oscillators = null;

        protected Timer moveTimer = null;
        protected Vector? moveTarget = null;
        protected double moveSpeed;
        protected Action arrivedAction = null;

        /// <summary>
        /// Siirt�� oliota.
        /// </summary>
        /// <param name="movement">Vektori, joka m��ritt�� kuinka paljon siirret��n.</param>
        public virtual void Move( Vector movement )
        {
            Position += movement;
        }

        /// <summary>
        /// Yritt�� siirty� annettuun paikkaan annetulla nopeudella.
        /// Laukaisee annetun aliohjelman, kun paikkaan on p��sty.
        /// </summary>
        /// <param name="location">Paikka johon siirryt��n</param>
        /// <param name="speed">
        /// Nopeus (paikkayksikk�� sekunnissa) jolla liikutaan.
        /// Nopeus on maksiminopeus. Jos v�liss� on hitaampaa maastoa tai
        /// esteit�, liikkumisnopeus voi olla alle sen.
        /// </param>
        /// <param name="doWhenArrived">
        /// Aliohjelma, joka ajetaan kun paikkaan on p��sty.
        /// Voi olla null, jos ei haluta mit��n aliohjelmaa.
        /// </param>
        public override void MoveTo( Vector location, double speed, Action doWhenArrived )
        {
            if ( moveTimer == null )
            {
                moveTimer = new Timer();
                moveTimer.Timeout += MoveToTarget;
                moveTimer.Interval = 0.01;
            }
            else if ( moveTimer.Enabled )
                moveTimer.Stop();

            moveSpeed = speed;
            moveTarget = location;
            arrivedAction = doWhenArrived;
            moveTimer.Start();
        }

        /// <summary>
        /// Pys�ytt�� MoveTo-aliohjelmalla aloitetun liikkeen.
        /// </summary>
        public void StopMoveTo()
        {
            if ( moveTimer != null )
            {
                moveTimer.Stop();
                moveTarget = null;
            }
        }

        protected virtual void MoveToTarget()
        {
            if ( !moveTarget.HasValue )
            {
                moveTimer.Stop();
                return;
            }

            Vector d = moveTarget.Value - Position;
            double vt = moveSpeed * moveTimer.Interval;

            if ( d.Magnitude < vt )
            {
                Vector targetLoc = moveTarget.Value;
                moveTimer.Stop();
                Position = moveTarget.Value;
                moveTarget = null;

                if ( arrivedAction != null )
                    arrivedAction();
            }
            else
            {
                Position += Vector.FromLengthAndAngle( vt, d.Angle );
            }
        }

        /// <summary>
        /// Laittaa kappaleen v�r�htelem��n edestakaisin nykyisen paikkansa ymp�rill� tietyn akselin suuntaisesti.
        /// </summary>
        /// <param name="axis">Akseli, jonka suunnassa v�r�hdell��n. Pituudella ei v�li�, vain suunnalla.</param>
        /// <param name="amplitude">Amplitudi eli ��rip�iden v�linen et�isyys.</param>
        /// <param name="frequency">Taajuus, eli kuinka monta jaksoa sekunnissa v�r�hdell��n.</param>
        /// <param name="phase">Vaihe, eli mist� kohtaa jaksoa aloitetaan. Vaihteluv�li 0 - 2 * Math.PI (siniaalto)</param>
        /// <param name="damping">Vaimennuskerroin. 0 = ei vaimene, mit� suurempi niin sit� nopeammin vaimenee.</param>
        public void Oscillate( Vector axis, double amplitude, double frequency, double phase = 0, double damping = 0 )
        {
            if ( oscillators == null )
                oscillators = new SynchronousList<Oscillator>();

            oscillators.Add( new LinearOscillator( this, axis, amplitude, frequency, phase, damping ) );
            IsUpdated = true;
        }

        /// <summary>
        /// Laittaa kappaleen kulman v�r�htelem��n edestakaisin.
        /// </summary>
        /// <param name="direction">V�r�htelyn suunta (1 = my�t�p�iv��n, -1 = vastap�iv��n)</param>
        /// <param name="amplitude">Amplitudi eli ��rip�iden v�linen et�isyys (radiaaneina).</param>
        /// <param name="frequency">Taajuus, eli kuinka monta jaksoa sekunnissa v�r�hdell��n.</param>
        /// <param name="damping">Vaimennuskerroin. 0 = ei vaimene, mit� suurempi niin sit� nopeammin vaimenee.</param>
        public void OscillateAngle( double direction, UnlimitedAngle amplitude, double frequency, double damping = 0 )
        {
            if ( oscillators == null )
                oscillators = new SynchronousList<Oscillator>();

            oscillators.Add( new AngularOscillator( this, direction, amplitude, frequency, damping ) );
            IsUpdated = true;
        }

        /// <summary>
        /// Asettaa uuden tasapainoaseman v�r�htelyille.
        /// </summary>
        public void SetEquilibrium()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Poistaa kaikki v�r�htelyt kappaleelta.
        /// </summary>
        public void ClearOscillations()
        {
            oscillators = null;
        }

        /// <summary>
        /// Pys�ytt�� kaiken liikkeen.
        /// </summary>
        public virtual void Stop()
        {
            StopMoveTo();
            ClearOscillations();
        }
    }
}
