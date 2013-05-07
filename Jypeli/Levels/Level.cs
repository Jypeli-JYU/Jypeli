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

using System;
using System.Linq;
using System.Collections.Generic;
using AdvanceMath;

namespace Jypeli
{
    /// <summary>
    /// Pelikenttä, johon voi lisätä olioita. Kentällä voi myös olla reunat ja taustaväri tai taustakuva.
    /// </summary>
    [Save]
    public class Level
    {
        [Save] double _width = 1000;
        [Save] double _height = 800;

        private Game game;

        /// <summary>
        /// Kentän keskipiste.
        /// </summary>
        public readonly Vector Center = Vector.Zero;

        /// <summary>
        /// Kentän taustaväri.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Kentän leveys.
        /// </summary>
        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// Kentän korkeus.
        /// </summary>
        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Kentän koko (leveys ja korkeus).
        /// </summary>
        public Vector Size
        {
            get { return new Vector( _width, _height ); }
            set { _width = value.X; _height = value.Y; }
        }

        /// <summary>
        /// Kentän vasemman reunan x-koordinaatti.
        /// </summary>
        public double Left
        {
            get { return -Width / 2; }
        }

        /// <summary>
        /// Kentän oikean reunan x-koordinaatti.
        /// </summary>
        public double Right
        {
            get { return Width / 2; }
        }

        /// <summary>
        /// Kentän yläreunan y-koordinaatti.
        /// </summary>
        public double Top
        {
            get { return Height / 2; }
        }

        /// <summary>
        /// Kentän alareunan y-koordinaatti.
        /// </summary>
        public double Bottom
        {
            get { return -Height / 2; }
        }

        internal Level( Game game )
        {
            this.game = game;
            BackgroundColor = Color.LightBlue; // default color
        }

        internal void Clear()
        {
        }
    }
}
