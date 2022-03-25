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
 * Original Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 * Modified for Farseer engine by Mikko Röyskö
 */

using System.Numerics;
using FarseerPhysics.Dynamics;
using Jypeli.Farseer;

namespace Jypeli
{
    public partial class PhysicsBody
    {
        private const double DefaultMass = 1.0;
        private bool _momentOfInertiaSet = false;

        /// <summary>
        /// Jos <c>false</c>, olio ei voi pyöriä.
        /// </summary>
        public bool CanRotate
        {
            get { return !FSBody.FixedRotation; }
            set
            {
                FSBody.FixedRotation = !value;
            }
        }

        /// <summary>
        /// Olion massa. Mitä suurempi massa, sitä suurempi voima tarvitaan olion liikuttamiseksi.
        /// </summary>
        /// <remarks>
        /// Massan asettaminen muuttaa myös hitausmomenttia (<c>MomentOfInertia</c>).
        /// </remarks>
        public double Mass
        {
            get { return FSBody.Mass * FSConvert.SimToDisplay * FSConvert.SimToDisplay; }
            set
            {
                // We change the moment of inertia as well. If the mass is changed from
                // 1.0 to 100000.0 for example, it would look funny if a heavy object would
                // spin wildly from a small touch.
                double val = value * FSConvert.DisplayToSim * FSConvert.DisplayToSim;

                if (_momentOfInertiaSet)
                {
                    // The moment of inertia has been set manually,
                    // let's keep it that way and just set the mass.
                    FSBody.Mass = (float)val;
                }
                else
                {
                    SetMassAndInertia((float)val);
                }
            }
        }

        public double MassInv
        {
            get { return 1 / FSBody.Mass; }
            set
            {
                Mass = 1 / value;
            }
        }

        /// <summary>
        /// Olion hitausmomentti. Mitä suurempi hitausmomentti, sitä enemmän vääntöä tarvitaan
        /// olion pyörittämiseksi.
        /// </summary>
        public double MomentOfInertia
        {
            get { return FSBody.Inertia * FSConvert.SimToDisplay; }
            set
            {

                FSBody.Inertia = (float)value * FSConvert.DisplayToSim;
                _momentOfInertiaSet = true;
            }
        }

        /// <summary>
        /// Tekee oliosta staattisen. Staattinen olio ei liiku muiden olioiden törmäyksistä,
        /// vaan ainoastaan muuttamalla suoraan sen paikkaa tai nopeutta.
        /// </summary>
        public void MakeStatic()
        {
            FSBody.BodyType = BodyType.Kinematic;
        }

        /// <summary>
        /// Olion hidastuminen. Hidastaa olion vauhtia, vaikka se ei
        /// osuisi mihinkään. Vähän kuin väliaineen (esim. ilman tai veden)
        /// vastus. Oletusarvo on 1.0, jolloin hidastumista ei ole. Mitä
        /// pienempi arvo, sitä enemmän kappale hidastuu.
        /// 
        /// Yleensä kannattaa käyttää arvoja, jotka ovat lähellä ykköstä,
        /// esim. 0.95.
        /// </summary>
        public double LinearDamping
        {
            get { return FSBody.LinearDamping; }
            set { FSBody.LinearDamping = (float)value; }
        }

        /// <summary>
        /// Olion pyörimisen hidastuminen.
        /// 
        /// <see cref="LinearDamping"/>
        /// </summary>
        public double AngularDamping
        {
            get { return FSBody.AngularDamping; }
            set { FSBody.AngularDamping = (float)value; }
        }

        /// <summary>
        /// Päivittää kappaleen fixtuurien massan vastaamaan uutta asetettua massaa
        /// sekä laskee tämän massan pohjalta uuden hitausmomentin.
        /// </summary>
        private void SetMassAndInertia(float mass)
        {
            FSBody.Mass = mass;

            Vector2 localCenter = Vector2.Zero;
            float inertia = 0;
            float totalArea = 0;

            foreach (var f in FSBody.FixtureList)
            {
                totalArea += f.Shape.MassData.Area;
            }

            float newDensity = mass / totalArea;

            foreach (Fixture f in FSBody.FixtureList)
            {
                f.Shape._density = newDensity;
                f.Shape.ComputeProperties();
                var massData = f.Shape.MassData;
                localCenter += massData.Mass * massData.Centroid;

                inertia += massData.Inertia;
            }

            // TODO: Meneeköhän tää nyt ihan oikein?
            // Luultavasti ainakin "tarpeeksi lähellä"?

            inertia -= mass * Vector2.Dot(localCenter, localCenter);

            FSBody.Inertia = inertia;
        }
    }
}
