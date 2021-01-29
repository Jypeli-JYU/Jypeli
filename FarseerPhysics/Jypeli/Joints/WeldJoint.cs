#region MIT License
/*
 * Copyright (c) 2021 University of Jyväskylä, Department of Mathematical
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
 * Author: Mikko Röyskö
 */

using System;

using FSJoint = FarseerPhysics.Dynamics.Joints.WeldJoint;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;
using Jypeli.Farseer;

namespace Jypeli
{
    /// <summary>
    /// Jöykkä liitos kahden olion välille.
    /// Liitetyt oliot eivät voi liikkua tai pyöriä toistensa suhteen.
    /// </summary>
    public class WeldJoint : AbstractJoint
    {
        /// <summary>
        /// Pyörimisakselin (tämänhetkiset) koordinaatit.
        /// </summary>
        public override Vector AxlePoint
        {
            get
            {
                return (Object1.Position + Object2.Position) / 2; // TODO: Pitäisikö oikeastaan laskea painopiste?
            }
        }

        internal FSJoint InnerJoint
        {
            get
            {
                return (FSJoint)innerJoint;
            }
            set
            {
                innerJoint = value;
            }

        }

        /// <summary>
        /// Ei käytössä, liitos ei jousta.
        /// </summary>
        public override double Softness
        {
            get
            {
                return 0;
            }
            set { }
        }

        /// <summary>
        /// Luo uuden hitsausliitoksen kahden olion välille.
        /// </summary>
        /// <param name="firstObject">Ensimmäinen olio</param>
        /// <param name="secondObject">Toinen olio</param>
        /// <param name="axlePosition">Liitoksen akselin paikka</param>
        public WeldJoint(PhysicsObject firstObject, PhysicsObject secondObject, Vector axlePosition)
        {
            World world = PhysicsGame.Instance.Engine as World;
            var first = firstObject.Body as PhysicsBody;
            var second = secondObject.Body as PhysicsBody;
            InnerJoint = JointFactory.CreateWeldJoint(world, first.FSBody, second.FSBody, axlePosition * FSConvert.DisplayToSim, Vector.Zero);
            Object1 = firstObject;
            Object2 = secondObject;
        }

        /// <summary>
        /// Luo uuden hitsausliitoksen kahden olion välille.
        /// Liitos sijoitetaan toisen olion keskipisteeseen.
        /// </summary>
        /// <param name="firstObject">Ensimmäinen olio</param>
        /// <param name="secondObject">Toinen olio</param>
        public WeldJoint(PhysicsObject firstObject, PhysicsObject secondObject)
        {
            World world = PhysicsGame.Instance.Engine as World;
            var first = firstObject.Body as PhysicsBody;
            var second = secondObject.Body as PhysicsBody;
            InnerJoint = JointFactory.CreateWeldJoint(world, first.FSBody, second.FSBody, secondObject.Position * FSConvert.DisplayToSim, firstObject.Position*FSConvert.DisplayToSim);
            Object1 = firstObject;
            Object2 = secondObject;
        }
    }
}
