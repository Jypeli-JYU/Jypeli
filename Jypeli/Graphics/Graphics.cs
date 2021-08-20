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

        public static FontRenderer FontRenderer;

        // Having global batch objects saves memory.
        // The same batch object must not be used from more
        // than one place at a time, though.
        internal static ImageBatch ImageBatch = new ImageBatch();
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

#if !WINDOWS_PHONE && !DISABLE_LIGHTING_EFFECT
        static Effect LightingEffect;
#endif

        private static Matrix ViewMatrix;
        private static Matrix ProjectionMatrix;
        private static Matrix viewProjectionMatrix;

        public static void Initialize()
        {
#if DESKTOP
            BasicTextureShader = Game.GraphicsDevice.CreateShader(Game.ResourceContent.LoadInternalText("Shaders.OpenGL.DefaultVertexShader.glsl"), Game.ResourceContent.LoadInternalText("Shaders.OpenGL.DefaultTextureShader.glsl"));
            BasicColorShader = Game.GraphicsDevice.CreateShader(Game.ResourceContent.LoadInternalText("Shaders.OpenGL.DefaultVertexShader.glsl"), Game.ResourceContent.LoadInternalText("Shaders.OpenGL.DefaultColorShader.glsl"));
#elif ANDROID
            BasicTextureShader = Game.GraphicsDevice.CreateShader(Game.ResourceContent.LoadInternalText("Shaders.OpenGLES.DefaultVertexShader.glsl"), Game.ResourceContent.LoadInternalText("Shaders.OpenGLES.DefaultTextureShader.glsl"));
            BasicColorShader = Game.GraphicsDevice.CreateShader(Game.ResourceContent.LoadInternalText("Shaders.OpenGLES.DefaultVertexShader.glsl"), Game.ResourceContent.LoadInternalText("Shaders.OpenGLES.DefaultColorShader.glsl"));
#endif
            ImageBatch.Initialize();
            ShapeBatch.Initialize();
            LineBatch.Initialize();
            FontRenderer = new FontRenderer();
        }

#if !WINDOWS_PHONE && !DISABLE_LIGHTING_EFFECT
        private static Effect GetLightingEffect( ref Matrix worldMatrix )
        {
            Effect effect = LightingEffect;

            Vector3 lightPos = new Vector3( 0, 0, 40 );
            float lightPower = 0.0f;

            if ( Game.Lights.Count > 0 )
            {
                Light light = Game.Lights[0];
                lightPos = new Vector3( (float)light.Position.X, (float)light.Position.Y, (float)light.Distance );
                lightPower = (float)light.Intensity;
            }

            Vector3 transformedLightPos;
            Vector3.Transform( ref lightPos, ref worldMatrix, out transformedLightPos );

            effect.Parameters["xWorldViewProjection"].SetValue( worldMatrix * viewProjectionMatrix );
            effect.Parameters["xWorld"].SetValue( worldMatrix );
            effect.Parameters["xLightPos"].SetValue( transformedLightPos );
            effect.Parameters["xLightPower"].SetValue( lightPower );
            effect.Parameters["xAmbient"].SetValue( (float)Game.Instance.Level.AmbientLight );

            return effect;
        }
#endif

        public static void ResetScreenSize()
        {
            ViewMatrix = Matrix.CreateLookAt(
                new Vector3( 0.0f, 0.0f, 1.0f ),
                Vector3.Zero,
                Vector3.UnitY
                );
            ProjectionMatrix = Matrix.CreateOrthographic(
                (float)Game.Screen.ViewportWidth,
                (float)Game.Screen.ViewportHeight,
                1.0f, 2.0f
                );

            viewProjectionMatrix = ViewMatrix * ProjectionMatrix;
            
            Game.GraphicsDevice.View = ViewMatrix;
            Game.GraphicsDevice.Projection = ProjectionMatrix;
            //BasicTextureEffect.View = ViewMatrix;
            //BasicTextureEffect.Projection = ProjectionMatrix;
        }
    }
}
