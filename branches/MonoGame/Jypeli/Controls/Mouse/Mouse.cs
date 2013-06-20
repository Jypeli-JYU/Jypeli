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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jypeli.Controls;
using Microsoft.Xna.Framework.Graphics;

namespace Jypeli
{
    using Matrix = Microsoft.Xna.Framework.Matrix;
    using XnaV2 = Microsoft.Xna.Framework.Vector2;
    using XnaMouse = Microsoft.Xna.Framework.Input.Mouse;
    using XnaButtonState = Microsoft.Xna.Framework.Input.ButtonState;

    /// <summary>
    /// Hiiri.
    /// </summary>
    public class Mouse : Controller<MouseState>
    {
        private ScreenView screen;

        /// <summary>
        /// Käytetäänkö hiiren kursoria.
        /// Jos käytetään, hiiren paikka ruudulla on mitattavissa, mutta hiiri ei
        /// voi liikkua ruudun ulkopuolelle.
        /// Jos ei käytetä, hiirtä voidaan liikuttaa rajatta, mutta sen paikkaa
        /// ruudulla ei voida määrittää.
        /// </summary>
        public bool IsCursorVisible
        {
            get
            {
#if WINDOWS_PHONE
                return false;
#else
                return Game.Instance.IsMouseVisible;
#endif
            }
            set { Game.Instance.IsMouseVisible = value; }
        }

        /// <summary>
        /// Kursorin paikka ruutukoordinaateissa.
        /// </summary>
        public Vector PositionOnScreen
        {
            get
            {
                Vector pos = ScreenView.FromXnaCoords( CurrentState.Position, screen.ViewportSize, Vector.Zero );
                return pos.Transform( screen.GetScreenTransform() );
            }
            set
            {
#if !WINRT
                // Not supported on WinRT... only sets xna coords
                Vector pos = value.Transform( screen.GetScreenInverse() );
                XnaV2 xnapos = ScreenView.ToXnaCoords( pos, screen.ViewportSize, Vector.Zero );

                CurrentState = new MouseState( CurrentState, xnapos );
                XnaMouse.SetPosition( (int)xnapos.X, (int)xnapos.Y );
#endif
            }
        }

        /// <summary>
        /// Kursorin paikka maailmankoordinaateissa.
        /// </summary>
        public Vector PositionOnWorld
        {
            get
            {
                return Game.Instance.Camera.ScreenToWorld( PositionOnScreen );
            }
            set
            {
                PositionOnScreen = Game.Instance.Camera.WorldToScreen( value );
            }
        }

        /// <summary>
        /// Kursorin viimeisin liike ruutukoordinaateissa.
        /// </summary>
        public Vector MovementOnScreen
        {
            get
            {
                Matrix screenTransform = screen.GetScreenTransform();

                Vector curScr = ScreenView.FromXnaCoords( CurrentState.Position, screen.ViewportSize, Vector.Zero );
                Vector prevScr = ScreenView.FromXnaCoords( PrevState.Position, screen.ViewportSize, Vector.Zero );               
                Vector curr = curScr.Transform( screenTransform );
                Vector prev = prevScr.Transform( screenTransform );

                return curr - prev;
            }
        }

        /// <summary>
        /// Kursorin viimeisin liike maailmankoordinaateissa.
        /// </summary>
        public Vector MovementOnWorld
        {
            get
            {
                Matrix screenTransform = screen.GetScreenTransform();

                Vector curScr = ScreenView.FromXnaCoords( CurrentState.Position, screen.ViewportSize, Vector.Zero );
                Vector prevScr = ScreenView.FromXnaCoords( PrevState.Position, screen.ViewportSize, Vector.Zero );
                Vector curr = curScr.Transform( screenTransform );
                Vector prev = prevScr.Transform( screenTransform );

                return Game.Instance.Camera.ScreenToWorld( curr ) - Game.Instance.Camera.ScreenToWorld( prev );
            }
        }

        /// <summary>
        /// Rullan asento. Vähenee alaspäin ja kasvaa ylöspäin rullattaessa.
        /// </summary>
        public int WheelState
        {
            get
            {
                return CurrentState.Wheel / 120;
            }
        }

        /// <summary>
        /// Rullan asennon muutos viime tarkistuksesta. Vähenee alaspäin ja kasvaa ylöspäin rullattaessa.
        /// Nolla jos rullaa ei ole käytetty.
        /// </summary>
        public int WheelChange
        {
            get
            {
                return ( CurrentState.Wheel - PrevState.Wheel ) / 120;
            }
        }

        internal Mouse( ScreenView screen )
        {
            var xnaState = XnaMouse.GetState();
            CurrentState = new MouseState( CurrentState, new XnaV2( xnaState.X, xnaState.Y ) );
            this.screen = screen;
        }

