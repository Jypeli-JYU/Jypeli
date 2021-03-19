using System;
using System.IO;

#if WINRT
using Windows.Storage.Streams;
#endif

namespace Jypeli
{
    public partial class FileManager
    {
        public LoadState BeginLoadContent( string assetName )
        {
            if ( Game.Instance == null )
                throw new InvalidOperationException( "Content can not be loaded here, because the game has not been initialized." );

            StorageFile contentFile = Game.Instance.Content.Load<StorageFile>( assetName );
            return new LoadState( contentFile, assetName );
        }

        public T LoadContent<T>( T obj, string assetName )
        {
            if ( Game.Instance == null )
                throw new InvalidOperationException( "Content can not be loaded here, because the game has not been initialized." );

            byte[] contentData = Game.Instance.Content.Load<byte[]>( assetName );
            MemoryStream contentStream = new MemoryStream( contentData );
            StorageFile contentFile = new StorageFile( assetName, contentStream );

            LoadState state = BeginLoad( contentFile, assetName );
            T result = state.Load<T>( obj, "default" );
            state.EndLoad();
            return result;
        }
    }
}
