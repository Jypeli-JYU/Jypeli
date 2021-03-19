using System;
using System.Collections.Generic;

namespace Jypeli
{
    /// <summary>
    /// Aivot, joiden tehtävänä on laittaa omistajansa seuraamaan jotakin kohdetta.
    /// </summary>
    public class FollowerBrain : AbstractMoverBrain
    {
        double _logicDelay = 0;
        double remainingDelay = 0;
        Vector targetPosition;

        #region Properties

        /// <summary>
        /// Tagit, joita seurataan.
        /// </summary>
        public List<string> TagsToFollow { get; set; }

        /// <summary>
        /// Oliot, joita seurataan.
        /// </summary>
        public List<IGameObject> ObjectsToFollow { get; set; }

        /// <summary>
        /// Metodi, joka vertailee kahta oliota keskenään.
        /// Kumpi olio tulee ensimmäiseksi, sitä seurataan.
        /// </summary>
        public Comparison<IGameObject> FollowComparer { get; set; }

        /// <summary>
        /// Olio, jota parhaillaan seurataan.
        /// </summary>
        public IGameObject CurrentTarget { get; protected set; }

        /// <summary>
        /// Etäisyys lähimpään kohteeseen.
        /// </summary>
        public DoubleMeter DistanceToTarget { get; protected set; }

        /// <summary>
        /// Etäisyys, jolloin ollaan lähellä kohdetta.
        /// </summary>
        public double DistanceClose { get; set; }

        /// <summary>
        /// Etäisyys, jolloin ollaan kaukana kohteesta ja lopetetaan sen seuraaminen.
        /// </summary>
        public double DistanceFar { get; set; }

        /// <summary>
        /// Aivot, joita käytetään näiden sijasta kun ollaan kaukana kaikista kohteista,
        /// esim. RandomMoverBrain
        /// </summary>
        public Brain FarBrain { get; set; }

        /// <summary>
        /// Aivot, joita käytetään näiden sijasta kun ollaan lähellä kohdestta
        /// </summary>
        public Brain CloseBrain { get; set; }

        /// <summary>
        /// Tapahtuma, joka suoritetaan, kun ollaan tarpeeksi lähellä seurattavaa.
        /// </summary>
        public event Action TargetClose;

        /// <summary>
        /// Pysähdytäänkö, kun ollaan lähellä kohdetta.
        /// </summary>
        /// <value>
        /// 	<c>true</c> jos pysähdytään; jos ei, niin <c>false</c>.
        /// </value>
        public bool StopWhenTargetClose { get; set; }

        /// <summary>
        /// Viive seuraamislogiikassa. Oletuksena nolla (seuraa reaaliajassa)
        /// </summary>
        public double Delay
        {
            get { return _logicDelay; }
            set
            {
                _logicDelay = value;
                remainingDelay = ( Owner == null ) ? 0 : value;
            }
        }

        #endregion

        #region Comparers

        /// <summary>
        /// Luo FollowComparer-vertailijan, jolla aivot seuraavat aina lähintä oliota.
        /// </summary>
        /// <param name="changeTargetDistance">
        /// Ero kahden olion etäisyyden välillä ennen kuin vaihdetaan seurattavaa kohdetta.
        /// Mitä pienempi arvo, sitä helpommin kohdetta vaihdetaan.
        /// </param>
        /// <returns></returns>
        public Comparison<IGameObject> CreateDistanceComparer( double changeTargetDistance )
        {
            return delegate ( IGameObject obj1, IGameObject obj2 )
            {
                if ( Owner == null ) return 0;
                if ( obj1 == null ) return 1;
                if ( obj2 == null ) return -1;

                double d1 = Vector.Distance( Owner.Position, obj1.Position );
                double d2 = Vector.Distance( Owner.Position, obj2.Position );
                double diff = Math.Abs( d1 - d2 );

                if ( CurrentTarget == obj1 && diff < changeTargetDistance ) return -1;
                if ( CurrentTarget == obj2 && diff < changeTargetDistance ) return 1;

                return d1.CompareTo( d2 );
            };
        }

        #endregion

        /// <summary>
        /// Luo aivot.
        /// </summary>
        public FollowerBrain()
            : this( null )
        {
        }

