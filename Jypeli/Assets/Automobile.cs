using System;


namespace Jypeli.Assets
{
    /// <summary>
    /// Auto.
    /// </summary>
    public class Automobile : PhysicsObject
    {
        private static Image commonImage = null;

        private double topspeed;

        /// <summary>
        /// Nopeusmittari, joka mittaa auton nopeuden (moottorin nopeus).
        /// Huomaa, että ei vastaa aina <c>Velocity</c>-ominaisuuden arvoa.
        /// </summary>
        public DoubleMeter SpeedMeter { get; set; }

        /// <summary>
        /// Auton moottorin nopeus.
        /// Huomaa, että ei vastaa aina <c>Velocity</c>-ominaisuuden arvoa.
        /// </summary>
        public double Speed
        {
            get { return SpeedMeter.Value; }
        }

        /// <summary>
        /// Suurin nopeus, johon auto voi kiihdyttää.
        /// </summary>
        public double TopSpeed
        {
            get { return topspeed; }
            set
            {
                topspeed = value;
                SpeedMeter.MaxValue = topspeed;
            }
        }

        /// <summary>
        /// Auton kiihtyvyys, eli kuinka monta yksikköä korkeintaan nopeutta voidaan
        /// lisätä sekunnissa kiihdyttämällä.
        /// </summary>
        public new double Acceleration { get; set; }

        /// <summary>
        /// Jarrujen hidastavuus, eli kuinka monta yksikköä korkeintaan nopeutta voidaan
        /// vähentää sekunnissa jarruttamalla.
        /// </summary>
        public double BrakeDeceleration { get; set; }

        /// <summary>
        /// Ohjattavuus, eli kulma jonka auto voi korkeintaan kääntyä sekunnissa.
        /// </summary>
        public Angle Maneuverability { get; set; }

        private double pendingAcceleration = 0;
        private double pendingDeceleration = 0;

        /// <summary>
        /// Alustaa uuden auton.
        /// </summary>
        /// <param name="width">Auton leveys (X-suunnassa).</param>
        /// <param name="height">Auton korkeus (Y-suunnassa).</param>
        public Automobile( double width, double height )
            : base( width, height, Shape.Rectangle )
        {
            SpeedMeter = new DoubleMeter( 0 );
            TopSpeed = 1000;
            Acceleration = 100;
            BrakeDeceleration = 200;
            Maneuverability = Angle.FromDegrees( 20 );
            if ( commonImage == null )
                commonImage = Game.LoadImageFromResources( "Auto.png" );
            Image = commonImage;// Image.Color( commonImage, Color ); // TODO: Image coloring.
            IsUpdated = true;
        }

        /// <summary>
        /// Kiihdyttää.
        /// </summary>
        /// <param name="time">Kuinka monta sekuntia kiihdytetään.</param>
        public void Accelerate( double time )
        {
            pendingAcceleration += Acceleration * time;
        }

        /// <summary>
        /// Jarruttaa.
        /// </summary>
        /// <param name="time">Kuinka monta sekuntia jarrutetaan.</param>
        public void Brake( double time )
        {
            pendingDeceleration += BrakeDeceleration * time;
        }

        /// <summary>
        /// Kiihdyttää.
        /// </summary>
        public void Accelerate()
        {
            pendingAcceleration += Acceleration * Game.Time.SinceLastUpdate.TotalSeconds;
        }

        /// <summary>
        /// Kiihdyttää takaperin.
        /// </summary>
        public void Reverse()
        {
            pendingAcceleration += -Acceleration * Game.Time.SinceLastUpdate.TotalSeconds;
        }

        /// <summary>
        /// Jarruttaa.
        /// </summary>
        public void Brake()
        {
            pendingDeceleration += BrakeDeceleration * Game.Time.SinceLastUpdate.TotalSeconds;
        }

        /// <summary>
        /// Kääntyy niin paljon kuin auton ohjattavuus sallii.
        /// </summary>
        /// <param name="angle">Kääntökulma.</param>
        /// <param name="time">Aika, joka kääntämiseen käytetään.</param>
        public void Turn( Angle angle, double time )
        {
            int sign = Math.Sign( angle.Radians );
            Angle += ( sign * angle <= time * Maneuverability ) ? angle : sign * time * Maneuverability;
        }

        /// <summary>
        /// Ajetaan kun pelitilannetta päivitetään. Päivityksen voi toteuttaa omassa luokassa toteuttamalla tämän
        /// metodin. Perityn luokan metodissa tulee kutsua kantaluokan metodia.
        /// </summary>
        public override void Update( Time time )
        {
            double dt = time.SinceLastUpdate.TotalSeconds;
            bool nochange = true;

            if ( pendingAcceleration != 0 )
            {
                // Accelerate                
                double accel = Math.Min( pendingAcceleration, Acceleration * dt );
                pendingAcceleration -= accel;

                Velocity += Vector.FromLengthAndAngle( accel, Angle );
                SpeedMeter.Value += accel;

                nochange = false;
            }

            if ( pendingDeceleration > 0 )
            {
                // Brake
                double decel = Math.Min(Math.Min((float)pendingDeceleration, (float)(BrakeDeceleration * dt)), (float)Velocity.Magnitude);
                pendingDeceleration -= decel;

                Velocity += Vector.FromLengthAndAngle( -decel, Velocity.Angle );
                SpeedMeter.Value -= decel;

                nochange = false;

            }

            if ( nochange )
            {
                SpeedMeter.Value -= dt * Acceleration;
            }

            base.Update( time );
        }
    }
}
