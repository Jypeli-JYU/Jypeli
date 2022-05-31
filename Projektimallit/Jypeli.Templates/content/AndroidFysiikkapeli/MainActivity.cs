using Android.App;
using Android.Content.PM;
using Jypeli;

namespace AndroidFysiikkapeli
{
    [Activity(MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : JypeliActivity
    {
        protected override void OnRun()
        {
            using var game = new AndroidFysiikkapeli();
            game.Run();
        }
    }
}