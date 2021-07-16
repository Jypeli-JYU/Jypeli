using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;

namespace Jypeli.Rendering.OpenGl
{
    public unsafe class RenderTarget
    {
        private GL gl;

        private uint bufferHandle;
        private uint textureHandle;

        public double Width { get; set; }
        public double Height { get; set; }

        public RenderTarget(int width, int height)
        {
            gl = GraphicsDevice.Gl;
            bufferHandle = gl.GenRenderbuffer();
            gl.BindRenderbuffer(GLEnum.Framebuffer, bufferHandle);

            uint handle;
            gl.GenTextures(1, &handle);
            textureHandle = handle;

            Bind();

            gl.TexImage2D(GLEnum.Texture, 0, (int)GLEnum.Rgb, (uint)width, (uint)height, 0, GLEnum.Rgb, GLEnum.UnsignedByte, 0);

            gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
            gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);

            gl.FramebufferTexture(GLEnum.Framebuffer, GLEnum.ColorAttachment0, textureHandle, 0);
            gl.DrawBuffers(1, GLEnum.ColorAttachment0);

            gl.GetError();
        }

        public void Bind()
        {
            gl.BindTexture(GLEnum.Texture2D, textureHandle);
        }
    }
}
