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

using System.Collections.Generic;
using System;
using Jypeli.Physics;
using FarseerPhysics.Dynamics;

namespace Jypeli
{
    internal class AdaptedIgnorer //: Physics2DDotNet.Ignorers.Ignorer
    {
        
        public Jypeli.Ignorer Adaptee { get; private set; }

        public bool BothNeeded
        {
            get { return Adaptee.BothNeeded; }
        }

        public AdaptedIgnorer( Jypeli.Ignorer adaptee )
        {
            this.Adaptee = adaptee;
        }

        protected bool CanCollide( Body thisBody, Body otherBody/*, Physics2DDotNet.Ignorers.Ignorer otherIgnorer*/ )
        {
            var other = (PhysicsBody)( otherBody.Tag );
            return Adaptee.CanCollide( (IPhysicsBody)thisBody.Tag, other, other.CollisionIgnorer );
        }
    }

    public partial class PhysicsBody
    {
        private int _ignoreGroup = 0;
        private AdaptedIgnorer _adaptedIgnorer = null;

        /// <summary>
        /// Olio, jolla voi välttää oliota osumasta tiettyihin muihin olioihin.
        /// </summary>
        public virtual Ignorer CollisionIgnorer
        {
            get { return _adaptedIgnorer == null ? null : _adaptedIgnorer.Adaptee; }
            set { Body.CollisionIgnorer = _adaptedIgnorer = new AdaptedIgnorer( value ); }
        }
        
        // TODO: Kaikki

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
        public event CollisionHandler<IPhysicsBody, IPhysicsBody> Collided;

        private void OnCollided( object sender/*, CollisionEventArgs e*/ )
        {
            if ( Collided != null )
            {
                /*
                var other = e.Other.Tag as IPhysicsBody;
                Collided( this, other );
                */
            }
        }

        /// <summary>
        /// Tapahtuu kun olio on törmäyksessä toiseen.
        /// </summary>
        public event AdvancedCollisionHandler<IPhysicsBody, IPhysicsBody> Colliding;

        private void OnColliding( object sender/*, CollisionEventArgs e*/ )
        {
            if ( Colliding != null )
            {
                /*var other = e.Other.Tag as IPhysicsBody;
                var contacts = new List<Collision.ContactPoint>();
                contacts.AddRange( e.Contact.Points.ConvertAll( p => new Collision.ContactPoint(p.Position, p.Normal) ) );
                Colliding( this, other, new Collision(this, other, contacts) );*/
            }
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
            throw new NotImplementedException();
            //this.CollisionIgnorer = new OneWayPlatformIgnorer( AdvanceMath.Vector2D.YAxis, Size.Y );
        }

        /// <summary>
        /// Tekee oliosta läpimentävän vektorin suuntaan.
        /// Huom. ei toimi yhdessä CollisionIgnoreGroupien kanssa!
        /// </summary>
        public void MakeOneWay( Vector direction )
        {
            throw new NotImplementedException();
            //this.CollisionIgnorer = new OneWayPlatformIgnorer( (Vector2D)direction, Size.Y );
        }
    }
}
