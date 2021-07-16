#region MIT License
/*
 * Copyright (c) 2009-2011 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using Matrix = System.Numerics.Matrix4x4;

namespace Jypeli.Widgets
{
    /// <summary>
    /// Palkki, jonka korkeutta voi säätää.
    /// </summary>
    public class BarGauge : Widget
    {
        /// <summary>
        /// Mihin suuntaa liut piirretään
        /// </summary>
        public enum BarDirection
        {
            /// <summary>
            /// Liuku liikuu pystysuuntaan ylös
            /// </summary>
            BarVerticalUp,
            /// <summary>
            /// Liuku liikkuu vaakasuuntaan oikealle
            /// </summary>
            BarHorizontalRight,
            /// <summary>
            /// Liuku liikuu pystysuuntaan alas
            /// </summary>
            BarVerticalDown,
            /// <summary>
            /// Liuku liikkuu vaakasuuntaan vasemmalle
            /// </summary>
            BarHorizontalLeft
        };


        private double h = -1;
        private double w = -1;
        private BarDirection direction = BarDirection.BarVerticalUp;

        /// <summary>
        /// Liun suunta pystyyn vai vaakaan ja mihin suuntaa.
        /// Älä käytä yhdessä Anglen kanssa.
        /// </summary>
        public BarDirection Direction
        {
            get { return direction; }
            set
            {
                direction = value;
                if (h == -1 && w == -1) { h = Height; w = Width; };
                switch (value)
                {
                    case BarDirection.BarVerticalUp:
                        Angle = Angle.FromDegrees(0); 
                        Height = h; Width = w;
                        return;
                    case BarDirection.BarVerticalDown: 
                        Angle = Angle.FromDegrees(180); 
                        Height = h; Width = w;
                        return;
                    case BarDirection.BarHorizontalLeft:
                        Angle = Angle.FromDegrees(-90);
                        Height = w; Width = h;
                        return;
                    case BarDirection.BarHorizontalRight:
                        Angle = Angle.FromDegrees(90);
                        Height = w; Width = h;
                        return;
                }

            }
        }

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

        private static readonly ShapeCache shapeCache = new ShapeCache(barVertices, barIndices);

        private static readonly DoubleMeter defaultMeter = new DoubleMeter( 0, 0, 100 );

        private Meter boundMeter = defaultMeter;

        /// <summary>
        /// Palkin väri.
        /// </summary>
        public Color BarColor { get; set; }

        /// <summary>
        /// Palkin rakentaja.
        /// </summary>
        public BarGauge(double width, double height)
            : base(width, height)
        {
            Color = Color.Transparent;
            BarColor = Color.Red;
        }

        /// <summary>
        /// Asettaa palkin näyttämään <c>meter</c>-olion arvoa.
        /// Palkin maksimiarvoksi tulee olion <c>meter</c> maksimiarvo.
        /// </summary>
        public void BindTo(Meter meter)
        {
            boundMeter = meter;
        }

        /// <inheritdoc/>
        public override void Draw(Matrix parentTransformation, Matrix transformation)
        {
            double barLength = Size.Y * boundMeter.RelativeValue;
            Matrix m =
                Matrix.CreateScale((float)Size.X, (float)barLength, 1f)
                * Matrix.CreateTranslation(0, (float)(-Height / 2), 0)
                * Matrix.CreateRotationZ((float)(Angle).Radians)
                * Matrix.CreateTranslation((float)Position.X, (float)Position.Y, 0.0f)
                * parentTransformation;

            Renderer.DrawFilledShape(shapeCache, ref m, BarColor);

            Vector[] borderVertices = new Vector[]
            {
                new Vector(-0.5, 0.5),
                new Vector(-0.5, -0.5),
                new Vector(0.5, -0.5),
                new Vector(0.5, 0.5),
            };

            // The border that is drawn by base class gets obscured by the bar.
            // Let's draw it again.
            Renderer.DrawPolygon(borderVertices, ref transformation, BorderColor);

            base.Draw(parentTransformation, transformation);

        }
    }
}
