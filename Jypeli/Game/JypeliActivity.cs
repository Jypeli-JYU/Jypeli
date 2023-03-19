
#if ANDROID
using System;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Silk.NET.Windowing.Sdl.Android;
using Jypeli.Controls;
using Andr = Android;

namespace Jypeli
{
    public class JypeliActivity : SilkActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            ActionBar?.Hide();
            Window?.AddFlags(WindowManagerFlags.Fullscreen);
            Game.AssetManager = Assets;
            // SDL vaatii uudella androidilla Bluetoothin Steam controller tuen takia.
            if (CheckSelfPermission(Andr.Manifest.Permission.BluetoothConnect) != Permission.Granted)
            {
                RequestPermissions(new string[] { Andr.Manifest.Permission.BluetoothConnect }, 0);
            }
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            base.OnCreate(savedInstanceState);
        }
        protected override void OnRun()
        {
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            // Silk ei toistaiseksi salli minun tietääkseni mitään muuta keinoa paluunäppäimen havaitsemiseen Androidilla.
            if (e.KeyCode == Keycode.Back)
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

                switch (ev.ActionMasked)
                {
                    case MotionEventActions.Down:
                    case MotionEventActions.PointerDown:
                        AddOrUpdateTouchEvent(ev.GetPointerId(ev.ActionIndex), new Vector(ev.GetX(ev.ActionIndex), ev.GetY(ev.ActionIndex) - re.Top), TouchAction.Down);
                        break;
                    case MotionEventActions.Move:
                        for (int i = 0; i < ev.PointerCount; i++)
                        {
                            AddOrUpdateTouchEvent(ev.GetPointerId(i), new Vector(ev.GetX(i), ev.GetY(i) - re.Top), TouchAction.Move);
                        }
                        break;
                    case MotionEventActions.Up:
                    case MotionEventActions.PointerUp:
                        AddOrUpdateTouchEvent(ev.GetPointerId(ev.ActionIndex), new Vector(ev.GetX(ev.ActionIndex), ev.GetY(ev.ActionIndex) - re.Top), TouchAction.Up);
                        break;
         
                    case MotionEventActions.Outside:
                    case MotionEventActions.Cancel:
                        for (int i = 0; i < ev.PointerCount; i++)
                        {
                            AddOrUpdateTouchEvent(ev.GetPointerId(i), new Vector(ev.GetX(i), ev.GetY(i) - re.Top), TouchAction.Up);
                        }
                        break;
                    default:
                        Console.WriteLine($"Käsittelemätön event: {ev.ActionMasked}");
                        break;

                };

                return base.DispatchTouchEvent(ev);
            }
        }

        private void AddOrUpdateTouchEvent(int id, Vector pos, TouchAction action)
        {
            var raw = Game.Instance.TouchPanel.RawTouches;

            RawTouch r = new RawTouch();
            r.Position = pos;
            r.Id = id;
            r.Action = action;

            // On mahdollista että sormi koskee näyttöön, liikkuu ja nousee ylös yhden päivityksen aikana.
            int index = raw.FindIndex(ra => ra.Id == r.Id && ra.Action == r.Action);

            if (index == -1)
            {
                raw.Add(r);
            }
            else
            {
                var ra = raw[index];
                ra.Position = pos;
                raw[index] = ra;
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