#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XnaV2 = Microsoft.Xna.Framework.Vector2;
using XnaV3 = Microsoft.Xna.Framework.Vector3;


namespace Jypeli
{
    /// <summary>
    /// Luokka, joka sisältää metodeita kuvioiden ja tekstuurien piirtämiseen 2D-tasossa.
    /// </summary>
    /// <remarks>
    /// Toteutus on yksinkertainen ja siten hidas. Jos on paljon samankaltaisia piirrettäviä
    /// kohteita, niin käytä mielummin Batch-luokkia.
    /// </remarks>
    public static class Renderer
    {
        /// <summary>
        /// Vertices that form a rectangle on which to draw textures.
        /// </summary>
        static readonly VertexPositionTexture[] textureVertices = new VertexPositionTexture[]
        {
            new VertexPositionTexture( new XnaV3(-0.5f, 0.5f, 0), new XnaV2(0.0f, 0.0f) ),
            new VertexPositionTexture( new XnaV3(-0.5f, -0.5f, 0), new XnaV2(0.0f, 1.0f) ),
            new VertexPositionTexture( new XnaV3(0.5f, 0.5f, 0), new XnaV2(1.0f, 0.0f) ),
            new VertexPositionTexture( new XnaV3(0.5f, -0.5f, 0), new XnaV2(1.0f, 1.0f) )
        };

        /// <summary>
        /// Indices that form two triangles from the vertex array.
        /// </summary>
        static readonly Int16[] textureTriangleIndices = new short[]
        {
            0, 1, 2,
            1, 3, 2
        };

        public static bool LightingEnabled { get; set; }

        static readonly BlendState NoDrawingToScreenBufferBlendState = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None,
        };

        static readonly DepthStencilState drawShapeToStencilBufferState = new DepthStencilState
        {
            StencilEnable = true,
            ReferenceStencil = 0,
            StencilFunction = CompareFunction.Equal,
            StencilPass = StencilOperation.IncrementSaturation,
        };

        static readonly DepthStencilState drawAccordingToStencilBufferState = new DepthStencilState
        {
            StencilEnable = true,
            ReferenceStencil = 1,
            StencilFunction = CompareFunction.LessEqual,
            StencilPass = StencilOperation.Keep,
        };

        private static bool isDrawingInsideShape = false;
        private static DepthStencilState currentStencilState = DepthStencilState.None;

        private static VertexPositionTexture[] MakeTextureVertices( Vector wrapSize )
        {
            VertexPositionTexture[] tempVertices = new VertexPositionTexture[textureVertices.Length];
            for ( int i = 0; i < textureVertices.Length; i++ )
            {
                tempVertices[i].Position = textureVertices[i].Position;
            }

            float px = MathHelper.Clamp( (float)wrapSize.X, -1, 1 );
            float py = MathHelper.Clamp( (float)wrapSize.Y, -1, 1 );

            // Since the origin in texture coordinates is at upper left corner,
            // but at center in an object's coordinates, we need to make some
            // adjustments here. Also, this makes partial textures possible.
            float left = -(float)Math.Sign( wrapSize.X ) / 2 + 0.5f;
            float right = left + px;
            float top = -(float)Math.Sign( wrapSize.Y ) / 2 + 0.5f;
            float bottom = top + py;
                       
            tempVertices[0].TextureCoordinate = new XnaV2( left, top );
            tempVertices[1].TextureCoordinate = new XnaV2( left, bottom );
            tempVertices[2].TextureCoordinate = new XnaV2( right, top );
            tempVertices[3].TextureCoordinate = new XnaV2( right, bottom );

            return tempVertices;
        }

