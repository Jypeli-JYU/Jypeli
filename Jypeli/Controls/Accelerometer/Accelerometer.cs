using System;
using Jypeli.Controls;
using Microsoft.Xna.Framework;

namespace Jypeli
{
    public class DummyAccelerometer : Accelerometer
    {
    }

    public abstract class Accelerometer : Controller<Vector3, AccelerometerDirection>
    {
        protected bool started = false;

        /// <summary>
        /// Puhelimen kallistuksen nollakohta.
        /// </summary>
        public AccelerometerCalibration Calibration { get; set; }

        /// <summary>
        /// Kallistuksen t‰m‰nhetkinen suunta.
        /// </summary>
        public Vector Reading
        {
            get
            {
                return Project2d( CurrentState );
            }
        }

        /// <summary>
        /// Kallistuksen edellinen suunta.
        /// </summary>
        public Vector PreviousReading
        {
            get
            {
                return Project2d( PrevState );
            }
        }

        public Vector3 Reading3d
        {
            get
            {
                return CurrentState;
            }
        }

        public Vector3 PreviousReading3d
        {
            get
            {
                return PrevState;
            }
        }

        /// <summary>
        /// N‰ytˆn suunta.
        /// </summary>
        public DisplayOrientation DisplayOrientation
        {
            get { return Game.Device.DisplayOrientation; }
        }

        /// <summary>
        /// Herkkyys jos kuunnellaan suuntia ja eleit‰ ilman erikseen annettua herkkyytt‰.
        /// </summary>
        public double DefaultSensitivity { get; private set; }

        /// <summary>
        /// Herkkyys jos kuunnellaan suuntia analogisesti ilman erikseen annettua herkkyytt‰.
        /// </summary>
        public double DefaultAnalogSensitivity { get; private set; }

        /// <summary>
        /// M‰‰ritt‰‰ onko Shake ja Tap k‰ytˆss‰.
        /// </summary>
        public bool GesturesEnabled { get; set; }

        /// <summary>
        /// Aika millisekunteina joka pit‰‰ kulua napautusten v‰liss‰.
        /// </summary>
        public int TimeBetweenTaps { get; set; }

        /// <summary>
        /// Aika millisekunteina joka pit‰‰ kulua ravistusten v‰liss‰.
        /// </summary>
        public int TimeBetweenShakes { get; set; }

        public Accelerometer()
        {
            Calibration = AccelerometerCalibration.HalfRightAngle;
            DefaultSensitivity = 0.2;
            DefaultAnalogSensitivity = 0.01;
            GesturesEnabled = true;
            TimeBetweenTaps = 300;
            TimeBetweenShakes = 500;
        }

        internal static Accelerometer Create()
        {
#if ANDROID
            return new Jypeli.Android.AndroidAccelerometer();
#else
            return new DummyAccelerometer();
#endif
        }

        internal virtual Vector Project2d( Vector3 spaceVector )
        {
            switch ( Calibration )
            {
                case AccelerometerCalibration.ZeroAngle:
                    return new Vector( spaceVector.X, spaceVector.Y );
                case AccelerometerCalibration.HalfRightAngle:
                    return new Vector( spaceVector.X, spaceVector.Y - spaceVector.Z );
                case AccelerometerCalibration.RightAngle:
                    return new Vector( spaceVector.X, -spaceVector.Z );
                default:
                    return new Vector( spaceVector.X, spaceVector.Y );
            }
        }

        /// <summary>
        /// K‰ynnist‰‰ kiihtyvyysanturin.
        /// </summary>
        public virtual void Start()
        {
            if ( started )
                return;

            started = true;
        }

        /// <summary>
        /// Pys‰ytt‰‰ kiihtyvyysanturin.
        /// </summary>
        public virtual void Stop()
        {
            if ( !started )
                return;

            started = false;
        }

        internal override Vector3 GetState()
        {
            return Vector3.Zero;
        }

        /// <summary>
        /// Pys‰ytt‰‰ kiihtyvyysanturin annetuksi ajaksi.
        /// </summary>
        /// <param name="seconds">Aika sekunteina</param>
        public void PauseForDuration( double seconds )
        {
            Stop();
            Timer.SingleShot( seconds, Start );
        }

        private string GetDirectionName( AccelerometerDirection direction )
        {
            switch (direction)
            {
                case AccelerometerDirection.Up:
                    return "Tilt up";
                case AccelerometerDirection.Down:
                    return "Tilt down";
                case AccelerometerDirection.Left:
                    return "Tilt left";
                case AccelerometerDirection.Right:
                    return "Tilt right";
                case AccelerometerDirection.Tap:
                    return "Tap device";
                case AccelerometerDirection.Shake:
                    return "Shake";
                default:
                    return "Accelerometer";
            }
        }

        private ChangePredicate<Vector3> MakeTriggerRule( AccelerometerDirection direction, double trigger )
        {
            switch ( direction )
            {
                case AccelerometerDirection.Any:
                    return ( Vector3 prev, Vector3 curr ) => Project2d(curr).Magnitude > trigger;
                case AccelerometerDirection.Left:
                    return ( Vector3 prev, Vector3 curr ) => Project2d( curr ).X < -trigger;
                case AccelerometerDirection.Right:
                    return ( Vector3 prev, Vector3 curr ) => Project2d( curr ).X > trigger;
                case AccelerometerDirection.Up:
                    return ( Vector3 prev, Vector3 curr ) => Project2d( curr ).Y > trigger;
                case AccelerometerDirection.Down:
                    return ( Vector3 prev, Vector3 curr ) => Project2d( curr ).Y < -trigger;
                /*case AccelerometerDirection.Shake:
                    return GestureTriggered( direction, trigger, 3 );
                case AccelerometerDirection.Tap:
                    return GestureTriggered( direction, trigger, 1 );*/
                default:
                    return NeverTrigger;
            }
        }

#region Listen with no parameters

        public void Listen( AccelerometerDirection direction, Action handler, string helpText )
        {
            this.Listen( direction, DefaultSensitivity, handler, helpText );
        }

        public void ListenAnalog( Action<AnalogState> handler, string helpText )
        {
            this.ListenAnalog( DefaultAnalogSensitivity, handler, helpText );
        }

        public void Listen( AccelerometerDirection direction, AccelerometerSensitivity sensitivity, Action handler, string helpText )
        {
            this.Listen( direction, (int)sensitivity / 100, handler, helpText );
        }

        public void ListenAnalog( AccelerometerSensitivity sensitivity, Action<AnalogState> handler, string helpText )
        {
            this.ListenAnalog( (int)sensitivity / 100, handler, helpText );
        }

        public Listener Listen( AccelerometerDirection direction, double trigger, Action handler, string helpText )
        {
            Start();
            ChangePredicate<Vector3> rule = MakeTriggerRule(direction, trigger);
            return AddListener( rule, direction, GetDirectionName( direction ), helpText, handler );
        }

        public Listener ListenAnalog( double trigger, Action<AnalogState> handler, string helpText )
        {
            Start();
            ChangePredicate<Vector3> rule = ( Vector3 prev, Vector3 curr ) => Project2d(curr).Magnitude >= trigger;
            Action analogHandler = delegate { handler( new AccelerometerAnalogState( this.Reading, this.PreviousReading ) ); };
            return AddListener( rule, AccelerometerDirection.Any, "Accelerometer", helpText, analogHandler );
        }

#endregion
    }
}
