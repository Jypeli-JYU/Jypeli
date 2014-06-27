using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;

namespace Jypeli
{
    public class IsolatedStorageManager : FileManager
    {
        public IsolatedStorageManager()
        {
            _currentDir = ".";
        }

        /// <summary>
        /// Kertoo onko tiedosto tai hakemisto olemassa.
        /// </summary>
        /// <param name="fileName">Tiedoston tai hakemiston nimi</param>
        /// <returns>Onko olemassa</returns>
        public override bool Exists( string fileName )
        {
            var userStore = FMAssert( IsolatedStorageFile.GetUserStoreForApplication, false, null );
            if ( userStore == null ) return false;

            MakeAbsolute( ref fileName );
            return FMAssert( userStore.FileExists, false, false, fileName ) || FMAssert( userStore.DirectoryExists, false, false, fileName );
        }

        /// <summary>
        /// Vaihtaa työhakemistoa.
        /// </summary>
        /// <param name="path">Hakemistopolku.</param>
        /// <returns>Onnistuiko hakemiston vaihtaminen (palauttaa false jos hakemistoa ei ole).</returns>
        public override bool ChDir( string path )
        {
            var userStore = FMAssert( IsolatedStorageFile.GetUserStoreForApplication, false, null );
            if ( userStore == null ) return false;

            string newPath = Path.Combine( _currentDir, path );

            if ( !FMAssert( userStore.DirectoryExists, false, false, newPath ) )
                return false;

            _currentDir = newPath;
            return true;
        }

        /// <summary>
        /// Luo uuden hakemiston.
        /// </summary>
        /// <param name="path">Hakemistopolku.</param>
        public override void MkDir( string path )
        {
            var userStore = FMAssert( IsolatedStorageFile.GetUserStoreForApplication, false, null );
            if ( userStore == null ) return;

            MakeAbsolute( ref path );
            FMAssert( userStore.CreateDirectory, true, path );
        }

        /// <summary>
        /// Tuhoaa hakemiston. Heittää poikkeuksen jos hakemisto ei ole tyhjä.
        /// </summary>
        /// <param name="path"></param>
        public override void RmDir( string path )
        {
            var userStore = FMAssert( IsolatedStorageFile.GetUserStoreForApplication, false, null );
            if ( userStore == null ) return;

            MakeAbsolute( ref path );
            FMAssert( userStore.DeleteDirectory, true, path );
        }

        public override IList<string> GetFileList()
        {
            var userStore = FMAssert( IsolatedStorageFile.GetUserStoreForApplication, false, null );
            if ( userStore == null ) return new List<string>();

            string[] fileList = FMAssert( userStore.GetFileNames, false, new string[] { }, _currentDir + "\\*" );
            return fileList.ToList<string>().AsReadOnly();
        }

        /// <summary>
        /// Avaa tiedoston.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi.</param>
        /// <param name="write">Tarvitaanko tiedostoon kirjoitusoikeus.</param>
        /// <returns>StorageFile-olio</returns>
        public override StorageFile Open( string fileName, bool write )
        {
            MakeAbsolute( ref fileName );
            var stream = FMAssert( openFileStream, write, null, fileName, write );
            return new StorageFile( fileName, stream );
        }

        private Stream openFileStream( string fileName, bool write )
        {
            var userStore = IsolatedStorageFile.GetUserStoreForApplication();
            FileMode mode = write ? FileMode.Create : FileMode.Open;
            FileAccess access = write ? FileAccess.ReadWrite : FileAccess.Read;
            return userStore.OpenFile( fileName, mode, access );
        }

        /// <summary>
        /// Poistaa tiedoston.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi</param>
        public override void Delete( string fileName )
        {
            var userStore = FMAssert( IsolatedStorageFile.GetUserStoreForApplication, false, null );
            if ( userStore == null ) return;

            MakeAbsolute( ref fileName );
            FMAssert( userStore.DeleteFile, true, fileName );
        }
    }
}
