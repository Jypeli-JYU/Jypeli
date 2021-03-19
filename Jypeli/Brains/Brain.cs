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

using System.ComponentModel;
using System;

namespace Jypeli
{
    /// <summary>
    /// Aivoluokka peliolioille.
    /// Voidaan käyttää tekoälyn ja tilannekohtaisten toimintamallien luomiseen
    /// peliolioille, esimerkkinä tietokoneen ohjaamat viholliset.
    /// </summary>    
    public class Brain
    {
        /// <summary>
        /// Tyhjät aivot, eivät sisällä mitään toiminnallisuutta.
        /// </summary>
        internal static readonly Brain None = new Brain();

        private bool active = true;        

        /// <summary>
        /// Aivot käytössä tai pois käytöstä.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        /// <summary>
        /// Tapahtuu kun aivoja päivitetään.
        /// </summary>
        public event Action<Brain> Updated;

        private IGameObject _owner;

        /// <summary>
        /// Aivojen haltija.
        /// </summary>
        public IGameObject Owner
        {
            get { return _owner; }
            set
            {
                if ( _owner == value ) return;
                IGameObject prevOwner = _owner;
                _owner = value;
                if ( prevOwner != null ) OnRemove( prevOwner );
                if ( value != null ) OnAdd( value );
            }
        }

        internal void AddToGameEvent()
        {
            OnAddToGame();
        }

        internal void DoUpdate( Time time )
        {
            if ( Active )
            {
                Update( time );
                if ( Updated != null ) Updated( this );
            }
        }

        /// <summary>
        /// Kutsutaan, kun aivot lisätään oliolle.
        /// </summary>
        /// <param name="newOwner">Olio, jolle aivot lisättiin.</param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected virtual void OnAdd( IGameObject newOwner )
        {
        }

        /// <summary>
        /// Kutsutaan, kun aivot poistetaan oliolta.
        /// </summary>
        /// <param name="prevOwner">Olio, jolta aivot poistettiin.</param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected virtual void OnRemove( IGameObject prevOwner )
        {
        }

        /// <summary>
        /// Kutsutaan, kun aivojen omistaja lisätään peliin tai omistajaksi
        /// asetetaan olio, joka on jo lisätty peliin.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected virtual void OnAddToGame() { }

        /// <summary>
        /// Kutsutaan, kun tilaa päivitetään.
        /// Suurin osa päätöksenteosta tapahtuu täällä.
        /// Perivässä luokassa methodin kuuluu kutsua vastaavaa
        /// kantaluokan methodia.
        /// </summary>
        /// <param name="time">Päivityksen ajanhetki.</param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected virtual void Update( Time time ) { }

        /// <summary>
        /// Kutsutaan, kun tapahtuu törmäys.
        /// Perivässä luokassa methodin kuuluu kutsua vastaavaa
        /// kantaluokan methodia.
        /// </summary>
        /// <param name="target">Olio, johon törmätään.</param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public virtual void OnCollision( IGameObject target )
        {
        }
    }
}
