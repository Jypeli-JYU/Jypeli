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
            public ShapeCache Cache { get; set; }
            public Color Color { get; set; }
            public Vector Position { get; set; }
            public Vector Size { get; set; }
            public float Rotation { get; set; }
            public TextureCoordinates Texcoords { get; set; }

            public System.Drawing.Rectangle? SourceRectangle { get; set; }
            public System.Drawing.Color Dcolor { get; set; }
            public Vector2 Origin { get; set; }

            public BatchItem(ShapeCache cache, Color color, Vector position, Vector size, float rotation)
            {
                Cache = cache;
                Color = color;
                Position = position;
                Size = size;
                Rotation = rotation;
            }

            public BatchItem(TextureCoordinates texcoords, Vector position, Vector size, float rotation)
            {
                Texcoords = texcoords;
                Position = position;
                Size = size;
                Rotation = rotation;
            }

            public BatchItem(Vector2 position, System.Drawing.Rectangle? sourceRectangle, System.Drawing.Color color, Vector2 size, float rotation, Vector2 origin)
            {
                Position = position;
                SourceRectangle = sourceRectangle;
                Dcolor = color;
                Size = size;
                Rotation = rotation;
                Origin = origin;
            }
        }

        // Järjestetään piirrettävät esineet niiden transformaatiomatriisin mukaan.
        // Matriisin vaihto tarkoittaa aina uutta piirtokomentoa, joten tämä lienee paras, tai ainakin helpoin, tapa.
        // Ihan vain yksinkertaisuuden takia jaetaan teksti omaan dictionaryyn, vaikka sekin on vain kuva, jonka palasia piirretään.
        private Dictionary<Image, Dictionary<Matrix4x4, List<BatchItem>>> ImageBatches = new Dictionary<Image, Dictionary<Matrix4x4, List<BatchItem>>>();
        private Dictionary<Image, Dictionary<Matrix4x4, List<BatchItem>>> TextBatches = new Dictionary<Image, Dictionary<Matrix4x4, List<BatchItem>>>();
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
            if (!TextBatches.ContainsKey(image))
            {
                TextBatches.Add(image, new Dictionary<Matrix4x4, List<BatchItem>>());
            }
            Dictionary<Matrix4x4, List<BatchItem>> batch = TextBatches[image];

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
                    Graphics.ShapeBatch.Draw(item.Cache, item.Color, item.Position, item.Size, item.Rotation);
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
                        Graphics.ImageBatch.Draw(item.Texcoords, item.Position, item.Size, item.Rotation);
                    }
                    Graphics.ImageBatch.End();
                    ImageBatches[img][batch.Key].Clear();
                }
            }

            var textImages = TextBatches.Keys;

            foreach (var img in textImages)
            {
                foreach (var batch in TextBatches[img])
                {
                    Matrix4x4 matrix = batch.Key;
                    Graphics.ImageBatch.Begin(ref matrix, img);
                    foreach (var item in TextBatches[img][batch.Key])
                    {
                        Graphics.ImageBatch.Draw(img, item.Position, item.SourceRectangle, item.Dcolor, item.Size, item.Rotation, item.Origin);
                    }
                    Graphics.ImageBatch.End();
                    TextBatches[img][batch.Key].Clear();
                }
            }
        }
    }
}
