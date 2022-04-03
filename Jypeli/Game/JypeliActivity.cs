#if ANDROID
using Android.App;
using Android.Content.PM;
using Android.OS;
using Silk.NET.Windowing.Sdl.Android;

namespace Jypeli
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class JypeliActivity : SilkActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            ActionBar.Hide();
            Game.AssetManager = Assets;
            base.OnCreate(savedInstanceState);
        }
        protected override void OnRun()
        {
        }
    }
}
#endif