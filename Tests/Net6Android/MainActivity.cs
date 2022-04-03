using Android.App;
using Jypeli;

namespace Net6Android
{
    [Activity(MainLauncher = true)]    
    public class MainActivity : JypeliActivity
    {
        protected override void OnRun()
        {
            using var game = new Tasohyppelypeli();
            game.Run();
        }
    }
}