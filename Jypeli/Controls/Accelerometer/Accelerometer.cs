using System;
using Jypeli.Controls;
using Vector3 = System.Numerics.Vector3;

namespace Jypeli
{
    /// <summary>
    /// Dummy -kiihtyvyysanturi tietokonetta varten. Ei tee mitään.
    /// </summary>
    public class DummyAccelerometer : Accelerometer
    {
    }

    /// <summary>
    /// Kiihtyvyysanturi
    /// </summary>
    public abstract class Accelerometer : Controller<Vector3, AccelerometerDirection>
    {
        /// <summary>
        /// Onko anturi käynnistetty
        /// </summary>
        protected bool started = false;

        /// <summary>
        /// Puhelimen kallistuksen nollakohta.
        /// </summary>
        public AccelerometerCalibration Calibration { get; set; }

        /// <summary>
        /// Kallistuksen tämänhetkinen suunta.
        /// </summary>
        public Vector Reading
        {
            get
            {
                return Project2d(CurrentState);
            }
        }

        /// <summary>
        /// Kallistuksen edellinen suunta.
        /// </summary>
        public Vector PreviousReading
        {
            get
            {
                return Project2d(PrevState);
            }
        }

        /// <summary>
        /// Kiihtyvyyden arvo.
        /// </summary>
        public Vector3 Reading3d
        {
            get
            {
                return CurrentState;
            }
        }

        /// <summary>
        /// Edellinen kiihtyvyyden arvo.
        /// </summary>
        public Vector3 PreviousReading3d
        {
            get
            {
                return PrevState;
            }
        }

        /// <summary>
        /// Näytön suunta.
        /// </summary>
        public DisplayOrientation DisplayOrientation
        {
            get { return Game.Device.DisplayOrientation; }
        }

        /// <summary>
        /// Herkkyys jos kuunnellaan suuntia ja eleitä ilman erikseen annettua herkkyyttä.
        /// </summary>
        public double DefaultSensitivity { get; private set; }

        /// <summary>
        /// Herkkyys jos kuunnellaan suuntia analogisesti ilman erikseen annettua herkkyyttä.
        /// </summary>
        public double DefaultAnalogSensitivity { get; private set; }

        /// <summary>
        /// Määrittää onko Shake ja Tap käytössä.
        /// </summary>
        public bool GesturesEnabled { get; set; }

        /// <summary>
        /// Aika millisekunteina joka pitää kulua napautusten välissä.
        /// </summary>
        public int TimeBetweenTaps { get; set; }

        /// <summary>
        /// Aika millisekunteina joka pitää kulua ravistusten välissä.
        /// </summary>
        public int TimeBetweenShakes { get; set; }

