using System;
using Windows.Phone.Devices.Notification;

namespace Jypeli.Devices
{
    public class WindowsPhone81Device : Device
    {
        public override bool IsMobileDevice
        {
            get { return true; }
        }

        public override bool IsPhone
        {
            get { return true; }
        }
		
		public WindowsPhone81Device()
        {
            this.Storage = new RTFileManager();
        }

        public override void Vibrate( int milliSeconds )
        {
            if ( !IsMobileDevice )
                return;

            Windows.Phone.Devices.Notification.VibrationDevice v = VibrationDevice.GetDefault();
            v.Vibrate( TimeSpan.FromMilliseconds( 500 ) );
        }

        /// <summary>
        /// Lopettaa puhelimen värinän.
        /// </summary>
        public override void StopVibrating()
        {
            if ( !IsMobileDevice )
                return;

            Windows.Phone.Devices.Notification.VibrationDevice v = VibrationDevice.GetDefault();
            v.Cancel();
        }

        protected override void UpdateScreen()
        {
            Vector defaultSize = Game.Screen.ViewportSize;
            Vector defaultScale = Vector.Diagonal;

            if ( DisplayResolution == DisplayResolution.Small )
            {
                defaultSize = new Vector( 400, 240 );
                defaultScale = new Vector( Game.Screen.ViewportSize.X / defaultSize.X, Game.Screen.ViewportSize.Y / defaultSize.Y );
            }

            if ( DisplayOrientation == DisplayOrientation.Portrait || DisplayOrientation == DisplayOrientation.PortraitInverse )
            {
                Game.Screen.Size = defaultSize;
                Game.Screen.Scale = defaultScale;
                Game.Screen.Angle = DisplayOrientation == DisplayOrientation.PortraitInverse ? Angle.StraightAngle : Angle.Zero;
            }
            else
            {
                Game.Screen.Size = defaultSize.Transpose();
                Game.Screen.Scale = defaultScale.Transpose();
                Game.Screen.Angle = Angle.Zero;
            }
        }
    }
}
