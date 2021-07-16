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
 * Authors: Tero Jäntti.
 */

using System;

using Matrix = System.Numerics.Matrix4x4;

namespace Jypeli
{
    /// <summary>
    /// Piirtoalusta.
    /// </summary>
    public class Canvas
    {
        Image previousImage = null;
        internal Matrix worldMatrix;

        /// <summary>
        /// Vasen reuna.
        /// </summary>
        public double Left { get; private set; }

        /// <summary>
        /// Oikea reuna.
        /// </summary>
        public double Right { get; private set; }

        /// <summary>
        /// Alareuna.
        /// </summary>
        public double Bottom { get; private set; }

        /// <summary>
        /// Yläreuna.
        /// </summary>
        public double Top { get; private set; }

        /// <summary>
        /// Vasen ylänurkka.
        /// </summary>
        public Vector TopLeft { get; private set; }

        /// <summary>
        /// Oikea ylänurkka.
        /// </summary>
        public Vector TopRight { get; private set; }

        /// <summary>
        /// Vasen alanurkka.
        /// </summary>
        public Vector BottomLeft { get; private set; }

        /// <summary>
        /// Oikea alanurkka.
        /// </summary>
        public Vector BottomRight { get; private set; }

        /// <summary>
        /// Pensselin väri.
        /// </summary>
        public Color BrushColor { get; set; }

        internal Canvas()
        {
            Reset();
        }

        /// <summary>
        /// Aloittaa canvaksen piirtämisen.
        /// Sinun ei tarvitse kutsua tätä.
        /// </summary>
        /// <param name="worldMatrix"></param>
        /// <param name="dimensionSource"></param>
        public void Begin( ref Matrix worldMatrix, Dimensional dimensionSource )
        {
            Graphics.LineBatch.Begin( ref worldMatrix );
            Graphics.ImageBatch.Begin( ref worldMatrix, null );
            Graphics.Canvas.Reset( dimensionSource );
            this.worldMatrix = worldMatrix;
        }
        
        /// <summary>
        /// Lopettaa piirtämisen.
        /// Sinun ei tarvitse kutsua tätä.
        /// </summary>
        public void End()
        {
            Graphics.ImageBatch.End();
            Graphics.LineBatch.End();
        }

        private void Reset()
        {
            BrushColor = Color.Black;
            previousImage = null;
        }

        internal void Reset( Dimensional dimensionSource )
        {
            Reset();
            Left = dimensionSource.Left;
            Right = dimensionSource.Right;
            Bottom = dimensionSource.Bottom;
            Top = dimensionSource.Top;
            TopLeft = new Vector( Left, Top );
            TopRight = new Vector( Right, Top );
            BottomLeft = new Vector( Left, Bottom );
            BottomRight = new Vector( Right, Bottom );
        }

        /// <summary>
        /// Piirtää janan.
        /// </summary>
        /// <param name="startPoint">Alkupiste</param>
        /// <param name="endPoint">Loppupiste</param>
        public void DrawLine( Vector startPoint, Vector endPoint )
        {
            Graphics.LineBatch.Draw( startPoint, endPoint, BrushColor );
        }

        /// <summary>
        /// Piirtää janan.
        /// </summary>
        /// <param name="x1">Alkupisteen x-koordinaatti</param>
        /// <param name="y1">Alkupisteen y-koordinaatti</param>
        /// <param name="x2">Loppupisteen x-koordinaatti</param>
        /// <param name="y2">Loppupisteen y-koordinaatti</param>
        public void DrawLine( double x1, double y1, double x2, double y2 )
        {
            Graphics.LineBatch.Draw( new Vector( x1, y1 ), new Vector( x2, y2 ), BrushColor );
        }

        /// <summary>
        /// Piirtää kuvan.
        /// </summary>
        /// <param name="point">Koordinaatti johon piirretään</param>
        /// <param name="image">Kuva</param>
        /// <param name="scale">Skaalaus (1x1 normaalikoko)</param>
        /// <param name="angle">Kiertokulma</param>
        public void DrawImage( Vector point, Image image, Vector scale, Angle angle )
        {
            if ( !Object.ReferenceEquals( image, previousImage ) )
            {
                // Start a new batch with different image
                Graphics.ImageBatch.End();
                Graphics.ImageBatch.Begin( ref worldMatrix, image.XNATexture );
                previousImage = image;
            }

            Vector scaleV = new Vector( (float)( image.Width * scale.X ), (float)( image.Height * scale.Y ) );
            Graphics.ImageBatch.Draw( Graphics.DefaultTextureCoords, point, scaleV, (float)angle.Radians );
        }

        /// <summary>
        /// Piirtää kuvan.
        /// </summary>
        /// <param name="point">Koordinaatti johon piirretään</param>
        /// <param name="image">Kuva</param>
        public void DrawImage( Vector point, Image image )
        {
            DrawImage( point, image, Vector.Diagonal, Angle.Zero );
        }
    }
}
