
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
using System.Linq;

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
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
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
            // Voi olla, että tänne tullaan uudestaan, ennen kuin edelliset kosketukset on käsitelty,
            // tai jopa kesken käsittelyn koska tätä kutsutaan eri säikeestä.
            lock (Game.Instance.TouchPanel.TouchLock)
            {
                var raw = Game.Instance.TouchPanel.RawTouches;

                Rect re = new Rect();
                Window.DecorView.GetWindowVisibleDisplayFrame(re);
                for (int i = 0; i < ev.PointerCount; i++)
                {
                    Controls.RawTouch r = new Controls.RawTouch();
                    r.Position = new Vector(ev.GetX(i), ev.GetY(i) - re.Top);
                    r.Id = ev.GetPointerId(i);
                    int index = raw.FindIndex(ra => ra.Id == r.Id);
                    if (index == -1)
                    {
                        raw.Add(r);
                    }
                    else
                    {
                        var ra = raw[index];
                        ra.Up = (int)ev.Action == 1 || (int)ev.Action == 6;
                        ra.Position = new Vector(ev.GetX(i), ev.GetY(i) - re.Top);
                        raw[index] = ra;
                    }
                }
                Game.Instance.TouchPanel.RawTouchesUpdated = true;
                return base.DispatchTouchEvent(ev);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
#endif