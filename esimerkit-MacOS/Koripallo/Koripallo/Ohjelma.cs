using System;
using System.Drawing;
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
	Koripallo peli;

	public override void FinishedLaunching (NSObject notification)
	{
		peli = new Koripallo ();
		peli.Run ();
	}

	public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
	{
		return true;
	}
}
