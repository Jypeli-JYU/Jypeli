using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Jypeli.Rendering.OpenGl
{
    /// <summary>
    /// OpenGL renderöintilaite
    /// </summary>
    public unsafe class GraphicsDevice : IGraphicsDevice
    {
        /// <inheritdoc/>
        public GL Gl;

        private BufferObject<VertexPositionColorTexture> Vbo;
        private BufferObject<uint> Ebo;
        private VertexArrayObject<VertexPositionColorTexture, uint> Vao;

        /// <inheritdoc/>
        public int BufferSize { get; } = 16384;
        private uint[] Indices;
        private VertexPositionColorTexture[] Vertices;

        /// <inheritdoc/>
        public Matrix4x4 World { get; set; }
        /// <inheritdoc/>
        public Matrix4x4 View { get; set; }
        /// <inheritdoc/>
        public Matrix4x4 Projection { get; set; }
        /// <inheritdoc/>
        public string Name { get; internal set; }
        /// <inheritdoc/>
        public string Version { get => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public GraphicsDevice(IView window)
        {
            Indices = new uint[BufferSize * 2];
            Vertices = new VertexPositionColorTexture[BufferSize];

            Create(window);
        }

        /// <summary>
        /// Alustaa näyttökortin käyttöön
        /// </summary>
        /// <param name="window">Pelin ikkuna</param>
        public void Create(IView window)
        {
            Gl = GL.GetApi(window);
            Name = window.API.API.ToString();
            Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
            Vbo = new BufferObject<VertexPositionColorTexture>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<VertexPositionColorTexture, uint>(Gl, Vbo, Ebo);

            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 0);
            Vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 12);
            Vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 28);
        }

        /// <inheritdoc/>
        public IShader CreateShader(string vert, string frag)
        {
            return new Shader(Gl, vert, frag);
        }


        public IShader CreateShaderFromInternal(string vertPath, string fragPath)
        {
            return CreateShader(Game.ResourceContent.LoadInternalText($"Shaders.{Name}.{vertPath}"), Game.ResourceContent.LoadInternalText($"Shaders.{Name}.{fragPath}"));
        }

        /// <inheritdoc/>
        public void DrawIndexedPrimitives(PrimitiveType primitivetype, VertexPositionColorTexture[] vertexBuffer, uint numIndices, uint[] indexBuffer)
        {
            Gl.Enable(GLEnum.Blend);
            Gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

            Ebo.UpdateBuffer(0, indexBuffer);
            Vbo.UpdateBuffer(0, vertexBuffer);

            Vao.Bind();

            Gl.DrawElements((GLEnum)primitivetype, numIndices, DrawElementsType.UnsignedInt, null);
        }

        /// <inheritdoc/>
        public void DrawPrimitives(PrimitiveType primitivetype, VertexPositionColorTexture[] vertexBuffer, uint numIndices, bool normalized = false)
        {
            Gl.Enable(GLEnum.Blend);
            Gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

            Vbo.UpdateBuffer(0, vertexBuffer);

            Vao.Bind();

            Gl.DrawArrays((GLEnum)primitivetype, 0, numIndices);
        }

        /// <inheritdoc/>
        public void DrawPrimitivesInstanced(PrimitiveType primitivetype, VertexPositionColorTexture[] textureVertices, uint count, bool normalized = false)
        {

            Gl.DrawArraysInstanced((GLEnum)primitivetype, 0, 4, count);
        }

        /// <inheritdoc/>
        public void Clear(Color bgColor)
        {
            Gl.ClearColor(bgColor.RedComponent / 255f, bgColor.GreenComponent / 255f, bgColor.BlueComponent / 255f, bgColor.AlphaComponent / 255f);
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        }

        /// <inheritdoc/>
        public void SetRenderTarget(IRenderTarget renderTarget)
        {
            if (renderTarget is null)
                Gl.BindFramebuffer(GLEnum.Framebuffer, 0);
            else
                renderTarget.Bind();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Vbo.Dispose();
            Ebo.Dispose();
            Vao.Dispose();
        }

        /// <inheritdoc/>
        public IRenderTarget CreateRenderTarget(uint width, uint height)
        {
            return new RenderTarget(width, height);
        }

        /// <inheritdoc/>
        public void LoadImage(Image image)
        {
            fixed (void* data = &MemoryMarshal.GetReference(image.image.GetPixelRowSpan(0)))
            {
                image.handle = Gl.GenTexture();
                BindTexture(image);

                Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)image.Width, (uint)image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);

                GLEnum scaling = image.Scaling == ImageScaling.Linear ? GLEnum.Linear : GLEnum.Nearest;

                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)scaling);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)scaling);
            }
        }

        /// <inheritdoc/>
        public void UpdateTextureData(Image image)
        {
            fixed (void* data = &MemoryMarshal.GetReference(image.image.GetPixelRowSpan(0)))
            {
                BindTexture(image);

                Gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)image.Width, (uint)image.Height, PixelFormat.Rgba, PixelType.UnsignedByte, data);

                GLEnum scaling = image.Scaling == ImageScaling.Linear ? GLEnum.Linear : GLEnum.Nearest;

                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)scaling);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)scaling);
                Gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
            }
        }

        /// <inheritdoc/>
        public void UpdateTextureScaling(Image image)
        {
            GLEnum scaling = image.Scaling == ImageScaling.Linear ? GLEnum.Linear : GLEnum.Nearest;
            BindTexture(image);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)scaling); // TODO: Entä jos halutaan vain muuttaa skaalausta, ilman kuvan datan muuttamista?
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)scaling);
        }

        /// <inheritdoc/>
        public void BindTexture(Image image)
        {
            // Jos kuvaa ei ole vielä viety näytönohjaimelle.
            if (image.handle == 0)
                LoadImage(image);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2D, image.handle);
        }

        /// <inheritdoc/>
        public void ResizeWindow(Vector newSize)
        {
            Gl.Viewport(new System.Drawing.Size((int)newSize.X, (int)newSize.Y));
        }

        public void SetTextureToRepeat(Image image)
        {
            BindTexture(image);

            Gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
            Gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
        }
    }
}
