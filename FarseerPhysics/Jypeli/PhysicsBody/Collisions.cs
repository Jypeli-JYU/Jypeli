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
 * Original Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 * Modified for Farseer engine by Mikko Röyskö
 */

using Jypeli.Physics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;

namespace Jypeli
{
    internal class AdaptedIgnorer //: Physics2DDotNet.Ignorers.Ignorer
    {

        public Jypeli.Ignorer Adaptee { get; private set; }

        public bool BothNeeded
        {
            get { return Adaptee.BothNeeded; }
        }

        public AdaptedIgnorer(Jypeli.Ignorer adaptee)
        {
            this.Adaptee = adaptee;
        }

        protected bool CanCollide(Body thisBody, Body otherBody/*, Physics2DDotNet.Ignorers.Ignorer otherIgnorer*/ )
        {
            var other = (PhysicsBody)(otherBody.Tag);
            return Adaptee.CanCollide((IPhysicsBody)thisBody.Tag, other, other.CollisionIgnorer);
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
            set { FSBody.CollisionIgnorer = _adaptedIgnorer = new AdaptedIgnorer(value); }
        }

        /// <summary>
        /// Jättääkö olio törmäyksen huomioimatta. Jos tosi, törmäyksestä
        /// tulee tapahtuma, mutta itse törmäystä ei tapahdu.
        /// </summary>
        public bool IgnoresCollisionResponse
        {// TODO: tälle helpompi tapa
            get { return FSBody.FixtureList[0].IsSensor; }
            set { FSBody.SetIsSensor(value); }
        }

        /// <summary>
        /// Tapahtuu kun olio törmää toiseen.
        /// </summary>
        public event CollisionHandler<IPhysicsBody, IPhysicsBody> Collided
        {
            add
            {
                FSBody.OnCollision += (a, b, c) => OnCollided(a.Body.owner, b.Body.owner, c, value);
            }
            remove
            {
                FSBody.OnCollision -= (a, b, c) => OnCollided(a.Body.owner, b.Body.owner, c, value);
            }
        }

        // TODO: Muut Farseerin tukemat törmäystapahtumat
        private bool OnCollided(PhysicsBody a, PhysicsBody b, Contact contact, CollisionHandler<IPhysicsBody, IPhysicsBody> func)
        {
            // Jos törmäystapahtuman käsittelijässä kutsutaan jotain fysiikkamoottoriin vaikuttavaa funktiota, 
            // Kuten esimerkiksi ClearAll, tuottaa tämän suorittaminen välittömästi ongelmia.
            // Siksi suoritetaan kaikki törmäyskäsittelijät vasta aivan viimeisenä, jolloin moottori ei ole keskellä päivitystä.
            Game.DoNextUpdate(() => func.Invoke(a, b));

            return true; // Huomioidaanko törmäyksessä fysiikka, eli jos palautetaan false, mennään toisen läpi.
                         // TODO: Kuinka toteuttaa käyttäjän tapahtumakäsittelijälle, ilman että kaikki vanha koodi hajoaa.
        }

        /// <summary>
        /// Tapahtuu kun olio on törmäyksessä toiseen.
        /// </summary>
        public event AdvancedCollisionHandler<IPhysicsBody, IPhysicsBody> Colliding;

        private void OnColliding(object sender/*, CollisionEventArgs e*/ )
        {
            if (Colliding != null)
            {
                /*var other = e.Other.Tag as IPhysicsBody;
                var contacts = new List<Collision.ContactPoint>();
                contacts.AddRange( e.Contact.Points.ConvertAll( p => new Collision.ContactPoint(p.Position, p.Normal) ) );
                Colliding( this, other, new Collision(this, other, contacts) );*/
            }
        }

        /// <summary>
        /// Tekee oliosta läpimentävän alhaalta ylöspäin (tasohyppelytaso).
        /// Huom. ei toimi yhdessä CollisionIgnoreGroupien kanssa!
        /// </summary>
        public void MakeOneWay()
        {
            FSBody.oneWayDir = Vector.UnitY;
        }

        /// <summary>
        /// Tekee oliosta läpimentävän vektorin suuntaan.
        /// Huom. ei toimi yhdessä CollisionIgnoreGroupien kanssa!
        /// </summary>
        public void MakeOneWay(Vector direction)
        {
            FSBody.oneWayDir = direction;
        }
    }
}
