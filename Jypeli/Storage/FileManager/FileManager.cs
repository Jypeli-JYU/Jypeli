// unset

using System;
using System.IO;

namespace Jypeli
{
    public partial class FileManager
    {
        protected string[] pathCandidates;

        public FileManager(params string[] pathCandidates)
        {
            this.pathCandidates = pathCandidates;
        }

        protected void Initialize()
        {
            if (_currentDir != null)
                return;

            for (int i = 0; i < pathCandidates.Length; i++)
            {
                _currentDir = pathCandidates[i];

                if (Directory.Exists(_currentDir))
                    return;
                else
                {
                    var parent = Directory.GetParent(_currentDir);
                    if (!parent.Exists)
                        continue;
                    Directory.CreateDirectory(_currentDir);
                }
            }
        }
    }

    /// <summary>
    /// Usein käytettyjä polkuja Windowsissa.
    /// </summary>
    public static class FileLocation
    {
        /// <summary>
        /// Omat tiedostot.
        /// </summary>
        public static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /// <summary>
        /// Oma musiikki.
        /// </summary>
        public static readonly string MyMusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        /// <summary>
        /// Omat kuvatiedostot.
        /// </summary>
        public static readonly string MyPictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        /// <summary>
        /// Omat videot.
        /// </summary>
        public static readonly string MyVideos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        /// <summary>
        /// Ohjelman hakemisto.
        /// </summary>
        public static readonly string ExePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

        /// <summary>
        /// Ohjelman data-alihakemisto.
        /// </summary>
        public static readonly string DataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        /// <summary>
        /// Ohjelman content-alihakemisto.
        /// </summary>
        public static readonly string ContentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
    }
}