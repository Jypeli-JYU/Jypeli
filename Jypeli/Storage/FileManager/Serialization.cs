using System;
using System.IO;

namespace Jypeli
{
    public partial class FileManager
    {
        public LoadState BeginLoad( string fileName )
        {
            return new LoadState( this, fileName );
        }

        internal LoadState BeginLoad( StorageFile file, string fileName )
        {
            return new LoadState( file, fileName );
        }

        public SaveState BeginSave( string tag )
        {
            return new SaveState( this, tag );
        }

        public T Load<T>( T obj, string fileName )
        {
            if ( !Exists( fileName ) )
                throw new FileNotFoundException();

            LoadState state = BeginLoad( fileName );
            T result = state.Load<T>( obj, "default" );
            state.EndLoad();
            return result;
        }

        public T TryLoad<T>( T obj, string fileName )
        {
            try
            {
                return Load<T>( obj, fileName );
            }
            catch ( Exception )
            {
                return obj;
            }
        }

        public void Save<T>( T obj, string fileName )
        {
            SaveState state = BeginSave( fileName );
            state.Save( obj, typeof( T ), "default" );
            state.EndSave();
        }

        public void TrySave<T>( T obj, string fileName )
        {
            try
            {
                Save<T>( obj, fileName );
            }
            catch ( Exception )
            {
            }
        }

        public void Save( object obj, string fileName )
        {
            SaveState state = BeginSave( fileName );
            state.Save( obj, obj.GetType(), "default" );
            state.EndSave();
        }

        public void TrySave( object obj, string fileName )
        {
            try
            {
                Save( obj, fileName );
            }
            catch ( Exception )
            {
            }
        }

        /// <summary>
        /// Vie virran sisällön tiedostoon.
        /// </summary>
        /// <param name="objStream">Virta</param>
        /// <param name="fileName">Tiedoston nimi</param>
        public void Export( Stream objStream, string fileName )
        {
            using ( var f = Create( fileName ) )
            {
                objStream.CopyStreamTo( f.Stream );
            }
        }
    }
}
