using System;
using System.Diagnostics;
using System.Collections.Generic;
using Jypeli.Rendering;

using Matrix = System.Numerics.Matrix4x4;
using System.Reflection.Metadata;

#if !DISABLE_EFFECTS
using Jypeli.Effects;
#endif

namespace Jypeli
{
    internal class TextureCoordinates
    {
        public Vector TopLeft;
        public Vector TopRight;
        public Vector BottomLeft;
        public Vector BottomRight;
    }

    /// <summary>
    /// DUMMY
    /// </summary>
    internal class Effect
    {
        public Effect()
        {

        }
    }

    /// <summary>
    /// Draws images efficiently. // TODO: Kaikkea muuta kuin tehokas.
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

        static readonly uint VerticesPerTexture = (uint)Vertices.Length;

        Matrix matrix;
        Image texture;
        Effect effect;
        VertexPositionColorTexture[] vertexBuffer;
        int BufferSize;
        uint iTexture = 0;
        bool beginHasBeenCalled = false;

        public bool LightingEnabled = true;


        public ImageBatch()
        {
        }

        public void Initialize()
        {
            this.BufferSize = Game.GraphicsDevice.BufferSize/6;
            vertexBuffer = new VertexPositionColorTexture[BufferSize * VerticesPerTexture];
        }

        public void Begin()
        {
            Matrix mat = Matrix.Identity;
            Begin(ref mat, null);
        }

        public void Begin(ref Matrix matrix, Image texture)
        {
            Debug.Assert(!beginHasBeenCalled);
            beginHasBeenCalled = true;

            this.matrix = matrix;
            this.texture = texture;
            iTexture = 0;
        }

        public void End()
        {
            Debug.Assert(beginHasBeenCalled);

            Flush();

            beginHasBeenCalled = false;
        }

        private void Flush()
        {
            if (iTexture > 0)
            {
                if (texture.handle == 0)
                {
                    // Viedään tekstuuri näytönohjaimelle kun sitä ensimmäisen kerran käytetään.
                    Game.GraphicsDevice.LoadImage(texture);
                }
                else if (texture.dirty)
                {
                    // Jos kuvan data on muuttunut, pitää se viedä uudestaan.
                    Game.GraphicsDevice.UpdateTextureData(texture);
                    texture.dirty = false;
                }

                Game.GraphicsDevice.World = matrix;
                Game.GraphicsDevice.BindTexture(texture.handle);

                Game.GraphicsDevice.DrawPrimitives(
                    PrimitiveType.OpenGlTriangles,
                    vertexBuffer, iTexture * 6);

                Graphics.ResetSamplerState();
            }

            iTexture = 0;
        }

        public void Draw(TextureCoordinates c, Vector position, Vector size, float angle)
        {
            Debug.Assert(beginHasBeenCalled);

            if (iTexture >= BufferSize)
                Flush();

            Matrix matrix =
                Matrix.CreateScale((float)size.X, (float)size.Y, 1f)
                * Matrix.CreateRotationZ(angle)
                * Matrix.CreateTranslation((float)position.X, (float)position.Y, 0);

            Vector3[] transformedPoints = new Vector3[VerticesPerTexture];
            Vector3.Transform(Vertices, ref matrix, transformedPoints);

            uint startIndex = (iTexture * VerticesPerTexture);

            for (int i = 0; i < VerticesPerTexture; i++)
            {
                uint bi = (uint)((iTexture * VerticesPerTexture) + i);
                vertexBuffer[bi].Position = transformedPoints[i];
            }

            var color = System.Drawing.Color.FromArgb(255, 255, 255, 255);

            // Triangle 1
            vertexBuffer[startIndex + 0].TexCoordsX = (float)c.TopLeft.X;
            vertexBuffer[startIndex + 0].TexCoordsY = (float)c.TopLeft.Y;
            vertexBuffer[startIndex + 0].SetColor(color);
            vertexBuffer[startIndex + 1].TexCoordsX = (float)c.BottomLeft.X;
            vertexBuffer[startIndex + 1].TexCoordsY = (float)c.BottomLeft.Y;
            vertexBuffer[startIndex + 1].SetColor(color);
            vertexBuffer[startIndex + 2].TexCoordsX = (float)c.TopRight.X;
            vertexBuffer[startIndex + 2].TexCoordsY = (float)c.TopRight.Y;
            vertexBuffer[startIndex + 2].SetColor(color);

            // Triangle 2
            vertexBuffer[startIndex + 3].TexCoordsX = (float)c.BottomLeft.X;
            vertexBuffer[startIndex + 3].TexCoordsY = (float)c.BottomLeft.Y;
            vertexBuffer[startIndex + 3].SetColor(color);
            vertexBuffer[startIndex + 4].TexCoordsX = (float)c.BottomRight.X;
            vertexBuffer[startIndex + 4].TexCoordsY = (float)c.BottomRight.Y;
            vertexBuffer[startIndex + 4].SetColor(color);
            vertexBuffer[startIndex + 5].TexCoordsX = (float)c.TopRight.X;
            vertexBuffer[startIndex + 5].TexCoordsY = (float)c.TopRight.Y;
            vertexBuffer[startIndex + 5].SetColor(color);

            iTexture++;
        }

