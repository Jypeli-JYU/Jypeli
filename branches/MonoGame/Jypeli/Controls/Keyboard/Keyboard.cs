﻿#region MIT License
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
    public class Keyboard : Controller<KeyboardState, Key>
    {
        internal Keyboard()
        {
        }

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

        private string GetKeyName( Key k )
        {
            string keyStr = k.ToString();

            if ( k == Key.OemQuotes ) keyStr = "ä";
            if ( k == Key.OemTilde ) keyStr = "ö";
            if ( k == Key.OemPlus || k == Key.Add ) keyStr = "+";
            if ( k == Key.Subtract ) keyStr = "-";
            if ( k == Key.Multiply ) keyStr = "*";
            if ( k == Key.Divide ) keyStr = "/";
            if ( k == Key.Aring ) keyStr = "å";
            if ( k == Key.LessOrGreater ) keyStr = "<";

            return "Keyboard " + keyStr;
        }

        /// <summary>
        /// Palauttaa annetun näppäimen tilan (ks. <c>ButtonState</c>).
        /// </summary>
        /// <param name="k">Näppäin.</param>
        /// <returns>Näppäimen tila</returns>
        public ButtonState GetKeyState( Key k )
        {
            Keys key = (Keys)k;
            bool down = CurrentState.IsKeyDown( key );
            bool lastdown = PrevState.IsKeyDown( key );

            if ( lastdown && down )
                return ButtonState.Down;
            if ( !lastdown && down )
                return ButtonState.Pressed;
            if ( lastdown && !down )
                return ButtonState.Released;

            return ButtonState.Up;
        }

        /// <summary>
        /// Tarkistaa, onko kumpikaan shift-näppäimistä painettuna.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> jos alhaalla, muuten <c>painettuna</c>.
        /// </returns>
        public bool IsShiftDown()
        {
            return CurrentState.IsKeyDown( Keys.LeftShift ) || CurrentState.IsKeyDown( Keys.RightShift );
        }

        /// <summary>
        /// Tarkistaa, onko kumpikaan ctrl-näppäimistä painettuna.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> jos alhaalla, muuten <c>painettuna</c>.
        /// </returns>
        public bool IsCtrlDown()
        {
            return CurrentState.IsKeyDown( Keys.LeftControl ) || CurrentState.IsKeyDown( Keys.RightControl );
        }

        /// <summary>
        /// Tarkistaa, onko kumpikaan alt-näppäimistä painettuna.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> jos alhaalla, muuten <c>painettuna</c>.
        /// </returns>
        public bool IsAltDown()
        {
            return CurrentState.IsKeyDown( Keys.LeftAlt ) || CurrentState.IsKeyDown( Keys.RightAlt );
        }

        /// <summary>
        /// Kuuntelee näppäinten painalluksia.
        /// </summary>
        /// <param name="k">Näppäin</param>
        /// <param name="state">Näppäimen tila</param>
        /// <param name="handler">Mitä tehdään</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener Listen( Key k, ButtonState state, Action handler, string helpText )
        {
            ChangePredicate<KeyboardState> rule = MakeTriggerRule( k, state );
            return AddListener( rule, k, GetKeyName( k ), helpText, handler );
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
        public Listener Listen<T>( Key k, ButtonState state, Action<T> handler, string helpText, T p )
        {
            ChangePredicate<KeyboardState> rule = MakeTriggerRule( k, state );
            return AddListener( rule, k, GetKeyName( k ), helpText, handler, p );
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
        public Listener Listen<T1, T2>( Key k, ButtonState state, Action<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            ChangePredicate<KeyboardState> rule = MakeTriggerRule( k, state );
            return AddListener( rule, k, GetKeyName( k ), helpText, handler, p1, p2 );
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
        /// <param name="p3">3. parametri</param>
        public Listener Listen<T1, T2, T3>( Key k, ButtonState state, Action<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            ChangePredicate<KeyboardState> rule = MakeTriggerRule( k, state );
            return AddListener( rule, k, GetKeyName( k ), helpText, handler, p1, p2, p3 );
        }

        /// <summary>
        /// Kuuntelee näppäinten painalluksia.
        /// </summary>
        /// <typeparam name="T1">1. parametrin tyyppi</typeparam>
        /// <typeparam name="T2">2. parametrin tyyppi</typeparam>
        /// <typeparam name="T3">3. parametrin tyyppi</typeparam>
        /// <typeparam name="T4">4. parametrin tyyppi</typeparam>
        /// <param name="k">Näppäin</param>
        /// <param name="state">Näppäimen tila</param>
        /// <param name="handler">Mitä tehdään</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        /// <param name="p3">3. parametri</param>
        /// <param name="p4">4. parametri</param>
        public Listener Listen<T1, T2, T3, T4>( Key k, ButtonState state, Action<T1, T2, T3, T4> handler, string helpText, T1 p1, T2 p2, T3 p3, T4 p4 )
        {
            ChangePredicate<KeyboardState> rule = MakeTriggerRule( k, state );
            return AddListener( rule, k, GetKeyName( k ), helpText, handler, p1, p2, p3, p4 );
        }

        #region ListenWSAD
        /// <summary>
        /// Kuuntelee W, S, A ja D -näppäimiä.
        /// </summary>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        public void ListenWSAD( ButtonState state, Action<Vector> handler, String helpText )
        {
            Listen( Key.W, state, handler, helpText, Vector.UnitY );
            Listen( Key.S, state, handler, helpText, -Vector.UnitY );
            Listen( Key.A, state, handler, helpText, -Vector.UnitX );
            Listen( Key.D, state, handler, helpText, Vector.UnitX );
        }

        /// <summary>
        /// Kuuntelee W, S, A ja D -näppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        public void ListenWSAD<T1>( ButtonState state, Action<Vector, T1> handler, String helpText, T1 p1 )
        {
            Listen( Key.W, state, handler, helpText, Vector.UnitY, p1 );
            Listen( Key.S, state, handler, helpText, -Vector.UnitY, p1 );
            Listen( Key.A, state, handler, helpText, -Vector.UnitX, p1 );
            Listen( Key.D, state, handler, helpText, Vector.UnitX, p1 );
        }

        /// <summary>
        /// Kuuntelee W, S, A ja D -näppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        /// <param name="p2">Toisen oman parametrin arvo</param>
        public void ListenWSAD<T1, T2>( ButtonState state, Action<Vector, T1, T2> handler, String helpText, T1 p1, T2 p2 )
        {
            Listen( Key.W, state, handler, helpText, Vector.UnitY, p1, p2 );
            Listen( Key.S, state, handler, helpText, -Vector.UnitY, p1, p2 );
            Listen( Key.A, state, handler, helpText, -Vector.UnitX, p1, p2 );
            Listen( Key.D, state, handler, helpText, Vector.UnitX, p1, p2 );
        }

        /// <summary>
        /// Kuuntelee W, S, A ja D -näppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        /// <param name="p2">Toisen oman parametrin arvo</param>
        public void ListenWSAD<T1, T2, T3>( ButtonState state, Action<Vector, T1, T2, T3> handler, String helpText, T1 p1, T2 p2, T3 p3 )
        {
            Listen( Key.W, state, handler, helpText, Vector.UnitY, p1, p2, p3 );
            Listen( Key.S, state, handler, helpText, -Vector.UnitY, p1, p2, p3 );
            Listen( Key.A, state, handler, helpText, -Vector.UnitX, p1, p2, p3 );
            Listen( Key.D, state, handler, helpText, Vector.UnitX, p1, p2, p3 );
        }
        #endregion

        #region ListenArrows
        /// <summary>
        /// Kuuntelee nuolinäppäimiä.
        /// </summary>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        public void ListenArrows( ButtonState state, Action<Vector> handler, String helpText )
        {
            Listen( Key.Up, state, handler, helpText, Vector.UnitY );
            Listen( Key.Down, state, handler, helpText, -Vector.UnitY );
            Listen( Key.Left, state, handler, helpText, -Vector.UnitX );
            Listen( Key.Right, state, handler, helpText, Vector.UnitX );
        }

        /// <summary>
        /// Kuuntelee nuolinäppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        public void ListenArrows<T1>( ButtonState state, Action<Vector, T1> handler, String helpText, T1 p1 )
        {
            Listen( Key.Up, state, handler, helpText, Vector.UnitY, p1 );
            Listen( Key.Down, state, handler, helpText, -Vector.UnitY, p1 );
            Listen( Key.Left, state, handler, helpText, -Vector.UnitX, p1 );
            Listen( Key.Right, state, handler, helpText, Vector.UnitX, p1 );
        }

        /// <summary>
        /// Kuuntelee nuolinäppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        /// <param name="p2">Toisen oman parametrin arvo</param>
        public void ListenArrows<T1, T2>( ButtonState state, Action<Vector, T1, T2> handler, String helpText, T1 p1, T2 p2 )
        {
            Listen( Key.Up, state, handler, helpText, Vector.UnitY, p1, p2 );
            Listen( Key.Down, state, handler, helpText, -Vector.UnitY, p1, p2 );
            Listen( Key.Left, state, handler, helpText, -Vector.UnitX, p1, p2 );
            Listen( Key.Right, state, handler, helpText, Vector.UnitX, p1, p2 );
        }

        /// <summary>
        /// Kuuntelee nuolinäppäimiä.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen oman parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen oman parametrin tyyppi</typeparam>
        /// <param name="state">Näppäinten kuunneltava tila</param>
        /// <param name="handler">Tapahtumakäsittelijä. Ensimmäinen parametri on automaattisesti yksikköpituinen vektori.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Ensimmäisen oman parametrin arvo</param>
        /// <param name="p2">Toisen oman parametrin arvo</param>
        public void ListenArrows<T1, T2, T3>( ButtonState state, Action<Vector, T1, T2, T3> handler, String helpText, T1 p1, T2 p2, T3 p3 )
        {
            Listen( Key.Up, state, handler, helpText, Vector.UnitY, p1, p2, p3 );
            Listen( Key.Down, state, handler, helpText, -Vector.UnitY, p1, p2, p3 );
            Listen( Key.Left, state, handler, helpText, -Vector.UnitX, p1, p2, p3 );
            Listen( Key.Right, state, handler, helpText, Vector.UnitX, p1, p2, p3 );
        }
        #endregion
    }
}
