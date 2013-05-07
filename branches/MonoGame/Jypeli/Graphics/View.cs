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

namespace Jypeli
{
    /// <summary>
    /// Sisältää näytön leveyden ja korkeuden sekä reunojen koordinaatit.
    /// Y-koordinaatti kasvaa ylöspäin.
    /// Koordinaatteja ei voi muuttaa.
    /// </summary>
    public class ScreenView
    {
        internal Viewport viewPort;

        /// <summary>
        /// Alustaa uuden näyttönäkymän.
        /// </summary>
        /// <param name="viewPort">Näytön viewport.</param>
        public ScreenView( Viewport viewPort )
        {
            this.viewPort = viewPort;
        }

        /// <summary>
        /// Näytön keskipiste.
        /// </summary>
        public Vector Center
        {
            get { return Vector.Zero; }
        }

        /// <summary>
        /// Näytön leveys x-suunnassa.
        /// </summary>
        public double Width
        {
            get { return viewPort.Width; }
        }

        /// <summary>
        /// Näytön korkeus y-suunnassa.
        /// </summary>
        public double Height
        {
            get { return viewPort.Height; }
        }

        /// <summary>
        /// Näytön koko vektorina.
        /// </summary>
        public Vector Size
        {
            get { return new Vector( viewPort.Width, viewPort.Height ); }
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
        /// Näytön "turvallinen" ts. laiteriippumaton leveys x-suunnassa.
        /// </summary>
        public double WidthSafe
        {
            get { return viewPort.TitleSafeArea.Width; }
        }

        /// <summary>
        /// Näytön "turvallinen" ts. laiteriippumaton korkeus y-suunnassa.
        /// </summary>
        public double HeightSafe
        {
            get { return viewPort.TitleSafeArea.Height; }
        }

        /// <summary>
        /// Näytön vasemman reunan "turvallinen" ts. laiteriippumaton x-koordinaatti.
        /// </summary>
        public double LeftSafe
        {
            get { return -viewPort.TitleSafeArea.Width / 2; }
        }

        /// <summary>
        /// Näytön oikean reunan "turvallinen" ts. laiteriippumaton x-koordinaatti.
        /// </summary>
        public double RightSafe
        {
            get { return viewPort.TitleSafeArea.Width / 2; }
        }

        /// <summary>
        /// Näytön yläreunan "turvallinen" ts. laiteriippumaton y-koordinaatti.
        /// </summary>
        public double TopSafe
        {
            get { return viewPort.TitleSafeArea.Height / 2; }
        }

        /// <summary>
        /// Näytön alareunan "turvallinen" ts. laiteriippumaton y-koordinaatti.
        /// </summary>
        public double BottomSafe
        {
            get { return -viewPort.TitleSafeArea.Height / 2; }
        }

        internal Vector FromXnaScreenCoordinates(Vector2 position)
        {
            double x = position.X - (viewPort.Width / 2);
            double y = -position.Y + (viewPort.Height / 2);
            return new Vector(x, y);
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

