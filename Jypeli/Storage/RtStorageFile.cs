using System;
using System.IO;
using System.Xml;
using Jypeli.Storage;
using Windows.Storage.Streams;

namespace Jypeli
{
    /// <summary>
    /// Tiedosto.
    /// </summary>
    public class StorageFile : IDisposable
    {
        public string Name { get; set; }
        public Stream Stream { get; private set; }

        /// <summary>
        /// Luo uuden tiedoston virrasta.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stream"></param>
        public StorageFile( string name, Stream stream )
        {
            Name = name;
            Stream = stream;
            if ( Stream == null ) Stream = new MemoryStream();
        }

        /// <summary>
        /// Luo uuden tiedoston virrasta.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stream"></param>
        public StorageFile( string name, IRandomAccessStream stream )
            : this(name, stream.AsStream())
        {
        }

        /// <summary>
        /// Sulkee tiedoston.
        /// </summary>
        public void Close()
        {
            Stream.Dispose();
        }

        /// <summary>
        /// Sulkee tiedoston.
        /// </summary>
        public void Dispose()
        {
            Stream.Dispose();
        }

        public void SaveData( XmlWriter writer, Type type, object obj, bool saveAllFields )
        {
            RTStorageSerializer.Serialize( writer, type, obj, saveAllFields );
        }

        public object LoadData( XmlReader reader, Type type, object obj )
        {
            return RTStorageSerializer.Deserialize( reader, type, obj );
        }
    }
}