        public void Draw(Image img, System.Numerics.Vector2 position, System.Drawing.Rectangle? sourceRectangle, System.Drawing.Color color, System.Numerics.Vector2 scale, float angle, System.Numerics.Vector2 origin, float depth)
        {
            Debug.Assert(beginHasBeenCalled);

            if (iTexture >= BufferSize)
                Flush();

            texture = img;
            float iw = img.Width;
            float ih = img.Height;

            System.Drawing.Rectangle rect = sourceRectangle.Value;

            Matrix matrix =
                Matrix.CreateScale(scale.X * rect.Width, scale.Y * rect.Height, 1f)
                * Matrix.CreateRotationZ(angle)
                * Matrix.CreateTranslation(position.X + rect.Width / 2 - origin.X, position.Y + origin.Y / 2, 0);

            Vector3[] transformedPoints = new Vector3[VerticesPerTexture];
            Vector3.Transform(Vertices, ref matrix, transformedPoints);

            uint startIndex = (iTexture * VerticesPerTexture);

            for (int i = 0; i < VerticesPerTexture; i++)
            {
                uint bi = (uint)((iTexture * VerticesPerTexture) + i);
                vertexBuffer[bi].Position = transformedPoints[i];
            }

            // Triangle 1
            vertexBuffer[startIndex + 0].TexCoordsX = rect.X / iw;
            vertexBuffer[startIndex + 0].TexCoordsY = rect.Y / ih;
            vertexBuffer[startIndex + 0].SetColor(color);
            vertexBuffer[startIndex + 1].TexCoordsX = rect.X / iw;
            vertexBuffer[startIndex + 1].TexCoordsY = (rect.Y + rect.Height) / ih;
            vertexBuffer[startIndex + 1].SetColor(color);
            vertexBuffer[startIndex + 2].TexCoordsX = (rect.X + rect.Width) / iw;
            vertexBuffer[startIndex + 2].TexCoordsY = rect.Y / ih;
            vertexBuffer[startIndex + 2].SetColor(color);

            // Triangle 2
            vertexBuffer[startIndex + 3].TexCoordsX = rect.X / iw;
            vertexBuffer[startIndex + 3].TexCoordsY = (rect.Y + rect.Height) / ih;
            vertexBuffer[startIndex + 3].SetColor(color);
            vertexBuffer[startIndex + 4].TexCoordsX = (rect.X + rect.Width) / iw;
            vertexBuffer[startIndex + 4].TexCoordsY = (rect.Y + rect.Height) / ih;
            vertexBuffer[startIndex + 4].SetColor(color);
            vertexBuffer[startIndex + 5].TexCoordsX = (rect.X + rect.Width) / iw;
            vertexBuffer[startIndex + 5].TexCoordsY = rect.Y / ih;
            vertexBuffer[startIndex + 5].SetColor(color);

            iTexture++;
        }
    }
}
