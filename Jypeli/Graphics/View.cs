#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
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

using Silk.NET.Maths;
using Jypeli.Rendering;

using Matrix = System.Numerics.Matrix4x4;
using Vector3 = System.Numerics.Vector3;
using System.Numerics;

namespace Jypeli
{
    /// <summary>
    /// Sisältää näytön leveyden ja korkeuden sekä reunojen koordinaatit.
    /// Y-koordinaatti kasvaa ylöspäin.
    /// Koordinaatteja ei voi muuttaa.
    /// </summary>
    public class ScreenView : Dimensional
    {
        private IRenderTarget renderTarget;
        //private SpriteBatch renderBatch;
        private Vector2D<float> center = Vector2D<float>.Zero;
        private Vector3 scale = Vector3.One;
        private Vector3 scaleInv = Vector3.One;
        private Vector size;
        private float angle = 0;
        private bool flipAndMirror;
        private Color color = Color.White;
        private Color bgcolor = Color.Black;

        /// <summary>
        /// Tekstuuri johon näkymä piirretään.
        /// </summary>
        internal IRenderTarget RenderTarget
        {
            get
            {
                if (renderTarget == null)
                {
                    renderTarget = Game.GraphicsDevice.CreateRenderTarget((uint)size.X, (uint)size.Y);
                    Graphics.ResetScreenSize();
                }

                return renderTarget;
            }
        }

        /// <summary>
        /// Alustaa uuden näyttönäkymän.
        /// </summary>
        public ScreenView()
        {
            size = new Vector(Game.Instance.Window.Size.X * Scale.X, Game.Instance.Window.Size.Y * Scale.Y);
        }

        /// <summary>
        /// Ruudulla näkyvä kuva.
        /// </summary>
        public Image Image
        {
            get { return Screencap.Capture(); }
        }

        /// <summary>
        /// Näytön taustakuva.
        /// </summary>
        public Image Background
        {
            get
            {
                return Image;// new Image( _bgTex );
            }
            set
            {
                // Clone ensures that the image doesn't change unexpectedly
                //_bgTex = value.Clone().XNATexture;
            }
        }

        /// <summary>
        /// Ruudun "taustalla" näkyvä väri jos ruutua on kierretty
        /// tai se on pienempi kuin ikkuna.
        /// </summary>
        public Color BackgroundColor
        {
            get { return bgcolor; }
            set
            {
                bgcolor = value;
            }
        }

        /// <summary>
        /// Näytön keskipiste.
        /// </summary>
        public Vector2D<float> Center
        {
            get { return center; }
            set { center = value; }
        }

        /// <summary>
        /// Kaikkea näytön sisältöä sävyttävä väri (valkoinen = normaali)
        /// </summary>
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Peilataanko kuva pystysuunnassa.
        /// </summary>
        public bool IsFlipped
        {
            get { return flipAndMirror /*|| _effect == SpriteEffects.FlipVertically*/; }
            set
            {
                if (IsMirrored)
                {
                    /*_effect = SpriteEffects.None;*/
                    flipAndMirror = true;
                }
                else
                {
                    /*_effect = SpriteEffects.FlipVertically;*/
                    flipAndMirror = false;
                }
            }
        }

        /// <summary>
        /// Peilataanko kuva vaakasuunnassa.
        /// </summary>
        public bool IsMirrored
        {
            get { return flipAndMirror /* || _effect == SpriteEffects.FlipVertically*/; }
            set
            {
                if (IsFlipped)
                {
                    // _effect = SpriteEffects.None;
                    flipAndMirror = true;
                }
                else
                {
                    //_effect = SpriteEffects.FlipHorizontally;
                    flipAndMirror = false;
                }
            }
        }

        /// <summary>
        /// Näytön kiertokulma.
        /// </summary>
        public Angle Angle
        {
            get { return Angle.FromRadians(-angle); }
            set { angle = -(float)value.Radians; }
        }

        /// <summary>
        /// Näytön skaalaus.
        /// </summary>
        public Vector Scale
        {
            get
            {
#if ANDROID
                return Vector.One; // FrameBufferSize ei toimi luotettavasti Androidilla, toivotaan että millään Android laitteella tämä ei olisi mitään muuta kuin (1,1).
#else
                var fbs = Game.Instance.Window.FramebufferSize;
                var ws = Game.Instance.Window.Size;
                return new Vector(fbs.X / (double)ws.X, fbs.Y / (double)ws.Y);
#endif
            }
            set
            {
                // TODO: Skaalausta ei voi muuttaa, joten tätä asetusta ei myöskään voi tehdä.
            }
        }

