using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;
using Jypeli;

/*class MainClass
{
	static void Main (string[] args)
	{
		NSApplication.Init ();
		NSApplication.Main (args);
	}
}*/

public class Koripallo : Game
{
	public override void Begin()
	{
		MessageDisplay.Add ("testi");
	}
}