        /// <summary>
        /// Luo aivot ja asettaa ne seuraamaan yhtä tai useampaa kohdetta.
        /// </summary>
        /// <param name="targets">Seurattavat oliot. Voit antaa olioiden lisäksi myös tageja.</param>
        public FollowerBrain( params object[] targets )
            : base()
        {
            ObjectsToFollow = new List<IGameObject>();
            TagsToFollow = new List<string>();
            DistanceToTarget = new DoubleMeter( double.PositiveInfinity, 0, double.PositiveInfinity );
            FollowComparer = CreateDistanceComparer( 20 );
            DistanceFar = double.PositiveInfinity;
            DistanceClose = 100.0;
            StopWhenTargetClose = false;
            FarBrain = Brain.None;

            if (targets == null)
                return;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] is IGameObject)
                    ObjectsToFollow.Add((IGameObject)targets[i]);
                else if (targets[i] is string)
                    TagsToFollow.Add((string)targets[i]);
                else
                    throw new ArgumentException(string.Format("Target type not recognized: {0} ({1})", targets[i].ToString(), targets[i].GetType().Name));
            }
        }

        private void SelectTarget()
        {
            CurrentTarget = null;

            foreach ( var layer in Game.Instance.Layers )
            {
                foreach ( var obj in layer.Objects )
                {
                    if (obj == Owner || obj.IsDestroyed)
                        continue;

                    if ( !ObjectsToFollow.Contains( obj ) && !TagsToFollow.Contains( obj.Tag as string ) )
                        continue;

                    if ( CurrentTarget == null || FollowComparer( CurrentTarget, obj ) > 0 )
                        CurrentTarget = obj;
                }
            }

            if ( Owner != null && CurrentTarget != null )
                DistanceToTarget.Value = Vector.Distance( Owner.Position, CurrentTarget.Position );
            else
                DistanceToTarget.Value = double.PositiveInfinity;
        }

        private void SetTargetPosition( double dt )
        {
            targetPosition = CurrentTarget.Position;

            if ( Delay > 0 && DistanceToTarget.Value > float.Epsilon )
            {
                double maxlength = Math.Sqrt( Math.Pow( Game.Instance.Level.Width, 2 ) + Math.Pow( Game.Instance.Level.Height, 2 ) );
                targetPosition += Vector.FromLengthAndAngle( maxlength, ( CurrentTarget.Position - Owner.Position ).Angle );
            }

            IPhysicsObject physTarget = CurrentTarget as IPhysicsObject;

            if ( physTarget != null )
            {
                // Take speed and acceleration into account
                targetPosition += ( ( physTarget.Acceleration * dt ) + physTarget.Velocity ) * dt;
            }
        }

        /// <summary>
        /// Kutsutaan, kun tilaa päivitetään.
        /// Suurin osa päätöksenteosta tapahtuu täällä.
        /// </summary>
        protected override void Update( Time time )
        {
            double dt = Game.Time.SinceLastUpdate.TotalSeconds;
            if ( dt <= 0 ) return;
            
            remainingDelay -= dt;
            if ( remainingDelay <= 0 || CurrentTarget == null || !CurrentTarget.IsAddedToGame )
            {
                SelectTarget();
                if ( CurrentTarget == null )
                    return;

                SetTargetPosition( dt );
                remainingDelay = Delay;
            }
            else
            {
                DistanceToTarget.Value = Vector.Distance( Owner.Position, CurrentTarget.Position );

                if ( TurnWhileMoving )
                {
                    Vector realDist = CurrentTarget.Position - Owner.Position;
                    if ( !double.IsNaN( realDist.Magnitude ) && !double.IsInfinity( realDist.Magnitude ) )
                        Turn( realDist.Angle );
                }
            }
            
            double distance = DistanceToTarget.Value;
            bool targetClose = Math.Abs( distance ) < DistanceClose;
            bool targetFar = Math.Abs( distance ) > DistanceFar;
            if ( targetClose && TargetClose != null ) TargetClose();

            if ( targetClose )
            {
                if (CloseBrain != null)
                {
                    CloseBrain.Owner = this.Owner;
                    CloseBrain.DoUpdate(time);
                }

                if (StopWhenTargetClose)
                {
                    if (Owner is PhysicsObject)
                        ((PhysicsObject)Owner).Stop();

                    return;
                }
            }

            if ( targetFar )
            {
                FarBrain.Owner = this.Owner;
                FarBrain.DoUpdate( time );
                return;
            }

            Vector d = targetPosition - Owner.Position;
            if ( d.Magnitude > float.Epsilon )
                Move( d / dt );
            else if ( Owner is PhysicsObject )
                ( (PhysicsObject)Owner ).Stop();
            
            base.Update( time );
        }
    }
}
