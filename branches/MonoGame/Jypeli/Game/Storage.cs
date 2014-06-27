using System;

namespace Jypeli
{
	public partial class Game
	{
		private FileManager dataStorage = null;

		/// <summary>
		/// Tietovarasto, johon voi tallentaa tiedostoja pidempiaikaisesti.
		/// Sopii esimerkiksi pelitilanteen lataamiseen ja tallentamiseen.
		/// </summary>
		public static FileManager DataStorage { get { return Instance.dataStorage; } }

		private void InitStorage()
		{
			#if WINDOWS
			dataStorage = new WindowsFileManager( WindowsLocation.DataPath, WindowsLocation.MyDocuments );
			#elif LINUX
			dataStorage = new LinuxFileManager();
            #elif WINRT
            dataStorage = new RTFileManager();
            #elif WINDOWS_PHONE
            dataStorage = new IsolatedStorageManager();
			#endif
		}
	}
}

