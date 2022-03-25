using System;

namespace Jypeli
{
    public partial class PhysicsObject
    {
        private double maxAngularV = double.PositiveInfinity;
        private double maxLinearV = double.PositiveInfinity;

        /// <summary>
        /// Nopeus.
        /// </summary>
        /// <value>The velocity.</value>
        public Vector Velocity
        {
            get { return Body.Velocity; }
            set { Body.Velocity = value; }
        }

        /// <summary>
        /// Kiihtyvyys.
        /// </summary>
        /// <value>The acceleration.</value>
        public Vector Acceleration
        {
            get { return Body.Acceleration; }
            set { Body.Acceleration = value; }
        }

        /// <summary>
        /// Kulmanopeus.
        /// </summary>
        /// <value>The angular velocity.</value>
        public double AngularVelocity
        {
            get { return Body.AngularVelocity; }
            set { Body.AngularVelocity = value; }
        }

        /// <summary>
        /// Kulmakiihtyvyys.
        /// </summary>
        /// <value>The angular acceleration.</value>
        public double AngularAcceleration
        {
            get { return Body.AngularAcceleration; }
            set { Body.AngularAcceleration = value; }
        }

        /// <summary>
        /// Suurin nopeus, jonka olio voi saavuttaa.
        /// </summary>
        [Save]
        public double MaxVelocity
        {
            get { return maxLinearV; }
            set { maxLinearV = value; IsUpdated = true; }
        }

        /// <summary>
        /// Suurin kulmanopeus, jonka olio voi saavuttaa.
        /// </summary>
        [Save]
        public double MaxAngularVelocity
        {
            get { return maxAngularV; }
            set { maxAngularV = value; IsUpdated = true; }
        }

        /// <summary>
        /// Työntää oliota.
        /// </summary>
        /// <param name="force">Voima, jolla oliota työnnetään.</param>
        public virtual void Push(Vector force)
        {
            Body.ApplyForce(force);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        [Obsolete("Ei käytössä")]
        public void Push(Vector force, TimeSpan time)
        {
            throw new NotImplementedException();
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Kohdistaa kappaleeseen impulssin. Tällä kappaleen saa nopeasti liikkeeseen.
        /// </summary>
        public virtual void Hit(Vector impulse)
        {
            Body.ApplyImpulse(impulse);
        }

        /// <summary>
        /// Kohdistaa kappaleeseen vääntövoiman. Voiman suunta riippuu merkistä.
        /// </summary>
        /// <param name="torque">Vääntövoima.</param>
        public virtual void ApplyTorque(double torque)
        {
            Body.ApplyTorque(torque);
        }

        /// <summary>
        /// Pysäyttää olion.
        /// </summary>
        public override void Stop()
        {
            Body.Stop();
            base.Stop();
        }

        /// <summary>
        /// Pysäyttää liikkeen akselin suunnassa.
        /// </summary>
        /// <param name="axis">Akseli vektorina (ei väliä pituudella)</param>
        public void StopAxial(Vector axis)
        {
            StopMoveTo();

            // TODO: Distinguish between horizontal and vertical oscillations
            ClearOscillations();

            Body.StopAxial(axis);
        }

        /// <summary>
        /// Pysäyttää olion liikkeen vaakasuunnassa.
        /// </summary>
        public void StopHorizontal()
        {
            StopAxial(Vector.UnitX);
        }

        /// <summary>
        /// Pysäyttää olion liikkeen pystysuunnassa.
        /// </summary>
        public void StopVertical()
        {
            StopAxial(Vector.UnitY);
        }

        /// <summary>
        /// Pysäyttää kaiken pyörimisliikkeen.
        /// </summary>
        public void StopAngular()
        {
            Body.StopAngular();
        }

        /// <summary>
        /// Valmistelee heitettävän kappaleen heittoa varten valmiiksi, ei lisää sitä peliin.
        /// </summary>
        /// <param name="obj">Heitettävä</param>
        /// <param name="angle">Lähtökulma</param>
        /// <param name="force">Voima</param>
        /// <param name="distanceDelta">Kuinka kaukaa hahmosta kappale lähtee liikkeelle</param>
        /// <param name="axialDelta">Kuinka paljon lähtöpistettä siirretään sen liikeakselin suhteen</param>
        protected virtual void PrepareThrowable(PhysicsObject obj, Angle angle, double force, double distanceDelta, double axialDelta)
        {
            double d = (Width + obj.Width) / 2 + distanceDelta;
            Angle a = Angle + angle;
            obj.Position = Position + a.GetVector() * d + (a + Angle.RightAngle).GetVector() * axialDelta;
            obj.Hit(Vector.FromLengthAndAngle(force, a));
        }

        /// <summary>
        /// Heittää kappaleen hahmon rintamasuuntaa kohti.
        /// </summary>
        /// <param name="obj">Heitettävä kappale</param>
        /// <param name="angle">Suhteellinen kulma (0 astetta suoraan, 90 ylös)</param>
        /// <param name="force">Heiton voimakkuus</param>
        /// <param name="distOffset">Offset ammuksen etäisyydelle</param>
        /// <param name="layer">Pelimaailman kerros</param>
        /// <param name="axialOffset">Offset ammuksen akselin suuntaiselle paikalle</param>
        public void Throw(PhysicsObject obj, Angle angle, double force, double distOffset = 0, int layer = 0, double axialOffset = 0)
        {
            PrepareThrowable(obj, angle, force, distOffset, axialOffset);
            Game.Add(obj, layer);
        }

        /// <summary>
        /// Siirtää oliota.
        /// </summary>
        /// <param name="movement">Vektori, joka määrittää kuinka paljon siirretään.</param>
        public override void Move(Vector movement)
        {
            Vector dv = movement - this.Velocity;
            Hit(Mass * dv);
        }

        /// <summary>
        /// Siirtää oliota kohti kohdetta
        /// </summary>
        protected override void MoveToTarget()
        {
            if (!moveTarget.HasValue)
            {
                Stop();
                moveTimer.Stop();
                return;
            }

            Vector d = moveTarget.Value - Position;
            double vt = moveSpeed * moveTimer.Interval;

            if (d.Magnitude < vt)
            {
                Stop();
                moveTimer.Stop();
                moveTarget = null;

                if (arrivedAction != null)
                    arrivedAction();
            }
            else
            {
                Vector dv = Vector.FromLengthAndAngle(moveSpeed, d.Angle) - Velocity;
                Hit(Mass * dv);
            }
        }
    }
}
