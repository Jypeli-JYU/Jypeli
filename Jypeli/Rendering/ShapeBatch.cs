
using System;
using System.Diagnostics;
using Jypeli.Rendering;
using Jypeli.Rendering.OpenGl;
using Silk.NET.OpenGL;

using Matrix = System.Numerics.Matrix4x4;

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
        const int DefaultBufferSize = 512;

        VertexPositionColor[] vertexBuffer;

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

        //Effect effect;
        Matrix matrix;

        //SamplerState samplerState;

        uint iVertexBuffer = 0;
        uint iIndexBuffer = 0;
        bool beginHasBeenCalled = false;

        public bool LightingEnabled = true;


        public void Initialize()
        {
            /*
            samplerState = new SamplerState
            {
                AddressU = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
            };
            */
            //var capabilities = Game.GraphicsDevice.GraphicsDeviceCapabilities;
            // Capabilities no longer supported in XNA 4.0
            // GraphicsProfile.Reach maximum primitive count = 65535
            int vertexBufferSize = Math.Min( DefaultBufferSize, 65535 * 3 );
            
            vertexBuffer = new VertexPositionColor[vertexBufferSize];
            indexBuffer = new uint[vertexBufferSize * 2];
        }

        public void Begin( ref Matrix matrix )
        {
            Debug.Assert( !beginHasBeenCalled );
            beginHasBeenCalled = true;

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
                /*Game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                Game.GraphicsDevice.SamplerStates[0] = samplerState;

                effect = Graphics.GetColorEffect(ref matrix, LightingEnabled);
                for ( int i = 0; i < effect.CurrentTechnique.Passes.Count; i++ )
                    effect.CurrentTechnique.Passes[i].Apply();*/

                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.Triangles,
                    vertexBuffer, iIndexBuffer,
                    indexBuffer);
            }

            iVertexBuffer = 0;
            iIndexBuffer = 0;
        }

        public void Draw(ShapeCache cache, Color color, Vector position, Vector size, float angle)
        {
            if ( ( iVertexBuffer + cache.Vertices.Length ) > vertexBuffer.Length ||
                ( iIndexBuffer + cache.Triangles.Length ) > indexBuffer.Length )
            {
                Flush();
            }

            Matrix matrix =
                Matrix.CreateScale((float)size.X, (float)size.Y, 1f)
                * Matrix.CreateRotationZ(angle)
                * Matrix.CreateTranslation((float)position.X, (float)position.Y, 0)
                * Matrix.CreateOrthographic(1024, 768, 1, -1); // TODO: Jos tähän laittaa Game.Screen.ViewportWidth... Ei käyttäydy oikein ikkunan kokoa muutettaessa

            uint startIndex = iVertexBuffer;

            for (int i = 0; i < cache.Vertices.Length; i++)
            {
                Vector v = cache.Vertices[i];
                vertexBuffer[iVertexBuffer++] = new VertexPositionColor(Vector3.Transform(new Vector3((float)v.X, (float)v.Y, 0), matrix), color);
            }

            for (int i = 0; i < cache.Triangles.Length; i++)
            {
                indexBuffer[iIndexBuffer++] = (uint)cache.Triangles[i].i1 + startIndex;
                indexBuffer[iIndexBuffer++] = (uint)cache.Triangles[i].i2 + startIndex;
                indexBuffer[iIndexBuffer++] = (uint)cache.Triangles[i].i3 + startIndex;
            }

        }
    }
}
