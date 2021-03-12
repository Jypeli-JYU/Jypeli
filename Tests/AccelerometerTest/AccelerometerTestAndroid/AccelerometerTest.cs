using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace AccelerometerTest
{
    public class AccelerometerTest : PhysicsGame
    {
        Label[] label3d = new Label[] { new Label(), new Label(), new Label() };
        Label[] label2d = new Label[] { new Label(), new Label() };

        public override void Begin()
        {
            LuoMittarit();
            LuoPainikkeet();
            LuoPallo();
        }

        private void LuoPallo()
        {
            PhysicsObject pallo = new PhysicsObject(50, 50, Shape.Circle);
            Add(pallo);

            Level.CreateHorizontalBorders();
            Level.CreateTopBorder().Color = Color.Green;
            Level.CreateBottomBorder().Color = Color.Red;
            Accelerometer.ListenAnalog(MuutaPainovoimaa, null);
        }

        private void LuoPainikkeet()
        {
            PushButton zeroAngleButton = new PushButton("0");
            zeroAngleButton.Position = new Vector(Screen.Right - 50, Screen.Top - 50);
            zeroAngleButton.Clicked += () => Accelerometer.Calibration = AccelerometerCalibration.ZeroAngle;
            Add(zeroAngleButton);

            PushButton halfRightAngleButton = new PushButton("45");
            halfRightAngleButton.Position = new Vector(Screen.Right - 50, Screen.Top - 100);
            halfRightAngleButton.Clicked += () => Accelerometer.Calibration = AccelerometerCalibration.HalfRightAngle;
            Add(halfRightAngleButton);

            PushButton rightAngleButton = new PushButton("90");
            rightAngleButton.Position = new Vector(Screen.Right - 50, Screen.Top - 150);
            rightAngleButton.Clicked += () => Accelerometer.Calibration = AccelerometerCalibration.RightAngle;
            Add(rightAngleButton);
        }

        private void LuoMittarit()
        {
            label3d.ForEach(l => l.X = -100);
            label2d.ForEach(l => l.X = 100);
            label3d[0].Y = 2 * label3d[1].Height;
            label3d[2].Y = -2 * label3d[1].Height;
            label2d[0].Y = label2d[0].Height;
            label2d[1].Y = -label2d[0].Height;
            label3d.ForEach( Add );
            label2d.ForEach( Add );
        }

        protected override void Update( Time t )
        {
            var v = Accelerometer.Reading3d;
            double r = Accelerometer.Reading3d.Length();
            double inc = r > 0 ? Math.Acos( v.Z / r ) : 0;
            double az = v.Z > 0 ? Math.Atan2( v.Y, v.Z ) : 0;

            label3d[0].Text = String.Format( "X: {0:F3}, r = {1:F3}", Accelerometer.Reading3d.X, r );
            label3d[1].Text = String.Format( "Y: {0:F3}, inc = {1:F3}", Accelerometer.Reading3d.Y, inc );
            label3d[2].Text = String.Format( "Z: {0:F3}, az = {1:F3}", Accelerometer.Reading3d.Z, az );

            label2d[0].Text = String.Format( "x: {0:F3}", Accelerometer.Reading.X );
            label2d[1].Text = String.Format( "y: {0:F3}", Accelerometer.Reading.Y );

            base.Update( t );
        }

        private void MuutaPainovoimaa( AnalogState state )
        {
            Gravity = 1000 * state.StateVector;
        }
    }
}
