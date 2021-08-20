
using System;
using System.Diagnostics;
using Jypeli.Assets;
using Jypeli.Rendering;

using Matrix = System.Numerics.Matrix4x4;
using Vector3 = System.Numerics.Vector3;

namespace Jypeli
{
    /// <summary>
    /// Draws simple shapes efficiently.
    /// Draw() calls should be made only between Begin() and End() calls.
    /// Other drawing operations can be done between Begin() and
    /// End().
    /// </summary>
    /// <remarks>
    /// The shapes are added to a buffer. Only when
    /// the buffer is full, a draw call is made to the graphics device.
    /// </remarks>
    internal class ShapeBatch
    {
        VertexPositionColorTexture[] vertexBuffer;

        /// <summary>
        /// Buffer for index values to the vertex buffer.
        /// </summary>
        /// <remarks>
        /// Since some low-end graphics cards don't support 32-bit indices,
        /// 16-bit indices must be used. Which is fine, as it means less traffic
        /// to the graphics card. We just have to make sure that the buffer size
        /// isn't larger than 16000.
        /// </remarks>
        uint[] indexBuffer;

        IShader shader;
        Matrix matrix;

        //SamplerState samplerState;

        uint iVertexBuffer = 0;
        uint iIndexBuffer = 0;
        bool beginHasBeenCalled = false;

        public bool LightingEnabled = true;

        private PrimitiveType primitivetype;


        public void Initialize()
        {
            shader = Graphics.BasicColorShader;

            int vertexBufferSize = Game.GraphicsDevice.BufferSize;
            
            vertexBuffer = new VertexPositionColorTexture[vertexBufferSize];
            indexBuffer = new uint[vertexBufferSize * 2];
        }

        public void Begin( ref Matrix matrix, PrimitiveType p = PrimitiveType.OpenGlTriangles )
        {
            Debug.Assert( !beginHasBeenCalled );
            beginHasBeenCalled = true;

            primitivetype = p;
            this.matrix = matrix;
            iVertexBuffer = 0;
            iIndexBuffer = 0;
        }

        public void End()
        {
            Debug.Assert( beginHasBeenCalled );
            Flush();
            beginHasBeenCalled = false;
        }

        private void Flush()
        {
            if ( iIndexBuffer != 0 )
{
                shader.Use();
                shader.SetUniform("world", matrix * Game.GraphicsDevice.View * Game.GraphicsDevice.Projection);

                Game.GraphicsDevice.DrawIndexedPrimitives(
                    primitivetype,
                    vertexBuffer, iIndexBuffer,
                    indexBuffer);
            }

            iVertexBuffer = 0;
            iIndexBuffer = 0;
        }

        public void Draw(ShapeCache cache, Color color, Vector position, Vector size, float angle)
        {
            if ((iVertexBuffer + cache.Vertices.Length) > vertexBuffer.Length || (iIndexBuffer + cache.Triangles.Length) > indexBuffer.Length)
            {
                Flush();
            }

            Matrix matrix =
                Matrix.CreateScale((float)size.X, (float)size.Y, 1f)
                * Matrix.CreateRotationZ(angle)
                * Matrix.CreateTranslation((float)position.X, (float)position.Y, 0);

            uint startIndex = iVertexBuffer;

            for (int i = 0; i < cache.Vertices.Length; i++)
            {
                Vector v = cache.Vertices[i];
                vertexBuffer[iVertexBuffer++] = new VertexPositionColorTexture(Vector3.Transform(new Vector3((float)v.X, (float)v.Y, 0), matrix), color, Vector.Zero);
            }
            
            for (int i = 0; i < cache.Triangles.Length; i++)
            {
                indexBuffer[iIndexBuffer++] = (uint)cache.Triangles[i].i1 + startIndex;
                indexBuffer[iIndexBuffer++] = (uint)cache.Triangles[i].i2 + startIndex;
                indexBuffer[iIndexBuffer++] = (uint)cache.Triangles[i].i3 + startIndex;
            }
        }
        
        public void DrawOutlines(ShapeCache cache, Color color, Vector position, Vector size, float angle)
        {
            if ((iVertexBuffer + cache.Vertices.Length) > vertexBuffer.Length || (iIndexBuffer + cache.Triangles.Length) > indexBuffer.Length)
            {
                Flush();
            }

            Matrix matrix =
                Matrix.CreateScale((float)size.X, (float)size.Y, 1f)
                * Matrix.CreateRotationZ(angle)
                * Matrix.CreateTranslation((float)position.X, (float)position.Y, 0);

            uint startIndex = iVertexBuffer;

            for (int i = 0; i < cache.Vertices.Length; i++)
            {
                Vector v = cache.Vertices[i];
                vertexBuffer[iVertexBuffer++] = new VertexPositionColorTexture(Vector3.Transform(new Vector3((float)v.X, (float)v.Y, 0), matrix), color, Vector.Zero);
            }

            for (int i = 0; i < cache.OutlineIndices.Length - 1; i++)
            {
                indexBuffer[iIndexBuffer++] = (uint)cache.OutlineIndices[i] + startIndex;
                indexBuffer[iIndexBuffer++] = (uint)cache.OutlineIndices[i + 1] + startIndex;
            }
            
            indexBuffer[iIndexBuffer++] = (uint)cache.OutlineIndices[^1] + startIndex;
            indexBuffer[iIndexBuffer++] = (uint)cache.OutlineIndices[0] + startIndex;
        }
    }
}
