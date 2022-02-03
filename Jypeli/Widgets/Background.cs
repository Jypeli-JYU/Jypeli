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
 * Authors: Tomi Karppinen, Tero Jäntti
 */

using System;
using Jypeli.Rendering;
using Matrix = System.Numerics.Matrix4x4;

namespace Jypeli.Widgets
{
    /// <summary>
    /// Taustakuva
    /// </summary>
    public class Background : Widget
    {
        /// <inheritdoc/>
        public override Animation Animation
        {
            get { return base.Animation; }
            set
            {
                base.Animation = value;
                if ( value != null )
                    Size = new Vector( value.Width, value.Height );
            }
        }

        TextureCoordinates tx = new TextureCoordinates();

        /// <summary>
        /// Liikkuuko taustakuva kameran mukana vai ei.
        /// </summary>
        /// <value><c>true</c> jos liikkuu; muuten <c>false</c>.</value>
        public bool MovesWithCamera { get; set; }

        /// <summary>
        /// Taustakuvan skaala.
        /// <example>
        /// Jos halutaan kaksinkertainen kuva, niin laitetaan skaalaksi 2, puolet pienemmän kuvan skaala on vastaavasti 0.5.
        /// </example>
        /// </summary>
        /// <value>The scale.</value>
        public double Scale { get; set; }

        /// <summary>
        /// Sovittaa taustakuvan kentän kokoiseksi.
        /// </summary>
        public void FitToLevel()
        {
            Level level = Game.Instance.Level;
            Size = new Vector( level.Width, level.Height );
            TextureWrapSize = Vector.Diagonal;
        }

        /// <summary>
        /// Toistaa kuvaa vaaka- ja pystysuunnassa niin että kuvio peittää koko kentän.
        /// </summary>
        public void TileToLevel()
        {
            Level level = Game.Instance.Level;
            Size = level.Size;
            TextureWrapSize = new Vector( level.Width / Image.Width, level.Height / Image.Height );

            tx.TopLeft = new Vector(0, 0);
            tx.TopRight = new Vector(TextureWrapSize.X, 0);
            tx.BottomLeft = new Vector(0, TextureWrapSize.Y);
            tx.BottomRight = new Vector(TextureWrapSize.X, TextureWrapSize.Y);

            Game.GraphicsDevice.SetTextureToRepeat(Image);
        }

        /// <summary>
        /// Sovittaa taustakuvan kentän kokoiseksi pitäen kuvasuhteen.
        /// Kuva sovitetaan niin, että se ei ylitä kentän rajoja.
        /// </summary>
        public void ScaleToLevel()
        {
            Level level = Game.Instance.Level;
            Size *= Math.Min( level.Width / this.Width, level.Height / this.Height );
        }

        /// <summary>
        /// Sovittaa taustakuvan kentän kokoiseksi pitäen kuvasuhteen.
        /// Kuva sovitetaan niin, että se täyttää koko kentän ja osa rajautuu kentän ulkopuolelle.
        /// </summary>
        public void ScaleToLevelFull()
        {
            Level level = Game.Instance.Level;
            Size *= Math.Max( level.Width / this.Width, level.Height / this.Height );
        }

        /// <summary>
        /// Sovittaa taustakuvan kentän kokoiseksi pitäen kuvasuhteen.
        /// Kuva sovitetaan niin, että se täyttää kentän vaakasuunnassa.
        /// </summary>
        public void ScaleToLevelByWidth()
        {
            Level level = Game.Instance.Level;
            Size *= level.Width / this.Width;
        }

        /// <summary>
        /// Sovittaa taustakuvan kentän kokoiseksi pitäen kuvasuhteen.
        /// Kuva sovitetaan niin, että se täyttää kentän pystysuunnassa.
        /// </summary>
        public void ScaleToLevelByHeight()
        {
            Level level = Game.Instance.Level;
            Size *= level.Height / this.Height;
        }

        /// <summary>
        /// Luo uuden taustakuvan.
        /// </summary>
        /// <param name="width">Taustakuvan leveys.</param>
        /// /// <param name="height">Taustakuvan korkeus.</param>
        public Background( double width, double height )
            : base( width, height )
        {
            this.Size = new Vector( width, height );
            this.Scale = 1.0f;
            this.MovesWithCamera = true;
        }
        
        /// <summary>
        /// Luo uuden taustakuvan.
        /// </summary>
        /// <param name="size">Taustakuvan koko.</param>
        public Background( Vector size )
            : this( size.X, size.Y )
        {
        }
        
        /// <summary>
        /// Luo avaruustaustakuvan.
        /// </summary>
        /// <param name="amount">Tähtien määrä.</param>
        /// <returns>Kuva.</returns>
        public Image CreateStars( int amount )
        {
            int imageWidth = (int)Game.Screen.Width;
            int imageHeight = (int)Game.Screen.Height;

            Image image = Image.CreateStarSky( imageWidth, imageHeight, amount );

            Image = image;
            MovesWithCamera = false;

            return image;
        }

        /// <summary>
        /// Luo avaruustaustakuvan.
        /// </summary>
        /// <returns>Kuva.</returns>
        public Image CreateStars()
        {
            int textureWidth = (int)Game.Screen.Width;
            int textureHeight = (int)Game.Screen.Height;
            int amount = ( textureWidth * textureHeight ) / 1000;

            Image image = this.CreateStars( amount );

            Image = image;
            MovesWithCamera = false;

            return image;
        }

        /// <summary>
        /// Luo liukuväritaustan taustakuvaksi.
        /// </summary>
        /// <param name="lowerColor">Alempi väri.</param>
        /// <param name="upperColor">Ylempi väri.</param>
        /// <returns>Kuva.</returns>
        public Image CreateGradient( Color lowerColor, Color upperColor )
        {
            int textureWidth = (int)Game.Screen.Width;
            int textureHeight = (int)Game.Screen.Height;
            int amount = ( textureWidth * textureHeight ) / 800;

            Image image = Image.FromGradient( textureWidth, textureHeight, lowerColor, upperColor );

            Image = image;
            MovesWithCamera = false;

            return image;
        }

        /// <inheritdoc/>
        public override void Draw( Matrix parentTransformation, Matrix transformation )
        {
            if (Image == null) return;

            if (MovesWithCamera)
            {
                Matrix matrix =
                    transformation
                    * parentTransformation;

                Graphics.ImageBatch.Begin(ref matrix, Image);

                Graphics.ImageBatch.Draw(tx, Position, Size, 0);
                Graphics.ImageBatch.End();
            }
        }
    }
}
