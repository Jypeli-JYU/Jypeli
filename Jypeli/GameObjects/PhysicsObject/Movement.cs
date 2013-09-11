using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    public partial class PhysicsObject
    {
        /// <summary>
        /// Työntää oliota.
        /// </summary>
        /// <param name="force">Voima, jolla oliota työnnetään.</param>
        public virtual void Push( Vector force )
        {
            Body.ApplyForce( force );
        }

        public void Push( Vector force, TimeSpan time )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Kohdistaa kappaleeseen impulssin. Tällä kappaleen saa nopeasti liikkeeseen.
        /// </summary>
        public virtual void Hit( Vector impulse )
        {
            Body.ApplyImpulse( impulse );
        }

        /// <summary>
        /// Kohdistaa kappaleeseen vääntövoiman. Voiman suunta riippuu merkistä.
        /// </summary>
        /// <param name="torque">Vääntövoima.</param>
        public virtual void ApplyTorque( double torque )
        {
            Body.ApplyTorque( torque );
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
        public void StopAxial( Vector axis )
        {
            StopMoveTo();

            // TODO: Distinguish between horizontal and vertical oscillations
            ClearOscillations();

            Body.StopAxial( axis );
        }

        /// <summary>
        /// Pysäyttää olion liikkeen vaakasuunnassa.
        /// </summary>
        public void StopHorizontal()
        {
            StopAxial( Vector.UnitX );
        }

        /// <summary>
        /// Pysäyttää olion liikkeen pystysuunnassa.
        /// </summary>
        public void StopVertical()
        {
            StopAxial( Vector.UnitY );
        }

        /// <summary>
        /// Pysäyttää kaiken pyörimisliikkeen.
        /// </summary>
        public void StopAngular()
        {
            Body.StopAngular();
        }

        protected virtual void PrepareThrowable( PhysicsObject obj, Angle angle, double force, double distanceDelta, double axialDelta )
        {
            double d = ( this.Width + obj.Width ) / 2 + distanceDelta;
            Angle a = this.AbsoluteAngle + angle;
            obj.Position = this.AbsolutePosition + a.GetVector() * d + ( a + Angle.RightAngle ).GetVector() * axialDelta;
            obj.Hit( Vector.FromLengthAndAngle( force, a ) );
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
        public void Throw( PhysicsObject obj, Angle angle, double force, double distOffset = 0, int layer = 0, double axialOffset = 0 )
        {
            PrepareThrowable( obj, angle, force, distOffset, axialOffset );
            Game.Add( obj, layer );
        }

        /// <summary>
        /// Siirtää oliota.
        /// </summary>
        /// <param name="movement">Vektori, joka määrittää kuinka paljon siirretään.</param>
        public override void Move( Vector movement )
        {
            Vector dv = movement - this.Velocity;
            Hit( Mass * dv );
        }

        protected override void MoveToTarget()
        {
            if ( !moveTarget.HasValue )
            {
                Stop();
                moveTimer.Stop();
                return;
            }

            Vector d = moveTarget.Value - AbsolutePosition;
            double vt = moveSpeed * moveTimer.Interval;

            if ( d.Magnitude < vt )
            {
                Vector targetLoc = moveTarget.Value;
                Stop();
                moveTimer.Stop();
                moveTarget = null;

                if ( arrivedAction != null )
                    arrivedAction();
            }
            else
            {
                Vector dv = Vector.FromLengthAndAngle( moveSpeed, d.Angle ) - this.Velocity;
                Hit( Mass * dv );
            }
        }
    }
}