        private void SetPosition( Vector pos )
        {
#if !WINRT
            // Not supported on WinRT... only sets xna coords
            Vector screenpos = pos.Transform( screen.GetScreenInverse() );
            XnaV2 center = ScreenView.ToXnaCoords( screenpos, screen.ViewportSize, Vector.Zero );
            XnaMouse.SetPosition( (int)center.X, (int)center.Y );
#endif
        }

        internal override MouseState GetState()
        {
            var xnaState = XnaMouse.GetState();
            MouseState state = new MouseState();

            state.Position = new XnaV2( xnaState.X, xnaState.Y );
            state.LeftDown = xnaState.LeftButton == XnaButtonState.Pressed;
            state.RightDown = xnaState.RightButton == XnaButtonState.Pressed;
            state.MiddleDown = xnaState.MiddleButton == XnaButtonState.Pressed;
            state.X1Down = xnaState.XButton1 == XnaButtonState.Pressed;
            state.X2Down = xnaState.XButton2 == XnaButtonState.Pressed;
            state.Wheel = xnaState.ScrollWheelValue;

#if !WINRT
            if ( !IsCursorVisible )
            {
                // Reset the mouse to the center of the screen
                SetPosition( Vector.Zero );
            }
#endif

            return state;
        }
        
        /// <summary>
        /// Palauttaa napin tilan.
        /// </summary>
        /// <param name="button">Nappi</param>
        /// <returns></returns>
        public ButtonState GetButtonState( MouseButton button )
        {
            return MouseState.GetButtonState( PrevState, CurrentState, button );
        }

        private ChangePredicate<MouseState> MakeTriggerRule( MouseButton button, ButtonState state )
        {
            if ( button == MouseButton.None || state == ButtonState.Irrelevant )
                return AlwaysTrigger;

            return delegate( MouseState prev, MouseState curr ) { return MouseState.GetButtonState( prev, curr, button ) == state; };
        }

        private ChangePredicate<MouseState> MakeTriggerRule( double moveTrigger )
        {
            return delegate( MouseState prev, MouseState curr )
            {
                double xdist = prev.X - curr.X;
                double ydist = prev.Y - curr.Y;
                return xdist * xdist + ydist * ydist > moveTrigger * moveTrigger;
            };
        }

        /// <summary>
        /// Kuuntelee hiiren nappulan painalluksia.
        /// </summary>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public void Listen( MouseButton button, ButtonState state, Action handler, string helpText )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( button, state );
            AddListener( rule, helpText, handler );
        }

