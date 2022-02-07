#region MIT License
/*
 * Copyright (c) 2013 University of Jyväskylä, Department of Mathematical
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

using System;
using System.Collections.Generic;

namespace Jypeli.Controls
{
    /// <summary>
    /// Ohjainlaite.
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// Poistaa kaikki kuuntelijat.
        /// </summary>
        void Clear();

        /// <summary>
        /// Lukee uuden tilan laitteelta ja päivittää sen nykyiseksi sekä
        /// laukaisee tapahtumia.
        /// </summary>
        void Update();

        /// <summary>
        /// Palauttaa asetettujen kuuntelijoiden ohjetekstit.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetHelpTexts();
    }

    /// <summary>
    /// Mikä tahansa ohjauslaite.
    /// </summary>
    /// <typeparam name="ControllerState">Ohjauslaitteen tilaolio</typeparam>
    /// <typeparam name="Control">Ohjauslaitteen ohjain (näppäin tms)</typeparam>
    public abstract class Controller<ControllerState, Control> : IController where Control : IComparable
    {
        protected static readonly ChangePredicate<ControllerState> AlwaysTrigger
            = delegate { return true; };
        protected static readonly ChangePredicate<ControllerState> NeverTrigger
            = delegate { return false; };

        private SynchronousList<Listener<ControllerState, Control>> listeners = new SynchronousList<Listener<ControllerState, Control>>();
        private SynchronousList<Listener<ControllerState, Control>> disabledListeners = new SynchronousList<Listener<ControllerState, Control>>();

        /// <summary>
        /// Viimeisin tila.
        /// </summary>
        public ControllerState PrevState { get; protected set; }

        /// <summary>
        /// Nykyinen tila.
        /// </summary>
        public ControllerState CurrentState { get; protected set; }

        /// <summary>
        /// Lukee ja palauttaa laitteen viimeisimmän tilan.
        /// </summary>
        internal abstract ControllerState GetState();

        /// <summary>
        /// Lukee uuden tilan laitteelta ja päivittää sen nykyiseksi sekä
        /// laukaisee tapahtumia.
        /// </summary>
        public void Update()
        {
            PrevState = CurrentState;
            CurrentState = GetState();

            listeners.Update( Game.Time );
            disabledListeners.Update(Game.Time);
            listeners.ForEach( l => l.CheckAndInvoke( PrevState, CurrentState ) );
        }

        /// <summary>
        /// Lisää kuuntelijan ohjaimelle.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="control"></param>
        /// <param name="controlName"></param>
        /// <param name="helpText"></param>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected Listener AddListener( ChangePredicate<ControllerState> rule, Control control, string controlName, string helpText, Delegate handler, params object[] args )
        {
            var l = new Listener<ControllerState, Control>( rule, Game.Instance.ControlContext, control, controlName, helpText, handler, args );
            listeners.Add( l );
            return l;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            listeners.Clear();
        }

        /// <summary>
        /// Poistaa tietyt kuuntelutapahtumat käytöstä.
        /// </summary>
        /// <param name="predicate">Ehto, jonka tapahtuman on toteutettava.</param>
        public void Disable( Predicate<Listener<ControllerState, Control>> predicate )
        {
            foreach ( var l in listeners )
            {
                if ( predicate( l ) )
                {
                    // Note: synchronous list, does not actually change the list while iterating
                    listeners.Remove( l );
                    disabledListeners.Add( l );
                }
            }
        }

        /// <summary>
        /// Ottaa käytöstä poistetun kontrollin takaisin käyttöön.
        /// </summary>
        /// <param name="predicate">Ehto, jonka tapahtuman on toteutettava.</param>
        public void Enable( Predicate<Listener<ControllerState, Control>> predicate )
        {
            foreach ( var l in disabledListeners )
            {
                if ( predicate( l ) )
                {
                    // Note: synchronous list, does not actually change the list while iterating
                    listeners.Add( l );
                    disabledListeners.Remove( l );
                }
            }
        }

        /// <summary>
        /// Poistaa kontrollin käytöstä.
        /// </summary>
        /// <param name="c">Kontrolli.</param>
        public void Disable( Control c )
        {
            Disable( l => l.Control.Equals( c ) );
        }

        /// <summary>
        /// Ottaa kontrollin takaisin käyttöön.
        /// </summary>
        /// <param name="c">Kontrolli.</param>
        public void Enable( Control c )
        {
            Enable( l => l.Control.Equals( c ) );
        }

        /// <summary>
        /// Ottaa takaisin käyttöön kaikki <c>Disable</c>-metodilla poistetut kontrollit.
        /// </summary>
        public void EnableAll()
        {
            Enable( x => true );
        }

        /// <summary>
        /// Poistaa kaikki kontrollit käytöstä.
        /// </summary>
        public void DisableAll()
        {
            Disable( x => true );
        }

        public IEnumerable<string> GetHelpTexts()
        {
            foreach ( var l in listeners )
            {
                if ( l.ControlName != null && l.HelpText != null )
                    yield return String.Format( "{0} - {1}", l.ControlName, l.HelpText );
            }
        }
    }
}