        /// <summary>
        /// Näytön leveys x-suunnassa.
        /// </summary>
        public double Width
        {
            get { return RenderTarget.Width / Scale.X; }
            set
            {
                size.X = (int)value * Scale.X;
                renderTarget?.Dispose();
                renderTarget = null;
            }
        }

        /// <summary>
        /// Näytön korkeus y-suunnassa.
        /// </summary>
        public double Height
        {
            get { return RenderTarget.Height / Scale.Y; }
            set
            {
                size.Y = (int)value * Scale.Y;
                renderTarget?.Dispose();
                renderTarget = null;
            }
        }

        /// <summary>
        /// Näytön koko vektorina.
        /// </summary>
        public Vector Size
        {
            get { return new Vector(Width, Height); }
            set
            {
#if DESKTOP
                ((Silk.NET.Windowing.IWindow)Game.Instance.Window).Size = new Vector2D<int>((int)value.X, (int)value.Y);
                size.X = (int)value.X * Scale.X;
                size.Y = (int)value.Y * Scale.Y;
                renderTarget?.Dispose();
                renderTarget = null;
#endif
            }
        }

        /// <summary>
        /// Näytön todellinen leveys.
        /// </summary>
        public double ViewportWidth
        {
            get
            {
                return Game.Instance.Window.Size.X;
            }
        }

        /// <summary>
        /// Näytön todellinen korkeus.
        /// </summary>
        public double ViewportHeight
        {
            get
            {
                return Game.Instance.Window.Size.Y;
            }
        }

        internal void Resize(Vector newSize)
        {
            renderTarget?.Dispose();
            renderTarget = null;
            size = new Vector(newSize.X * Scale.X, newSize.Y * Scale.Y);
            Game.GraphicsDevice.ResizeWindow(size);
        }

        /// <summary>
        /// Näytön todellinen koko.
        /// </summary>
        public Vector ViewportSize
        {
            get { return new Vector(ViewportWidth, ViewportHeight); }
        }

        /// <summary>
        /// Näytön vasemman reunan x-koordinaatti.
        /// </summary>
        public double Left
        {
            get { return -Width / 2; }
        }

        /// <summary>
        /// Näytön oikean reunan x-koordinaatti.
        /// </summary>
        public double Right
        {
            get { return Width / 2; }
        }

        /// <summary>
        /// Näytön yläreunan y-koordinaatti.
        /// </summary>
        public double Top
        {
            get { return Height / 2; }
        }

        /// <summary>
        /// Näytön alareunan y-koordinaatti.
        /// </summary>
        public double Bottom
        {
            get { return -Height / 2; }
        }

        /// <summary>
        /// Vasemman reunan sijainti johon lisätty pieni marginaali
        /// </summary>
        public double LeftSafe { get { return Left + 10; } }

        /// <summary>
        /// Oikean reunan sijainti johon lisätty pieni marginaali
        /// </summary>
        public double RightSafe { get { return Right - 10; } }

        /// <summary>
        /// Alareunan sijainti johon lisätty pieni marginaali
        /// </summary>
        public double BottomSafe { get { return Bottom + 10; } }

        /// <summary>
        /// Yläreunan sijainti johon lisätty pieni marginaali
        /// </summary>
        public double TopSafe { get { return Top + 10; } }

        /// <summary>
        /// Leveys johon lisätty pieni marginaali
        /// </summary>
        public double WidthSafe { get { return RightSafe - LeftSafe; } }

        /// <summary>
        /// Korkeus johon lisätty pieni marginaali
        /// </summary>
        public double HeightSafe { get { return TopSafe - BottomSafe; } }

        /// <summary>
        /// Skaalaa näkymän mahtumaan ruudulle
        /// </summary>
		public void ScaleToFit()
        {
            // TODO: Angle
            this.Scale = new Vector(ViewportSize.X / Size.X, ViewportSize.Y / Size.Y);
        }

