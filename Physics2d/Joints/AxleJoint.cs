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
using AdvanceMath;

using XnaJoint = Physics2DDotNet.Joints.Joint;
using XnaHingeJoint = Physics2DDotNet.Joints.HingeJoint;
using XnaFixedHinge = Physics2DDotNet.Joints.FixedHingeJoint;

namespace Jypeli
{
    /// <summary>
    /// Saranaliitos kahden olion välille.
    /// </summary>
    public class AxleJoint : IAxleJoint
    {
        Vector pivot;
        Vector initialPosition;

        /// <summary>
        /// Ensimmäinen olio.
        /// </summary>
        public PhysicsObject Object1 { get; private set; }

        /// <summary>
        /// Toinen olio (null jos ensimmäinen olio on sidottu pisteeseen)
        /// </summary>
        public PhysicsObject Object2 { get; private set; }

        /// <summary>
        /// Pyörimisakselin (tämänhetkiset) koordinaatit.
        /// </summary>
        public Vector AxlePoint
        {
            get
            {
                return Object2 != null ? pivot + Object2.Position - initialPosition : pivot;
            }
        }

        internal XnaJoint innerJoint;

        /// <summary>
        /// Liitoksen pehmeys eli kuinka paljon sillä on liikkumavaraa.
        /// </summary>
        public double Softness
        {
            get
            {
                if ( innerJoint is XnaHingeJoint )
                    return ( (XnaHingeJoint)innerJoint ).Softness;
                else if ( innerJoint is XnaFixedHinge )
                    return ( (XnaFixedHinge)innerJoint ).Softness;
                else
                    throw new ArgumentException( "Invalid type for inner joint: " + innerJoint.GetType() );
            }
            set
            {
                if ( innerJoint is XnaHingeJoint )
                    ( (XnaHingeJoint)innerJoint ).Softness = value;
                else if ( innerJoint is XnaFixedHinge )
                    ( (XnaFixedHinge)innerJoint ).Softness = value;
                else
                    throw new ArgumentException( "Invalid type for inner joint: " + innerJoint.GetType() );
            }
        }

        /// <summary>
        /// Kiinnittää olion akselilla pelikenttään.
        /// </summary>
        /// <param name="obj">Olio</param>
        public AxleJoint( PhysicsObject obj )
        {
            var body = obj.Body as PhysicsBody;
            Vector2D pos = new Vector2D( obj.Position.X, obj.Position.Y );
            innerJoint = new XnaFixedHinge( body.Body, pos, new Physics2DDotNet.Lifespan() );
            Object1 = obj;
            Object2 = null;
            pivot = obj.Position;
        }

        /// <summary>
        /// Luo uuden akseliliitoksen olion ja pisteen välille.
        /// </summary>
        /// <param name="obj">Ensimmäinen olio</param>
        /// <param name="axlePosition">Liitoksen akselin paikka</param>
        public AxleJoint( PhysicsObject obj, Vector axlePosition )
        {
            var body = obj.Body as PhysicsBody;
            Vector2D pos = new Vector2D( axlePosition.X, axlePosition.Y );
            innerJoint = new XnaFixedHinge( body.Body, pos, new Physics2DDotNet.Lifespan() );
            Object1 = obj;
            Object2 = null;
            pivot = axlePosition;
        }

        /// <summary>
        /// Luo uuden akseliliitoksen kahden olion välille.
        /// </summary>
        /// <param name="firstObject">Ensimmäinen olio</param>
        /// <param name="secondObject">Toinen olio</param>
        /// <param name="axlePosition">Liitoksen akselin paikka</param>
        public AxleJoint( PhysicsObject firstObject, PhysicsObject secondObject, Vector axlePosition )
        {
            var first = firstObject.Body as PhysicsBody;
            var second = secondObject.Body as PhysicsBody;
            Vector2D pos = new Vector2D( axlePosition.X, axlePosition.Y );
            innerJoint = new XnaHingeJoint( first.Body, second.Body, pos, new Physics2DDotNet.Lifespan() );
            Object1 = firstObject;
            Object2 = secondObject;
            pivot = axlePosition;
            initialPosition = secondObject.Position;
        }

        /// <summary>
        /// Luo uuden akseliliitoksen kahden olion välille.
        /// Liitos sijoitetaan toisen olion keskipisteeseen.
        /// </summary>
        /// <param name="firstObject">Ensimmäinen olio</param>
        /// <param name="secondObject">Toinen olio</param>
        public AxleJoint( PhysicsObject firstObject, PhysicsObject secondObject )
        {
            var first = firstObject.Body as PhysicsBody;
            var second = secondObject.Body as PhysicsBody;
            Vector2D pos = new Vector2D( secondObject.Position.X, secondObject.Position.Y );
            innerJoint = new XnaHingeJoint( first.Body, second.Body, pos, new Physics2DDotNet.Lifespan() );
            Object1 = firstObject;
            Object2 = secondObject;
            pivot = secondObject.Position;
            initialPosition = secondObject.Position;
        }

        Physics.IPhysicsEngine engine = null;

        public void SetEngine( Physics.IPhysicsEngine engine )
        {
            this.engine = engine;
        }

        public void AddToEngine()
        {
            if ( this.engine == null ) throw new InvalidOperationException( "AddToEngine: physics engine not set" );
            if ( this.Object1 == null ) throw new InvalidOperationException( "AddToEngine: joint.Object1 == null" );
            if ( !this.Object1.IsAddedToGame ) throw new InvalidOperationException( "AddToEngine: object 1 not added to game" );
            if ( this.Object2 == null && !this.Object2.IsAddedToGame ) throw new InvalidOperationException( "AddToEngine: object 2 not added to game" );

            engine.AddJoint( this );
        }

        #region Destroyable

        /// <summary>
        /// Onko liitos tuhottu.
        /// </summary>
        public bool IsDestroyed
        {
            get { return innerJoint.Lifetime.IsExpired; }
        }

        /// <summary>
        /// Tapahtuu kun liitos on tuhottu.
        /// </summary>
        public event Action Destroyed;

        /// <summary>
        /// Tuhoaa liitoksen.
        /// </summary>
        public void Destroy()
        {
            innerJoint.Lifetime.IsExpired = true;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Destroy();
        }

        #endregion
    }
}
