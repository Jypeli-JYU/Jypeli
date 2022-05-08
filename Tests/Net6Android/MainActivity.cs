using System.Diagnostics;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Jypeli;

namespace Net6Android
{
    [Activity(MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : JypeliActivity
    {
        protected override void OnRun()
        {
            using var game = new Tasohyppelypeli();
            game.Run();
        }
    }
}