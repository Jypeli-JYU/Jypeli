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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XnaColor = Microsoft.Xna.Framework.Color;

namespace Jypeli
{
    /// <summary>
    /// Sisältää näytön leveyden ja korkeuden sekä reunojen koordinaatit.
    /// Y-koordinaatti kasvaa ylöspäin.
    /// Koordinaatteja ei voi muuttaa.
    /// </summary>
    public class ScreenView : Dimensional
    {
        private RenderTarget2D _renderTarget = null;
        private SpriteBatch renderBatch;
        private Vector2 _center = Vector2.Zero;
        private Vector3 _scale = Vector3.One;
        private Vector2 _scale2 = Vector2.One;
        private Vector3 _scaleInv = Vector3.One;
        private Point _size;
        private float _angle = 0;
        private SpriteEffects _effect = SpriteEffects.None;
        private bool _flipAndMirror;
        private XnaColor _color = XnaColor.White;
        private XnaColor _bgcolor = XnaColor.Black;
        private Texture2D _bgTex = null;

        /// <summary>
        /// Näyttölaite.
        /// </summary>
        internal GraphicsDevice device;

        /// <summary>
        /// Tekstuuri johon näkymä piirretään.
        /// </summary>
        internal RenderTarget2D RenderTarget
        {
            get
            {
                if ( _renderTarget == null )
                {
                    _renderTarget = new RenderTarget2D( device, _size.X, _size.Y, false, device.DisplayMode.Format, DepthFormat.Depth24Stencil8 );
                    Graphics.ResetScreenSize();
                }

                return _renderTarget;
            }
        }

        /// <summary>
        /// Alustaa uuden näyttönäkymän.
        /// </summary>
        /// <param name="device">XNA:n grafiikkalaite.</param>
        public ScreenView( GraphicsDevice device )
        {
            this.device = device;
            this.renderBatch = new SpriteBatch( device );
            this._bgTex = new Texture2D( device, 1, 1 );

            PresentationParameters pp = device.PresentationParameters;
            this._size = new Point( pp.BackBufferWidth, pp.BackBufferHeight );
        }

        /// <summary>
        /// Ruudulla näkyvä kuva.
        /// </summary>
        public Image Image
        {
            get { return new Image( RenderTarget ); }
        }

        /// <summary>
        /// Näytön taustakuva.
        /// </summary>
        public Image Background
        {
            get
            {
                return new Image( _bgTex );
            }
            set
            {
                // Clone ensures that the image doesn't change unexpectedly
                _bgTex = value.Clone().XNATexture;
            }
        }

        /// <summary>
        /// Ruudun "taustalla" näkyvä väri jos ruutua on kierretty
        /// tai se on pienempi kuin ikkuna.
        /// </summary>
        public Color BackgroundColor
        {
            get { return (Color)_bgcolor; }
            set
            {
                _bgcolor = (XnaColor)value;
                _bgTex = new Texture2D( RenderTarget.GraphicsDevice, 1, 1 );
                _bgTex.SetData<XnaColor>( new XnaColor[] { (XnaColor)value } );
            }
        }

        /// <summary>
        /// Näytön keskipiste.
        /// </summary>
        public Vector Center
        {
            get { return (Vector)_center; }
            set { _center = (Vector2)value; }
        }

        /// <summary>
        /// Kaikkea näytön sisältöä sävyttävä väri (valkoinen = normaali)
        /// </summary>
        public Color Color
        {
            get { return (Color)_color; }
            set { _color = (XnaColor)value; }
        }

        /// <summary>
        /// Peilataanko kuva pystysuunnassa.
        /// </summary>
        public bool IsFlipped
        {
            get { return _flipAndMirror || _effect == SpriteEffects.FlipVertically; }
            set
            {
                if ( IsMirrored )
                {
                    _effect = SpriteEffects.None;
                    _flipAndMirror = true;
                }
                else
                {
                    _effect = SpriteEffects.FlipVertically;
                    _flipAndMirror = false;
                }
            }
        }

