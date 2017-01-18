﻿namespace Jypeli.Devices
{
    public class ComputerDevice : Device
    {
        public override bool IsMobileDevice
        {
            get { return false; }
        }

        public override bool IsPhone
        {
            get { return false; }
        }

        public ComputerDevice()
        {
			// TODO: merge these into one class
			#if WINDOWS
            this.Storage = new WindowsFileManager( WindowsLocation.DataPath, WindowsLocation.MyDocuments );
			#else
			this.Storage = new LinuxFileManager();
			#endif
        }
    }
}
