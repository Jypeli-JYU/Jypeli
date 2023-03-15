using System.Diagnostics;
using Matrix = System.Numerics.Matrix4x4;
using Vector3 = System.Numerics.Vector3;
using FSVertexPositionColorTexture = FontStashSharp.Interfaces.VertexPositionColorTexture;

namespace Jypeli.Rendering
{
    internal class TextBatch
    {
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
        IShader shader;
        IShader customShader;
        VertexPositionColorTexture[] vertexBuffer;
        uint[] indexData;
        int BufferSize;
        uint iTexture = 0;
        bool beginHasBeenCalled = false;

        public bool LightingEnabled = true;


        public TextBatch()
        {
        }

        public void Initialize()
        {
            this.BufferSize = Game.GraphicsDevice.BufferSize / 6;
            vertexBuffer = new VertexPositionColorTexture[BufferSize * VerticesPerTexture];
            indexData = GenerateIndexArray();
            shader = Graphics.BasicTextureShader;
        }

        public void Begin()
        {
            Matrix mat = Matrix.Identity;
            Begin(ref mat, null);
        }

        public void Begin(ref Matrix matrix)
        {
            Begin(ref matrix, null);
        }

        public void Begin(ref Matrix matrix, Image texture, IShader shader = null)
        {
            Debug.Assert(!beginHasBeenCalled);
            beginHasBeenCalled = true;

            this.matrix = matrix;
            this.texture = texture;
            iTexture = 0;
            customShader = shader;
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
                if (texture.dirty)
                {
                    // Jos kuvan data on muuttunut, pitää se viedä uudestaan.
                    Game.GraphicsDevice.UpdateTextureData(texture);
                    texture.dirty = false;
                }

                if (customShader is null)
                {
                    shader.Use();
                    shader.SetUniform("world", matrix * Graphics.ViewProjectionMatrix);
                }
                else
                {
                    customShader.Use();
                    customShader.SetUniform("world", matrix * Graphics.ViewProjectionMatrix);
                }

                Game.GraphicsDevice.BindTexture(texture);

                Game.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.OpenGlTriangles,
                    vertexBuffer, (uint)(iTexture * 6 / 4), indexData);
            }

            iTexture = 0;
        }

        internal unsafe void Add(Image texture, FSVertexPositionColorTexture topLeft, FSVertexPositionColorTexture topRight, FSVertexPositionColorTexture bottomLeft, FSVertexPositionColorTexture bottomRight)
        {
            if (iTexture >= BufferSize || this.texture != texture)
                Flush();

            vertexBuffer[iTexture++] = new VertexPositionColorTexture() { Position = topLeft.Position, Color = topLeft.Color.ToVector4(), TexCoords = topLeft.TextureCoordinate};
            vertexBuffer[iTexture++] = new VertexPositionColorTexture() { Position = topRight.Position, Color = topRight.Color.ToVector4(), TexCoords = topRight.TextureCoordinate };
            vertexBuffer[iTexture++] = new VertexPositionColorTexture() { Position = bottomLeft.Position, Color = bottomLeft.Color.ToVector4(), TexCoords = bottomLeft.TextureCoordinate };
            vertexBuffer[iTexture++] = new VertexPositionColorTexture() { Position = bottomRight.Position, Color = bottomRight.Color.ToVector4(), TexCoords = bottomRight.TextureCoordinate };

            this.texture = texture;
        }

        // https://github.com/FontStashSharp/FontStashSharp/blob/main/samples/FontStashSharp.Samples.Silk.NET/Platform/Renderer.cs
        private uint[] GenerateIndexArray()
        {
            uint[] result = new uint[vertexBuffer.Length/ 4 * 6];
            for (int i = 0, j = 0; i < vertexBuffer.Length / 4 * 6; i += 6, j += 4)
            {
                result[i] = (uint)j;
                result[i + 1] = (uint)(j + 1);
                result[i + 2] = (uint)(j + 2);
                result[i + 3] = (uint)(j + 3);
                result[i + 4] = (uint)(j + 2);
                result[i + 5] = (uint)(j + 1);
            }
            return result;
        }
    }
}
