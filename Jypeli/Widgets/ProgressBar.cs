using Matrix = System.Numerics.Matrix4x4;

namespace Jypeli.Widgets
{
    /// <summary>
    /// Palkki, jolla voidaan ilmaista mittarin arvoa graafisesti.
    /// </summary>
    public class ProgressBar : BindableWidget
    {
        // private double h = -1;
        // private double w = -1;

        private static readonly Vector[] barVertices = new Vector[]
        {
            new Vector(-0.5, 0),
            new Vector(0.5, 0),
            new Vector(0.5, 1.0),
            new Vector(-0.5, 1.0)
        };

        private static readonly IndexTriangle[] barIndices = new IndexTriangle[]
        {
            new IndexTriangle(0, 3, 2),
            new IndexTriangle(0, 2, 1)
        };

        private static readonly Vector[] borderVertices = new Vector[]
        {
            new Vector(-0.5, 0.5),
            new Vector(-0.5, -0.5),
            new Vector(0.5, -0.5),
            new Vector(0.5, 0.5),
        };

        private static readonly ShapeCache shapeCache = new ShapeCache(barVertices, barIndices);

        /// <summary>
        /// Palkin kuva.
        /// Jos erisuuri kuin null, piirretään värin (BarColor) sijasta.
        /// </summary>
        public Image BarImage { get; set; }

        /// <summary>
        /// Palkin väri.
        /// </summary>
        public Color BarColor { get; set; }

        /// <summary>
        /// Palkin rakentaja.
        /// </summary>
        /// <param name="width">Palkin leveys</param>
        /// <param name="height">Palkin korkeus</param>
        public ProgressBar(double width, double height)
            : base(width, height)
        {
            Color = Color.Transparent;
            BarColor = Color.Red;
            BarImage = null;
        }

        /// <summary>
        /// Palkin rakentaja. Sitoo palkin arvon mittarin arvoon.
        /// </summary>
        /// <param name="width">Palkin leveys</param>
        /// <param name="height">Palkin korkeus</param>
        /// <param name="meter">Mittari</param>
        public ProgressBar(double width, double height, Meter meter)
            : this(width, height)
        {
            BindTo(meter);
        }

        Matrix imgFull;
        Matrix imgPart;
        Matrix colorPart;

        /// <summary>
        /// Päivittää mittarin näkymän vastaamaan sen arvoa
        /// </summary>
        protected override void UpdateValue()
        {
            double barLength = Size.X * Meter.RelativeValue;

            imgPart =
                    Matrix.CreateScale((float)barLength, (float)Size.Y, 1f)
                    * Matrix.CreateTranslation((float)(barLength / 2), 0, 0)
                    * Matrix.CreateTranslation((float)(-Width / 2), 0, 0)
                    * Matrix.CreateRotationZ((float)(Angle).Radians)
                    * Matrix.CreateTranslation((float)Position.X, (float)Position.Y, 0.0f);

            imgFull =
                Matrix.CreateScale((float)Size.X, (float)Size.Y, 1f)
                * Matrix.CreateRotationZ((float)(Angle).Radians)
                * Matrix.CreateTranslation((float)Position.X, (float)Position.Y, 0.0f);

            colorPart =
                Matrix.CreateScale((float)barLength, (float)Size.Y, 1f)
                * Matrix.CreateTranslation((float)(barLength / 2), 0, 0)
                * Matrix.CreateTranslation((float)(-Width / 2), (float)(-Height / 2), 0)
                * Matrix.CreateRotationZ((float)(Angle).Radians)
                * Matrix.CreateTranslation((float)Position.X, (float)Position.Y, 0.0f);
        }

        /// <inheritdoc/>
        public override void Draw(Matrix parentTransformation, Matrix transformation)
        {
            // TODO: Optimization?
            UpdateValue();

            if (BarImage != null)
            {
                Matrix imp = imgPart * parentTransformation;
                Matrix imf = imgFull * parentTransformation;

                Renderer.BeginDrawingInsideShape(Shape.Rectangle, ref imp);
                Renderer.DrawImage(BarImage, ref imf, TextureWrapSize);
                Renderer.EndDrawingInsideShape();
            }
            else
            {
                Matrix m = colorPart * parentTransformation;
                Renderer.DrawFilledShape(shapeCache, ref m, BarColor);
            }

            // The border that is drawn by base class gets obscured by the bar.
            // Let's draw it again.
            Renderer.DrawPolygon(borderVertices, ref transformation, BorderColor);

            base.Draw(parentTransformation, transformation);
        }
    }
}