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
using Jypeli.Controls;
using Microsoft.Xna.Framework.Input;

namespace Jypeli
{
    /// <summary>
    /// Näppäimistö.
    /// </summary>
    public class Keyboard : Controller<KeyboardState>
    {
        internal override KeyboardState GetState()
        {
            return Microsoft.Xna.Framework.Input.Keyboard.GetState();
        }

        private ChangePredicate<KeyboardState> MakeTriggerRule( Key k, ButtonState state )
        {
            Keys key = (Keys)k;

            switch ( state )
            {
                case ButtonState.Up:
                    return delegate( KeyboardState prev, KeyboardState curr ) { return ( curr.IsKeyUp( key ) ); };

                case ButtonState.Down:
                    return delegate( KeyboardState prev, KeyboardState curr ) { return ( curr.IsKeyDown( key ) ); };

                case ButtonState.Pressed:
                    return delegate( KeyboardState prev, KeyboardState curr ) { return ( prev.IsKeyUp( key ) && curr.IsKeyDown( key ) ); };

                case ButtonState.Released:
                    return delegate( KeyboardState prev, KeyboardState curr ) { return ( prev.IsKeyDown( key ) && curr.IsKeyUp( key ) ); };
            }

            return AlwaysTrigger;
        }

        /// <summary>
        /// Kuuntelee näppäinten painalluksia.
        /// </summary>
        /// <param name="k">Näppäin</param>
        /// <param name="state">Näppäimen tila</param>
        /// <param name="handler">Mitä tehdään</param>
        /// <param name="helpText">Ohjeteksti</param>
        public void Listen( Key k, ButtonState state, Action handler, string helpText )
        {
            ChangePredicate<KeyboardState> rule = MakeTriggerRule( k, state );
            AddListener( rule, helpText, handler );
        }

        /// <summary>
        /// Kuuntelee näppäinten painalluksia.
        /// </summary>
        /// <typeparam name="T">Parametrin tyyppi</typeparam>
        /// <param name="k">Näppäin</param>
        /// <param name="state">Näppäimen tila</param>
        /// <param name="handler">Mitä tehdään</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri</param>
        public void Listen<T>( Key k, ButtonState state, Action<T> handler, string helpText, T p )
        {
            ChangePredicate<KeyboardState> rule = MakeTriggerRule( k, state );
            AddListener( rule, helpText, handler, p );
        }

		/// <summary>
        /// Kuuntelee näppäinten painalluksia.
        /// </summary>
        /// <typeparam name="T1">1. parametrin tyyppi</typeparam>
		/// <typeparam name="T2">2. parametrin tyyppi</typeparam>
        /// <param name="k">Näppäin</param>
        /// <param name="state">Näppäimen tila</param>
        /// <param name="handler">Mitä tehdään</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
		/// <param name="p2">2. parametri</param>
        public void Listen<T1, T2>( Key k, ButtonState state, Action<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            ChangePredicate<KeyboardState> rule = MakeTriggerRule( k, state );
            AddListener( rule, helpText, handler, p1, p2 );
        }
    }
}