        /// <summary>
        /// Kiihtyvyysanturin muodostaja, tätä sinun tuskin tarvitsee käyttää itse.
        /// Käytä <c>Device.Accelerometer</c>
        /// </summary>
        public Accelerometer()
        {
            Calibration = AccelerometerCalibration.ZeroAngle;
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

        internal virtual Vector Project2d(Vector3 spaceVector)
        {
            switch (Calibration)
            {
                case AccelerometerCalibration.ZeroAngle:
                    return new Vector(spaceVector.X, spaceVector.Y);
                case AccelerometerCalibration.InvertXY:
                    return new Vector(spaceVector.Y, spaceVector.X);
                case AccelerometerCalibration.HalfRightAngle:
                    return new Vector(spaceVector.X, spaceVector.Y - spaceVector.Z);
                case AccelerometerCalibration.RightAngle:
                    return new Vector(spaceVector.X, -spaceVector.Z);
                default:
                    return new Vector(spaceVector.X, spaceVector.Y);
            }
        }

        /// <summary>
        /// Käynnistää kiihtyvyysanturin.
        /// </summary>
        public virtual void Start()
        {
            if (started)
                return;

            started = true;
        }

        /// <summary>
        /// Pysäyttää kiihtyvyysanturin.
        /// </summary>
        public virtual void Stop()
        {
            if (!started)
                return;

            started = false;
        }

        internal override Vector3 GetState()
        {
            return Vector3.Zero;
        }

        /// <summary>
        /// Pysäyttää kiihtyvyysanturin annetuksi ajaksi.
        /// </summary>
        /// <param name="seconds">Aika sekunteina</param>
        public void PauseForDuration(double seconds)
        {
            Stop();
            Timer.SingleShot(seconds, Start);
        }

        private string GetDirectionName(AccelerometerDirection direction)
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

        private ChangePredicate<Vector3> MakeTriggerRule(AccelerometerDirection direction, double trigger)
        {
            switch (direction)
            {
                case AccelerometerDirection.Any:
                    return (Vector3 prev, Vector3 curr) => Project2d(curr).Magnitude > trigger;
                case AccelerometerDirection.Left:
                    return (Vector3 prev, Vector3 curr) => Project2d(curr).X < -trigger;
                case AccelerometerDirection.Right:
                    return (Vector3 prev, Vector3 curr) => Project2d(curr).X > trigger;
                case AccelerometerDirection.Up:
                    return (Vector3 prev, Vector3 curr) => Project2d(curr).Y > trigger;
                case AccelerometerDirection.Down:
                    return (Vector3 prev, Vector3 curr) => Project2d(curr).Y < -trigger;
                /*case AccelerometerDirection.Shake:
                    return GestureTriggered( direction, trigger, 3 );
                case AccelerometerDirection.Tap:
                    return GestureTriggered( direction, trigger, 1 );*/
                default:
                    return NeverTrigger;
            }
        }

        #region Listen with no parameters

        /// <summary>
        /// Kuuntele kiihtyvyysanturin muutosta
        /// </summary>
        /// <param name="direction">Suunta</param>
        /// <param name="handler">Tapahtumankäsittelijä</param>
        /// <param name="helpText">Aputeksti</param>
        public void Listen(AccelerometerDirection direction, Action handler, string helpText)
        {
            this.Listen(direction, DefaultSensitivity, handler, helpText);
        }

        /// <summary>
        /// Kuuntele kiihtyvyysanturin muutosta
        /// </summary>
        /// <param name="handler">Tapahtumankäsittelijä</param>
        /// <param name="helpText">Aputeksti</param>
        public void ListenAnalog(Action<AnalogState> handler, string helpText)
        {
            this.ListenAnalog(DefaultAnalogSensitivity, handler, helpText);
        }

        /// <summary>
        /// Kuuntele kiihtyvyysanturin muutosta
        /// </summary>
        /// <param name="direction">Suunta</param>
        /// <param name="sensitivity">Herkkyys</param>
        /// <param name="handler">Tapahtumankäsittelijä</param>
        /// <param name="helpText">Aputeksti</param>
        public void Listen(AccelerometerDirection direction, AccelerometerSensitivity sensitivity, Action handler, string helpText)
        {
            this.Listen(direction, (int)sensitivity / 100, handler, helpText);
        }

        /// <summary>
        /// Kuuntele kiihtyvyysanturin muutosta
        /// </summary>
        /// <param name="sensitivity">Herkkyys</param>
        /// <param name="handler">Tapahtumankäsittelijä</param>
        /// <param name="helpText">Aputeksti</param>
        public void ListenAnalog(AccelerometerSensitivity sensitivity, Action<AnalogState> handler, string helpText)
        {
            this.ListenAnalog((int)sensitivity / 100, handler, helpText);
        }

        /// <summary>
        /// Kuuntele kiihtyvyysanturin muutosta
        /// </summary>
        /// <param name="direction">Suunta</param>
        /// <param name="trigger">Herkkyys</param>
        /// <param name="handler">Tapahtumankäsittelijä</param>
        /// <param name="helpText">Aputeksti</param>
        /// <returns></returns>
        public Listener Listen(AccelerometerDirection direction, double trigger, Action handler, string helpText)
        {
            Start();
            ChangePredicate<Vector3> rule = MakeTriggerRule(direction, trigger);
            return AddListener(rule, direction, GetDirectionName(direction), helpText, handler);
        }

        /// <summary>
        /// Kuuntele kiihtyvyysanturin muutosta
        /// </summary>
        /// <param name="trigger">Herkkyys</param>
        /// <param name="handler">Tapahtumankäsittelijä</param>
        /// <param name="helpText">Aputeksti</param>
        /// <returns></returns>
        public Listener ListenAnalog(double trigger, Action<AnalogState> handler, string helpText)
        {
            Start();
            ChangePredicate<Vector3> rule = (Vector3 prev, Vector3 curr) => Project2d(curr).Magnitude >= trigger;
            Action analogHandler = delegate
            { handler(new AccelerometerAnalogState(this.Reading, this.PreviousReading)); };
            return AddListener(rule, AccelerometerDirection.Any, "Accelerometer", helpText, analogHandler);
        }

        #endregion
    }
}
