using System;
using System.IO;
using Jypeli.Devices;

#if ANDROID
using Android.Content.Res;

using Essentials = Xamarin.Essentials;


namespace Jypeli.Android
{
    public class AndroidDevice : Device
    {
        private double _directionalSign = 1;

        public override bool IsPhone
        {
            get
            {
                // For now, just assume it is
                return true;
            }
        }

        public AndroidDevice()
        {
            Accelerometer = new AndroidAccelerometer();
            Storage = new FileManager(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            ContentPath = "Content/";
        }

        internal override Stream StreamContent(string name, string[] extensions)
        {
            Stream s;
            try
            {
                 s = Game.AssetManager.Open(ContentPath + name);
            }catch
            {
                s = FindContentFile(name, extensions);
            }
            
            return s;
        }

        private Stream FindContentFile(string name, string[] extensions)
        {
            foreach (var ext in extensions)
            {
                string withExt = Path.ChangeExtension(name, ext);
                Stream s;
                try
                {
                    s = Game.AssetManager.Open(ContentPath + withExt);
                }
                catch
                {
                    continue;
                }
                return s;
            }
            return null;
        }

        public override void Vibrate( int milliSeconds )
        {
            Essentials.Vibration.Vibrate(milliSeconds);
        }

        public override void StopVibrating()
        {
            Essentials.Vibration.Cancel();
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
                Game.Screen.Size = defaultSize.Transpose();
                Game.Screen.Scale = defaultScale.Transpose();
                Game.Screen.Angle = _directionalSign * (DisplayOrientation == DisplayOrientation.PortraitInverse ? -Angle.RightAngle : Angle.RightAngle);
            }
            else
            {
                Game.Screen.Size = defaultSize;
                Game.Screen.Scale = defaultScale;
                Game.Screen.Angle = Angle.Zero;
                _directionalSign = DisplayOrientation == DisplayOrientation.LandscapeRight ? -1 : 1;
            }
        }
    }
}
#endif