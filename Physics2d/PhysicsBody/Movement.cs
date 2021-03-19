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

using AdvanceMath;
using Physics2DDotNet;
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
                Vector2D v = Body.State.Velocity.Linear;
                return new Vector( v.X, v.Y );
            }
            set { Body.State.Velocity.Linear = new Vector2D( value.X, value.Y ); }
        }

        /// <summary>
        /// Olion kulmanopeus.
        /// </summary>
        [Save]
        public double AngularVelocity
        {
            get { return Body.State.Velocity.Angular; }
            set { Body.State.Velocity.Angular = value; }
        }

        /// <summary>
        /// Suurin nopeus, jonka olio voi saavuttaa.
        /// </summary>
        [Save]
        public double MaxVelocity
        {
            get { return _maxAngularV; }
            set { _maxAngularV = value; }
        }

        /// <summary>
        /// Suurin kulmanopeus, jonka olio voi saavuttaa.
        /// </summary>
        [Save]
        public double MaxAngularVelocity
        {
            get { return _maxLinearV; }
            set { _maxLinearV = value; }
        }

        /// <summary>
        /// Olion kiihtyvyys.
        /// </summary>
        [Save]
        public Vector Acceleration
        {
            get
            {
                Vector2D v = Body.State.Acceleration.Linear;
                return new Vector( v.X, v.Y );
            }
            set { Body.State.Acceleration.Linear = new Vector2D( value.X, value.Y ); }
        }

        /// <summary>
        /// Olion kulmakiihtyvyys.
        /// </summary>
        [Save]
        public double AngularAcceleration
        {
            get { return Body.State.Acceleration.Angular; }
            set { Body.State.Acceleration.Angular = value; }
        }

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

        /// <summary>
        /// Kohdistaa kappaleeseen voiman.
        /// </summary>
        /// <param name="force">Voima, jolla oliota työnnetään.</param>
        public void ApplyForce( Vector force )
        {
            Body.ApplyForce( new Vector2D( force.X, force.Y ) );
        }

        /// <summary>
        /// Kohdistaa kappaleeseen impulssin. Tällä kappaleen saa nopeasti liikkeeseen.
        /// </summary>
        public void ApplyImpulse( Vector impulse )
        {
            Body.ApplyImpulse( new Vector2D( impulse.X, impulse.Y ) );
        }

        /// <summary>
        /// Kohdistaa kappaleeseen vääntövoiman. Voiman suunta riippuu merkistä.
        /// </summary>
        /// <param name="torque">Vääntövoima.</param>
        public void ApplyTorque( double torque )
        {
            Body.ApplyTorque( torque );
        }

        /// <summary>
        /// Pysäyttää olion.
        /// </summary>
        public void Stop()
        {
            Body.ClearForces();
            Body.State.Acceleration = ALVector2D.Zero;
            Body.State.Velocity = ALVector2D.Zero;
            Body.State.ForceAccumulator = ALVector2D.Zero;
        }
        
        /// <summary>
        /// Pysäyttää liikkeen akselin suunnassa.
        /// </summary>
        /// <param name="axis">Akseli vektorina (ei väliä pituudella)</param>
        public void StopAxial( Vector axis )
        {
            Acceleration = Acceleration.Project( axis.LeftNormal );
            Velocity = Velocity.Project( axis.LeftNormal );

            Vector oldForce = new Vector( Body.State.ForceAccumulator.Linear.X, Body.State.ForceAccumulator.Linear.Y );
            Vector newForce = oldForce.Project( axis.LeftNormal );
            double aForce = Body.State.ForceAccumulator.Angular;
            Body.State.ForceAccumulator = new ALVector2D( aForce, newForce.X, newForce.Y );
        }

        /// <summary>
        /// Pysäyttää kaiken pyörimisliikkeen.
        /// </summary>
        public void StopAngular()
        {
            Body.State.Acceleration.Angular = 0;
            Body.State.Velocity.Angular = 0;
            Body.State.ForceAccumulator.Angular = 0;
        }
    }
}