        /// <summary>
        /// Muuntaa standardeista ruutukoordinaateista Jypelin ruutukoordinaateiksi.
        /// Normaaleissa ruutukoordinaateissa origo on vasemmassa yläkulmassa.
        /// Jypelissä se taas on ruudun keskellä.
        /// </summary>
        /// <param name="position">Sijainti joka muutetaan</param>
        /// <returns></returns>
        internal static Vector FromDisplayCoords(Vector position)
        {
            var screenSize = Game.Screen.Size;
            double x = -screenSize.X / 2 + position.X;
            double y = screenSize.Y / 2 - position.Y;
            return new Vector(x, y);
        }

        /// <summary>
        /// Muuntaa Jypelin ruutukoordinaateista standardeiksi ruutukoordinaateiksi.
        /// </summary>
        /// <param name="position">Sijainti joka muutetaan</param>
        /// <returns></returns>
        internal static Vector ToDisplayCoords(Vector position)
        {
            var screenSize = Game.Screen.Size;
            double x = screenSize.X / 2 + position.X;
            double y = screenSize.Y / 2 - position.Y;
            return new Vector(x, y);
        }

        // TODO: Tehdäänkö tällä enää mitään?
        /// <summary>
        /// Muuntaa matriisin Jypelin ruutukoordinaateista XNA:n ruutukoordinaatteihin.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="screenSize"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        internal static Matrix ToXnaCoords(ref Matrix matrix, Vector screenSize, Vector scale)
        {
            // Keskitetään sprite ruudulla, mutta toteutetaan alkuperäinen muunnos Jypelin koordinaateissa.
            var centralize = Matrix.CreateTranslation((screenSize - scale) / 2);
            var toXna = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(screenSize / 2);
            Matrix.Invert(toXna, out Matrix toJypeli);

            return centralize * toJypeli * matrix * toXna;
        }

        /// <summary>
        /// Palauttaa transformaatiomatriisin jolla voi ottaa huomioon ruudun kokoon,
        /// kiertoon ja paikkaan tehdyt muutokset.
        /// Ennen transformaatiota: paikkavektori ikkunan koordinaateissa
        /// Transformaation jälkeen: paikkavektori RenderTargetin koordinaateissa
        /// </summary>
        /// <returns></returns>
        internal Matrix GetScreenTransform()
        {
            return Matrix.CreateScale(scale)
                * Matrix.CreateRotationZ(angle)
                * Matrix.CreateTranslation(-center.X, -center.Y, 0);
        }

        /// <summary>
        /// Palauttaa käänteisen transformaatiomatriisin jolla voi ottaa huomioon ruudun kokoon,
        /// kiertoon ja paikkaan tehdyt muutokset.
        /// Ennen transformaatiota: paikkavektori RenderTargetin koordinaateissa
        /// Transformaation jälkeen: paikkavektori ikkunan koordinaateissa
        /// </summary>
        /// <returns></returns>
        internal Matrix GetScreenInverse()
        {
            return Matrix.CreateScale(scaleInv)
                * Matrix.CreateRotationZ(-angle)
                * Matrix.CreateTranslation(center.X, center.Y, 0);
        }

        internal void Render()
        {
            float angle = flipAndMirror ? this.angle + (float)MathHelper.Pi : this.angle;

            Game.GraphicsDevice.SetRenderTarget(null);
            Game.GraphicsDevice.Clear(Game.Instance.Level.BackgroundColor);
            RenderTarget.BindTexture();

            Graphics.BasicTextureShader.Use();
            Graphics.BasicTextureShader.SetUniform("world", Matrix.Identity);

            Game.GraphicsDevice.DrawPrimitives(PrimitiveType.OpenGlTriangles, Graphics.TextureVertices, 6, true);
            RenderTarget.UnBindTexture();
        }
    }

    /// <summary>
    /// Asemointi vaakasuunnassa.
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// Keskellä.
        /// </summary>
        Center,

        /// <summary>
        /// Vasemmassa reunassa.
        /// </summary>
        Left,

        /// <summary>
        /// Oikeassa reunassa.
        /// </summary>
        Right
    }

    /// <summary>
    /// Asemointi pystysuunnassa.
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// Keskellä.
        /// </summary>
        Center,

        /// <summary>
        /// Yläreunassa.
        /// </summary>
        Top,

        /// <summary>
        /// Alareunassa.
        /// </summary>
        Bottom
    }
}

