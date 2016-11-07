using System;

#if MACOS

using MonoMac.Foundation;
using MonoMac.AppKit;

static class Ohjelma
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main (string [] args)
    {
        NSApplication.Init ();

        using (var p = new NSAutoreleasePool ()) {
            NSApplication.SharedApplication.Delegate = new AppDelegate ();
            NSApplication.Main (args);
        }
    }
}

class AppDelegate : NSApplicationDelegate
{
    private static Peli game;

    public override void FinishedLaunching (NSObject notification)
    {
        game = new Peli();
        game.Run();
    }

    public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
    {
        return true;
    }
}

#else

public static class Ohjelma
{
    private static Peli game;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main ()
    {
        game = new Peli();
        game.Run();
    }
}

#endif
