using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Jypeli.Storage
{
    public class RandomAccessAdapter : IRandomAccessStream
    {
        Stream stream;

        public RandomAccessAdapter( Stream stream )
        {
            this.stream = stream;
        }

        public IInputStream GetInputStreamAt( ulong position )
        {
            stream.Position = (long)position;
            return stream.AsInputStream();
        }

        public IOutputStream GetOutputStreamAt( ulong position )
        {
            stream.Position = (long)position;
            return stream.AsOutputStream();
        }

        public ulong Size
        {
            get
            {
                return (ulong)stream.Length;
            }
            set
            {
                stream.SetLength( (long)value );
            }
        }

        public bool CanRead
        {
            get { return this.stream.CanRead; }
        }

        public bool CanWrite
        {
            get { return this.stream.CanWrite; }
        }

        public IRandomAccessStream CloneStream()
        {
            throw new NotSupportedException();
        }

        public ulong Position
        {
            get { return (ulong)this.stream.Position; }
        }

        public void Seek( ulong position )
        {
            this.stream.Seek( (long)position, SeekOrigin.Begin );
        }

        public void Dispose()
        {
            this.stream.Dispose();
        }

        public Windows.Foundation.IAsyncOperationWithProgress<IBuffer, uint> ReadAsync( IBuffer buffer, uint count, InputStreamOptions options )
        {
            return this.GetInputStreamAt( this.Position ).ReadAsync( buffer, count, options );
        }

        public Windows.Foundation.IAsyncOperation<bool> FlushAsync()
        {
            return this.GetOutputStreamAt( this.Position ).FlushAsync();
        }

        public Windows.Foundation.IAsyncOperationWithProgress<uint, uint> WriteAsync( IBuffer buffer )
        {
            return this.GetOutputStreamAt( this.Position ).WriteAsync( buffer );
        }
    }
}
