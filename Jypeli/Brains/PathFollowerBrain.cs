using System;
using System.Collections.Generic;

namespace Jypeli.Assets
{
    /// <summary>
    /// Aivot, jotka seuraavat annettua polkua.
    /// </summary>
    public class PathFollowerBrain : AbstractMoverBrain
    {
        private List<Vector> path;
        private int wayPointIndex = 0;
        private int step = 1;
        private double _waypointRadius = 10;
        private int stoppedAtCount = -1;

        /// <summary>
        /// Polku, eli lista pisteistä joita aivot seuraa.
        /// </summary>
        public IList<Vector> Path
        {
            get { return path; }
            set
            {
                path = new List<Vector>( value );
                wayPointIndex = 0;
            }
        }

        /// <summary>
        /// Seuraavan pisteen indeksi.
        /// </summary>
        public int NextWaypointIndex
        {
            get { return wayPointIndex; }
            set { wayPointIndex = value; }
        }

        /// <summary>
        /// Seuraavan pisteen paikka.
        /// </summary>
        public Vector NextWaypoint
        {
            get { return Path[wayPointIndex]; }
        }

        /// <summary>
        /// Askel (listassa).
        /// Seuraavan pisteen indeksi = tämän pisteen indeksi + askel.
        /// Voi olla myös negatiivinen (mennään reittiä toiseen suuntaan).
        /// </summary>
        public int Step
        {
            get { return step; }
            set { step = value; }
        }

        /// <summary>
        /// Etäisyys, jonka sisällä ollaan perillä pisteessä.
        /// </summary>
        public double WaypointRadius
        {
            get { return _waypointRadius; }
            set { _waypointRadius = value; }
        }

        /// <summary>
        /// Etäisyys seuraavaan pisteeseen.
        /// </summary>
        public DoubleMeter DistanceToWaypoint { get; private set; }

        /// <summary>
        /// Jos true, palataan alkupisteeseen ja kierretään reittiä loputtomiin.
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// Palataanko samaa reittiä takaisin.
        /// </summary>
        public bool ReverseReturn { get; set; }

        /// <summary>
        /// Tapahtuu, kun saavutetaan reitin piste.
        /// </summary>
        public event Action ArrivedAtWaypoint;

        /// <summary>
        /// Tapahtuu, kun saavutaan reitin päähän.
        /// </summary>
        public event Action ArrivedAtEnd;

        /// <summary>
        /// Luo uudet polunseuraaja-aivot.
        /// </summary>
        public PathFollowerBrain()
            : base()
        {
            Path = new Vector[] { };
            DistanceToWaypoint = new DoubleMeter(double.PositiveInfinity, 0, double.PositiveInfinity);
        }

        /// <summary>
        /// Luo uudet polunseuraaja-aivot ja asettaa niille nopeuden.
        /// </summary>
        public PathFollowerBrain( double speed )
            : this()
        {
            Speed = speed;
        }

        /// <summary>
        /// Luo aivot, jotka seuraavat polkua <c>path</c>.
        /// </summary>
        public PathFollowerBrain( params Vector[] path )
        {
            this.Path = path;
            DistanceToWaypoint = new DoubleMeter( double.PositiveInfinity, 0, double.PositiveInfinity );
        }

        /// <summary>
        /// Luo aivot, jotka seuraavat polkua <c>path</c>.
        /// </summary>
        public PathFollowerBrain( double speed, params Vector[] path )
        {
            this.Speed = speed;
            this.Path = path;
            DistanceToWaypoint = new DoubleMeter( double.PositiveInfinity, 0, double.PositiveInfinity );
        }

        /// <summary>
        /// Luo aivot, jotka seuraavat polkua <c>path</c>.
        /// </summary>
        public PathFollowerBrain( List<Vector>path )
        {
            this.Path = path.ToArray();
            DistanceToWaypoint = new DoubleMeter( double.PositiveInfinity, 0, double.PositiveInfinity );
        }

        /// <summary>
        /// Luo aivot, jotka seuraavat polkua <c>path</c>.
        /// </summary>
        public PathFollowerBrain( double speed, List<Vector> path )
        {
            this.Speed = speed;
            this.Path = path.ToArray();
            DistanceToWaypoint = new DoubleMeter( double.PositiveInfinity, 0, double.PositiveInfinity );
        }

        protected override void Update(Time time)
        {
            if ( Owner == null || Path == null || Path.Count == 0 ) return;

            if ( wayPointIndex < 0 )
                wayPointIndex = 0;
            else if ( wayPointIndex >= Path.Count )
                wayPointIndex = Path.Count - 1;

            if ( stoppedAtCount >= 0 )
            {
                if ( stoppedAtCount == Path.Count )
                    return;

                stoppedAtCount = -1;
            }

            Vector target = path[wayPointIndex];
            Vector dist = target - Owner.Position;
            DistanceToWaypoint.Value = dist.Magnitude;

            if ( DistanceToWaypoint.Value < WaypointRadius )
            {
                // Arrived at waypoint
                Arrived();
            }
            else if ( DistanceToWaypoint.Value > 2 * Speed * time.SinceLastUpdate.TotalSeconds )
            {
                // Continue moving
                Move( dist.Angle );
            }

            base.Update(time);
        }

        private void OnArrivedAtWaypoint()
        {
            if ( ArrivedAtWaypoint != null )
                ArrivedAtWaypoint();
        }

        private void OnArrivedAtEnd()
        {
            if ( ArrivedAtEnd != null )
                ArrivedAtEnd();

            if ( Owner is PhysicsObject )
                ( (PhysicsObject)Owner ).Stop();
        }

        private void Arrived()
        {
            if ( Path == null || Path.Count == 0 || wayPointIndex >= path.Count )
                return;

            OnArrivedAtWaypoint();

            int nextIndex = wayPointIndex + step;

            if ( nextIndex < 0 || nextIndex >= path.Count )
            {
                OnArrivedAtEnd();

                if ( ReverseReturn )
                {
                    step = -step;
                    wayPointIndex = nextIndex + step;
                }
                else if ( Loop )
                {
                    wayPointIndex = nextIndex + Math.Sign( step ) * Path.Count;
                    while ( wayPointIndex < 0 ) wayPointIndex += Path.Count;
                    while ( wayPointIndex >= Path.Count ) wayPointIndex -= Path.Count;
                }
                else
                {
                    stoppedAtCount = Path.Count;
                }

                return;
            }

            wayPointIndex = nextIndex;
        }
    }
}
