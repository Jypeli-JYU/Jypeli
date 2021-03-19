using System;
using System.Collections.Generic;

namespace Jypeli.Assets
{
    /// <summary>
    /// Ajajan aivot.
    /// Laskee checkpointteja ja kierroksia automaattisesti.
    /// Nämä aivot ovat ihmispelaajalle, käytä tietokoneautoille <c>ComputerDriverBrain</c>-luokkaa.
    /// </summary>
    public class DriverBrain : Brain
    {
        private List<PhysicsObject> chkpoints;

        /// <summary>
        /// Lista checkpointeista eli tarkistuspisteistä radalla.
        /// Auton tulee ajaa kaikkien näiden pisteiden läpi oikeassa järjestyksessä, että kierros
        /// lasketaan suoritetuksi.
        /// </summary>
        public List<PhysicsObject> Checkpoints
        {
            get { return chkpoints; }
            set
            {
                chkpoints = value;
                CheckpointsPassed.Value = 0;
                CheckpointsPassed.MaxValue = value.Count;
            }
        }

        /// <summary>
        /// Seuraava tarkistuspiste.
        /// </summary>
        public PhysicsObject NextCheckpoint
        {
            get
            {
                return ( CheckpointsPassed.Value < Checkpoints.Count ) ? Checkpoints[CheckpointsPassed.Value] : null;
            }
        }

        /// <summary>
        /// Mittari joka mittaa ajettuja kierroksia.
        /// </summary>
        public IntMeter RoundsDriven { get; set; }

        /// <summary>
        /// Mittari joka mittaa läpäistyjä tarkistuspisteitä tällä kierroksella.
        /// </summary>
        public IntMeter CheckpointsPassed { get; set; }


        /// <summary>
        /// Alustaa uudet ajajan aivot.
        /// </summary>
        /// <param name="checkpoints">Tarkistuspisteet, joiden läpi pelaajan on ajettava.</param>
        /// <param name="rounds">Kierrosten määrä.</param>
        public DriverBrain( List<PhysicsObject> checkpoints, int rounds )
        {
            RoundsDriven = new IntMeter( 0 );
            RoundsDriven.MaxValue = rounds;

            CheckpointsPassed = new IntMeter( 0 );
            CheckpointsPassed.MaxValue = 1;
            CheckpointsPassed.UpperLimit += nextRound;

            Checkpoints = checkpoints;
        }

        private void nextRound()
        {
            CheckpointsPassed.Reset();
            ++RoundsDriven.Value;
        }

        /// <summary>
        /// Kutsutaan, kun tapahtuu törmäys.
        /// </summary>
        /// <param name="target">Toinen törmääjä.</param>
        public override void OnCollision( IGameObject target )
        {            
            if ( target == NextCheckpoint )
            {
                // Checkpoint reached, set next
                CheckpointsPassed.MaxValue = Checkpoints.Count;
                ++CheckpointsPassed.Value;
            }

            base.OnCollision( target );
        }
    }

    /// <summary>
    /// Tietokoneajajan aivot.
    /// Kuten <c>DriverBrain</c>, mutta osaa automaattisesti suunnistaa checkpointilta toiselle
    /// vauhtia säädellen.
    /// </summary>
    public class ComputerDriverBrain : DriverBrain
    {
        /// <summary>
        /// Alustaa uudet tietokoneajajan aivot.
        /// </summary>
        /// <param name="checkpoints">Tarkistuspisteet, joiden läpi pelaajan on ajettava.</param>
        /// <param name="rounds">Kierrosten määrä.</param>
        public ComputerDriverBrain( List<PhysicsObject> checkpoints, int rounds )
            : base( checkpoints, rounds )
        {
        }

        /// <summary>
        /// Kutsutaan, kun tilaa päivitetään.
        /// Ajamislogiikka sijaitsee täällä.
        /// </summary>
        /// <param name="time">The game time.</param>
        protected override void Update( Time time )
        {
            Automobile OwnerAuto = Owner as Automobile;

            if ( OwnerAuto != null )
            {
                Vector distance = NextCheckpoint.Position - OwnerAuto.Position;
                Angle turnAngle = distance.Angle - OwnerAuto.Angle;

                double dt = time.SinceLastUpdate.TotalSeconds;
                double dist = Math.Max( distance.Magnitude, Double.Epsilon );
                double spd = Math.Max( OwnerAuto.Velocity.Magnitude, Double.Epsilon );
                double eta = dist / spd;
                double braketime = spd / OwnerAuto.BrakeDeceleration;
                double turntime = Math.Abs( turnAngle.Radians ) / OwnerAuto.Maneuverability.Radians;                

                if ( turntime >= eta || braketime * OwnerAuto.KineticFriction >= eta )
                {
                    OwnerAuto.Brake( dt );
                }

                else
                {
                    OwnerAuto.Accelerate( dt );
                }

                OwnerAuto.Turn( turnAngle, dt );
            }

            base.Update( time );
        }
    }
}
