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

using Physics2DDotNet;
namespace Jypeli
{
    public partial class PhysicsBody
    {
        internal static readonly Coefficients DefaultCoefficients = new Coefficients( 0.5, 0.4, 0.4 );

        /// <summary>
        /// Lepokitka. Liikkeen alkamista vastustava voima, joka ilmenee kun olio yrittää lähteä liikkeelle
        /// toisen olion pinnalta (esim. laatikkoa yritetään työntää eteenpäin).
        /// </summary>
        public double StaticFriction
        {
            get { return Body.Coefficients.StaticFriction; }
            set { Body.Coefficients.StaticFriction = value; }
        }

        /// <summary>
        /// Liikekitka. Liikettä vastustava voima joka ilmenee kun kaksi oliota liikkuu toisiaan vasten
        /// (esim. laatikko liukuu maata pitkin). Arvot välillä 0.0 (ei kitkaa) ja 1.0 (täysi kitka).
        /// </summary>
        public double KineticFriction
        {
            get { return Body.Coefficients.DynamicFriction; }
            set { Body.Coefficients.DynamicFriction = value; }
        }

        /// <summary>
        /// Olion kimmoisuus. Arvo välillä 0.0-1.0.
        /// Arvolla 1.0 olio säilyttää kaiken vauhtinsa törmäyksessä. Mitä pienempi arvo,
        /// sitä enemmän olion vauhti hidastuu törmäyksessä.
        /// </summary>
        public double Restitution
        {
            get { return Body.Coefficients.Restitution; }
            set { Body.Coefficients.Restitution = value; }
        }
    }
}
