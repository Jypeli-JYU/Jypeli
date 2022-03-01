using System.Numerics;
using Jypeli.Effects;

namespace Jypeli.Rendering.OpenGl
{
    internal unsafe struct LightData
    {
        public Vector2 Position;
        public float Radius;
        public float Intensity;
        public fixed float Color[4];
    }

    internal unsafe class BasicLightRenderer
    {
        VertexArrayObject<VertexPositionColorTexture, Vector4> Vao;
        BufferObject<VertexPositionColorTexture> vertexbuffer;
        BufferObject<LightData> databuffer;

        Silk.NET.OpenGL.GL gl;

        private LightData[] lightData;
        public static IRenderTarget RenderTarget;

        internal BasicLightRenderer(GraphicsDevice device)
        {
            // Piirretään valot omaan tekstuuriin, joka sitten piirretään muiden elementtien päälle.
            RenderTarget = device.CreateRenderTarget((uint)Game.Instance.Window.Size.X, (uint)Game.Instance.Window.Size.Y);

            Game.Instance.Window.Resize += (v) => ResizeRenderTarget();

            int maxAmountOfLights = 1000; // TODO: Mikä olisi hyvä rajoitus?
            lightData = new LightData[maxAmountOfLights];
            gl = ((GraphicsDevice)device).Gl;
            vertexbuffer = new BufferObject<VertexPositionColorTexture>(gl, Graphics.TextureVertices, Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<VertexPositionColorTexture, Vector4>(gl, vertexbuffer, null);

            // TODO: Tämä on hieman ruma ja kaipaisi jonkinlaista abstraktiota.
            Vao.VertexAttributePointer(0, 3, Silk.NET.OpenGL.VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 0);
            Vao.VertexAttributePointer(1, 4, Silk.NET.OpenGL.VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 12);
            Vao.VertexAttributePointer(2, 2, Silk.NET.OpenGL.VertexAttribPointerType.Float, (uint)sizeof(VertexPositionColorTexture), 28);

            databuffer = new BufferObject<LightData>(gl, lightData, Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer);
            databuffer.SetVertexAttribPointer(3, 2, (uint)sizeof(LightData), 0);
            databuffer.SetVertexAttribPointer(4, 1, (uint)sizeof(LightData), 8);
            databuffer.SetVertexAttribPointer(5, 1, (uint)sizeof(LightData), 12);
            databuffer.SetVertexAttribPointer(6, 4, (uint)sizeof(LightData), 16);

            gl.BindBuffer(Silk.NET.OpenGL.BufferTargetARB.ArrayBuffer, 0);
        }


        private static void ResizeRenderTarget()
        {
            RenderTarget.Dispose();
            RenderTarget = Game.GraphicsDevice.CreateRenderTarget((uint)Game.Screen.Width, (uint)Game.Screen.Height);
        }


        internal void Draw(Matrix4x4 matrix)
        {
            if (Game.Lights.Count == 0)
                return;

            Game.GraphicsDevice.SetRenderTarget(RenderTarget);
            Game.GraphicsDevice.Clear(Color.Transparent);
            RenderTarget.TextureSlot(0);
            RenderTarget.BindTexture();

            Matrix4x4 mat = matrix * Graphics.ViewProjectionMatrix;

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

            Graphics.SimpleFloodLightShader.Use();

            Vao.Bind();
            gl.Enable(Silk.NET.OpenGL.GLEnum.Blend);
            gl.BlendFunc(Silk.NET.OpenGL.GLEnum.SrcAlpha, Silk.NET.OpenGL.GLEnum.One);
            gl.DrawArraysInstanced(Silk.NET.OpenGL.GLEnum.Triangles, 0, 6, (uint)Game.Lights.Count);
        }
    }
}
