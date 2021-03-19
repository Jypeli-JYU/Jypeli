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

namespace Jypeli
{
    /// <summary>
    /// Parametrit analogisen ohjauksen (hiiren tai ohjaustikun) tapahtumalle.
    /// Vanhentunut tapa, käytä mieluummin esim. Mouse.PositionOnWorld ja Mouse.MovementOnWorld
    /// </summary>
    public interface AnalogState
    {
        /// <summary>
        /// Peliohjaimen analoginäppäimen paikkakoordinaatti.
        /// Arvo on välillä 0.0 - 1.0.
        /// </summary>
        double State { get; }

        /// <summary>
        /// Muutos peliohjaimen analoginäppäimen paikassa.
        /// </summary>
        double AnalogChange { get; }

        /// <summary>
        /// Analogisen Ohjainsauvan paikka tai puhelimen asento.
        /// Arvo on (0, 0) kun sauva on keskellä tai puhelinta ei ole kallistettu.
        /// X- sekä Y-koordinaattien arvot ovat välillä -1.0 - 1.0.
        /// </summary>
        Vector StateVector { get; }

        /// <summary>
        /// Hiiren liikevektori.
        /// </summary>
        Vector MouseMovement { get; }
    }
}
