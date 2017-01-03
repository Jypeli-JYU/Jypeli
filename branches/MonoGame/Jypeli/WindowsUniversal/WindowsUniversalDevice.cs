using System;
using Windows.Foundation.Metadata;
using Windows.Phone.Devices.Notification;

namespace Jypeli.Devices
{
    public class WindowsUniversalDevice : Device
    {
        public override bool IsMobileDevice
        {
            get { return this.IsPhone; }
        }

        public override bool IsPhone
        {
            get
            {
                ApiInformation.IsTypePresent( "" );
                return ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0);
            }
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
    }
}
