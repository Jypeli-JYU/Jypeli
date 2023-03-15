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

using System;
using Jypeli.Rendering;
using Matrix = System.Numerics.Matrix4x4;
using Vector3 = System.Numerics.Vector3;

namespace Jypeli
{
    /// <summary>
    /// Contains graphics resources.
    /// </summary>
    internal static class Graphics
    {
        public static IShader BasicTextureShader;
        public static IShader BasicColorShader;
        public static IShader SimpleFloodLightShader;
        public static IShader ParticleShader;
        public static IShader LightPassTextureShader;

        public static FontRenderer FontRenderer;

        // Having global batch objects saves memory.
        // The same batch object must not be used from more
        // than one place at a time, though.
        internal static ImageBatch ImageBatch = new ImageBatch();
        internal static TextBatch TextBatch = new TextBatch();
        internal static ShapeBatch ShapeBatch = new ShapeBatch();
        internal static LineBatch LineBatch = new LineBatch();
        internal static CustomBatcher CustomBatch = new CustomBatcher();

        public static Canvas Canvas = new Canvas();

        internal static readonly TextureCoordinates DefaultTextureCoords = new TextureCoordinates()
        {
            TopLeft = new Vector(0.0, 0.0),
            TopRight = new Vector(1.0, 0.0),
            BottomLeft = new Vector(0.0, 1.0),
            BottomRight = new Vector(1.0, 1.0),
        };

        internal static VertexPositionColorTexture[] TextureVertices = new VertexPositionColorTexture[]
        {
                new VertexPositionColorTexture(new Vector3(-1f, 1f, 0), Color.White, new Vector(0f, 1f)),
                new VertexPositionColorTexture(new Vector3(-1f, -1f, 0), Color.White, new Vector(0f, 0f)),
                new VertexPositionColorTexture(new Vector3(1f, -1f, 0), Color.White, new Vector(1f, 0f)),

                new VertexPositionColorTexture(new Vector3(-1f, 1f, 0), Color.White, new Vector(0f, 1f)),
                new VertexPositionColorTexture(new Vector3(1f, -1f, 0), Color.White, new Vector(1f, 0f)),
                new VertexPositionColorTexture(new Vector3(1f, 1f, 0), Color.White, new Vector(1f, 1f))
        };

        /// <summary>
        /// Transformaatiomatriisi kameran suuntaa varten
        /// </summary>
        public static Matrix ViewMatrix { get; internal set; }

        /// <summary>
        /// Transformaatiomatriisi paikkakoordinaattien muuttamiseksi ruutukoordinaatteihin
        /// </summary>
        public static Matrix ProjectionMatrix { get; internal set; }

        /// <summary>
        /// Yhdistetty transformaatio
        /// </summary>
        public static Matrix ViewProjectionMatrix { get; internal set; }

        public static void Initialize()
        {
            BasicTextureShader = Game.GraphicsDevice.CreateShaderFromInternal("DefaultVertexShader.glsl", "DefaultTextureShader.glsl");
            BasicColorShader = Game.GraphicsDevice.CreateShaderFromInternal("DefaultVertexShader.glsl", "DefaultColorShader.glsl");
            SimpleFloodLightShader = Game.GraphicsDevice.CreateShaderFromInternal("SimpleFloodLightVertex.glsl", "SimpleFloodLightFragment.glsl");
            ParticleShader = Game.GraphicsDevice.CreateShaderFromInternal("ParticleVertexShader.glsl", "DefaultTextureShader.glsl");
            LightPassTextureShader = Game.GraphicsDevice.CreateShaderFromInternal("DefaultVertexShader.glsl", "DefaultTextureShaderLightPass.glsl");

            ImageBatch.Initialize();
            ShapeBatch.Initialize();
            LineBatch.Initialize();
            TextBatch.Initialize();
            FontRenderer = new FontRenderer();
        }

        public static void ResetScreenSize()
        {
            ViewMatrix = Matrix.CreateLookAt(
                new Vector3(0.0f, 0.0f, 1.0f),
                Vector3.Zero,
                Vector3.UnitY
                );
            ProjectionMatrix = Matrix.CreateOrthographic(
                (float)Game.Screen.ViewportWidth,
                (float)Game.Screen.ViewportHeight,
                1.0f, 2.0f
                );

            ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;

            //BasicTextureEffect.View = ViewMatrix;
            //BasicTextureEffect.Projection = ProjectionMatrix;
        }
    }
}
