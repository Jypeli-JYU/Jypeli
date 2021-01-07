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

using FSJoint = FarseerPhysics.Dynamics.Joints.WheelJoint;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;

namespace Jypeli
{
    /// <summary>
    /// Rengasliitos kahden olion välille. 
    /// Voidaan käyttää esimerkiksi auton renkaiden luomiseen
    /// </summary>
    public class WheelJoint : AbstractJoint
    {
        /// <summary>
        /// Pyörimisakselin (tämänhetkiset) koordinaatit.
        /// </summary>
        public override Vector AxlePoint
        {
            get
            {
                return (Object1.Position + Object2.Position) / 2;
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
        /// Liitoksen joustavuus.
        /// ks. myös <c>DampingRatio</c>
        /// </summary>
        public override double Softness
        {
            get { return InnerJoint.Frequency; } // On muuten hyvin hämäävä nimi sisäisesti.
            set { InnerJoint.Frequency = (float)value; }
        }

        /// <summary>
        /// Liitoksen oskillaation vaimennuskerroin, väliltä 0-1;
        /// ks. myös <c>Softness</c>
        /// </summary>
        public double DampingRatio
        {
            get { return InnerJoint.DampingRatio; }
            set { InnerJoint.DampingRatio = (float)value; }
        }

        /// <summary>
        /// Onko liitoksen sisältämä moottori päällä.
        /// Päällä ollessa moottori yrittää pyörittää kappaletta <c>MotorSpeed</c> kentässä asetetulla nopeudella.
        /// </summary>
        public bool MotorEnabled
        {
            get { return InnerJoint.MotorEnabled; }
            set { InnerJoint.MotorEnabled = value; }
        }

        /// <summary>
        /// Moottorin pyörimisnopeus.
        /// </summary>
        public double MotorSpeed
        {
            get { return InnerJoint.MotorSpeed; }
            set { InnerJoint.MotorSpeed = (float)value; }
        }

        /// <summary>
        /// Suurin vääntömomentti jolla moottori yrittää pyörittää siihen liitettyä kappaletta.
        /// </summary>
        public double MaxMotorTorque
        {
            get { return InnerJoint.MaxMotorTorque; }
            set { InnerJoint.MaxMotorTorque = (float)value; }
        }

        /// <summary>
        /// Akseli, jonka suhteen kappaleet voivat liikkua.
        /// <c>Vector.One</c>(oletus) sallii liikkumisen x ja y-akselilla.
        /// <c>Vector.UnitY</c> sallii liikkeen vain Y-akselin suunnassa jne.
        /// </summary>
        public Vector Axis
        {
            get { return InnerJoint.Axis; }
            set { InnerJoint.Axis = value; }
        }

        /// <summary>
        /// Luo uuden liitoksen jota voidaan käyttää esimerkiksi auton renkaiden luomiseen.
        /// Liitos sisältää moottorin, jolla voidaan luoda vääntömomentti kappaleeseen.
        /// </summary>
        /// <param name="firstObject">Ensimmäinen olio</param>
        /// <param name="secondObject">Toinen olio</param>
        public WheelJoint(PhysicsObject firstObject, PhysicsObject secondObject)
        {
            World world = PhysicsGame.Instance.Engine as World;
            var first = firstObject.Body as PhysicsBody;
            var second = secondObject.Body as PhysicsBody;
            InnerJoint = JointFactory.CreateWheelJoint(world, first.FSBody, second.FSBody, Vector.One);
            InnerJoint.Frequency = 5;
            InnerJoint.DampingRatio = 0.5f;
            Object1 = firstObject;
            Object2 = secondObject;
        }

        /// <summary>
        /// Luo uuden liitoksen jota voidaan käyttää esimerkiksi auton renkaiden luomiseen.
        /// Liitos sisältää moottorin, jolla voidaan luoda vääntömomentti kappaleeseen.
        /// </summary>
        /// <param name="firstObject">Ensimmäinen olio</param>
        /// <param name="secondObject">Toinen olio</param>
        /// <param name="axlePosition">Liitoksen akselin paikka</param>
        public WheelJoint(PhysicsObject firstObject, PhysicsObject secondObject, Vector axlePosition)
        {
            World world = PhysicsGame.Instance.Engine as World;
            var first = firstObject.Body as PhysicsBody;
            var second = secondObject.Body as PhysicsBody;
            InnerJoint = JointFactory.CreateWheelJoint(world, first.FSBody, second.FSBody, axlePosition, Vector.One);
            InnerJoint.Frequency = 5;
            InnerJoint.DampingRatio = 0.5f;
            Object1 = firstObject;
            Object2 = secondObject;
        }
    }
}
