using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FontStashSharp.Interfaces;

namespace Jypeli.Rendering
{
    internal class Texture2DManager : ITexture2DManager
    {
        private readonly IGraphicsDevice _device;

        public Texture2DManager(IGraphicsDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            _device = device;
        }

        public object CreateTexture(int width, int height)
        {
            return new Image(width, height, Color.Transparent);
        }

        /// <summary>
        /// Returns size of the specified texture
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public Point GetTextureSize(object texture)
        {
            Image img = texture as Image;
            return new Point(img.Width, img.Height);
        }

        public void SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
        {
            Image img = texture as Image;

            int x = bounds.X;
            int y = bounds.Y;
            int w = bounds.Width;
            int h = bounds.Height;

            int nx = 0;
            int ny = 0;

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    Color c = new Color(data[4 * i * w + 4 * j], data[4 * i * w + 4 * j + 1], data[4 * i * w + 4 * j + 2], data[4 * i * w + 4 * j + 3]);
                    img[y + i, x + j] = c;
                    ny++;
                }
                nx = i;
            }
            img.dirty = true;
        }
    }

    internal class FontRenderer : IFontStashRenderer
    {
        private readonly IGraphicsDevice _device;
        private readonly ImageBatch _batch;
        private readonly Texture2DManager _textureManager;
        public ITexture2DManager TextureManager { get => _textureManager; }

        public FontRenderer(IGraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException(nameof(graphicsDevice));
            }

            _device = graphicsDevice;
            _textureManager = new Texture2DManager(graphicsDevice);
            _batch = Graphics.ImageBatch;
        }

        public void Begin()
        {
            _batch.Begin();
        }

        public void Begin(ref Matrix4x4 transformation)
        {
            _batch.Begin(ref transformation, null);
        }

        public void End()
        {
            _batch.End();
        }

        public void Draw(object texture, Vector2 position, System.Drawing.Rectangle? sourceRectangle, System.Drawing.Color color, float rotation, Vector2 origin, Vector2 scale, float depth)
        {
            Image img = texture as Image;

            _batch.Draw(img,
                position,
                sourceRectangle,
                color,
                scale,
                rotation,
                origin,
                depth);
        }
    }
}
