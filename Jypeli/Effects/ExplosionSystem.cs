
namespace Jypeli.Effects
{
    /// <summary>
    /// Järjestelmä räjähdyksille
    /// </summary>
    public class ExplosionSystem : ParticleSystem
    {
        /// <summary>
        /// Räjähdys, jonka tekstuuria ja maksimipartikkeleiden määrää voi vaihtaa
        /// </summary>
        /// <param name="particleImage">Partikkelin kuva.</param>
        /// <param name="maxAmountOfParticles">Suurin mahdollinen määrä partikkeleita joita järjestelmä voi käyttää</param>
        public ExplosionSystem(Image particleImage, int maxAmountOfParticles)
            : base(particleImage, maxAmountOfParticles)
        {
        }

        /// <summary>
        /// Määritetään oletusarvot efektille
        /// </summary>
        protected override void InitializeParticles()
        {
            MinLifetime = .5;
            MaxLifetime = .8;

            MinScale = 70;
            MaxScale = 100;

            MinVelocity = 70;
            MaxVelocity = 450;

            MinRotation = 0;
            MaxRotation = MathHelper.PiOver2;

            ScaleAmount = 1.0;
            AlphaAmount = 1.0;
            MinAcceleration = 1;
            MaxAcceleration = 2;

            MinRotationSpeed = -MathHelper.PiOver4;
            MaxRotationSpeed = MathHelper.PiOver4;

        }

        /// <summary>
        /// Alustetaan partikkeli vastaamaan räjähdyksen partikkelia
        /// </summary>
        /// <param name="p">Partikkeli joka alustetaan</param>
        /// <param name="position">Sijainti johon alustetaan</param>
        protected override void InitializeParticle(Particle p, Vector position)
        {
            base.InitializeParticle(p, position);
            // Asetetaan partikkelit käyttäytymään räjähdykselle ominaisella tavalla
            if (!IgnoreWind)
                p.Acceleration = -p.Velocity / p.MaxLifetime.TotalMilliseconds + Game.Wind;
            else
                p.Acceleration = -p.Velocity / p.MaxLifetime.TotalMilliseconds;
        }
    }
}
