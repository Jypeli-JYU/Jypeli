using System;
using System.ComponentModel;

namespace Jypeli
{
    /// <summary>
    /// Yleiset liikkumiseen tarkoitetut aivot.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class AbstractMoverBrain : Brain
    {
        private double _speed = 100;
        private UnlimitedAngle _turnSpeed = UnlimitedAngle.Infinity;

        /// <summary>
        /// Nopeus, jolla liikutaan.
        /// </summary>
        /// <value>Nopeus.</value>
        public virtual double Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        /// <summary>
        /// Käännytäänkö siihen suuntaan mihin liikutaan.
        /// </summary>
        public bool TurnWhileMoving { get; set; }

        /// <summary>
        /// Maksimikääntymisnopeus (radiaania sekunnissa)
        /// </summary>
        public UnlimitedAngle TurnSpeed
        {
            get { return _turnSpeed; }
            set { _turnSpeed = value; }
        }

        /// <summary>
        /// Yleiset liikkumiseen tarkoitetut aivot
        /// </summary>
        public AbstractMoverBrain()
        {
        }

        /// <summary>
        /// Yleiset liikkumiseen tarkoitetut aivot
        /// </summary>
        /// <param name="speed">Nopeus</param>
        public AbstractMoverBrain(double speed)
        {
            this.Speed = speed;
        }

        private double dist(double a1, double a2)
        {
            return Math.PI - Math.Abs(Math.Abs(a2 - a1) - Math.PI);
        }

        /// <summary>
        /// Kääntää aivojen hallitsemaan hahmoa
        /// </summary>
        /// <param name="angle">Kulma</param>
        public void Turn(Angle angle)
        {
            if (!TurnWhileMoving || Game.Time.SinceLastUpdate.TotalSeconds == 0 || Owner == null || Owner.Angle == angle)
                return;

            double maxTurn = TurnSpeed.Radians * Game.Time.SinceLastUpdate.TotalSeconds;

            if (Math.Abs(angle.Radians - Owner.Angle.Radians) <= maxTurn)
                Owner.Angle = angle;
            else
            {
                double a1 = Owner.Angle.Radians;
                double a2 = angle.Radians;
                double d = dist(a2, a1);
                double dcw = dist(a2, a1 - maxTurn);
                double dccw = dist(a2, a1 + maxTurn);

                double step = dcw < dccw ? -maxTurn : maxTurn;
                Owner.Angle = Angle.FromRadians(Owner.Angle.Radians + step);
            }
        }

        /// <summary>
        /// Liikuttaa aivojen hallitsemaa hahmoa
        /// </summary>
        /// <param name="direction">Suunta</param>
        protected void Move(Vector direction)
        {
            if (Owner == null || direction == Vector.Zero)
                return;
            double d = Math.Min(direction.Magnitude, Speed);
            Owner.Move(Vector.FromLengthAndAngle(d, direction.Angle));
            Turn(direction.Angle);
        }

        /// <summary>
        /// Liikuttaa aivojen hallitsemaa hahmoa
        /// </summary>
        /// <param name="direction">Suunta</param>
        protected void Move(Angle direction)
        {
            if (Owner == null)
                return;
            Owner.Move(Vector.FromLengthAndAngle(Speed, direction));
            Turn(direction);
        }
    }
}
