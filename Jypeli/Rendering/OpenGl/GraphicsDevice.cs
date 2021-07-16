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

        private static int bufferSize = 16384;
        private static uint[] Indices = new uint[bufferSize * 2];
        private static VertexPositionColor[] Vertices = new VertexPositionColor[bufferSize];

        private static Shader shader;

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

            shader = new Shader(Gl, VertexShaderSource, FragmentShaderSource);
        }

        internal static void DrawUserIndexedPrimitives(PrimitiveType primitives, VertexPositionColor[] vertexBuffer, uint numIndices, uint[] indexBuffer)
        {
            Gl.Enable(GLEnum.Blend);
            Gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

            Ebo.UpdateBuffer(0, indexBuffer);
            Vbo.UpdateBuffer(0, vertexBuffer);

            Vao.Bind();
            shader.Use();

            shader.SetUniform("uModel", Matrix4x4.CreateScale(0.2f));

            Gl.DrawElements(primitives, numIndices, DrawElementsType.UnsignedInt, null);
        }

        public static readonly string VertexShaderSource = @"
  
#version 330 core
layout (location = 0) in vec4 vPos;
layout (location = 1) in vec4 vUv;

uniform mat4 uModel;

out vec4 fCol;

void main()
{
    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position =  uModel * vPos;
    fCol = vUv;
}
        ";

        //Fragment shaders are run on each fragment/pixel of the geometry.
        public static readonly string FragmentShaderSource = @"
#version 330 core


in vec4 fCol;
out vec4 FragColor;

void main()
{
    FragColor = fCol;
}
        ";

        public static void Clear(Color bgColor)
        {
            Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            Gl.ClearColor(bgColor.RedComponent/255f, bgColor.GreenComponent / 255f, bgColor.BlueComponent / 255f, bgColor.AlphaComponent / 255f);
        }
    }
}
