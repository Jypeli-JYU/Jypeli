using System;

namespace Jypeli
{
    /// <summary>
    /// Aivot, jotka laittavat omistajansa hortoilemaan tasohyppelytasoa
    /// edestakaisin.
    /// </summary>
    public class PlatformWandererBrain : Brain
    {
        private double _speed = 50;
        private double _jumpSpeed = 200;
        private bool _fallsOffPlatforms = false;
        private bool _triesToJump = false;
        private Vector lastJumpingPosition;

        /// <summary>
        /// Suunta, johon aivot ovat ohjaamassa sen hallitsemaa oliota
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        /// Hyppynopeus.
        /// </summary>
        public double JumpSpeed
        {
            get { return _jumpSpeed; }
            set { _jumpSpeed = value; }
        }

        /// <summary>
        /// Nopeus.
        /// </summary>
        public double Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        /// <summary>
        /// Tippuuko aivojen omistaja tasojen reunoilta.
        /// </summary>
        public bool FallsOffPlatforms
        {
            get { return _fallsOffPlatforms; }
            set { _fallsOffPlatforms = value; }
        }

        /// <summary>
        /// Yrittääkö aivojen omistaja hypätä esteen päälle kun se kävelee esteeseen.
        /// </summary>
        public bool TriesToJump
        {
            get { return _triesToJump; }
            set { _triesToJump = value; }
        }

        /// <summary>
        /// Aivot, jotka laittavat omistajansa hortoilemaan tasohyppelytasoa
        /// edestakaisin.
        /// </summary>
        public PlatformWandererBrain()
        {
            Direction = Direction.Right;
        }

        /// <inheritdoc/>
        protected override void OnAddToGame()
        {
            base.OnAddToGame();
            lastJumpingPosition = this.Owner.Position;
        }

        /// <inheritdoc/>
        /// <param name="target"></param>
        public override void OnCollision( IGameObject target )
        {
            //if ( target is PhysicsObject && Owner.Y > target.Y && target.Width > Owner.Width )
            //{
            //    platform = (PhysicsObject)target;

            //    // ...
            //    platform.Color = RandomGen.NextColor();
            //    platform.Image = null;
            //}

            base.OnCollision( target );
        }

        /// <inheritdoc/>
        protected override void Update(Time time)
        {
            
            if (!(this.Owner is PlatformCharacter))
            {
                return;
            }

            //Calculate from Speed later?
            double yTolerance = 10.0;
            double xTolerance = 10.0;

            PlatformCharacter pc = this.Owner as PlatformCharacter;

            if (_triesToJump)
            {
                //Brains are walking against a wall:
                if (Math.Abs(pc.Velocity.X) < 5)
                {
                    //If position hasn't changed since last jump, change direction.
                    if ((pc.Position - lastJumpingPosition).Magnitude < 1)
                    {
                        pc.Stop();
                        this.Speed *= -1;
                    }
                    else
                    {
                        pc.Jump(JumpSpeed);
                        lastJumpingPosition = pc.Position;

                        //Brains don't change direction in mid-air while jumping:
                        if(!_fallsOffPlatforms)
                        {
                            _fallsOffPlatforms = true;
                            Timer.SingleShot(0.5, delegate { _fallsOffPlatforms = false;});
                        }
                    }
                }
            }

            //Changes direction if it's about to fall off a platform:
            if (!_fallsOffPlatforms && pc.IsAboutToFall() && Math.Abs(pc.Velocity.Y) < yTolerance)
            {
                pc.Stop();

                if (_triesToJump && Math.Abs(pc.Velocity.X) < xTolerance) this.Speed *= -1;
            }
    
            if (!_triesToJump && Math.Abs(pc.Velocity.X) < xTolerance) this.Speed *= -1;

            pc.Walk(this.Speed);

            base.Update(time);

        }
    }
}
