using Android.App;
using Android.Content.PM;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl.Android;

namespace Program
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : SilkActivity
    {
        protected override void OnRun()
        {
            Jypeli.Game.AssetManager = Assets;
            using (var game = new Tasohyppelypeli())
                game.Run();
        }
    }
}