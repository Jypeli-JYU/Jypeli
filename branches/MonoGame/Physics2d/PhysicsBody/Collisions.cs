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

using Physics2DDotNet.Ignorers;
using Physics2DDotNet;
using System.Collections.Generic;
using System;
using AdvanceMath;

namespace Jypeli
{
    public partial class PhysicsBody
    {
        private int _ignoreGroup = 0;

        /// <summary>
        /// Olio, jolla voi välttää oliota osumasta tiettyihin muihin olioihin.
        /// </summary>
        public virtual Ignorer CollisionIgnorer
        {
            get { return Body.CollisionIgnorer; }
            set { Body.CollisionIgnorer = value; }
        }
        
        /// <summary>
        /// Jättääkö olio törmäyksen huomioimatta. Jos tosi, törmäyksestä
        /// tulee tapahtuma, mutta itse törmäystä ei tapahdu.
        /// </summary>
        public bool IgnoresCollisionResponse
        {
            get { return Body.IgnoresCollisionResponse; }
            set { Body.IgnoresCollisionResponse = value; }
        }

        /// <summary>
        /// Tapahtuu kun olio törmää toiseen.
        /// </summary>
        public event CollisionHandler<IPhysicsObject, IPhysicsObject> Collided;

        private void OnCollided( object sender, CollisionEventArgs args )
        {
            /*if ( this.IsDestroyed || args.Other == null ) return;
            var other = (PhysicsObject)args.Other.Tag;
            if ( other.IsDestroyed ) return;

            if ( Collided != null )
            {
                if ( other.ParentStructure != null ) Collided( this, other.ParentStructure );
                Collided( this, other );
            }
            Brain.OnCollision( other );*/
        }
        
        private static CollisionShapeParameters GetDefaultParameters( double width, double height )
        {
            CollisionShapeParameters p;
            p.MaxVertexDistance = Math.Min( width, height ) / 3;
            p.DistanceGridSpacing = Math.Min( width, height ) / 2;
            return p;
        }

        /// <summary>
        /// Tekee oliosta läpimentävän alhaalta ylöspäin (tasohyppelytaso).
        /// Huom. ei toimi yhdessä CollisionIgnoreGroupien kanssa!
        /// </summary>
        public void MakeOneWay()
        {
            this.CollisionIgnorer = new OneWayPlatformIgnorer( AdvanceMath.Vector2D.YAxis, Size.Y );
        }

        /// <summary>
        /// Tekee oliosta läpimentävän vektorin suuntaan.
        /// Huom. ei toimi yhdessä CollisionIgnoreGroupien kanssa!
        /// </summary>
        public void MakeOneWay( Vector direction )
        {
            this.CollisionIgnorer = new OneWayPlatformIgnorer( (Vector2D)direction, Size.Y );
        }
    }
}