        /// <summary>
        /// Kuuntelee hiiren nappulan painalluksia.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri kuuntelija-aliohjelmalle</param>
        public void Listen<T>( MouseButton button, ButtonState state, Action<T> handler, string helpText, T p )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( button, state );
            AddListener( rule, helpText, handler, p );
        }

        /// <summary>
        /// Kuuntelee hiiren nappulan painalluksia.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p2">2. parametri kuuntelija-aliohjelmalle</param>
        public void Listen<T1, T2>( MouseButton button, ButtonState state, Action<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( button, state );
            AddListener( rule, helpText, handler, p1, p2 );
        }

        /// <summary>
        /// Kuuntelee hiiren nappulan painalluksia.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p2">2. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p3">3. parametri kuuntelija-aliohjelmalle</param>
        public void Listen<T1, T2, T3>( MouseButton button, ButtonState state, Action<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( button, state );
            AddListener( rule, helpText, handler, p1, p2, p3 );
        }

        /// <summary>
        /// Kuuntelee hiiren liikettä.
        /// </summary>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public void ListenMovement( double trigger, Action handler, string helpText )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( trigger );
            AddListener( rule, helpText, handler );
        }

        /// <summary>
        /// Kuuntelee hiiren liikettä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri kuuntelija-aliohjelmalle</param>
        public void ListenMovement<T>( double trigger, Action<T> handler, string helpText, T p )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( trigger );
            AddListener( rule, helpText, handler, p );
        }

        /// <summary>
        /// Kuuntelee hiiren liikettä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p2">2. parametri kuuntelija-aliohjelmalle</param>
        public void ListenMovement<T1, T2>( double trigger, Action<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( trigger );
            AddListener( rule, helpText, handler, p1, p2 );
        }

        /// <summary>
        /// Kuuntelee hiiren liikettä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p2">2. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p3">3. parametri kuuntelija-aliohjelmalle</param>
        public void ListenMovement<T1, T2, T3>( double trigger, Action<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( trigger );
            AddListener( rule, helpText, handler, p1, p2, p3 );
        }

        #region Backwards compatibility

        public class MouseAnalogState : AnalogState
        {
            private Mouse mouse;

            public double State
            {
                get { return mouse.GetButtonState( MouseButton.Left ) == ButtonState.Down ? 1 : 0; }
            }

            public double AnalogChange
            {
                get { return mouse.MovementOnScreen.Magnitude; }
            }

            public Vector StateVector
            {
                get { return Vector.Zero; }
            }

            public Vector MouseMovement
            {
                get { return mouse.MovementOnScreen; }
            }

            internal MouseAnalogState( Mouse mouse )
            {
                this.mouse = mouse;
            }
        }

        /// <summary>
        /// Kuuntelee hiiren nappulan painalluksia.
        /// </summary>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        [Obsolete( "Käytä ilman AnalogStatea. Hiiren liikkeen saa Mouse.MovementOnScreen tai Mouse.MovementOnWorld" )]
        public void Listen( MouseButton button, ButtonState state, Action<AnalogState> handler, string helpText )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( button, state );
            AddListener( rule, helpText, handler, new MouseAnalogState( this ) );
        }

        /// <summary>
        /// Kuuntelee hiiren nappulan painalluksia.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri kuuntelija-aliohjelmalle</param>
        [Obsolete( "Käytä ilman AnalogStatea. Hiiren liikkeen saa Mouse.MovementOnScreen tai Mouse.MovementOnWorld" )]
        public void Listen<T>( MouseButton button, ButtonState state, Action<AnalogState, T> handler, string helpText, T p )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( button, state );
            AddListener( rule, helpText, handler, new MouseAnalogState( this ), p );
        }

        /// <summary>
        /// Kuuntelee hiiren nappulan painalluksia.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p2">2. parametri kuuntelija-aliohjelmalle</param>
        [Obsolete( "Käytä ilman AnalogStatea. Hiiren liikkeen saa Mouse.MovementOnScreen tai Mouse.MovementOnWorld" )]
        public void Listen<T1, T2>( MouseButton button, ButtonState state, Action<AnalogState, T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( button, state );
            AddListener( rule, helpText, handler, new MouseAnalogState( this ), p1, p2 );
        }

        /// <summary>
        /// Kuuntelee hiiren nappulan painalluksia.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p2">2. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p3">3. parametri kuuntelija-aliohjelmalle</param>
        [Obsolete( "Käytä ilman AnalogStatea. Hiiren liikkeen saa Mouse.MovementOnScreen tai Mouse.MovementOnWorld" )]
        public void Listen<T1, T2, T3>( MouseButton button, ButtonState state, Action<AnalogState, T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( button, state );
            AddListener( rule, helpText, handler, new MouseAnalogState( this ), p1, p2, p3 );
        }

        /// <summary>
        /// Kuuntelee hiiren liikettä.
        /// </summary>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        [Obsolete( "Käytä ilman AnalogStatea. Hiiren liikkeen saa Mouse.MovementOnScreen tai Mouse.MovementOnWorld" )]
        public void ListenMovement( double trigger, Action<AnalogState> handler, string helpText )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( trigger );
            AddListener( rule, helpText, handler, new MouseAnalogState( this ) );
        }

        /// <summary>
        /// Kuuntelee hiiren liikettä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri kuuntelija-aliohjelmalle</param>
        [Obsolete( "Käytä ilman AnalogStatea. Hiiren liikkeen saa Mouse.MovementOnScreen tai Mouse.MovementOnWorld" )]
        public void ListenMovement<T>( double trigger, Action<AnalogState, T> handler, string helpText, T p )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( trigger );
            AddListener( rule, helpText, handler, new MouseAnalogState( this ), p );
        }

        /// <summary>
        /// Kuuntelee hiiren liikettä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p2">2. parametri kuuntelija-aliohjelmalle</param>
        [Obsolete( "Käytä ilman AnalogStatea. Hiiren liikkeen saa Mouse.MovementOnScreen tai Mouse.MovementOnWorld" )]
        public void ListenMovement<T1, T2>( double trigger, Action<AnalogState, T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( trigger );
            AddListener( rule, helpText, handler, new MouseAnalogState( this ), p1, p2 );
        }

        /// <summary>
        /// Kuuntelee hiiren liikettä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p2">2. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p3">3. parametri kuuntelija-aliohjelmalle</param>
        [Obsolete( "Käytä ilman AnalogStatea. Hiiren liikkeen saa Mouse.MovementOnScreen tai Mouse.MovementOnWorld" )]
        public void ListenMovement<T1, T2, T3>( double trigger, Action<AnalogState, T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( trigger );
            AddListener( rule, helpText, handler, new MouseAnalogState( this ), p1, p2, p3 );
        }

        #endregion
    }
}
