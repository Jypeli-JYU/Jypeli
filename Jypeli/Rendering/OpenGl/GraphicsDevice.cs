using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Jypeli.Rendering.OpenGl
{

    public unsafe class GraphicsDevice : IGraphicsDevice
    {
        public GL Gl;

        private BufferObject<VertexPositionColorTexture> Vbo;
        private BufferObject<uint> Ebo;
        private VertexArrayObject<VertexPositionColorTexture, uint> Vao;

        public int BufferSize { get; } = 16384;
        private uint[] Indices;
        private VertexPositionColorTexture[] Vertices;

        private Shader shader;

        public Matrix4x4 World { get; set; }
        public Matrix4x4 View { get; set; }
        public Matrix4x4 Projection { get; set; }
        public string Name { get => throw new NotImplementedException(); }
        public string Version { get => throw new NotImplementedException(); }

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

        public void DrawUserPrimitives(PrimitiveType primitivetype, VertexPositionColorTexture[] vertexBuffer, uint numIndices)
        {
            Gl.Disable(GLEnum.DepthTest);

            Vbo.UpdateBuffer(0, vertexBuffer);

            Vao.Bind();
            shader.Use();

            shader.SetUniform("world", Matrix4x4.Identity);
            shader.SetUniform("type", 1);

            Gl.DrawArrays((GLEnum)primitivetype, 0, numIndices);
        }

        public void Clear(Color bgColor)
        {
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Gl.ClearColor(bgColor.RedComponent/255f, bgColor.GreenComponent / 255f, bgColor.BlueComponent / 255f, bgColor.AlphaComponent / 255f);
        }

        public void SetRenderTarget(IRenderTarget renderTarget)
        {
            if(renderTarget is null)
                Gl.BindFramebuffer(GLEnum.Framebuffer, 0);
            else
                renderTarget.Bind();
        }

        public void Dispose()
        {
            Vbo.Dispose();
            Ebo.Dispose();
            Vao.Dispose();
            shader.Dispose();
        }

        public IRenderTarget CreateRenderTarget(uint width, uint height)
        {
            return new RenderTarget(width, height);
        }
    }
}