        /// <summary>
        /// Peilataanko kuva vaakasuunnassa.
        /// </summary>
        public bool IsMirrored
        {
            get { return _flipAndMirror || _effect == SpriteEffects.FlipVertically; }
            set
            {
                if ( IsFlipped )
                {
                    _effect = SpriteEffects.None;
                    _flipAndMirror = true;
                }
                else
                {
                    _effect = SpriteEffects.FlipHorizontally;
                    _flipAndMirror = false;
                }
            }
        }

        /// <summary>
        /// Näytön kiertokulma.
        /// </summary>
        public Angle Angle
        {
            get { return Angle.FromRadians( -_angle ); }
            set { _angle = -(float)value.Radians; }
        }

        /// <summary>
        /// Näytön skaalaus.
        /// </summary>
        public Vector Scale
        {
            get { return new Vector( _scale.X, _scale.Y ); }
            set
            {
                _scale = new Vector3( (float)value.X, (float)value.Y, 1 );
                _scale2 = new Vector2( _scale.X, _scale.Y );
                _scaleInv = new Vector3( 1 / _scale.X, 1 / _scale.Y, 1 );
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
                _size.X = (int)value;
                _renderTarget = null;
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
                _size.Y = (int)value;
                _renderTarget = null;
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
                _size.X = (int)value.X;
                _size.Y = (int)value.Y;
                _renderTarget = null;
            }
        }

        /// <summary>
        /// Näytön todellinen leveys.
        /// </summary>
        public double ViewportWidth
        {
			get
            {
				return Game.GraphicsDeviceManager.PreferredBackBufferWidth;
            }
        }

        /// <summary>
        /// Näytön todellinen korkeus.
        /// </summary>
        public double ViewportHeight
        {
            get
            {
                return Game.GraphicsDeviceManager.PreferredBackBufferHeight;
            }
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
        internal static Vector FromXnaCoords( Vector2 position, Vector screenSize, Vector objectSize )
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
        internal static Vector2 ToXnaCoords( Vector position, Vector screenSize, Vector objectSize )
        {
            return new Vector2(
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
            var toJypeli   = Matrix.Invert(toXna);

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
            return Matrix.CreateScale( _scale )
                * Matrix.CreateRotationZ( _angle )
                * Matrix.CreateTranslation( -_center.X, -_center.Y, 0 );
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
            return Matrix.CreateScale( _scaleInv )
                * Matrix.CreateRotationZ( -_angle )
                * Matrix.CreateTranslation( _center.X, _center.Y, 0 );
        }

        internal void Render()
        {
            float angle = _flipAndMirror ? _angle + Microsoft.Xna.Framework.MathHelper.Pi : _angle;

            device.SetRenderTarget( null );
            device.Clear( _bgcolor );

            Matrix rotate = Matrix.CreateRotationZ(angle);
            Vector2 devorigin = new Vector2( device.Viewport.Width, device.Viewport.Height ) / 2;
            Vector2 rtorigin = new Vector2( RenderTarget.Width * _scale.X, RenderTarget.Height * _scale.Y ) / 2;
            Vector2 diff = Vector2.Transform( -rtorigin, rotate );
            var rectangle = new Microsoft.Xna.Framework.Rectangle( 0, 0, device.Viewport.Width, device.Viewport.Height );

            renderBatch.Begin( SpriteSortMode.Immediate, BlendState.AlphaBlend, Graphics.GetDefaultSamplerState(), DepthStencilState.None, RasterizerState.CullCounterClockwise, null );
            renderBatch.Draw( _bgTex, rectangle, XnaColor.White );
            renderBatch.Draw( RenderTarget, devorigin + diff + _center, null, _color, angle, Vector2.Zero, _scale2, _effect, 1 );
            renderBatch.End();
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

