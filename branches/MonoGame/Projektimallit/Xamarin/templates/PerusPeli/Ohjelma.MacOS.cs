using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

static class Program
{
	static void Main (string[] args)
	{
		NSApplication.Init ();

		using (var p = new NSAutoreleasePool())
		{
			NSApplication.SharedApplication.Delegate = new AppDelegate ();
			NSApplication.Main (args);
		}
	}
}

class AppDelegate : NSApplicationDelegate
{
	Peli peli;

	public override void FinishedLaunching (NSObject notification)
	{
		peli = new Peli();
		peli.Run ();
	}

	public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
	{
		return true;
	}
}
