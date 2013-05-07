﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if !DISABLE_EFFECTS
using Jypeli.Effects;
#endif

namespace Jypeli
{
    internal class TextureCoordinates
    {
        public Vector2 TopLeft;
        public Vector2 TopRight;
        public Vector2 BottomLeft;
        public Vector2 BottomRight;
    }

    /// <summary>
    /// Draws images efficiently.
    /// Draw() calls should be made only between Begin() and End() calls.
    /// Other drawing operations can be done between Begin() and
    /// End().
    /// </summary>
    /// <remarks>
    /// The positions of images are added to a buffer. Only when
    /// the buffer is full, a draw call is made to the graphics device.
    /// </remarks>
    internal class ImageBatch
    {
        private const int DefaultBufferSize = 512;

        static readonly Vector3[] Vertices = new Vector3[]
        {
            // Triangle 1
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),

            // Triangle 2
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
        };

        static readonly int VerticesPerTexture = Vertices.Length;

        Matrix matrix;
        Texture2D texture;
        Effect effect;
        VertexPositionTexture[] vertexBuffer;
        int BufferSize;
        int iTexture = 0;
        bool beginHasBeenCalled = false;

        public bool LightingEnabled = true;


        public ImageBatch()
        {
        }

        public void Initialize()
        {
            //var capabilities = Game.GraphicsDevice.GraphicsDeviceCapabilities;
            // Capabilities no longer supported in XNA 4.0
            // GraphicsProfile.Reach maximum primitive count = 65535
            this.BufferSize = Math.Min( DefaultBufferSize, 65535 / 2 );
            vertexBuffer = new VertexPositionTexture[BufferSize * VerticesPerTexture];
        }

        public void Begin( ref Matrix matrix, Texture2D texture )
        {
            Debug.Assert( !beginHasBeenCalled );
            beginHasBeenCalled = true;

            this.matrix = matrix;
            this.texture = texture;
            iTexture = 0;
        }

        public void End()
        {
            Debug.Assert( beginHasBeenCalled );

            Flush();

            int textureCount = iTexture;
            beginHasBeenCalled = false;
        }

        private void Flush()
        {
            if ( iTexture > 0 )
            {
                var device = Game.GraphicsDevice;
                device.RasterizerState = RasterizerState.CullClockwise;
                device.BlendState = BlendState.AlphaBlend;

                effect = Graphics.GetTextureEffect(ref matrix, texture, LightingEnabled);
                effect.Techniques[0].Passes[0].Apply();

                // When drawing individual textures, set the texture addressing modes
                // to clamp in order to avoid unwanted edges.
                // EDIT (Denis Zhidkikh): changeable clamp state
                device.SamplerStates[0] = Graphics.GetDefaultSamplerState();

                device.DrawUserPrimitives<VertexPositionTexture>(
                    PrimitiveType.TriangleList,
                    vertexBuffer, 0,
                    iTexture * 2 );
            }

            iTexture = 0;
        }

        public void Draw( TextureCoordinates c, Vector2 position, Vector2 size, float angle )
        {
            Debug.Assert( beginHasBeenCalled );

            if ( iTexture >= BufferSize )
                Flush();

            Matrix matrix =
                Matrix.CreateScale( size.X, size.Y, 1f )
                * Matrix.CreateRotationZ( angle )
                * Matrix.CreateTranslation( position.X, position.Y, 0 )
                ;
            Vector3[] transformedPoints = new Vector3[VerticesPerTexture];
            Vector3.Transform( Vertices, ref matrix, transformedPoints );

            int startIndex = ( iTexture * VerticesPerTexture );

            for ( int i = 0; i < VerticesPerTexture; i++ )
            {
                int bi = ( iTexture * VerticesPerTexture ) + i;
                vertexBuffer[bi].Position = transformedPoints[i];
            }

            // Triangle 1
            vertexBuffer[startIndex + 0].TextureCoordinate = c.TopLeft;
            vertexBuffer[startIndex + 1].TextureCoordinate = c.BottomLeft;
            vertexBuffer[startIndex + 2].TextureCoordinate = c.TopRight;

            // Triangle 2
            vertexBuffer[startIndex + 3].TextureCoordinate = c.BottomLeft;
            vertexBuffer[startIndex + 4].TextureCoordinate = c.BottomRight;
            vertexBuffer[startIndex + 5].TextureCoordinate = c.TopRight;

            iTexture++;
        }
    }
}
