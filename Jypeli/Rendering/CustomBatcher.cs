using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Jypeli.Rendering
{
    /// <summary>
    /// Hoitaa omien piirtometodien omaavien olioiden piirtämisen tehokkaammin
    /// yhdistämällä piirtokutsuja.
    /// </summary>
    internal class CustomBatcher
    {
        private class BatchItem
        {
            public ShapeCache cache;
            public Color color;
            public Image image;
            public Vector position;
            public Vector size;
            public float rotation;
            public TextureCoordinates texcoords;

            public System.Drawing.Rectangle? sourceRectangle;
            public System.Drawing.Color dcolor;
            public Vector2 origin;

            public BatchItem(ShapeCache cache, Color color, Vector position, Vector size, float rotation)
            {
                this.cache = cache;
                this.color = color;
                this.position = position;
                this.size = size;
                this.rotation = rotation;
            }

            public BatchItem(TextureCoordinates texcoords, Vector position, Vector size, float rotation)
            {
                this.texcoords = texcoords;
                this.position = position;
                this.size = size;
                this.rotation = rotation;
            }

            public BatchItem(Vector2 position, System.Drawing.Rectangle? sourceRectangle, System.Drawing.Color color, Vector2 size, float rotation, Vector2 origin)
            {
                this.position = position;
                this.sourceRectangle = sourceRectangle;
                this.dcolor = color;
                this.size = size;
                this.rotation = rotation;
                this.origin = origin;
            }
        }

        private Dictionary<Image, Dictionary<Matrix4x4, List<BatchItem>>> ImageBatches = new Dictionary<Image, Dictionary<Matrix4x4, List<BatchItem>>>();
        private Dictionary<Matrix4x4, List<BatchItem>> ShapeBatches = new Dictionary<Matrix4x4, List<BatchItem>>();

        public void AddShape(Matrix4x4 matrix, ShapeCache cache, Color color, Vector position, Vector size, float rotation)
        {
            if (ShapeBatches.TryGetValue(matrix, out List<BatchItem> list))
            {
                if(list == null)
                {
                    list = new List<BatchItem>();
                    ShapeBatches[matrix] = list;
                }
                list.Add(new BatchItem(cache, color, position, size, rotation));
            }
            else
            {
                ShapeBatches.Add(matrix, new List<BatchItem>());
                ShapeBatches[matrix].Add(new BatchItem(cache, color, position, size, rotation));
            }
        }

        public void AddImage(Matrix4x4 matrix, Image image, TextureCoordinates texcoords, Vector position, Vector size, float rotation)
        {
            Dictionary<Matrix4x4, List<BatchItem>> batch = ImageBatches[image];

            if (batch.TryGetValue(matrix, out List<BatchItem> list))
            {
                if (list == null)
                {
                    list = new List<BatchItem>();
                    batch[matrix] = list;
                }
                list.Add(new BatchItem(texcoords, position, size, rotation));
            }
            else
            {
                batch.Add(matrix, new List<BatchItem>());
                batch[matrix].Add(new BatchItem(texcoords, position, size, rotation));
            }
        }

        public void AddText(Matrix4x4 matrix, Image image, Vector2 position, System.Drawing.Rectangle? sourceRectangle, System.Drawing.Color color, Vector2 size, float rotation, Vector2 origin)
        {
            if (!ImageBatches.ContainsKey(image))
            {
                ImageBatches.Add(image, new Dictionary<Matrix4x4, List<BatchItem>>());
            }
            Dictionary<Matrix4x4, List<BatchItem>> batch = ImageBatches[image];

            if (batch.TryGetValue(matrix, out List<BatchItem> list))
            {
                if (list == null)
                {
                    list = new List<BatchItem>();
                    batch[matrix] = list;
                }
                list.Add(new BatchItem(position, sourceRectangle, color, size, rotation, origin));
            }
            else
            {
                batch.Add(matrix, new List<BatchItem>());
                batch[matrix].Add(new BatchItem(position, sourceRectangle, color, size, rotation, origin));
            }
        }

        public void Flush()
        {
            var withoutImages = ShapeBatches.Keys;

            foreach(var m in withoutImages)
            {
                Matrix4x4 matrix = m;
                Graphics.ShapeBatch.Begin(ref matrix);
                foreach (var item in ShapeBatches[m])
                {
                    Graphics.ShapeBatch.Draw(item.cache, item.color, item.position, item.size, item.rotation);
                }
                Graphics.ShapeBatch.End();
                ShapeBatches[m].Clear();
            }

            var images = ImageBatches.Keys;

            foreach (var img in images)
            {
                foreach (var batch in ImageBatches[img])
                {
                    Matrix4x4 matrix = batch.Key;
                    Graphics.ImageBatch.Begin(ref matrix, img);
                    foreach (var item in ImageBatches[img][batch.Key])
                    {
                        Graphics.ImageBatch.Draw(img, item.position, item.sourceRectangle, item.dcolor, item.size, item.rotation, item.origin);
                    }
                    Graphics.ImageBatch.End();
                    ImageBatches[img][batch.Key].Clear();
                }
                
            }
        }
    }
}
