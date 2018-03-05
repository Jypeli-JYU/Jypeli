﻿#region MIT License
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
using Jypeli.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if !DISABLE_EFFECTS
using Jypeli.Effects;
#endif

namespace Jypeli
{
    /// <summary>
    /// Contains graphics resources.
    /// </summary>
    internal static class Graphics
    {
        public static BasicEffect BasicTextureEffect;
        public static BasicEffect BasicColorEffect;

        public static SpriteBatch SpriteBatch;

        // Having global batch objects saves memory.
        // The same batch object must not be used from more
        // than one place at a time, though.
        internal static ImageBatch ImageBatch = new ImageBatch();
        internal static ShapeBatch ShapeBatch = new ShapeBatch();
        internal static LineBatch LineBatch = new LineBatch();

        public static Canvas Canvas = new Canvas();

        internal static readonly TextureCoordinates DefaultTextureCoords = new TextureCoordinates()
        {
            TopLeft = new Vector2( 0.0f, 0.0f ),
            TopRight = new Vector2( 1.0f, 0.0f ),
            BottomLeft = new Vector2( 0.0f, 1.0f ),
            BottomRight = new Vector2( 1.0f, 1.0f ),
        };

#if !WINDOWS_PHONE && !DISABLE_LIGHTING_EFFECT
        static Effect LightingEffect;
#endif

        private static Matrix ViewMatrix;
        private static Matrix ProjectionMatrix;
        private static Matrix viewProjectionMatrix;

#if !WINDOWS_PHONE
        // XNA 4.0 requires PS 2.0
        private static bool is_PS_2_0_supported = true;
#else
        // ...except on windows phone.
        private static bool is_PS_2_0_supported = false;
#endif

        public static SamplerState GetDefaultSamplerState()
        {
            return Game.SmoothTextures ? SamplerState.LinearClamp : SamplerState.PointClamp;
        }

        public static void Initialize()
        {
            GraphicsDevice device = Game.GraphicsDevice;

            BasicTextureEffect = new BasicEffect( device );
            // This must be set to false for textures to work with BasicEffect.
            BasicTextureEffect.VertexColorEnabled = false;
            BasicTextureEffect.TextureEnabled = true;

            BasicColorEffect = new BasicEffect( device );
            BasicColorEffect.VertexColorEnabled = true;
            BasicColorEffect.TextureEnabled = false;

#if !WINDOWS_PHONE && !DISABLE_LIGHTING_EFFECT
            // A hack until we can use Game.ResourceContent.Load<Effect>( "Lighting" )
            LightingEffect = new Effect(Game.GraphicsDevice, Jypeli.Content.Lighting.rawData);
#endif

            SpriteBatch = new SpriteBatch( device );
            ImageBatch.Initialize();
            ShapeBatch.Initialize();

            ResetScreenSize();
#if !LINUX
            Game.GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;
#endif
        }

        public static DepthFormat SelectStencilMode()
        {
            GraphicsAdapter adapter = GraphicsAdapter.DefaultAdapter;
            SurfaceFormat format = adapter.CurrentDisplayMode.Format;
            return DepthFormat.Depth24Stencil8;
            /*if ( adapter.CheckDepthStencilMatch( DeviceType.Hardware, format, format, DepthFormat.Depth24Stencil8 ) )
                return DepthFormat.Depth24Stencil8;
            else if ( adapter.CheckDepthStencilMatch( DeviceType.Hardware, format, format, DepthFormat.Depth24Stencil8Single ) )
                return DepthFormat.Depth24Stencil8Single;
            else if ( adapter.CheckDepthStencilMatch( DeviceType.Hardware, format, format, DepthFormat.Depth24Stencil4 ) )
                return DepthFormat.Depth24Stencil4;
            else if ( adapter.CheckDepthStencilMatch( DeviceType.Hardware, format, format, DepthFormat.Depth15Stencil1 ) )
                return DepthFormat.Depth15Stencil1;
            else
                throw new ApplicationException( "Could Not Find Stencil Buffer for Default Adapter" );*/
        }

        private static SamplerState storedSamplerState;

        public static void SetSamplerState()
        {
            storedSamplerState = Game.GraphicsDevice.SamplerStates[0];
            Game.GraphicsDevice.SamplerStates[0] = GetDefaultSamplerState();
        }

        public static void ResetSamplerState()
        {
            Game.GraphicsDevice.SamplerStates[0] = storedSamplerState;
        }

        private static void GraphicsDevice_DeviceReset( object sender, EventArgs e )
        {
            ResetScreenSize();
        }

        public static Effect GetTextureEffect( ref Matrix worldMatrix, Texture2D texture, bool lightingEnabled )
        {
#if !WINDOWS_PHONE && !DISABLE_LIGHTING_EFFECT
            if ( lightingEnabled && is_PS_2_0_supported )
            {
                Effect effect = GetLightingEffect( ref worldMatrix );
                effect.CurrentTechnique = effect.Techniques["TextureLighting"];
                effect.Parameters["xTexture"].SetValue( texture );
                return effect;
            }
            else
#endif
            {
                BasicEffect effect = BasicTextureEffect;
                effect.Alpha = 1.0f;
                effect.World = worldMatrix;
                effect.Texture = texture;
                return effect;
            }
        }

        public static Effect GetColorEffect( ref Matrix worldMatrix, bool lightingEnabled )
        {
#if !WINDOWS_PHONE && !DISABLE_LIGHTING_EFFECT
            if ( lightingEnabled && is_PS_2_0_supported )
            {
                Effect effect = GetLightingEffect( ref worldMatrix );
                effect.CurrentTechnique = effect.Techniques["ColorLighting"];
                return effect;
            }
            else
#endif
            {
                BasicEffect effect = BasicColorEffect;
                effect.World = worldMatrix;
                return effect;
            }
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
            GraphicsDevice device = Game.GraphicsDevice;

            ViewMatrix = Matrix.CreateLookAt(
                new Vector3( 0.0f, 0.0f, 1.0f ),
                Vector3.Zero,
                Vector3.Up
                );
            ProjectionMatrix = Matrix.CreateOrthographic(
                (float)Game.Screen.Width,
                (float)Game.Screen.Height,
                1.0f, 2.0f
                );

            viewProjectionMatrix = ViewMatrix * ProjectionMatrix;

            BasicColorEffect.View = ViewMatrix;
            BasicColorEffect.Projection = ProjectionMatrix;
            BasicTextureEffect.View = ViewMatrix;
            BasicTextureEffect.Projection = ProjectionMatrix;
        }
    }
}