        public static void DrawImage( Image texture, ref Matrix matrix, Vector wrapSize )
        {
            if ( wrapSize.X == 0 || wrapSize.Y == 0 ) return;

            var device = Game.GraphicsDevice;
            var tempVertices = MakeTextureVertices( wrapSize );

            device.RasterizerState = RasterizerState.CullClockwise;
#if WINDOWS_PHONE
            // The WP7 emulator interprets cullmodes incorectly sometimes.
            device.RasterizerState = RasterizerState.CullNone;
#endif

            device.BlendState = BlendState.AlphaBlend;

            float wrapX = (float)Math.Abs( wrapSize.X );
            float wrapY = (float)Math.Abs( wrapSize.Y );

            if ( wrapX <= 1 && wrapY <= 1 )
            {
                // Draw only once
                DrawImageTexture( texture, matrix, device, tempVertices );
                return;
            }

            float wx = (float)( Math.Sign( wrapSize.X ) );
            float wy = (float)( Math.Sign( wrapSize.Y ) );
            float tileW = 1 / wrapX;
            float tileH = 1 / wrapY;
            float topLeftX = -0.5f + 0.5f * tileW;
            float topLeftY = 0.5f - 0.5f * tileH;
            float partX = wrapX - (int)wrapX;
            float partY = wrapY - (int)wrapY;
            
            for ( int y = 0; y < (int)wrapY; y++ )
            {
                for ( int x = 0; x < (int)wrapX; x++ )
                {
                    Matrix m =
                        Matrix.CreateScale( 1 / wrapX, 1 / wrapY, 1 ) *
                        Matrix.CreateTranslation( topLeftX + x * tileW, topLeftY - y * tileH, 0 ) *
                        matrix;
                    DrawImageTexture( texture, m, device, tempVertices );
                }

                if ( partX > 0 )
                {
                    // Draw a partial horizontal tile
                    Matrix m =
                        Matrix.CreateScale( partX, 1, 1 ) *
                        Matrix.CreateScale( 1 / wrapX, 1 / wrapY, 1 ) *
                        Matrix.CreateTranslation( -tileW / 2 + tileW * partX / 2, 0, 0 ) *
                        Matrix.CreateTranslation( topLeftX + (int)wrapX * tileW, topLeftY - y * tileH, 0 ) *
                        matrix;

                    DrawImage( texture, ref m, new Vector( wx * partX, wy ) );
                }
            }

            if ( partY > 0 )
            {
                for ( int x = 0; x < (int)wrapX; x++ )
                {
                    // Draw a partial vertical tile
                    Matrix m =
                        Matrix.CreateScale( 1, partY, 1 ) *
                        Matrix.CreateScale( 1 / wrapX, 1 / wrapY, 1 ) *
                        Matrix.CreateTranslation( 0, tileH / 2 - tileH * partY / 2, 0 ) *
                        Matrix.CreateTranslation( topLeftX + x * tileW, topLeftY - (int)wrapY * tileH, 0 ) *
                        matrix;

                    DrawImage( texture, ref m, new Vector( wx, wy * partY ) );
                }

                if ( partX > 0 )
                {
                    // Draw a partial diagonal tile
                    Matrix m =
                        Matrix.CreateScale( partX, partY, 1 ) *
                        Matrix.CreateScale( 1 / wrapX, 1 / wrapY, 1 ) *
                        Matrix.CreateTranslation( -tileW / 2 + tileW * partX / 2, tileH / 2 - tileH * partY / 2, 0 ) *
                        Matrix.CreateTranslation( topLeftX + (int)wrapX * tileW, topLeftY - (int)wrapY * tileH, 0 ) *
                        matrix;

                    DrawImage( texture, ref m, new Vector( wx * partX, wy * partY ) );
                }
            }
        }

        private static void DrawImageTexture( Image texture, Matrix matrix, GraphicsDevice device, VertexPositionTexture[] tempVertices )
        {
            Effect effect = Graphics.GetTextureEffect( ref matrix, texture.XNATexture, LightingEnabled );
            Graphics.SetSamplerState();

			foreach ( EffectPass pass in effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(
                    PrimitiveType.TriangleList,
                    tempVertices, 0, tempVertices.Length,
                    textureTriangleIndices, 0, textureTriangleIndices.Length / 3
                    );
            }

            Graphics.ResetSamplerState();
        }

        /// <summary>
        /// Makes all the subsequent draw calls until <c>EndDrawingInsideShape</c> limit the
        /// drawing inside <c>shape</c> (transformed by the matrix).
        /// </summary>
        /// <remarks>
        /// The draw calls between Begin and End must not change the DepthStencilState of the graphics device.
        /// If drawing is done with a sprite batch, the <c>spritebatch.Begin</c> call must be given the
        /// DepthStencilState that can be obtained from <c>currentStencilState</c> variable.
        /// </remarks>
        public static void BeginDrawingInsideShape( Shape shape, ref Matrix transformation )
        {
            if ( shape.Cache.Triangles == null )
                throw new ArgumentException( "The shape must have triangles" );
            if ( isDrawingInsideShape )
                throw new Exception( "EndDrawingInsideShape must be called before calling this function again" );

            isDrawingInsideShape = true;
            var device = Game.GraphicsDevice;

            device.Clear( ClearOptions.Stencil, Color.Black.AsXnaColor(), 0, 0 );
            device.DepthStencilState = currentStencilState = drawShapeToStencilBufferState;

            DrawFilledShape( shape.Cache, ref transformation, Color.White, NoDrawingToScreenBufferBlendState );

            device.DepthStencilState = currentStencilState = drawAccordingToStencilBufferState;
        }

