
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        Int16[] indexBuffer;

        Effect effect;
        Matrix matrix;

        SamplerState samplerState;

        int iVertexBuffer = 0;
        int iIndexBuffer = 0;
        bool beginHasBeenCalled = false;

        public bool LightingEnabled = true;


        public void Initialize()
        {
            samplerState = new SamplerState
            {
                AddressU = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
            };
            //var capabilities = Game.GraphicsDevice.GraphicsDeviceCapabilities;
            // Capabilities no longer supported in XNA 4.0
            // GraphicsProfile.Reach maximum primitive count = 65535
            int vertexBufferSize = Math.Min( DefaultBufferSize, 65535 * 3 );
            
            vertexBuffer = new VertexPositionColor[vertexBufferSize];
            indexBuffer = new Int16[vertexBufferSize * 2];
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
                Game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                Game.GraphicsDevice.SamplerStates[0] = samplerState;

                effect = Graphics.GetColorEffect(ref matrix, LightingEnabled);
                for ( int i = 0; i < effect.CurrentTechnique.Passes.Count; i++ )
                    effect.CurrentTechnique.Passes[i].Apply();

                Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList,
                    vertexBuffer, 0, iVertexBuffer,
                    indexBuffer, 0,
                    iIndexBuffer / 3 );
            }

            iVertexBuffer = 0;
            iIndexBuffer = 0;
        }

        public void Draw( Vector[] vertices, Int16[] indices, Color color, Vector2 position, Vector2 size, float angle )
        {
            if ( ( iVertexBuffer + vertices.Length ) > vertexBuffer.Length ||
                ( iIndexBuffer + indices.Length ) > indexBuffer.Length )
            {
                Flush();
            }

            Matrix matrix =
                Matrix.CreateScale( size.X, size.Y, 1f )
                * Matrix.CreateRotationZ( angle )
                * Matrix.CreateTranslation( position.X, position.Y, 0 )
                ;

            int startIndex = iVertexBuffer;

            for ( int i = 0; i < vertices.Length; i++ )
            {
                Vector p = vertices[i];
                Vector3 p3 = new Vector3( (float)p.X, (float)p.Y, 0f );
                vertexBuffer[iVertexBuffer].Position = Vector3.Transform( p3, matrix );
                vertexBuffer[iVertexBuffer].Color = color.AsXnaColor();
                iVertexBuffer++;
            }

            for ( int i = 0; i < indices.Length; i++ )
            {
                indexBuffer[iIndexBuffer] = (Int16)( startIndex + indices[i] );
                iIndexBuffer++;
            }
        }
    }
}
