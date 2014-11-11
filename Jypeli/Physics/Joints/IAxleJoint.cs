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
 * Authors: Vesa Lappalainen, Tero Jäntti, Tomi Karppinen.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvanceMath;

namespace Jypeli
{
    public interface IAxleJoint : Destroyable, IDisposable
    {
        /// <summary>
        /// Ensimmäinen olio.
        /// </summary>
        PhysicsObject Object1 { get; }

        /// <summary>
        /// Toinen olio (null jos ensimmäinen olio on sidottu pisteeseen)
        /// </summary>
        PhysicsObject Object2 { get; }

        /// <summary>
        /// Pyörimisakselin (tämänhetkiset) koordinaatit.
        /// </summary>
        Vector AxlePoint { get; }

        /// <summary>
        /// Liitoksen pehmeys eli kuinka paljon sillä on liikkumavaraa.
        /// </summary>
        double Softness { get; set; }
    }
}
