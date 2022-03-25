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
using System.ComponentModel;

namespace Jypeli
{
    /// <summary>
    /// Käytössä olevat liitostyypit
    /// </summary>
    public enum JointTypes
    {
        /// <summary>
        /// Akseliliitos, kappaleet pysyvät tietyllä etäisyydellä toisistaan mutta voivat pyöriä
        /// </summary>
        AxleJoint,
        /// <summary>
        /// Hitsausliitos, kappaleet eivät liiku toistensa suhteen.
        /// </summary>
        WeldJoint,
        /// <summary>
        /// Liitos renkaita varten, sisältää moottorin joka voi pyörittää renkaita.
        /// </summary>
        WheelJoint
    }

    // TODO: Ehkä tämä pitäisi nimetä uudestaan, IJoint?
    /// <summary>
    /// Akseliliitos
    /// </summary>
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

        /// <summary>
        /// Asettaa liitokselle fysiikkamoottorin
        /// </summary>
        /// <param name="engine"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void SetEngine(Jypeli.Physics.IPhysicsEngine engine);

        /// <summary>
        /// Lisää liitoksen fysiikkamoottorille
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void AddToEngine();
    }
}
