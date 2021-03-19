using System.Collections.Generic;

namespace Jypeli
{
    /// <summary>
    /// Aivot, jotka saavat omistajansa liikkumaan labyrinttimäisessä kentässä satunnaisesti edeten.
    /// </summary>
    public class LabyrinthWandererBrain : AbstractMoverBrain
    {
        private const double defaultSpeed = 100.0;
        private double tileSize;
        private string labyrinthWallTag;
        private Vector direction;
        private Vector destination;
        private double timeSinceNewDestination;
        private double directionChangeTimeout = 1.0;
        
        /// <summary>
        /// Jos jäädään jumiin, kuinka pian arvotaan uusi suunta. Oletuksena 1 sekunti.
        /// </summary>
        public double DirectionChangeTimeout
        {
            get
            {
                return directionChangeTimeout;
            }
            set
            {
                directionChangeTimeout = value;
            }
        }

        /// <summary>
        /// Tagi, jolla varustetut oliot tulkitaan seiniksi. Muilla tageilla varustettuja olioita ei väistetä.
        /// </summary>
        public string LabyrinthWallTag
        {
            get
            {
                return labyrinthWallTag;
            }
            set
            {
                labyrinthWallTag = value;
            }
        }

        
        /// <summary>
        /// Luo uudet aivot, jotka saavat omistajansa vaeltelemaan labyrintissä.
        /// </summary>
        /// <param name="tileSize">Kentän ruudun koko.</param>
        public LabyrinthWandererBrain(double tileSize)
            : base(defaultSpeed)
        {
            this.tileSize = tileSize;
        }


        /// <summary>
        /// Luo uudet aivot, jotka saavat omistajansa vaeltelemaan labyrintissä.
        /// </summary>
        /// <param name="tileSize">Kentän ruudun koko.</param>
        /// <param name="speed">Nopeus.</param>
        public LabyrinthWandererBrain(double tileSize, double speed)
            : base(speed)
        {
            this.tileSize = tileSize;
        }

        /// <summary>
        /// Luo uudet aivot, jotka saavat omistajansa vaeltelemaan labyrintissä.
        /// </summary>
        /// <param name="tileSize">Kentän ruudun koko.</param>
        /// <param name="speed">Nopeus.</param>
        /// <param name="wallTag">Tagilla varustetut oliot tulkitaan seiniksi, muita ei väistetä.</param>
        public LabyrinthWandererBrain(double tileSize, double speed, string wallTag)
            :base(speed)
        {
            this.tileSize = tileSize;
            labyrinthWallTag = wallTag;
        }


        protected override void OnAdd(IGameObject newOwner)
        {
            direction = RandomGen.NextDirection().GetVector() * tileSize;
            destination = newOwner.Position + direction;
            base.OnAdd(newOwner);
        }

        
        protected override void OnAddToGame()
        {
            Game.DoNextUpdate(SetNextDirectionAndDestination);
            base.OnAddToGame();
        }

        /// <summary>
        /// Asetetaan uusi suunta. Hakee olioita edestä ja sivuilta ja arpoo tyhjistä kohdista uuden suunnan.
        /// Jos eteen tai sivuille ei pääse, uusi suunta on taaksepäin.
        /// </summary>
        private void SetNextDirectionAndDestination()
        {
            PhysicsObject owner = this.Owner as PhysicsObject;

            if (owner == null || owner.Game == null) return;

            Game game = owner.Game;
            List<Vector> directions = new List<Vector>{direction, 
                                                        Vector.FromLengthAndAngle(tileSize, direction.Angle - Angle.RightAngle),
                                                        Vector.FromLengthAndAngle(tileSize, direction.Angle + Angle.RightAngle)};
            
            GameObject tile;
            double radius =  tileSize / 5;
            
            while (directions.Count > 0)
            {
                Vector directionCandidate = RandomGen.SelectOne<Vector>(directions);
                directions.Remove(directionCandidate);

                if (labyrinthWallTag != null)
                {
                    tile = game.GetObjectAt(owner.Position + directionCandidate, labyrinthWallTag,  radius);
                }
                else
                {
                    tile = game.GetObjectAt(owner.Position + directionCandidate, radius);
                }

                //PhysicsObjects only!
                if (tile == null || tile as PhysicsObject == null || (owner.CollisionIgnoreGroup != 0 && (tile as PhysicsObject).CollisionIgnoreGroup == owner.CollisionIgnoreGroup))
                {
                    direction = directionCandidate.Normalize() * tileSize;
                    //direction.X = Math.Round(direction.X);
                    //direction.Y = Math.Round(direction.Y);
                    
                    destination = owner.Position + direction;
                    return;
                }
                
            }

            direction = -direction.Normalize() * tileSize;
            //direction.X = Math.Round(direction.X);
            //direction.Y = Math.Round(direction.Y);
            
            destination = owner.Position + direction;
        }

        
        /// <summary>
        /// Liikuttaa omistajaa Move-metodilla.
        /// Asetetaan uusi suunta jos ollaan saavutettu annettu määränpää, annettu määränpää on kauempana kuin yksi ruudun koko tai edellisestä suunnan asettamisesta on kulunut riittävän pitkä aika.
        /// </summary>
        /// <param name="time"></param>
        protected override void Update(Time time)
        {
            base.Update(time);

            if (Owner != null)
            {
                Vector distanceToTravel = destination - Owner.Position;
                if (distanceToTravel.Magnitude < 0.1)
                {
                    SetNextDirectionAndDestination();
                    timeSinceNewDestination = time.SinceStartOfGame.TotalSeconds;
                }
                else if (distanceToTravel.Magnitude > tileSize)
                {
                    SetNextDirectionAndDestination();
                    timeSinceNewDestination = time.SinceStartOfGame.TotalSeconds;
                }
                else if (time.SinceStartOfGame.TotalSeconds - timeSinceNewDestination > DirectionChangeTimeout)
                {
                    //If stuck, let's change to opposite direction:
                    direction = -direction.Normalize() * tileSize;
                    SetNextDirectionAndDestination();
                    timeSinceNewDestination = time.SinceStartOfGame.TotalSeconds;
                }
                
                Move(direction.Angle);
            }
        }
    }
}
