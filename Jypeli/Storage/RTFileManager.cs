using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;

namespace Jypeli
{
    public class RTFileManager : FileManager
    {
        Stack<StorageFolder> folderStack = new Stack<StorageFolder>();

        protected override void Initialize()
        {
            if ( folderStack.Count > 0  )
                return;

            folderStack.Push( Windows.Storage.ApplicationData.Current.LocalFolder );
        }

        private void Await( IAsyncAction action )
        {
            // Quick and dirty way to do things synchronously
            // Don't use in threaded programming or deadlocks will ensue!
            action.AsTask().Wait();
            action.Close();
        }

        private T Await<T>(IAsyncOperation<T> op)
        {
            // Quick and dirty way to do things synchronously
            // Don't use in threaded programming or deadlocks will ensue!

            T result = default( T );
            Exception innerEx = null;
            
            try
            {
                op.AsTask().Wait();
                result = op.GetResults();
            }
            catch ( AggregateException ex )
            {
                innerEx = ex.InnerException;
            }
            finally
            {
                op.Close();
            }

            if ( innerEx != null )
                throw innerEx;

            return result;
        }

        public override bool ChDir( string path )
        {
            string[] parts = path.Split( '\\', '/' );
            StorageFolder folder = folderStack.Peek();
            
            // Two nested constructors required so not to reverse the stack!
            // You're welcome to find a better way, but _please_ test and make sure it works!
            Stack<StorageFolder> newStack = new Stack<StorageFolder>( new Stack<StorageFolder>( folderStack ) );

            for ( int i = 0; i < parts.Length; i++ )
            {
                if ( parts[i] == "." )
                    continue;

                if ( parts[i] == ".." )
                {
                    if ( newStack.Count > 1 )
                        newStack.Pop();

                    continue;
                }

                try
                {
                    StorageFolder newFolder = Await( newStack.Peek().GetFolderAsync( path ) );

                    if ( newFolder == null )
                        return false;

                    newStack.Push( newFolder );
                }
                catch ( FileNotFoundException )
                {
                    return false;
                }
                catch ( ArgumentException )
                {
                    // May be a file instead of a folder
                    return false;
                }
            }

            folderStack = newStack;

            return true;
        }

        public override void MkDir( string path )
        {
            Initialize();

            try
            {
                Await( folderStack.Peek().CreateFolderAsync( path ) );
            }
            catch ( Exception ex )
            {
                if ( (uint)ex.HResult == 0x800700B7 )
                {
                    // Directory already exists, do nothing
                }
                else
                {
                    // Other exception
                    throw ex;
                }
            }
        }

        public override void RmDir( string path )
        {
            Initialize();

            StorageFolder subdir = Await( folderStack.Peek().GetFolderAsync( path ) );
            Await( subdir.DeleteAsync() );
        }

        public override IList<string> GetFileList()
        {
            Initialize();

            var list = Await( folderStack.Peek().GetFilesAsync() );
            if (list == null) return new List<string>();

            return new List<string>( list.ConvertAll( file => file.Name ) );
        }

        public override bool Exists( string fileName )
        {
            Initialize();

            try
            {
                Await( folderStack.Peek().GetFileAsync( fileName ) );
                return true;
            }
            catch ( FileNotFoundException )
            {
            }
            catch ( ArgumentException )
            {
                // Is a folder?
            }

            try
            {
                Await( folderStack.Peek().GetFolderAsync( fileName ) );
                return true;
            }
            catch ( FileNotFoundException )
            {
            }

            return false;
        }

        public override StorageFile Open( string fileName, bool write )
        {
            Initialize();

            if ( write && !Exists( fileName ) )
            {
                Await( folderStack.Peek().CreateFileAsync( fileName ) );
            }

            var file = Await( folderStack.Peek().GetFileAsync( fileName ) );
            if ( file == null ) return null;

            return new Jypeli.StorageFile(
                fileName,
                Await( file.OpenAsync( write ? FileAccessMode.ReadWrite : FileAccessMode.Read ) )
            );
        }

        public override void Delete( string fileName )
        {
            Initialize();

            try
            {
                var file = Await( folderStack.Peek().GetFileAsync( fileName ) );
                if ( file != null ) Await( file.DeleteAsync() );
            }
            catch ( FileNotFoundException )
            {
            }
        }
    }
}
