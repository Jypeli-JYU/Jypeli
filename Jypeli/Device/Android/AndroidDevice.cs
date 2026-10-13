using System;
using System.IO;
using Jypeli.Devices;

#if ANDROID
using Android.App;
using Android.Content;

using AndroidApp = Android.App.Application;
using VibrationEffect = Android.OS.VibrationEffect;
using Vibrator = Android.OS.Vibrator;
using VibratorManager = Android.OS.VibratorManager;

[assembly: UsesPermission(global::Android.Manifest.Permission.Vibrate)]

namespace Jypeli.Android
{
    public class AndroidDevice : Device
    {
        private double _directionalSign = 1;
        private Vibrator _vibrator;

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
            Game.AudioOutput = new Audio.Android.AndroidAudio();
        }

        public override Stream StreamContent(string name, string[] extensions)
        {
            Stream s;
            string fullPath = Path.Combine(ContentPath, name);
            try
            {
                s = Game.AssetManager.Open(fullPath);
            }
            catch
            {
                s = FindContentFile(fullPath, extensions);
            }

            return s;
        }

        public override Stream StreamContent(string name)
        {
            return StreamContent(name, Array.Empty<string>());
        }

        private Stream FindContentFile(string name, string[] extensions)
        {
            foreach (var ext in extensions)
            {
                string withExt = Path.ChangeExtension(name, ext);
                Stream s;
                try
                {
                    s = Game.AssetManager.Open(withExt);
                }
                catch
                {
                    continue;
                }
                return s;
            }
            throw new FileNotFoundException($"Tiedostoa {name} ei löydy");
        }

        private Vibrator Vibrator
        {
            get
            {
                if (_vibrator == null)
                {
                    if (OperatingSystem.IsAndroidVersionAtLeast(31))
                    {
                        var manager = AndroidApp.Context.GetSystemService(Context.VibratorManagerService) as VibratorManager;
                        _vibrator = manager?.DefaultVibrator;
                    }
                    else
                    {
                        _vibrator = AndroidApp.Context.GetSystemService(Context.VibratorService) as Vibrator;
                    }
                }
                return _vibrator;
            }
        }

        public override void Vibrate(int milliSeconds)
        {
            var vibrator = Vibrator;
            if (vibrator == null || !vibrator.HasVibrator || milliSeconds <= 0)
                return;

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                vibrator.Vibrate(VibrationEffect.CreateOneShot(milliSeconds, VibrationEffect.DefaultAmplitude));
            }
            else
            {
                vibrator.Vibrate(milliSeconds);
            }
        }

        public override void StopVibrating()
        {
            Vibrator?.Cancel();
        }

        protected override void UpdateScreen()
        {
            Vector defaultSize = Game.Screen.ViewportSize;
            Vector defaultScale = Vector.Diagonal;

            if (DisplayResolution == DisplayResolution.Small)
            {
                defaultSize = new Vector(400, 240);
                defaultScale = new Vector(Game.Screen.ViewportSize.X / defaultSize.X, Game.Screen.ViewportSize.Y / defaultSize.Y);
            }

            if (DisplayOrientation == DisplayOrientation.Portrait || DisplayOrientation == DisplayOrientation.PortraitInverse)
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