        public static void EndDrawingInsideShape()
        {
            if ( !isDrawingInsideShape )
                throw new Exception( "BeginDrawingInsideShape must be called first" );

            Game.GraphicsDevice.DepthStencilState = currentStencilState = DepthStencilState.None;
            isDrawingInsideShape = false;
        }

        /// <summary>
        /// Piirtää tekstiä ruudulle
        /// </summary>
        /// <param name="text">Teksti</param>
        /// <param name="position">Paikka</param>
        /// <param name="font">Fontti</param>
        /// <param name="color">Tekstin väri</param>
        public static void DrawText( string text, Vector position, Font font, Color color )
        {
            Vector2 textSize = font.XnaFont.MeasureString(text);
            Vector2 xnaPos = ScreenView.ToXnaCoords( position, Game.Screen.Size, (Vector)textSize );

            SpriteBatch spriteBatch = Graphics.SpriteBatch;
            spriteBatch.Begin();
            font.XnaFont.DrawText(Graphics.FontRenderer, text, xnaPos.ToSystemNumerics(), color.AsXnaColor().ToSystemDrawing());
            spriteBatch.End();
        }

        /// <summary>
        /// Piirtää tekstiä ruudulle
        /// </summary>
        /// <param name="text">Teksti</param>
        /// <param name="transformation">Transformaatio</param>
        /// <param name="font">Fontti</param>
        /// <param name="color">Tekstin väri</param>
        public static void DrawText( string text, ref Matrix transformation, Font font, Color color )
        {
            Vector textSize = (Vector)font.XnaFont.MeasureString( text );
            Matrix m = ScreenView.ToXnaCoords(ref transformation, Game.Screen.Size, textSize );

            SpriteBatch spriteBatch = Graphics.SpriteBatch;
            spriteBatch.Begin( SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.LinearClamp, currentStencilState, RasterizerState.CullCounterClockwise, null, m );
            font.XnaFont.DrawText(Graphics.FontRenderer, text, Vector2.Zero.ToSystemNumerics(), color.AsXnaColor().ToSystemDrawing());
            spriteBatch.End();
        }

