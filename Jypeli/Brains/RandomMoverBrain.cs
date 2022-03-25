namespace Jypeli
{
    /// <summary>
    /// Aivot, jotka laittavat omistajansa liikkumaan satunnaisesti eri suuntiin törmäyksissä ja tietyn ajan jälkeen.
    /// </summary>
    public class RandomMoverBrain : AbstractMoverBrain
    {
        private Timer changeDirectionTimer;
        private Angle currentDirection;

        /// <summary>
        /// Aika sekunteina, jonka kuluttua muutetaan liikesuuntaa
        /// </summary>
        public double ChangeMovementSeconds
        {
            get { return changeDirectionTimer.Interval; }
            set { changeDirectionTimer.Interval = value; }
        }

        /// <summary>
        /// Paikka, jonka ympärillä harhaillaan, jos WanderRadius on asetettu.
        /// </summary>
        public Vector WanderPosition { get; set; }

        /// <summary>
        /// Säde, jonka sisälle harhailu rajoittuu.
        /// </summary>
        public double WanderRadius { get; set; }

        /// <summary>
        /// Luo uudet satunnaisliikkujan aivot.
        /// </summary>
        public RandomMoverBrain()
            : base()
        {
            changeDirectionTimer = new Timer();
            changeDirectionTimer.Timeout += ChangeDirection;
            changeDirectionTimer.Interval = 3;

            ChangeDirection();
            WanderRadius = double.PositiveInfinity;
        }

        /// <summary>
        /// Luo uudet satunnaisliikkujan aivot ja asettaa niille nopeuden.
        /// </summary>
        public RandomMoverBrain(double speed)
            : this()
        {
            Speed = speed;
        }

        /// <inheritdoc/>
        protected override void OnAdd(IGameObject newOwner)
        {
            changeDirectionTimer.Start();
            WanderPosition = newOwner.Position;
            base.OnAdd(newOwner);
        }

        /// <inheritdoc/>
        protected override void OnRemove(IGameObject prevOwner)
        {
            changeDirectionTimer.Stop();
            base.OnRemove(prevOwner);
        }

        void ChangeDirection()
        {
            currentDirection = RandomGen.NextAngle();
        }

        /// <summary>
        /// Kutsutaan, kun tapahtuu törmäys.
        /// </summary>
        /// <param name="target">Olio, johon törmätään.</param>
        public override void OnCollision(IGameObject target)
        {
            Vector d = target.Position - Owner.Position;
            Angle n1 = d.Angle - Angle.RightAngle;
            Angle n2 = d.Angle + Angle.RightAngle;

            currentDirection = RandomGen.NextAngle(n1, n2);

            base.OnCollision(target);
        }

        /// <inheritdoc/>
        protected override void Update(Time time)
        {
            if (Owner == null)
                return;

            if (!double.IsInfinity(WanderRadius))
            {
                Vector d = WanderPosition - Owner.Position;
                if (d.Magnitude > WanderRadius)
                    currentDirection = d.Angle;
            }

            Move(currentDirection);
            base.Update(time);
        }
    }
}
