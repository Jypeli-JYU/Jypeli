
#if ANDROID
using System;
using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Views = Android.Views;
using Java.Interop;
using Silk.NET.Windowing.Sdl.Android;

namespace Jypeli
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class JypeliActivity : SilkActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            ActionBar.Hide();
            Window.AddFlags(WindowManagerFlags.Fullscreen);
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
            Rect re = new Rect();
            Window.DecorView.GetWindowVisibleDisplayFrame(re);
            
            for (int i = 0; i < ev.PointerCount; i++)
            {
                Controls.RawTouch r = new Controls.RawTouch();
                r.Position = new Vector(ev.GetX(i), ev.GetY(i) - re.Top);
                r.Id = ev.GetPointerId(i);
                Game.Instance.TouchPanel.RawTouches.Add(r);
            }
            return base.DispatchTouchEvent(ev);
        }
    }
}
#endif