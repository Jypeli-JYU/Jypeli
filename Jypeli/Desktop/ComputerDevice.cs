namespace Jypeli.Devices
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

            this.Storage = new WindowsFileManager( WindowsLocation.DataPath, WindowsLocation.MyDocuments );

			//this.Storage = new LinuxFileManager();

        }
    }
}
