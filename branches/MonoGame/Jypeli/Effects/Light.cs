﻿#region MIT License
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

namespace Jypeli.Effects
{
#if !WINDOWS_PHONE
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
        /// Etäisyys kohtisuoraan 2D-tasosta. Mitä kauempana valo on,
        /// sitä laajemman alueen se valaisee.
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Voimakkuus väliltä [0.0, 1.0].
        /// </summary>
        public double Intensity { get; set; }

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

        public Light()
        {
            Distance = 10.0;
            Intensity = 0.5;
        }
    }
#endif
}
