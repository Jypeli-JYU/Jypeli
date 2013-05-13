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

namespace Jypeli
{
    /// <summary>
    /// Piirtoalusta.
    /// </summary>
    public class Canvas
    {
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

        private void Reset()
        {
            BrushColor = Color.Black;
        }

        internal void Reset( Level level )
        {
            Reset();
            Left = level.Left;
            Right = level.Right;
            Bottom = level.Bottom;
            Top = level.Top;
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
    }
}
