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

    /// <summary>
    /// Ohjaintapahtumien kuuntelija.
    /// </summary>
    public interface Listener : Destroyable
    {
        /// <summary>
        /// Kuuntelee tapahtumaa vain tietyssä kontekstissa.
        /// </summary>
        /// <param name="context"></param>
        Listener InContext( ListenContext context );

        /// <summary>
        /// Kuuntelee tapahtumaa vain tietyssä kontekstissa.
        /// Esim. Keyboard.Listen(parametrit).InContext(omaIkkuna) kuuntelee
        /// haluttua näppäimistötapahtumaa ainoastaan kun ikkuna on näkyvissä ja päällimmäisenä.
        /// </summary>
        /// <param name="context"></param>
        Listener InContext( ControlContexted obj );
    }

    /// <summary>
    /// Ohjaintapahtumien kuuntelija.
    /// </summary>
    /// <typeparam name="State">Tila</typeparam>
    public struct Listener<State> : Listener
    {
        private ChangePredicate<State> isTriggered;
        private Delegate handler;
        private object[] handlerParams;
        private string helpText;
        private bool isDestroyed;

        private bool dynamicContext;
        private ListenContext context;
        private ControlContexted contextedObject;

        internal ListenContext Context
        {
            get { return ( dynamicContext ? contextedObject.ControlContext : context ); }
        }

        public Listener( ChangePredicate<State> triggerRule, ListenContext context, string helpText, Delegate handler, params object[] args )
        {
            this.isDestroyed = false;
            this.isTriggered = triggerRule;
            this.handler = handler;
            this.helpText = helpText;
            this.handlerParams = args;
            this.Destroyed = null;
            this.dynamicContext = false;
            this.context = context;
            this.contextedObject = null;
        }

        public Listener( ChangePredicate<State> triggerRule, ControlContexted contexted, string helpText, Delegate handler, params object[] args )
        {
            this.isDestroyed = false;
            this.isTriggered = triggerRule;
            this.handler = handler;
            this.helpText = helpText;
            this.handlerParams = args;
            this.Destroyed = null;
            this.dynamicContext = true;
            this.context = null;
            this.contextedObject = contexted;
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
            if ( !IsDestroyed && Context != null && !Context.IsDestroyed && Context.Active && isTriggered( oldState, newState ) )
                Invoke();
        }

        /// <summary>
        /// Kuuntelee tapahtumaa vain tietyssä kontekstissa.
        /// </summary>
        /// <param name="context"></param>
        public Listener InContext( ListenContext context )
        {
            this.dynamicContext = false;
            this.context = context;
            return this;
        }

        /// <summary>
        /// Kuuntelee tapahtumaa vain tietyssä kontekstissa.
        /// Esim. Keyboard.Listen(parametrit).InContext(omaIkkuna) kuuntelee
        /// haluttua näppäimistötapahtumaa ainoastaan kun ikkuna on näkyvissä ja päällimmäisenä.
        /// </summary>
        /// <param name="context"></param>
        public Listener InContext( ControlContexted obj )
        {
            this.dynamicContext = true;
            this.contextedObject = obj;
            return this;
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
