﻿using System;
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

        private Shader shader;

        /// <inheritdoc/>
        public Matrix4x4 World { get; set; }
        /// <inheritdoc/>
        public Matrix4x4 View { get; set; }
        /// <inheritdoc/>
        public Matrix4x4 Projection { get; set; }
        /// <inheritdoc/>
        public string Name { get => throw new NotImplementedException(); }
        /// <inheritdoc/>
        public string Version { get => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public GraphicsDevice(IWindow window)
        {
            Indices = new uint[BufferSize * 2];
            Vertices = new VertexPositionColorTexture[BufferSize];

            Create(window);
        }

        /// <summary>
        /// Alustaa näyttökortin käyttöön
        /// </summary>
        /// <param name="window">Pelin ikkuna</param>
        public void Create(IWindow window)
        {
            Gl = GL.GetApi(window);

            Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
            Vbo = new BufferObject<VertexPositionColorTexture>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<VertexPositionColorTexture, uint>(Gl, Vbo, Ebo);

            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 1, 0);
            Vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 1, 12);
            Vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 1, 28);

            shader = new Shader(Gl, Game.ResourceContent.LoadInternalText("Shaders.OpenGl.DefaultVertexShader.glsl"), Game.ResourceContent.LoadInternalText("Shaders.OpenGl.DefaultFragmentShader.glsl"));
        }

        /// <inheritdoc/>
        public void DrawUserIndexedPrimitives(PrimitiveType primitivetype, VertexPositionColorTexture[] vertexBuffer, uint numIndices, uint[] indexBuffer)
        {
            Gl.Enable(GLEnum.Blend);
            Gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

            Ebo.UpdateBuffer(0, indexBuffer);
            Vbo.UpdateBuffer(0, vertexBuffer);

            Vao.Bind();
            shader.Use();

            shader.SetUniform("world", World * View * Projection);
            shader.SetUniform("type", 0);

            Gl.DrawElements((GLEnum)primitivetype, numIndices, DrawElementsType.UnsignedInt, null);
        }

        /// <inheritdoc/>
        public void DrawUserPrimitives(PrimitiveType primitivetype, VertexPositionColorTexture[] vertexBuffer, uint numIndices, bool normalized = false)
        {
            Gl.Enable(GLEnum.Blend);
            Gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

            Vbo.UpdateBuffer(0, vertexBuffer);

            Vao.Bind();
            shader.Use();

            shader.SetUniform("world", normalized ? Matrix4x4.Identity : World * View * Projection);
            shader.SetUniform("type", normalized ? 1 : 2); // TODO: Pitäisikö tehdä omat shaderit tekstiä tms. varten, eikä yhtä ja samaa käyttää kaikkialla?

            Gl.DrawArrays((GLEnum)primitivetype, 0, numIndices);
        }

        /// <inheritdoc/>
        public void Clear(Color bgColor)
        {
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Gl.ClearColor(bgColor.RedComponent/255f, bgColor.GreenComponent / 255f, bgColor.BlueComponent / 255f, bgColor.AlphaComponent / 255f);
        }

        /// <inheritdoc/>
        public void SetRenderTarget(IRenderTarget renderTarget)
        {
            if(renderTarget is null)
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
            shader.Dispose();
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
                BindTexture(image.handle);

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
                BindTexture(image.handle);

                Gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)image.Width, (uint)image.Height, PixelFormat.Rgba, PixelType.UnsignedByte, data);

                GLEnum scaling = image.Scaling == ImageScaling.Linear ? GLEnum.Linear : GLEnum.Nearest;

                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)scaling);
                Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)scaling);
            }
        }

        /// <inheritdoc/>
        public void UpdateTextureScaling(Image image)
        {
            GLEnum scaling = image.Scaling == ImageScaling.Linear ? GLEnum.Linear : GLEnum.Nearest;
            BindTexture(image.handle);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)scaling); // TODO: Entä jos halutaan vain muuttaa skaalausta, ilman kuvan datan muuttamista?
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)scaling);
        }

        /// <inheritdoc/>
        public void BindTexture(uint handle)
        {
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2D, handle);
        }

        /// <inheritdoc/>
        public void ResizeWindow(Vector newSize)
        {
            Gl.Viewport(new System.Drawing.Size((int)newSize.X, (int)newSize.Y));
        }
    }
}
