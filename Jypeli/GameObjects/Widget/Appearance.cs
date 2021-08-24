using Matrix = System.Numerics.Matrix4x4;

namespace Jypeli
{
    public partial class Widget
    {
        /// <summary>
        /// Reunojen väri.
        /// </summary>
        public Color BorderColor { get; set; }

        private void InitAppearance()
        {
            this.BorderColor = Color.Transparent;
            this.IgnoresLighting = true;
        }

        /// <summary>
        /// Piirtää elementin ruudulle
        /// </summary>
        /// <param name="parentTransformation"></param>
        /// <param name="transformation"></param>
        public virtual void Draw( Matrix parentTransformation, Matrix transformation )
        {
        }

        /// <summary>
        /// Piirtää elementin ruudulle
        /// </summary>
        /// <param name="parentTransformation"></param>
        public void Draw(Matrix parentTransformation)
        {
            // TODO: onko ikinä tilannetta milloin parentTransformation ei ole kameran transformaatiomatriisi?
            if (!IsVisible)
                return;

            var lightingEnabled = Renderer.LightingEnabled;
            Renderer.LightingEnabled &= !IgnoresLighting;

            if (Image != null && (!TextureFillsShape))
            {
                Renderer.DrawImage(parentTransformation, Image, new Rendering.TextureCoordinates(), Position, Size, (float)Angle.Radians);
            }
            else if (Image != null)
            {
                // TODO: TextureFillsShape kuntoon
                Renderer.DrawImage(parentTransformation, Image, new Rendering.TextureCoordinates(), Position, Size, (float)Angle.Radians);
            }
            else
            {
                Renderer.DrawFilledShape(Shape.Cache, ref parentTransformation, Position, Size, (float)Angle.Radians, Color);
            }

            if (BorderColor != Color.Transparent)
            {
                Graphics.LineBatch.Begin(ref parentTransformation);
                {
                    Vector[] vertices = Shape.Cache.OutlineVertices;
                    for (int i = 0; i < vertices.Length - 1; i++)
                    {
                        Graphics.LineBatch.Draw(vertices[i], vertices[i + 1], BorderColor);
                    }
                    Graphics.LineBatch.Draw(vertices[vertices.Length - 1], vertices[0], BorderColor);
                }
                Graphics.LineBatch.End();
            }

            Draw(parentTransformation, parentTransformation);

            if (_childObjects != null && _childObjects.Count > 0)
            {
                foreach (var child in Objects)
                {
                    Widget wc = child as Widget;

                    if (wc != null && wc.IsVisible)
                    {
                        wc.Draw(parentTransformation);
                    }
                }
            }

            Renderer.LightingEnabled = lightingEnabled;
        }
    }
}
