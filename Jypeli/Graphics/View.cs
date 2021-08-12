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

using Matrix = System.Numerics.Matrix4x4;
using Jypeli.Rendering;
using System;
using System.Drawing.Drawing2D;

namespace Jypeli
{
    /// <summary>
    /// Sisältää näytön leveyden ja korkeuden sekä reunojen koordinaatit.
    /// Y-koordinaatti kasvaa ylöspäin.
    /// Koordinaatteja ei voi muuttaa.
    /// </summary>
    public class ScreenView : Dimensional
    {
        private IRenderTarget _renderTarget = null;
        //private SpriteBatch renderBatch;
        private Vector2D<float> _center = Vector2D<float>.Zero;
        private Vector3 _scale = Vector3.One;
        private Vector _scale2 = Vector.One;
        private Vector3 _scaleInv = Vector3.One;
        private Vector _size;
        private float _angle = 0;
        //private SpriteEffects _effect = SpriteEffects.None;
        private bool _flipAndMirror;
        private Color _color = Color.White;
        private Color _bgcolor = Color.Black;
        //private Texture2D _bgTex = null;

        /// <summary>
        /// Tekstuuri johon näkymä piirretään.
        /// </summary>
        internal IRenderTarget RenderTarget
        {
            get
            {
                if ( _renderTarget == null )
                {
                    _renderTarget = Game.GraphicsDevice.CreateRenderTarget((uint)_size.X, (uint)_size.Y);
                    Graphics.ResetScreenSize();
                }

                return _renderTarget;
            }
        }

        /// <summary>
        /// Alustaa uuden näyttönäkymän.
        /// </summary>
        /// <param name="device">XNA:n grafiikkalaite.</param>
        public ScreenView()
        {
            //this.renderBatch = new SpriteBatch();
            //this._bgTex = new Texture2D( device, 1, 1 );
            //
            //PresentationParameters pp = device.PresentationParameters;
            this._size = new Vector(Game.Instance.window.Size.X, Game.Instance.window.Size.Y);
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
            get { return (Color)_bgcolor; }
            set
            {
                _bgcolor = value;
                //_bgTex = new Texture2D( RenderTarget.GraphicsDevice, 1, 1 );
                //_bgTex.SetData<Color>( new Color[] { value } );
            }
        }

        /// <summary>
        /// Näytön keskipiste.
        /// </summary>
        public Vector2D<float> Center
        {
            get { return _center; }
            set { _center = value; }
        }

        /// <summary>
        /// Kaikkea näytön sisältöä sävyttävä väri (valkoinen = normaali)
        /// </summary>
        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        /// <summary>
        /// Peilataanko kuva pystysuunnassa.
        /// </summary>
        public bool IsFlipped
        {
            get { return _flipAndMirror /*|| _effect == SpriteEffects.FlipVertically*/; }
            set
            {
                if ( IsMirrored )
                {
                    /*_effect = SpriteEffects.None;*/
                    _flipAndMirror = true;
                }
                else
                {
                    /*_effect = SpriteEffects.FlipVertically;*/
                    _flipAndMirror = false;
                }
            }
        }

        /// <summary>
        /// Peilataanko kuva vaakasuunnassa.
        /// </summary>
        public bool IsMirrored
        {
            get { return _flipAndMirror /* || _effect == SpriteEffects.FlipVertically*/; }
            set
            {
                if ( IsFlipped )
                {
                   // _effect = SpriteEffects.None;
                    _flipAndMirror = true;
                }
                else
                {
                    //_effect = SpriteEffects.FlipHorizontally;
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
                _scale2 = new Vector( _scale.X, _scale.Y );
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
                _renderTarget?.Dispose();
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
                _renderTarget?.Dispose();
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
                Game.Instance.window.Size = new Vector2D<int>((int)value.X, (int)value.Y);
                _size.X = (int)value.X;
                _size.Y = (int)value.Y;
                _renderTarget?.Dispose();
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
                return Game.Instance.window.Size.X;
            }
        }

        /// <summary>
        /// Näytön todellinen korkeus.
        /// </summary>
        public double ViewportHeight
        {
            get
            {
                return Game.Instance.window.Size.Y;
            }
        }

        internal void Resize(Vector newSize)
        {
            _renderTarget?.Dispose();
            _renderTarget = null;
            _size = newSize;
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
            get { return 0; }
        }

        /// <summary>
        /// Näytön oikean reunan x-koordinaatti.
        /// </summary>
        public double Right
        {
            get { return Width; }
        }

        /// <summary>
        /// Näytön yläreunan y-koordinaatti.
        /// </summary>
        public double Top
        {
            get { return 0; }
        }

        /// <summary>
        /// Näytön alareunan y-koordinaatti.
        /// </summary>
        public double Bottom
        {
            get { return Height; }
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
            float angle = _flipAndMirror ? _angle + MathHelper.Pi : _angle;

            Game.GraphicsDevice.SetRenderTarget(null);
            Game.GraphicsDevice.Clear(Game.Instance.Level.BackgroundColor);

            VertexPositionColorTexture[] textureVertices = new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(new Vector3(-1f, 1f, 0), Color.White, new Vector(0f, 1f)),
                new VertexPositionColorTexture(new Vector3(-1f, -1f, 0), Color.White, new Vector(0f, 0f)),
                new VertexPositionColorTexture(new Vector3(1f, -1f, 0), Color.White, new Vector(1f, 0f)),

                new VertexPositionColorTexture(new Vector3(-1f, 1f, 0), Color.White, new Vector(0f, 1f)),
                new VertexPositionColorTexture(new Vector3(1f, -1f, 0), Color.White, new Vector(1f, 0f)),
                new VertexPositionColorTexture(new Vector3(1f, 1f, 0), Color.White, new Vector(1f, 1f))
            };
            RenderTarget.BindTexture();

            Graphics.BasicTextureShader.Use();
            Graphics.BasicTextureShader.SetUniform("world", Matrix.Identity);

            Game.GraphicsDevice.DrawPrimitives(PrimitiveType.OpenGlTriangles, textureVertices, 6, true);
            RenderTarget.UnBindTexture();
            /*
            Matrix rotate = Matrix.CreateRotationZ(angle);
            Vector devorigin = new Vector( device.Viewport.Width, device.Viewport.Height ) / 2;
            Vector rtorigin = new Vector( RenderTarget.Width * _scale.X, RenderTarget.Height * _scale.Y ) / 2;
            Vector diff = Vector.Transform( -rtorigin, rotate );
            var rectangle = new Rectangle<float>(new Vector2D<float>(0, 0), new Vector2D<float>(device.Viewport.Width, device.Viewport.Height));
            
            renderBatch.Begin( SpriteSortMode.Immediate, BlendState.AlphaBlend, Graphics.GetDefaultSamplerState(), DepthStencilState.None, RasterizerState.CullCounterClockwise, null );
            renderBatch.Draw( _bgTex, rectangle, Color.White );
            renderBatch.Draw( RenderTarget, devorigin + diff + _center, null, _color, angle, Vector.Zero, _scale2, _effect, 1 );
            renderBatch.End();*/
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

