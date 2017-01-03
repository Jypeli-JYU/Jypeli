using System;
using Microsoft.Xna.Framework;

using XnaAccelerometer = Microsoft.Devices.Sensors.Accelerometer;

namespace Jypeli
{
    public class AndroidAccelerometer : Accelerometer
    {
        private readonly XnaAccelerometer _accelerometer = null;

        public AndroidAccelerometer()
            : base()
        {
            _accelerometer = new XnaAccelerometer();
            CurrentState = PrevState = new Vector3( 0, 1, 0 );
        }

        /// <summary>
        /// K‰ynnist‰‰ kiihtyvyysanturin.
        /// </summary>
        public override void Start()
        {
            if ( _accelerometer == null || started )
                return;

            _accelerometer.Start();
            base.Start();
        }

        /// <summary>
        /// Pys‰ytt‰‰ kiihtyvyysanturin.
        /// </summary>
        public override void Stop()
        {
            if ( _accelerometer == null || !started )
                return;

            _accelerometer.Stop();
            base.Stop();
        }

        internal override Vector Project2d( Vector3 spaceVector )
        {
            spaceVector = Transform3d( spaceVector );

            switch ( Calibration )
            {
                case AccelerometerCalibration.ZeroAngle:
                    return new Vector( spaceVector.X, spaceVector.Y - 1.0 );
                case AccelerometerCalibration.HalfRightAngle:
                    return new Vector( spaceVector.X, spaceVector.Y - spaceVector.Z );
                case AccelerometerCalibration.RightAngle:
                    return new Vector( spaceVector.X, -spaceVector.Z );
                default:
                    return new Vector( spaceVector.X, spaceVector.Y );
            }
        }

        private DisplayOrientation GetDisplayOrientation( Vector3 e )
        {
            double angle = Math.Atan2( -e.X, e.Y ) * 180.0 / Math.PI;
            double delta = 5;

            if ( angle > -45 + delta && angle < 45 - delta ) return DisplayOrientation.Portrait;
            if ( angle > 45 + delta && angle < 135 - delta ) return DisplayOrientation.LandscapeRight;
            if ( angle > -135 + delta && angle < -45 - delta ) return DisplayOrientation.LandscapeLeft;

            return DisplayOrientation.PortraitInverse;
        }

        internal override Vector3 GetState()
        {
            if ( _accelerometer == null || !started )
                return PrevState;

            var e = _accelerometer.CurrentValue.Acceleration;
            Game.Device.DisplayOrientation = GetDisplayOrientation( e );

            return new Vector3( e.X, e.Y, e.Z );
        }

        private Vector3 Transform3d( Vector3 e )
        {
            int xmul = DisplayOrientation.Xmul;
            int ymul = DisplayOrientation.Ymul;
            return new Vector3( xmul * e.Y + ymul * e.X, -xmul * e.X - ymul * e.Y, -e.Z );

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
