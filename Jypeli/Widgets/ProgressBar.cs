using Jypeli.Rendering;
using Matrix = System.Numerics.Matrix4x4;

namespace Jypeli.Widgets
{
    //TODO: Mitä eroa on ProgressBarilla ja BarGaugella?
    /// <summary>
    /// Palkki, jolla voidaan ilmaista mittarin arvoa graafisesti.
    /// </summary>
    public class ProgressBar : BindableWidget
    {
        private static readonly Vector[] barVertices = new Vector[]
        {
            new Vector(0.5, -0.5),
            new Vector(0.5, 0.5),
            new Vector(-0.5, 0.5),
            new Vector(-0.5, -0.5)
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

        /// <summary>
        /// Päivittää mittarin näkymän vastaamaan sen arvoa.
        /// </summary>
        protected override void UpdateValue()
        {
            // TODO: Luokkien toteutuksia voisi vähän pohtia paremmin. Tyhjän aliohjelman pitäminen on hieman turhaa ja hämäävää.
        }

        /// <inheritdoc/>
        public override void Draw(Matrix parentTransformation, Matrix transformation)
        {
            double barLength = Size.X * Meter.RelativeValue;

            if (BarImage != null)
            {
                TextureCoordinates tex = new TextureCoordinates();
                tex.TopLeft = new Vector(0, 0);
                tex.TopRight = new Vector(Meter.RelativeValue, 0);
                tex.BottomLeft = new Vector(0, TextureWrapSize.Y);
                tex.BottomRight = new Vector(Meter.RelativeValue, TextureWrapSize.Y);

                Renderer.DrawImage(parentTransformation, BarImage, tex, Position + new Vector(barLength / 2 - Size.X / 2, 0), new Vector(barLength, Size.Y), (float)Angle.Radians);
            }
            else
            {
                Renderer.DrawFilledShape(shapeCache, ref parentTransformation, Position - Vector.FromLengthAndAngle(barLength / 2 - Size.X / 2, Angle), new Vector(barLength, Size.Y), (float)Angle.Radians, BarColor);
            }

            base.Draw(parentTransformation, transformation);
        }
    }
}