using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Jypeli.Rendering;

namespace Jypeli.Effects
{
    internal unsafe struct LightData
    {
        public Vector2 Position;
        public float Radius;
        public float Intensity;
        public fixed float Color[4];
    }

    internal unsafe class BasicLights
    {
        Rendering.OpenGl.VertexArrayObject<VertexPositionColorTexture, Vector4> Vao;
        Rendering.OpenGl.BufferObject<VertexPositionColorTexture> vertexbuffer;
        Rendering.OpenGl.BufferObject<LightData> databuffer;

        Silk.NET.OpenGL.GL gl;
        IShader shader;

        private LightData[] lightData;
        public static IRenderTarget RenderTarget;

        internal BasicLights()
        {
            // Piirretään valot omaan tekstuuriin, joka sitten piirretään muiden elementtien päälle.
            RenderTarget = Game.GraphicsDevice.CreateRenderTarget((uint)Game.Screen.Width, (uint)Game.Screen.Height);

            Game.Instance.Window.Resize += (v) => ResizeRenderTarget();

            shader = Game.GraphicsDevice.CreateShader(Game.ResourceContent.LoadInternalText("Shaders.OpenGL.SimpleFloodLightVertex.glsl"), Game.ResourceContent.LoadInternalText("Shaders.OpenGL.SimpleFloodLightFragment.glsl"));
            int maxAmountOfLights = 1000; // TODO: Mikä olisi hyvä rajoitus?
            lightData = new LightData[maxAmountOfLights];
            gl = ((Rendering.OpenGl.GraphicsDevice)Game.GraphicsDevice).Gl;
            vertexbuffer = new Rendering.OpenGl.BufferObject<VertexPositionColorTexture>(gl, Graphics.TextureVertices, Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer);
            Vao = new Rendering.OpenGl.VertexArrayObject<VertexPositionColorTexture, Vector4>(gl, vertexbuffer, null);

            // TODO: Tämä on hieman ruma ja kaipaisi jonkinlaista abstraktiota.
            Vao.VertexAttributePointer(0, 3, Silk.NET.OpenGL.VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 0);
            Vao.VertexAttributePointer(1, 4, Silk.NET.OpenGL.VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 12);
            Vao.VertexAttributePointer(2, 2, Silk.NET.OpenGL.VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 28);

            databuffer = new Rendering.OpenGl.BufferObject<LightData>(gl, lightData, Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer);

            databuffer.Bind();
            gl.EnableVertexAttribArray(3);
            gl.VertexAttribPointer(3, 2, Silk.NET.OpenGL.VertexAttribPointerType.Float, false, (uint)sizeof(LightData), (void*)0);
            gl.VertexAttribDivisor(3, 1);

            gl.EnableVertexAttribArray(4);
            gl.VertexAttribPointer(4, 1, Silk.NET.OpenGL.VertexAttribPointerType.Float, false, (uint)sizeof(LightData), (void*)8); // Vika parametri on offsetin määrä tavuissa.
            gl.VertexAttribDivisor(4, 1);

            gl.EnableVertexAttribArray(5);
            gl.VertexAttribPointer(5, 1, Silk.NET.OpenGL.VertexAttribPointerType.Float, false, (uint)sizeof(LightData), (void*)12);
            gl.VertexAttribDivisor(5, 1);

            gl.EnableVertexAttribArray(6);
            gl.VertexAttribPointer(6, 4, Silk.NET.OpenGL.VertexAttribPointerType.Float, false, (uint)sizeof(LightData), (void*)16);
            gl.VertexAttribDivisor(6, 1);

            gl.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, 0);
        }


        private void ResizeRenderTarget()
        {
            RenderTarget.Dispose();
            Game.GraphicsDevice.CreateRenderTarget((uint)Game.Screen.Width, (uint)Game.Screen.Height);
        }


        internal void Draw(Matrix4x4 matrix)
        {
            if (Game.Lights.Count == 0)
                return;

            Game.GraphicsDevice.SetRenderTarget(RenderTarget);
            Game.GraphicsDevice.Clear(Color.Transparent);
            RenderTarget.TextureSlot(0);
            RenderTarget.BindTexture();

            Matrix4x4 mat = matrix * Game.GraphicsDevice.View * Game.GraphicsDevice.Projection;

            int i = 0;
            foreach (Light l in Game.Lights)
            {
                LightData ldata = new LightData();
                ldata.Position = l.Position.Transform(matrix);
                ldata.Intensity = (float)l.Intensity;
                ldata.Radius = (float)(l.Radius * Game.Instance.Camera.ZoomFactor);
                ldata.Color[0] = l.Color.RedComponent / 255f;
                ldata.Color[1] = l.Color.GreenComponent / 255f;
                ldata.Color[2] = l.Color.BlueComponent / 255f;
                lightData[i++] = ldata;
            }

            databuffer.UpdateBuffer(0, lightData);

            var device = Game.GraphicsDevice;

            shader.Use();

            Vao.Bind();
            gl.Enable(Silk.NET.OpenGL.GLEnum.Blend);
            gl.BlendFunc(Silk.NET.OpenGL.GLEnum.SrcAlpha, Silk.NET.OpenGL.GLEnum.One);
            gl.DrawArraysInstanced(Silk.NET.OpenGL.GLEnum.Triangles, 0, 6, (uint)Game.Lights.Count);
        }
    }
}
