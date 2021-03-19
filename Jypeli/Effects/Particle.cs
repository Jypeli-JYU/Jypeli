using System;

namespace Jypeli.Effects
{
    /// <summary>
    /// Partikkeli
    /// </summary>
    public class Particle
    {
        #region Variables, getters and setters

        private Vector position;
        private double scale;
        private double rotation;
        private double rotationSpeed;
        private Vector velocity;
        private Vector acceleration;
        private TimeSpan maxLifetime;
        private TimeSpan creationTime;

        /// <summary>
        /// Partikkelin sijainti
        /// </summary>
        public Vector Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Partikkelin skaalaus
        /// </summary>
        public double Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        /// <summary>
        /// Partikkelin kierto
        /// </summary>
        public double Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// Partikkelin kiertonopeus
        /// </summary>
        public double RotationSpeed
        {
            get { return rotationSpeed; }
            set { rotationSpeed = value; }
        }

        /// <summary>
        /// Partikkelin nopeus
        /// </summary>
        public Vector Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        /// <summary>
        /// Partikkelin kiihtyvyys
        /// </summary>
        public Vector Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }

        /// <summary>
        /// Onko partikkelin "elossa", eli päivitetäänkö ja piirretäänkö se
        /// </summary>
        public bool Alive
        {
            get { return Lifetime < MaxLifetime; }
        }

        /// <summary>
        /// Partikkelin elinikä
        /// </summary>
        public TimeSpan MaxLifetime
        {
            get { return maxLifetime; }
            set { maxLifetime = value; }
        }

        /// <summary>
        /// Partikkelin tämän hetkinen ikä
        /// </summary>
        public TimeSpan Lifetime
        {
            get { return Game.Time.SinceStartOfGame - creationTime; }
        }

        #endregion

        /// <summary>
        /// Alusta partikkeli
        /// </summary>
        /// <param name="position">Sijainti</param>
        /// <param name="scale">Skaalaus</param>
        /// <param name="rotation">Kierto</param>
        /// <param name="rotationSpeed">Kiertonopeus</param>
        /// <param name="velocity">Nopeus</param>
        /// <param name="acceleration">Kiihtyvyys</param>
        /// <param name="lifetime">Elinikä</param>
        public void Initialize(Vector position, double scale, double rotation, double rotationSpeed, Vector velocity, Vector acceleration, double lifetime)
        {
            this.position = position;
            this.scale = scale;
            this.maxLifetime = TimeSpan.FromSeconds(lifetime);
            this.creationTime = Game.Time.SinceStartOfGame;
            
            this.velocity = velocity;
            this.rotationSpeed = rotationSpeed;
            this.acceleration = acceleration;

            this.rotation = rotation;
        }


        /// <summary>
        /// Päivittää partikkelin sijannin, nopeuden ja kierron
        /// </summary>
        /// <param name="time">Aika viime päivityksestä</param>
        public void Update(double time)
        {
            position += velocity * time;
            velocity += acceleration * time;

            rotation += rotationSpeed * time;
        }
    }
}
