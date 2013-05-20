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
    public class Controller
    {
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

        protected void AddListener( ChangePredicate<ControllerState> rule, string helpText, Delegate handler, params object[] args )
        {
            listeners.Add( new Listener<ControllerState>( rule, helpText, handler, args ) );
        }
    }
}
