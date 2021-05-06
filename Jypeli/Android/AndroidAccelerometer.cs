//using System;
//using Microsoft.Xna.Framework;
// TODO: .NET Core 3.0 does not support Android. 
//       fix using Xamarin.Android, or wait for .NET 5.0 coming in november 2020?

//using XnaAccelerometer = Microsoft.Devices.Sensors.Accelerometer;

//namespace Jypeli.Android
//{
//    public class AndroidAccelerometer : Accelerometer
//    {
//        private readonly XnaAccelerometer _accelerometer = null;

//        public AndroidAccelerometer()
//            : base()
//        {
//            _accelerometer = new XnaAccelerometer();
//            CurrentState = PrevState = new Vector3( 0, 1, 0 );
//        }

//        /// <summary>
//        /// Käynnistää kiihtyvyysanturin.
//        /// </summary>
//        public override void Start()
//        {
//            if ( _accelerometer == null || started )
//                return;

//            _accelerometer.Start();
//            base.Start();
//        }

//        /// <summary>
//        /// Pysäyttää kiihtyvyysanturin.
//        /// </summary>
//        public override void Stop()
//        {
//            if ( _accelerometer == null || !started )
//                return;

//            _accelerometer.Stop();
//            base.Stop();
//        }

//        internal override Vector Project2d( Vector3 spaceVector )
//        {
//            spaceVector = Transform3d( spaceVector );

//            switch ( Calibration )
//            {
//                case AccelerometerCalibration.ZeroAngle:
//                    return new Vector( spaceVector.X, spaceVector.Y );
//                case AccelerometerCalibration.HalfRightAngle:
//                    return new Vector( spaceVector.X, spaceVector.Y - spaceVector.Z );
//                case AccelerometerCalibration.RightAngle:
//                    return new Vector( spaceVector.X, -spaceVector.Z );
//                default:
//                    return new Vector( spaceVector.X, spaceVector.Y );
//            }
//        }

//        private void UpdateDisplayOrientation( Vector3 e )
//        {
//            if ( e.Z > 0.8 )
//                return;

//            double angle = Math.Atan2( -e.X, e.Y ) * 180.0 / Math.PI;
//            double delta = 5;

//            if ( angle > -45 + delta && angle < 45 - delta )
//                Game.Device.DisplayOrientation = DisplayOrientation.Portrait;
//            else if ( angle > 45 + delta && angle < 135 - delta )
//                Game.Device.DisplayOrientation = DisplayOrientation.LandscapeRight;
//            else if ( angle > -135 + delta && angle < -45 - delta )
//                Game.Device.DisplayOrientation = DisplayOrientation.LandscapeLeft;
//            else
//                Game.Device.DisplayOrientation = DisplayOrientation.PortraitInverse;
//        }

//        internal override Vector3 GetState()
//        {
//            if ( _accelerometer == null || !started )
//                return PrevState;

//            var e = _accelerometer.CurrentValue.Acceleration;
//            UpdateDisplayOrientation( e );

//            return new Vector3( e.X, e.Y, e.Z );
//        }

//        private Vector3 Transform3d( Vector3 e )
//        {
//            int xmul = DisplayOrientation.Xmul;
//            int ymul = DisplayOrientation.Ymul;
//            return new Vector3( xmul * e.Y - ymul * e.X, -xmul * e.X - ymul * e.Y, -e.Z );

//            /*switch ( DisplayOrientation )
//            {
//                case DisplayOrientation.Landscape:
//                case DisplayOrientation.LandscapeLeft:
//                    return new Vector3( e.Y, -e.X, -e.Z );
//                case DisplayOrientation.LandscapeRight:
//                    return new Vector3( -e.Y, e.X, -e.Z );
//                case DisplayOrientation.PortraitInverse:
//                    return new Vector3( e.X, e.Y, -e.Z );
//                case DisplayOrientation.Portrait:
//                default:
//                    return new Vector3( -e.X, -e.Y, -e.Z );
//            }*/
//        }
//    }
//}
