#if ANDROID
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
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

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            // Silk ei toistaiseksi salli minun tietääkseni mitään muuta keinoa paluunäppäimen havaitsemiseen Androidilla.
            if(e.KeyCode == Keycode.Back)
            {
                Game.Instance.PhoneBackButton.State = e.Action == KeyEventActions.Down ? true : false;
            }
            return base.DispatchKeyEvent(e);
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            for (int i = 0; i < ev.PointerCount; i++)
            {
                System.Diagnostics.Debug.WriteLine($"{ev.GetX(i)}, {ev.GetY(i)}");
            }

            return base.DispatchTouchEvent(ev);
        }
    }
}
#endif