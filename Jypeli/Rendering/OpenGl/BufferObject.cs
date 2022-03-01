using System;
using Silk.NET.OpenGL;

namespace Jypeli.Rendering.OpenGl
{
    public unsafe class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        //Our handle, buffertype and the GL instance this class will use, these are private because they have no reason to be public.
        //Most of the time you would want to abstract items to make things like this invisible.
        private uint _handle;
        private BufferTargetARB _bufferType;
        private GL _gl;

        public BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
        {
            //Setting the gl instance and storing our buffer type.
            _gl = gl;
            _bufferType = bufferType;

            //Getting the handle, and then uploading the data to said handle.
            _handle = _gl.GenBuffer();
            Bind();
            fixed (void* d = data)
            {
                _gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.DynamicDraw);
            }
        }

        public void UpdateBuffer(nint offset, Span<TDataType> data)
        {
            Bind();
            fixed (void* d = data)
            {
                _gl.BufferSubData(_bufferType, offset, (nuint)(data.Length * sizeof(TDataType)), d);
            }
        }

        /// <summary>
        /// Asettaa verteksiattribuutteja lähetettävään (float) dataan
        /// </summary>
        /// <param name="index">Indeksi</param>
        /// <param name="size">Vastaanotettavan muuttujan koko tavuina</param>
        /// <param name="stride">Taulukon yhden alkion koko</param>
        /// <param name="offset">Offset tavuina mistä kohtaa alkion dataa luetaan</param>
        /// <param name="vertexDivisor">Kuinka moneen instansoituun esiintymään käytetään yhden taulukon alkion dataa</param>
        public void VertexAttributePointer(uint index, int size, VertexAttribPointerType type, uint stride, uint offset, uint vertexDivisor)
        {
            Bind();
            _gl.EnableVertexAttribArray(index);
            _gl.VertexAttribPointer(index, size, type, false, stride, (void*)offset);
            _gl.VertexAttribDivisor(index, vertexDivisor);
        }

        /// <summary>
        /// Asettaa verteksiattribuutteja lähetettävään (float) dataan
        /// </summary>
        /// <param name="index">Indeksi</param>
        /// <param name="size">Vastaanotettavan muuttujan koko tavuina</param>
        /// <param name="stride">Taulukon yhden alkion koko</param>
        /// <param name="offset">Offset tavuina mistä kohtaa alkion dataa luetaan</param>
        public void VertexAttributePointer(uint index, int size, VertexAttribPointerType type, uint stride, uint offset)
        {
            VertexAttributePointer(index, size, type, stride, offset, 1);
        }

        public void Bind()
        {
            //Binding the buffer object, with the correct buffer type.
            _gl.BindBuffer(_bufferType, _handle);
        }

        public void Dispose()
        {
            //Remember to delete our buffer.
            _gl.DeleteBuffer(_handle);
        }
    }
}
