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

using Jypeli.Farseer;

namespace Jypeli
{
    public partial class PhysicsBody
    {
        private double _maxAngularV = double.PositiveInfinity;
        private double _maxLinearV = double.PositiveInfinity;

        /// <summary>
        /// Olion nopeus.
        /// </summary>
        [Save]
        public Vector Velocity
        {
            get
            {
                return FSBody.LinearVelocity * FSConvert.SimToDisplay;
            }
            set { FSBody.LinearVelocity = value * FSConvert.DisplayToSim; }
        }

        /// <summary>
        /// Olion kulmanopeus.
        /// </summary>
        [Save]
        public double AngularVelocity
        {
            get { return FSBody.AngularVelocity; }
            set { FSBody.AngularVelocity = (float)value; }
        }

        /// <summary>
        /// Suurin nopeus, jonka olio voi saavuttaa.
        /// </summary>
        [Save]
        public double MaxVelocity
        {
            get { return _maxLinearV * FSConvert.SimToDisplay; }
            set { _maxLinearV = value * FSConvert.DisplayToSim; }
        }

        /// <summary>
        /// Suurin kulmanopeus, jonka olio voi saavuttaa.
        /// </summary>
        [Save]
        public double MaxAngularVelocity
        {
            get { return _maxAngularV; }
            set { _maxAngularV = value; }
        }


        /// <summary>
        /// Olion kiihtyvyys.
        /// </summary>
        [Save]
        public Vector Acceleration
        {
            get
            {
                return Vector.Zero;
            }
            set { }
        }

        /// <summary>
        /// Olion kulmakiihtyvyys.
        /// </summary>
        [Save]
        public double AngularAcceleration
        {
            get { return 0; }
            set { }
        }
        /*
        [Save]
        internal Vector ForceAccumulator
        {
            get { return new Vector( Body.State.ForceAccumulator.Linear.X, Body.State.ForceAccumulator.Linear.Y ); }
            set { Body.State.ForceAccumulator.Linear = new Vector2D( value.X, value.Y ); }
        }

        [Save]
        internal double AngularForceAccumulator
        {
            get { return Body.State.ForceAccumulator.Angular; }
            set { Body.State.ForceAccumulator.Angular = value; }
        }
        */
        /**
         * Massa on mooottorin sisällä konvertoitu toiseen potenssiin, ja 
         * itse pelissä käytetyt pituusyksiköt ovat konvertoitu kerran.
         */
        /// <summary>
        /// Kohdistaa kappaleeseen voiman.
        /// </summary>
        /// <param name="force">Voima, jolla oliota työnnetään.</param>
        public void ApplyForce(Vector force)
        {
            FSBody.ApplyForce(force * FSConvert.DisplayToSim * FSConvert.DisplayToSim * FSConvert.DisplayToSim);
        }

        /// <summary>
        /// Kohdistaa kappaleeseen impulssin. Tällä kappaleen saa nopeasti liikkeeseen.
        /// </summary>
        public void ApplyImpulse(Vector impulse)
        {
            FSBody.ApplyLinearImpulse(impulse * FSConvert.DisplayToSim * FSConvert.DisplayToSim * FSConvert.DisplayToSim);
        }

        /// <summary>
        /// Kohdistaa kappaleeseen vääntövoiman. Voiman suunta riippuu merkistä.
        /// </summary>
        /// <param name="torque">Vääntövoima.</param>
        public void ApplyTorque(double torque)
        {
            FSBody.ApplyTorque((float)torque * FSConvert.DisplayToSim);
        }

        /// <summary>
        /// Pysäyttää olion.
        /// </summary>
        public void Stop()
        {
            FSBody.AngularVelocity = 0f;
            FSBody.LinearVelocity = Vector.Zero;
        }

        /// <summary>
        /// Pysäyttää liikkeen akselin suunnassa.
        /// </summary>
        /// <param name="axis">Akseli vektorina (ei väliä pituudella)</param>
        public void StopAxial(Vector axis)
        {
            Acceleration = Acceleration.Project(axis.LeftNormal);
            Velocity = Velocity.Project(axis.LeftNormal);
            //TODO: Voimien nollaus
            /*
            Vector oldForce = new Vector( Body.State.ForceAccumulator.Linear.X, Body.State.ForceAccumulator.Linear.Y );
            Vector newForce = oldForce.Project( axis.LeftNormal );
            double aForce = Body.State.ForceAccumulator.Angular;
            Body.State.ForceAccumulator = new ALVector2D( aForce, newForce.X, newForce.Y );
            */
        }

        /// <summary>
        /// Pysäyttää kaiken pyörimisliikkeen.
        /// </summary>
        public void StopAngular()
        {
            FSBody.AngularVelocity = 0f;
        }
    }
}
