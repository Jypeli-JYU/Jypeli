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
    public interface Controller
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
    public abstract class Controller<ControllerState> : Controller
    {
        protected static readonly ChangePredicate<ControllerState> AlwaysTrigger
            = delegate { return true; };

        private SynchronousList<Listener<ControllerState>> listeners = new SynchronousList<Listener<ControllerState>>();

        /// <summary>
        /// Viimeisin tila.
        /// </summary>
        public ControllerState PrevState { get; private set; }

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
            listeners.ForEach( l => l.CheckAndInvoke( PrevState, CurrentState ) );
        }

        protected Listener AddListener( ChangePredicate<ControllerState> rule, string controlName, string helpText, Delegate handler, params object[] args )
        {
            var l = new Listener<ControllerState>( rule, Game.Instance.ControlContext, controlName, helpText, handler, args );
            listeners.Add( l );
            return l;
        }

        public void Clear()
        {
            listeners.Clear();
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
