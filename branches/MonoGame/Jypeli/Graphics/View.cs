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
using AdvanceMath;

using XnaColor = Microsoft.Xna.Framework.Color;
using XnaHelper = Microsoft.Xna.Framework.MathHelper;

namespace Jypeli
{
    /// <summary>
    /// Sisältää näytön leveyden ja korkeuden sekä reunojen koordinaatit.
    /// Y-koordinaatti kasvaa ylöspäin.
    /// Koordinaatteja ei voi muuttaa.
    /// </summary>
    public class ScreenView
    {
        private SpriteBatch renderBatch;
        private Vector2 _center = Vector2.Zero;
        private float _angle = 0;
        private SpriteEffects _effect = SpriteEffects.None;
        private bool _flipAndMirror;
        private XnaColor _color = XnaColor.White;

        internal GraphicsDevice device;
        internal RenderTarget2D renderTarget;

        /// <summary>
        /// Alustaa uuden näyttönäkymän.
        /// </summary>
        /// <param name="viewPort">Näytön viewport.</param>
        public ScreenView( GraphicsDevice device )
        {
            this.device = device;
            this.renderBatch = new SpriteBatch( device );

            PresentationParameters pp = device.PresentationParameters;
            this.renderTarget = new RenderTarget2D( device, pp.BackBufferWidth, pp.BackBufferHeight, false, device.DisplayMode.Format, DepthFormat.Depth24Stencil8 );
        }

        /// <summary>
        /// Ruudulla näkyvä kuva.
        /// </summary>
        public Image Image
        {
            get { return new Image( renderTarget ); }
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
            get { return Angle.FromRadians( _angle ); }
            set { _angle = (float)value.Radians; }
        }

        /// <summary>
        /// Näytön leveys x-suunnassa.
        /// </summary>
        public double Width
        {
            get { return renderTarget.Width; }
            set
            {
                renderTarget = new RenderTarget2D( device, (int)value, renderTarget.Height );
            }
        }

        /// <summary>
        /// Näytön korkeus y-suunnassa.
        /// </summary>
        public double Height
        {
            get { return renderTarget.Height; }
            set
            {
                renderTarget = new RenderTarget2D( device, (int)value, renderTarget.Height );
            }
        }

        /// <summary>
        /// Näytön koko vektorina.
        /// </summary>
        public Vector Size
        {
            get { return new Vector( renderTarget.Width, renderTarget.Height ); }
            set
            {
                renderTarget = new RenderTarget2D( device, (int)value.X, (int)value.Y );
            }
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

        public double LeftSafe { get { return Left + 10; } }
        public double RightSafe { get { return Right - 10; } }
        public double BottomSafe { get { return Bottom + 10; } }
        public double TopSafe { get { return Top + 10; } }

        public double WidthSafe { get { return RightSafe - LeftSafe; } }
        public double HeightSafe { get { return TopSafe - BottomSafe; } }

        /// <summary>
        /// Muuntaa XNA:n ruutukoordinaateista Jypelin ruutukoordinaateiksi.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="objectSize"></param>
        /// <returns></returns>
        internal Vector FromXnaCoords( Vector2 position, Vector objectSize )
        {
            double x = -renderTarget.Width / 2 + objectSize.X / 2 + position.X;
            double y = renderTarget.Height / 2 - objectSize.Y / 2 - position.Y;
            return new Vector( x, y );
        }

        private float xToXna( float x, float w )
        {
            return renderTarget.Width / 2 - w / 2 + x;
        }

        private float yToXna( float y, float h )
        {
            return renderTarget.Height / 2 - h / 2 - y;
        }

        /// <summary>
        /// Muuntaa Jypelin ruutukoordinaateista XNA:n ruutukoordinaateiksi.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="objectSize"></param>
        /// <returns></returns>
        internal Vector2 ToXnaCoords( Vector position, Vector objectSize )
        {
            return new Vector2( xToXna( (float)position.X, (float)objectSize.X ), yToXna( (float)position.Y, (float)objectSize.Y ) );
        }        

        /// <summary>
        /// Muuntaa matriisin Jypelin ruutukoordinaateista XNA:n ruutukoordinaatteihin.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        internal Matrix ToXnaCoords( Matrix matrix, Vector scale )
        {
            float xnamat_M41 = xToXna( matrix.M41, matrix.M11 * (float)scale.X );
            float xnamat_M42 = yToXna( matrix.M42, matrix.M22 * (float)scale.Y );

            return new Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                xnamat_M41, xnamat_M42, matrix.M43, matrix.M44
            );
        }

        internal void Render( Color bgColor )
        {
            float angle = _flipAndMirror ? _angle + Microsoft.Xna.Framework.MathHelper.Pi : _angle;

            device.SetRenderTarget( null );
            device.Clear( bgColor.AsXnaColor() );
            //device.Clear( ClearOptions.Target | ClearOptions.DepthBuffer, XnaColor.DarkSlateBlue, 1.0f, 0 );

            renderBatch.Begin( SpriteSortMode.Immediate, BlendState.Opaque );
            renderBatch.Draw( renderTarget, _center, null, _color, angle, Vector2.Zero, 1, _effect, 1 );
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

