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
            this.Storage = new FileManager(FileLocation.DataPath, FileLocation.MyDocuments);
        }
    }
}
