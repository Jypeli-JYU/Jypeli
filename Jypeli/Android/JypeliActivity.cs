using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Microsoft.Xna.Framework;

namespace Jypeli.Android
{
    /// <summary>
    /// Custom Android activity that enables use of the on-screen keyboard.
    /// </summary>
    public class JypeliActivity : AndroidGameActivity, IOnScreenKeyboardManager
    {
        private Microsoft.Xna.Framework.Game xnaGame;
        private View pView;

        public void InitActivity(Microsoft.Xna.Framework.Game xnaGame)
        {
            this.xnaGame = xnaGame;
            pView = xnaGame.Services.GetService<View>();
        }

        public void ShowOnScreenKeyboard()
        {
            var inputMethodManager = Application.GetSystemService(Context.InputMethodService) as InputMethodManager;
            inputMethodManager.ShowSoftInput(pView, ShowFlags.Forced);
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
        }

        public void HideOnScreenKeyboard()
        {
            InputMethodManager inputMethodManager = Application.GetSystemService(Context.InputMethodService) as InputMethodManager;
            inputMethodManager.HideSoftInputFromWindow(pView.WindowToken, HideSoftInputFlags.None);
        }
    }
}