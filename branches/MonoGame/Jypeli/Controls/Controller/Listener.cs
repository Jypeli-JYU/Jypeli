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
using System.Reflection;
using Jypeli.Controls;

namespace Jypeli
{
    /// <summary>
    /// Metodityyppi, joka ottaa parametrikseen entisen ja nykyisen muuttujan arvon ja palauttaa totuusarvon.
    /// Voidaan käyttää erilaisten sääntöjen tekemiseen.
    /// </summary>
    /// <typeparam name="T">Arvojen tyyppi</typeparam>
    /// <param name="prev">Vanha arvo</param>
    /// <param name="curr">Uusi arvo</param>
    /// <returns>true tai false</returns>
    public delegate bool ChangePredicate<T>( T prev, T curr );

    public interface Listener : Destroyable
    {
    }

    /// <summary>
    /// Yleinen kuuntelija.
    /// </summary>
    /// <typeparam name="State">Tila</typeparam>
    public struct Listener<State> : Listener
    {
        private ChangePredicate<State> isTriggered;
        private Delegate handler;
        private object[] handlerParams;
        private string helpText;
        private bool isDestroyed;

        public Listener( ChangePredicate<State> triggerRule, string helpText, Delegate handler, params object[] args )
        {
            this.isDestroyed = false;
            this.isTriggered = triggerRule;
            this.handler = handler;
            this.helpText = helpText;
            this.handlerParams = args;
            this.Destroyed = null;
        }

        public void Invoke()
        {
#if NETFX_CORE
            // Win8
            MethodInfo handlerMethod = handler.GetMethodInfo();
#else
            MethodInfo handlerMethod = handler.Method;
#endif
            handlerMethod.Invoke( handler.Target, handlerParams );
        }

        public void CheckAndInvoke( State oldState, State newState )
        {
            if ( !IsDestroyed && isTriggered( oldState, newState ) )
                Invoke();
        }

        #region Destroyable Members

        /// <summary>
        /// Onko olio tuhottu.
        /// </summary>
        /// <returns></returns>
        public bool IsDestroyed
        {
            get { return isDestroyed; }
        }

        public void Destroy()
        {
            isDestroyed = true;
            OnDestroyed();
        }

        /// <summary> 
        /// Tapahtuu, kun olio tuhotaan. 
        /// </summary> 
        public event Action Destroyed;

        private void OnDestroyed()
        {
            if ( Destroyed != null )
                Destroyed();
        }

        #endregion
    }
}
