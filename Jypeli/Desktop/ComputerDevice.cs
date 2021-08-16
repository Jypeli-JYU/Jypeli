namespace Jypeli.Devices
{
    /// <summary>
    /// Fyysinen laite, joka on tietokone
    /// </summary>
    public class ComputerDevice : Device
    {
        /// <summary>
        /// Onko kyseessä mobiililaite (ei)
        /// </summary>
        public override bool IsMobileDevice
        {
            get { return false; }
        }

        /// <summary>
        /// Onko kyseessä puhelin (ei)
        /// </summary>
        public override bool IsPhone
        {
            get { return false; }
        }

        /// <summary>
        /// Fyysinen laite, joka on tietokone
        /// </summary>
        public ComputerDevice()
        {
            Storage = new FileManager(FileLocation.DataPath, FileLocation.MyDocuments);
            ContentPath = FileLocation.ContentPath;
        }
    }
}
