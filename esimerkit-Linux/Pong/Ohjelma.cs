#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

static class Program
{
	private static Pong game;

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main ()
	{
		game = new Pong();
		game.Run();
	}
}
