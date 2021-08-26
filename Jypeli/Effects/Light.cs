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


using System;
using System.Collections.Generic;
using System.Numerics;
using Jypeli.Rendering;

namespace Jypeli.Effects
{
    /// <summary>
    /// Pistemäinen valonlähde.
    /// </summary>
    public class Light
    {
        /// <summary>
        /// Paikka.
        /// </summary>
        public Vector Position { get; set; }

        /// <summary>
        /// Kuinka suuren ympyränmuotoisen alueen valo valaisee
        /// </summary>
        public double Radius { get; set; } // TODO: Tämä säde on nyt hieman hämäävä.

        /// <summary>
        /// Voimakkuus väliltä [0.0, 1.0].
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// Väri
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Paikan X-koordinaatti.
        /// </summary>
        public double X
        {
            get
            {
                return Position.X;
            }
            set
            {
                Position = new Vector( value, Position.Y );
            }
        }

        /// <summary>
        /// Paikan Y-koordinaatti.
        /// </summary>
        public double Y
        {
            get
            {
                return Position.Y;
            }
            set
            {
                Position = new Vector( Position.X, value );
            }
        }

        /// <summary>
        /// Valo.
        /// </summary>
        public Light()
        {
            Radius = 1.0;
            Intensity = 1;
            Color = Color.Red;
        }
    }
}
