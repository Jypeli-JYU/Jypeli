using System;
using System.Diagnostics;

#if ANDROID
using Essentials = Xamarin.Essentials;

using Vector3 = System.Numerics.Vector3;

namespace Jypeli.Android
{
    public class AndroidAccelerometer : Accelerometer
    {

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
            Essentials.Accelerometer.Start(Essentials.SensorSpeed.Default);
            Essentials.Accelerometer.ReadingChanged += AccelerometerReadingChanged;
            base.Start();
        }

        private void AccelerometerReadingChanged(object sender, Essentials.AccelerometerChangedEventArgs e)
        {
            var v = e.Reading.Acceleration;
            CurrentState = new Vector3(v.X, v.Y, v.Z);
        }

        /// <summary>
        /// Pysäyttää kiihtyvyysanturin.
        /// </summary>
        public override void Stop()
        {
            Essentials.Accelerometer.Stop();
            base.Stop();
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