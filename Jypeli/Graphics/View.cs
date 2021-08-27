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
                if ( renderTarget == null )
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
        /// <param name="device">XNA:n grafiikkalaite.</param>
        public ScreenView()
        {
            size = new Vector(Game.Instance.Window.Size.X, Game.Instance.Window.Size.Y);
        }

        /// <summary>
        /// Ruudulla näkyvä kuva.
        /// </summary>
        public Image Image
        {
            get { return new Image(20,20 /*RenderTarget*/ ); }
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
                if ( IsMirrored )
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
                if ( IsFlipped )
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
            get { return Angle.FromRadians( -angle ); }
            set { angle = -(float)value.Radians; }
        }

        /// <summary>
        /// Näytön skaalaus.
        /// </summary>
        public Vector Scale
        {
            get { return new Vector( scale.X, scale.Y ); }
            set
            {
                scale = new Vector3( (float)value.X, (float)value.Y, 1 );
                scaleInv = new Vector3( 1 / scale.X, 1 / scale.Y, 1 );
            }
        }

        /// <summary>
        /// Näytön leveys x-suunnassa.
        /// </summary>
        public double Width
        {
            get { return RenderTarget.Width; }
            set
            {
                size.X = (int)value;
                renderTarget?.Dispose();
                renderTarget = null;
            }
        }

        /// <summary>
        /// Näytön korkeus y-suunnassa.
        /// </summary>
        public double Height
        {
            get { return RenderTarget.Height; }
            set
            {
                size.Y = (int)value;
                renderTarget?.Dispose();
                renderTarget = null;
            }
        }

        /// <summary>
        /// Näytön koko vektorina.
        /// </summary>
        public Vector Size
        {
            get { return new Vector( RenderTarget.Width, RenderTarget.Height ); }
            set
            {
#if DESKTOP
                ((Silk.NET.Windowing.IWindow)Game.Instance.Window).Size = new Vector2D<int>((int)value.X, (int)value.Y);
                size.X = (int)value.X;
                size.Y = (int)value.Y;
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
            size = newSize;
            Game.GraphicsDevice.ResizeWindow(newSize);
        }

        /// <summary>
        /// Näytön todellinen koko.
        /// </summary>
        public Vector ViewportSize
        {
            get { return new Vector( ViewportWidth, ViewportHeight ); }
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
			this.Scale = new Vector( ViewportSize.X / Size.X, ViewportSize.Y / Size.Y );
		}

        /// <summary>
        /// Muuntaa XNA:n ruutukoordinaateista Jypelin ruutukoordinaateiksi.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="screenSize"></param>
        /// <param name="objectSize"></param>
        /// <returns></returns>
        internal static Vector FromXnaCoords( Vector position, Vector screenSize, Vector objectSize )
        {
            double x = ( -screenSize.X + objectSize.X ) / 2 + position.X;
            double y = ( screenSize.Y - objectSize.Y ) / 2 - position.Y;
            return new Vector( x, y );
        }

        private static float xToXna( float x, float scrW, float objW )
        {
            return ( scrW - objW ) / 2 + x;
        }

        private static float yToXna( float y, float scrH, float objH )
        {
            return ( scrH - objH ) / 2 - y;
        }

        /// <summary>
        /// Muuntaa Jypelin ruutukoordinaateista XNA:n ruutukoordinaateiksi.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="screenSize"></param>
        /// <param name="objectSize"></param>
        /// <returns></returns>
        internal static Vector ToXnaCoords( Vector position, Vector screenSize, Vector objectSize )
        {
            return new Vector(
                xToXna( (float)position.X, (float)screenSize.X, (float)objectSize.X ),
                yToXna( (float)position.Y, (float)screenSize.Y, (float)objectSize.Y ) );
        }
        
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
            var toXna      = Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(screenSize / 2);
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
            return Matrix.CreateScale( scale )
                * Matrix.CreateRotationZ( angle )
                * Matrix.CreateTranslation( -center.X, -center.Y, 0 );
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
            return Matrix.CreateScale( scaleInv )
                * Matrix.CreateRotationZ( -angle )
                * Matrix.CreateTranslation( center.X, center.Y, 0 );
        }

        internal void Render()
        {
            float angle = flipAndMirror ? this.angle + MathHelper.Pi : this.angle;

            Game.GraphicsDevice.SetRenderTarget(null);
            Game.GraphicsDevice.Clear(Color.Black);

            Graphics.LightPassTextureShader.Use();
            Graphics.LightPassTextureShader.SetUniform("world", Matrix.Identity);

            Graphics.LightPassTextureShader.SetUniform("texture0", 0);
            Graphics.LightPassTextureShader.SetUniform("texture1", 1);

            Graphics.LightPassTextureShader.SetUniform("ambientLight", Game.Instance.Level.AmbientLight.ToNumerics());

            RenderTarget.TextureSlot(0);
            RenderTarget.BindTexture();

            // TODO: Tätä ei tarvitsisi tehdä, jos valoja ei käytetä.
            // TODO: Menee myös käyttöliittymäelementtien päälle.
            Effects.BasicLights.RenderTarget.TextureSlot(1);
            Effects.BasicLights.RenderTarget.BindTexture();

            Game.GraphicsDevice.DrawPrimitives(PrimitiveType.OpenGlTriangles, Graphics.TextureVertices, 6, true);

            RenderTarget.UnBindTexture();
            RenderTarget.TextureSlot(0);
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

