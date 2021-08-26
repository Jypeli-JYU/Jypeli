using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using Silk.NET.SDL;

namespace Jypeli.Rendering.OpenGl
{
    /// <summary>
    /// Tekstuuri johon piirretään kuva ennen sen piirtämistä ruudulle
    /// </summary>
    public unsafe class RenderTarget : IRenderTarget
    {
        private GL gl;

        private uint framebufferHandle;
        private uint texturebufferHandle;
        private uint rbo;

        /// <inheritdoc/>
        public double Width { get; set; }
        /// <inheritdoc/>
        public double Height { get; set; }

        /// <summary>
        /// Tekstuuri johon voidaan piirtää kuva.
        /// </summary>
        /// <param name="width">Leveys</param>
        /// <param name="height">Korkeus</param>
        public RenderTarget(uint width, uint height)
        {
            Width = width;
            Height = height;
            gl = ((GraphicsDevice)Game.GraphicsDevice).Gl;
            framebufferHandle = gl.GenFramebuffer();
            gl.BindFramebuffer(GLEnum.Framebuffer, framebufferHandle);

            texturebufferHandle = gl.GenTexture();

            gl.BindTexture(GLEnum.Texture2D, texturebufferHandle);

            // Tässä pitää jostain syystä käyttää InternalFormat-enumia, mutta muualla voi käyttää GLEnumia...
            gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgb, width, height, 0, GLEnum.Rgb, GLEnum.UnsignedByte, null);

            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);

            gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, texturebufferHandle, 0);

            rbo = gl.GenRenderbuffer();
            gl.BindRenderbuffer(GLEnum.Renderbuffer, rbo);
            gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.Depth24Stencil8, width, height);
            gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthStencilAttachment, GLEnum.Renderbuffer, rbo);

            gl.BindRenderbuffer(GLEnum.Renderbuffer, 0);

            gl.GetError();

            if(gl.CheckFramebufferStatus(GLEnum.Framebuffer) != GLEnum.FramebufferComplete)
            {
                Debug.WriteLine("Framebuffer is not complete");
            }

            gl.BindFramebuffer(GLEnum.Framebuffer, 0);
        }

        /// <inheritdoc/>
        public void Bind()
        {
            gl.BindFramebuffer(GLEnum.Framebuffer, framebufferHandle);
        }

        /// <inheritdoc/>
        public void UnBind()
        {
            gl.BindFramebuffer(GLEnum.Framebuffer, 0);
        }

        /// <inheritdoc/>>
        public void TextureSlot(int slot)
        {
            gl.ActiveTexture(GLEnum.Texture0 + slot);
        }

        /// <inheritdoc/>
        public void BindTexture()
        {
            gl.BindTexture(GLEnum.Texture2D, texturebufferHandle);
        }

        /// <inheritdoc/>
        public void UnBindTexture()
        {
            gl.BindTexture(GLEnum.Texture2D, 0);
        }

        /// <inheritdoc/>
        public void SetData(byte[] data, int startX, int startY, uint width, uint height)
        {
            if (data.Length < width * height)
                throw new ArgumentException("Not enough pixel data", nameof(data));

            BindTexture();
            fixed (void* ptr = data)
                gl.TexSubImage2D(GLEnum.Texture2D, 0, startX, startY, width, height, GLEnum.Rgb, GLEnum.UnsignedByte, ptr);
            UnBindTexture();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            gl.DeleteTexture(texturebufferHandle);
            gl.DeleteFramebuffer(framebufferHandle);
            gl.DeleteRenderbuffer(rbo);

            GC.SuppressFinalize(this);
        }
    }
}
