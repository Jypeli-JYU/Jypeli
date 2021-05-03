using System.Collections.Generic;
using System.IO;

namespace Jypeli
{
    public partial class FileManager
    {
        /// <summary>
        /// Luo uuden tiedoston
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public StorageFile Create( string fileName )
        {
            return Open( fileName, true );
        }
        
        /// <summary>
        /// Kertoo onko tiedosto tai hakemisto olemassa.
        /// </summary>
        /// <param name="fileName">Tiedoston tai hakemiston nimi</param>
        /// <returns>Onko olemassa</returns>
        public virtual bool Exists( string fileName )
        {
            Initialize();
            MakeAbsolute( ref fileName );
            return FMAssert( File.Exists, false, false, fileName ) || FMAssert( Directory.Exists, false, false, fileName );
        }
        
        /// <summary>
        /// Avaa tiedoston.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi.</param>
        /// <param name="write">Tarvitaanko tiedostoon kirjoitusoikeus.</param>
        /// <returns>StorageFile-olio</returns>
        public virtual StorageFile Open( string fileName, bool write )
        {
            Initialize();
            MakeAbsolute( ref fileName );
            Stream stream = FMAssert<string, bool, Stream?>( openFileStream, write, null, fileName, write );
            return new StorageFile( fileName, stream );
        }
        
        private Stream openFileStream(string fileName, bool write)
        {
            FileMode mode = write ? FileMode.Create : FileMode.Open;
            FileAccess access = write ? FileAccess.ReadWrite : FileAccess.Read;
            return new FileStream( fileName, mode, access );
        }
        
        /// <summary>
        /// Poistaa tiedoston.
        /// Ei heitä poikkeusta, jos tiedostoa ei ole olemassa.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi</param>
        public virtual void Delete( string fileName )
        {
            Initialize();
            MakeAbsolute( ref fileName );
            FMAssert( File.Delete, true, fileName );
        }
    }
}
