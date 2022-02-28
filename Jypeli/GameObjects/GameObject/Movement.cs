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
using Jypeli.GameObjects;

namespace Jypeli
{
    public partial class GameObject
    {
        private SynchronousList<Oscillator> oscillators = null;

        /// <summary>
        /// Ajastin joka liikuttaa kappaletta kohti kohdepistettä
        /// </summary>
        protected Timer moveTimer = null;

        /// <summary>
        /// Piste johon liikutaan
        /// </summary>
        protected Vector? moveTarget = null;

        /// <summary>
        /// Liikkumisnopeus kohdetta kohti
        /// </summary>
        protected double moveSpeed;

        /// <summary>
        /// Kun olio saapuu kohteeseen
        /// </summary>
        protected Action arrivedAction = null;

        /// <summary>
        /// Siirtää oliota.
        /// </summary>
        /// <param name="movement">Vektori, joka määrittää kuinka paljon siirretään.</param>
        public virtual void Move( Vector movement )
        {
            Position += movement;
        }

        /// <summary>
        /// Yrittää siirtyä annettuun paikkaan annetulla nopeudella.
        /// Laukaisee annetun aliohjelman, kun paikkaan on päästy.
        /// </summary>
        /// <param name="location">Paikka johon siirrytään</param>
        /// <param name="speed">
        /// Nopeus (paikkayksikköä sekunnissa) jolla liikutaan.
        /// Nopeus on maksiminopeus. Jos välissä on hitaampaa maastoa tai
        /// esteitä, liikkumisnopeus voi olla alle sen.
        /// </param>
        /// <param name="doWhenArrived">
        /// Aliohjelma, joka ajetaan kun paikkaan on päästy.
        /// Voi olla null, jos ei haluta mitään aliohjelmaa.
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
        /// Pysäyttää MoveTo-aliohjelmalla aloitetun liikkeen.
        /// </summary>
        public void StopMoveTo()
        {
            if ( moveTimer != null )
            {
                moveTimer.Stop();
                moveTarget = null;
            }
        }

        /// <summary>
        /// Liikuttaa kappaletta kohti määränpäätä.
        /// </summary>
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
        /// Laittaa kappaleen värähtelemään edestakaisin nykyisen paikkansa ympärillä tietyn akselin suuntaisesti.
        /// </summary>
        /// <param name="axis">Akseli, jonka suunnassa värähdellään. Pituudella ei väliä, vain suunnalla.</param>
        /// <param name="amplitude">Amplitudi eli ääripäiden välinen etäisyys.</param>
        /// <param name="frequency">Taajuus, eli kuinka monta jaksoa sekunnissa värähdellään.</param>
        /// <param name="phase">Vaihe, eli mistä kohtaa jaksoa aloitetaan. Vaihteluväli 0 - 2 * Math.PI (siniaalto)</param>
        /// <param name="damping">Vaimennuskerroin. 0 = ei vaimene, mitä suurempi niin sitä nopeammin vaimenee.</param>
        public void Oscillate( Vector axis, double amplitude, double frequency, double phase = 0, double damping = 0 )
        {
            if ( oscillators == null )
                oscillators = new SynchronousList<Oscillator>();

            oscillators.Add( new LinearOscillator( this, axis, amplitude, frequency, phase, damping ) );
            IsUpdated = true;
        }

        /// <summary>
        /// Laittaa kappaleen kulman värähtelemään edestakaisin.
        /// </summary>
        /// <param name="direction">Värähtelyn suunta (1 = myötäpäivään, -1 = vastapäivään)</param>
        /// <param name="amplitude">Amplitudi eli ääripäiden välinen etäisyys (radiaaneina).</param>
        /// <param name="frequency">Taajuus, eli kuinka monta jaksoa sekunnissa värähdellään.</param>
        /// <param name="damping">Vaimennuskerroin. 0 = ei vaimene, mitä suurempi niin sitä nopeammin vaimenee.</param>
        public void OscillateAngle( double direction, UnlimitedAngle amplitude, double frequency, double damping = 0 )
        {
            if ( oscillators == null )
                oscillators = new SynchronousList<Oscillator>();

            oscillators.Add( new AngularOscillator( this, direction, amplitude, frequency, damping ) );
            IsUpdated = true;
        }

        /// <summary>
        /// Asettaa uuden tasapainoaseman värähtelyille.
        /// </summary>
        public void SetEquilibrium()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Poistaa kaikki värähtelyt kappaleelta.
        /// </summary>
        /// <param name="returnToOriginalPosition">Palautetaanko kappale takaisin sen alkuperäiseen sijaintiin.</param>
        /// <param name="stopGradually">Suoritetaanko oskillaatio ensin loppuun, jonka jälkeen vasta pysähdytään alkuperäiseen sijaintiin.</param>
        public void ClearOscillations(bool returnToOriginalPosition = false, bool stopGradually = false)
        {
            if(oscillators != null && oscillators.Count >= 1)
            {
                oscillators.ForEach(o => o.Stop(returnToOriginalPosition, stopGradually));
            }
            if (!stopGradually)
            {
                oscillators = null;
            }
        }

        /// <summary>
        /// Poistaa kaikki värähtelyt kappaleelta.
        /// </summary>
        public void ClearOscillations()
        {
            ClearOscillations(false, false);
        }

        /// <summary>
        /// Pysäyttää kaiken liikkeen.
        /// </summary>
        public virtual void Stop()
        {
            StopMoveTo();
            ClearOscillations();
        }
    }
}
