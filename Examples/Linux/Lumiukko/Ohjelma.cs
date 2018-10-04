#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Lumiukko
{
	static class Program
	{
		private static Peli game;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			game = new Peli();
			game.Run();
		}
	}
}