        /// <summary>
        /// Piirtää tekstiä ruudulle
        /// </summary>
        /// <param name="text">Teksti</param>
        /// <param name="transformation">Transformaatio</param>
        /// <param name="font">Fontti</param>
        /// <param name="colors">Tekstin kirjainten väri</param>
        public static void DrawText(string text, ref Matrix transformation, Font font, Color[] colors)
        {
            Vector textSize = (Vector)font.XnaFont.MeasureString(text);
            Matrix m = ScreenView.ToXnaCoords(ref transformation, Game.Screen.Size, textSize);

            SpriteBatch spriteBatch = Graphics.SpriteBatch;
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.LinearClamp, currentStencilState, RasterizerState.CullCounterClockwise, null, m);
            font.XnaFont.DrawText(Graphics.FontRenderer, text, Vector2.Zero.ToSystemNumerics(), colors.ConvertAll(v => v.AsXnaColor().ToSystemDrawing()).ToArray());
            spriteBatch.End();
        }

        /// <summary>
        /// Piirtää kuvion niin, että tekstuuri täyttää sen.
        /// </summary>
        public static void DrawShape( Shape shape, ref Matrix transformation, ref Matrix textureTransformation, Image texture, Vector textureWrapSize, Color color )
        {
            BeginDrawingInsideShape( shape, ref transformation );
            DrawImage( texture, ref textureTransformation, textureWrapSize );
            EndDrawingInsideShape();
        }

        public static void DrawShape( Shape shape, ref Matrix matrix, Color color )
        {
            if ( shape is RaySegment )
            {
                DrawRaySegment( (RaySegment)shape, ref matrix, color );
            }
            else if ( shape.Cache.Triangles != null )
            {
                DrawFilledShape( shape.Cache, ref matrix, color );
            }
            else
            {
                DrawPolygon( shape.Cache.OutlineVertices, ref matrix, color );
            }
        }

        public static void DrawRaySegment( RaySegment segment, ref Matrix matrix, Color color )
        {
            var device = Game.GraphicsDevice;

            Vector endPoint = segment.Origin + segment.Direction * segment.Length;

            VertexPositionColor[] colorVertices = new VertexPositionColor[2];
            colorVertices[0].Position = new Vector3( (float)segment.Origin.X, (float)segment.Origin.Y, 0 );
            colorVertices[0].Color = color.AsXnaColor();
            colorVertices[1].Position = new Vector3( (float)endPoint.X, (float)endPoint.Y, 0 );
            colorVertices[1].Color = color.AsXnaColor();

            BasicEffect effect = Graphics.BasicColorEffect;
            effect.World = matrix;
            Graphics.SetSamplerState();
            foreach ( EffectPass pass in effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawUserPrimitives<VertexPositionColor>( PrimitiveType.LineStrip, colorVertices, 0, 1 );
            }
            Graphics.ResetSamplerState();
        }

        /// <summary>
        /// Piirtää suorakulmion.
        /// </summary>
        internal static void DrawRectangle( ref Matrix matrix, Color color )
        {
            Vector[] vertices = new Vector[]
            {
                new Vector(-0.5, 0.5),
                new Vector(-0.5, -0.5),
                new Vector(0.5, -0.5),
                new Vector(0.5, 0.5),
            };
            Renderer.DrawPolygon( vertices, ref matrix, color );
        }

        internal static void DrawFilledShape( ShapeCache cache, ref Matrix matrix, Color color )
        {
            DrawFilledShape( cache, ref matrix, color, BlendState.NonPremultiplied );
        }

        internal static void DrawFilledShape( ShapeCache cache, ref Matrix matrix, Color color, BlendState blendState )
        {
            var device = Game.GraphicsDevice;

            VertexPositionColor[] vertices = new VertexPositionColor[cache.Vertices.Length];
            for ( int i = 0; i < vertices.Length; i++ )
            {
                Vector v = cache.Vertices[i];
                vertices[i] = new VertexPositionColor( new XnaV3( (float)v.X, (float)v.Y, 0 ), color.AsXnaColor() );
            }

            Int16[] indices = new Int16[cache.Triangles.Length * 3];
            for ( int i = 0; i < cache.Triangles.Length; i++ )
            {
                indices[3 * i] = cache.Triangles[i].i1;
                indices[3 * i + 1] = cache.Triangles[i].i2;
                indices[3 * i + 2] = cache.Triangles[i].i3;
            }

            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.BlendState = blendState;
#if WINDOWS_PHONE
            // The WP7 emulator interprets cullmodes incorectly sometimes.
            device.RasterizerState = RasterizerState.CullNone;
#endif

            Effect effect = Graphics.GetColorEffect( ref matrix, LightingEnabled );
            Graphics.SetSamplerState();
            foreach ( EffectPass pass in effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList,
                    vertices, 0, vertices.Length,
                    indices, 0, indices.Length / 3
                    );
            }
            Graphics.ResetSamplerState();
        }

        public static void DrawPolygon( Vector[] vertices, ref Matrix matrix, Color color )
        {
            if ( vertices.Length < 3 )
                throw new ArgumentException( "Polygon must have at least three vertices" );

            var device = Game.GraphicsDevice;

            VertexPositionColor[] colorVertices = new VertexPositionColor[vertices.Length];
            for ( int i = 0; i < colorVertices.Length; i++ )
            {
                Vector p = vertices[i];
                colorVertices[i] = new VertexPositionColor(
                    new XnaV3( (float)p.X, (float)p.Y, 0 ),
                    color.AsXnaColor()
                    );
            }

            int n = colorVertices.Length;
            Int16[] indices = new Int16[2 * n];
            for ( int i = 0; i < ( n - 1 ); i++ )
            {
                indices[2 * i] = (Int16)i;
                indices[2 * i + 1] = (Int16)( i + 1 );
            }
            indices[2 * ( n - 1 )] = (Int16)( n - 1 );
            indices[2 * ( n - 1 ) + 1] = (Int16)0;

            Effect effect = Graphics.GetColorEffect( ref matrix, LightingEnabled );
            Graphics.SetSamplerState();
            foreach ( EffectPass pass in effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.LineStrip,
                    colorVertices, 0, colorVertices.Length,
                    indices, 0, indices.Length - 1
                    );

            }
            Graphics.ResetSamplerState();
        }


        internal static void DrawVertices( Vector[] vertices, Matrix matrix, Color color )
        {
            VertexPositionColor[] pointVertices = new VertexPositionColor[vertices.Length];
            for ( int i = 0; i < pointVertices.Length; i++ )
            {
                Vector p = vertices[i];
                pointVertices[i] = new VertexPositionColor(
                    new XnaV3( (float)p.X, (float)p.Y, 0 ),
                    Color.Red.AsXnaColor()
                    );
            }

            var device = Game.GraphicsDevice;
            //device.RenderState.PointSize = 2;

            BasicEffect effect = Graphics.BasicColorEffect;
            effect.World = matrix;
            Graphics.SetSamplerState();
            foreach ( var pass in effect.CurrentTechnique.Passes )
            {
                pass.Apply();
                device.DrawUserPrimitives<VertexPositionColor>(
                    PrimitiveType.LineList,
                    pointVertices, 0, pointVertices.Length
                    );
            }
            Graphics.ResetSamplerState();
        }
    }
}

