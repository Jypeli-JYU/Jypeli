using System;
using System.IO;
using Jypeli.Devices;

#if ANDROID
using Android.Content.Res;
#endif

namespace Jypeli.Android
{
    public class AndroidDevice : Device
    {
        private double _directionalSign = 1;
#if ANDROID
        private AssetManager AssetManager;
#endif
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
            this.Accelerometer = new AndroidAccelerometer();
            this.Storage = new FileManager(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            ContentPath = Environment.GetFolderPath(Environment.SpecialFolder.Resources);
#if ANDROID
            AssetManager = Game.AssetManager;
#endif
        }

        internal override Stream StreamContent(string name, string[] extensions)
        {
#if ANDROID
            Stream s = AssetManager.Open(name);
            if(s == null)
            {
                s = FindContentFile(name, extensions);
            }
            return s;
#else
            throw new InvalidOperationException("Ohjelma ei ole käännetty ANDROID-vakiolla. Tämän poikkeuksen ei pitäisi ikinä tapahtua.");
#endif
        }
#if ANDROID
        private Stream FindContentFile(string name, string[] extensions)
        {
            foreach (var ext in extensions)
            {
                string withExt = Path.ChangeExtension(name, ext);
                Stream s = AssetManager.Open(withExt);
                if (s != null)
                    return s;
            }
            return null;
        }
#endif
        public override void Vibrate( int milliSeconds )
        {
            Xamarin.Essentials.Vibration.Vibrate(milliSeconds);
        }

        public override void StopVibrating()
        {
            Xamarin.Essentials.Vibration.Cancel();
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
