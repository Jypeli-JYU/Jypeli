using System.IO;

namespace Jypeli.Devices
{
    /// <summary>
    /// Fyysinen laite, joka on tietokone
    /// </summary>
    public class ComputerDevice : Device
    {
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

        internal override Stream StreamContent(string name, string[] extensions)
        {
            string fullPath = Path.Combine(ContentPath, name);
            if (File.Exists(fullPath))
                return File.OpenRead(fullPath);
            else
            {
                return File.OpenRead(FindContentFile(fullPath, extensions));
            }
        }

        private string FindContentFile(string fullPath, string[] extensions)
        {
            foreach (var ext in extensions)
            {
                string withExt = Path.ChangeExtension(fullPath, ext);
                if (File.Exists(withExt))
                    return withExt;
            }
            throw new FileNotFoundException($"Tiedostoa {fullPath} ei löydy");
        }
    }
}
