using System;

namespace Jypeli.Effects
{
    /// <summary>
    /// Savuefekti.
    /// </summary>
    public class Smoke : ParticleSystem
    {
        private double addTime;
        private double width;

        /// <summary>
        /// Savu, johon vaikuttaa tuuli
        /// </summary>
        public Smoke()
            : base(Game.LoadImageFromResources("Smoke.png"), 180)
        {
            this.Angle = Angle.FromDegrees(90);
            this.width = 5;
        }

        /// <summary>
        /// Savu, johon vaikuttaa tuuli ja jonka tekstuuria ja leveyttä voi vaihtaa.
        /// </summary>
        /// <param name="particleImage">Partikkelin kuva</param>
        /// <param name="width">Savun leveys</param>
        public Smoke(Image particleImage, double width)
            : base(particleImage, (int)width * 8)
        {
            this.Angle = Angle.FromDegrees(90);
            this.width = width;
        }

        /// <summary>
        /// Määritetään oletusarvot efektille
        /// </summary>
        protected override void InitializeParticles()
        {
            MinLifetime = 4.0;
            MaxLifetime = 4.2;

            MinScale = 70;
            MaxScale = 100;

            ScaleAmount = 1;

            AlphaAmount = 0.3;

            MinVelocity = 40;
            MaxVelocity = 60;

            MinAcceleration = 1;
            MaxAcceleration = 1;

            MinRotationSpeed = -MathHelper.PiOver4;
            MaxRotationSpeed = MathHelper.PiOver4;

        }

        /// <summary>
        /// Lasketaan savun suunnalle satunnaisuutta
        /// </summary>
        /// <returns>Partikkelin suunta</returns>
        protected override Vector GiveRandomDirection()
        {
            return Vector.FromLengthAndAngle(1, Angle + Angle.FromDegrees(RandomGen.NextDouble(-MaxAngleChange, MaxAngleChange)));
        }

        /// <summary>
        /// Päivitetään savua
        /// </summary>
        /// <param name="time"></param>
        public override void Update(Time time)
        {
            double t = time.SinceLastUpdate.TotalSeconds;
            addTime += t;
            if (addTime > 0.1)
            {
                for (int i = 0; i < (int)Math.Ceiling(width / 50); i++)
                {
                    base.AddEffect(RandomGen.NextDouble(Position.X - width / 2, Position.X + width / 2), Position.Y, 2);
                }

                addTime = 0;
            }
            base.Update(time);
        }

        /// <summary>
        /// Alustetaan partikkeli
        /// </summary>
        /// <param name="p">Partikkeli joka alustetaan</param>
        /// <param name="position">Sijainti johon alustetaan</param>
        protected override void InitializeParticle(Particle p, Vector position)
        {
            base.InitializeParticle(p, position);

            if (!IgnoreWind)
                p.Acceleration = Game.Wind;

        }
    }
}
