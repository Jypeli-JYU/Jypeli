using System;
using System.Diagnostics;

#if ANDROID
using Android.Content;
using Android.Hardware;
using Android.Runtime;

using AndroidApp = Android.App.Application;
using Vector3 = System.Numerics.Vector3;

namespace Jypeli.Android
{
    public class AndroidAccelerometer : Accelerometer
    {
        private SensorManager sensorManager;
        private Sensor sensor;
        private SensorListener listener;

        public AndroidAccelerometer()
            : base()
        {
            CurrentState = PrevState = new Vector3(0, 1, 0);
        }

        /// <summary>
        /// Käynnistää kiihtyvyysanturin.
        /// </summary>
        public override void Start()
        {
            if (listener != null)
                return;

            sensorManager ??= AndroidApp.Context.GetSystemService(Context.SensorService) as SensorManager;
            sensor ??= sensorManager?.GetDefaultSensor(SensorType.Accelerometer);
            if (sensor == null)
            {
                Debug.WriteLine("Laitteessa ei ole kiihtyvyysanturia.");
                return;
            }

            listener = new SensorListener(this);
            sensorManager.RegisterListener(listener, sensor, SensorDelay.Normal);
            base.Start();
        }

        /// <summary>
        /// Pysäyttää kiihtyvyysanturin.
        /// </summary>
        public override void Stop()
        {
            if (listener != null)
            {
                sensorManager.UnregisterListener(listener);
                listener.Dispose();
                listener = null;
            }
            base.Stop();
        }

        /// <summary>
        /// Vastaanottaa anturin lukemat ja muuntaa ne maan vetovoiman kerrannaisiksi (g),
        /// jolloin paikallaan oleva puhelin antaa lukeman, jonka pituus on noin 1.
        /// </summary>
        private sealed class SensorListener : Java.Lang.Object, ISensorEventListener
        {
            private readonly AndroidAccelerometer owner;

            public SensorListener(AndroidAccelerometer owner)
            {
                this.owner = owner;
            }

            public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
            {
            }

            public void OnSensorChanged(SensorEvent e)
            {
                var v = e.Values;
                owner.CurrentState = new Vector3(v[0], v[1], v[2]) / SensorManager.GravityEarth;
            }
        }

        internal override Vector Project2d(Vector3 spaceVector)
        {
            spaceVector = Transform3d(spaceVector);

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

        private void UpdateDisplayOrientation(Vector3 e)
        {
            if (e.Z > 0.8)
                return;

            double angle = Math.Atan2(-e.X, e.Y) * 180.0 / Math.PI;
            double delta = 5;

            if (angle > -45 + delta && angle < 45 - delta)
                Game.Device.DisplayOrientation = DisplayOrientation.Portrait;
            else if (angle > 45 + delta && angle < 135 - delta)
                Game.Device.DisplayOrientation = DisplayOrientation.LandscapeRight;
            else if (angle > -135 + delta && angle < -45 - delta)
                Game.Device.DisplayOrientation = DisplayOrientation.LandscapeLeft;
            else
                Game.Device.DisplayOrientation = DisplayOrientation.PortraitInverse;
        }

        internal override Vector3 GetState()
        {
            return CurrentState;
        }

        private Vector3 Transform3d(Vector3 e)
        {
            int xmul = DisplayOrientation.Xmul;
            int ymul = DisplayOrientation.Ymul;
            return new Vector3(xmul * e.Y - ymul * e.X, -xmul * e.X - ymul * e.Y, -e.Z);

            /*switch ( DisplayOrientation )
            {
                case DisplayOrientation.Landscape:
                case DisplayOrientation.LandscapeLeft:
                    return new Vector3( e.Y, -e.X, -e.Z );
                case DisplayOrientation.LandscapeRight:
                    return new Vector3( -e.Y, e.X, -e.Z );
                case DisplayOrientation.PortraitInverse:
                    return new Vector3( e.X, e.Y, -e.Z );
                case DisplayOrientation.Portrait:
                default:
                    return new Vector3( -e.X, -e.Y, -e.Z );
            }*/
        }
    }
}
#endif