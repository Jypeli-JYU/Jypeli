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

    public static unsafe class GraphicsDevice
    {
        public static GL Gl;

        private static BufferObject<VertexPositionColor> Vbo;
        private static BufferObject<uint> Ebo;
        private static VertexArrayObject<VertexPositionColor, uint> Vao;

        public static int bufferSize = 16384;
        private static uint[] Indices = new uint[bufferSize * 2];
        private static VertexPositionColor[] Vertices = new VertexPositionColor[bufferSize];

        private static Shader shader;

        public static Matrix4x4 World { get; internal set; }
        public static Matrix4x4 View { get; internal set; }
        public static Matrix4x4 Projection { get; internal set; }

        /// <summary>
        /// Alustaa näyttökortin käyttöön
        /// </summary>
        /// <param name="window">Pelin ikkuna</param>
        public static void Create(IWindow window)
        {
            Gl = GL.GetApi(window);

            Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
            Vbo = new BufferObject<VertexPositionColor>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<VertexPositionColor, uint>(Gl, Vbo, Ebo);

            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 1, 0);
            Vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, 1, 12);

            shader = new Shader(Gl, Game.ResourceContent.LoadInternalText("Shaders.OpenGl.DefaultVertexShader.glsl"), Game.ResourceContent.LoadInternalText("Shaders.OpenGl.DefaultFragmentShader.glsl"));
        }

        internal static void DrawUserIndexedPrimitives(PrimitiveType primitives, VertexPositionColor[] vertexBuffer, uint numIndices, uint[] indexBuffer)
        {
            Gl.Enable(GLEnum.Blend);
            Gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

            Ebo.UpdateBuffer(0, indexBuffer);
            Vbo.UpdateBuffer(0, vertexBuffer);

            Vao.Bind();
            shader.Use();

            shader.SetUniform("world", World * View * Projection);

            Gl.DrawElements(primitives, numIndices, DrawElementsType.UnsignedInt, null);
        }

        internal static void SetUniformMat4(string uName, Matrix4x4 value)
        {
            shader.SetUniform(uName, value);
        }

        public static void Clear(Color bgColor)
        {
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Gl.ClearColor(bgColor.RedComponent/255f, bgColor.GreenComponent / 255f, bgColor.BlueComponent / 255f, bgColor.AlphaComponent / 255f);
        }

        internal static void SetRenderTarget(RenderTarget renderTarget)
        {
            renderTarget.Bind();
        }

        public static void Dispose()
        {
            Vbo.Dispose();
            Ebo.Dispose();
            Vao.Dispose();
            shader.Dispose();
        }
    }
}
