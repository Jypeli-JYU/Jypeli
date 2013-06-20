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
    using MouseState = Microsoft.Xna.Framework.Input.MouseState;
    using XnaButtonState = Microsoft.Xna.Framework.Input.ButtonState;

    /// <summary>
    /// Hiiri.
    /// </summary>
    public class Mouse : Controller<MouseState>
    {
        //private static Dictionary<MouseButton, Func<MouseState, bool>> GetButtonDown = null;
        private static readonly Dictionary<MouseButton, Func<MouseState, bool>> GetButtonDown = new Dictionary<MouseButton, Func<MouseState, bool>>( 5 )
        {
            { MouseButton.Left, delegate( MouseState state ) { return state.LeftButton == XnaButtonState.Pressed; } },
            { MouseButton.Right, delegate( MouseState state ) { return state.RightButton == XnaButtonState.Pressed; } },
            { MouseButton.Middle, delegate( MouseState state ) { return state.MiddleButton == XnaButtonState.Pressed; } },
            { MouseButton.XButton1, delegate( MouseState state ) { return state.XButton1 == XnaButtonState.Pressed; } },
            { MouseButton.XButton2, delegate( MouseState state ) { return state.XButton2 == XnaButtonState.Pressed; } }
        };

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
                XnaV2 xnaPos = new XnaV2( CurrentState.X, CurrentState.Y );
                Vector pos = ScreenView.FromXnaCoords( xnaPos, screen.ViewportSize, Vector.Zero );
                return pos.Transform( screen.GetScreenTransform() );
            }
            set
            {
#if !WINRT
                // Not supported on WinRT... only sets xna coords
                Vector pos = value.Transform( screen.GetScreenInverse() );
                XnaV2 xnapos = ScreenView.ToXnaCoords( pos, screen.ViewportSize, Vector.Zero );

                //CurrentState = new MouseState( CurrentState, xnapos );
                CurrentState = new MouseState(
                    (int)xnapos.X, (int)xnapos.Y, CurrentState.ScrollWheelValue,
                    CurrentState.LeftButton, CurrentState.MiddleButton, CurrentState.RightButton,
                    CurrentState.XButton1, CurrentState.XButton2 );
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

                XnaV2 curXna = new XnaV2( CurrentState.X, CurrentState.Y );
                XnaV2 prevXna = new XnaV2( PrevState.X, PrevState.Y );
                Vector curScr = ScreenView.FromXnaCoords( curXna, screen.ViewportSize, Vector.Zero );
                Vector prevScr = ScreenView.FromXnaCoords( prevXna, screen.ViewportSize, Vector.Zero );
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

                XnaV2 curXna = new XnaV2( CurrentState.X, CurrentState.Y );
                XnaV2 prevXna = new XnaV2( PrevState.X, PrevState.Y );
                Vector curScr = ScreenView.FromXnaCoords( curXna, screen.ViewportSize, Vector.Zero );
                Vector prevScr = ScreenView.FromXnaCoords( prevXna, screen.ViewportSize, Vector.Zero );
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
                return CurrentState.ScrollWheelValue / 120;
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
                return ( CurrentState.ScrollWheelValue - PrevState.ScrollWheelValue ) / 120;
            }
        }

        internal Mouse( ScreenView screen )
        {
            this.screen = screen;
            this.CurrentState = XnaMouse.GetState();

            /*if ( GetButtonDown == null )
            {
                GetButtonDown = new Dictionary<MouseButton, Func<MouseState, bool>>( 5 );
                GetButtonDown.Add( MouseButton.Left, delegate( MouseState state ) { return state.LeftButton == XnaButtonState.Pressed; } );
                GetButtonDown.Add( MouseButton.Right, delegate( MouseState state ) { return state.RightButton == XnaButtonState.Pressed; } );
                GetButtonDown.Add( MouseButton.Middle, delegate( MouseState state ) { return state.MiddleButton == XnaButtonState.Pressed; } );
                GetButtonDown.Add( MouseButton.XButton1, delegate( MouseState state ) { return state.XButton1 == XnaButtonState.Pressed; } );
                GetButtonDown.Add( MouseButton.XButton2, delegate( MouseState state ) { return state.XButton2 == XnaButtonState.Pressed; } );
            }*/
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
            /*var xnaState = XnaMouse.GetState();
            MouseState state = new MouseState();

            state.Position = new XnaV2( xnaState.X, xnaState.Y );
            state.LeftDown = xnaState.LeftButton == XnaButtonState.Pressed;
            state.RightDown = xnaState.RightButton == XnaButtonState.Pressed;
            state.MiddleDown = xnaState.MiddleButton == XnaButtonState.Pressed;
            state.X1Down = xnaState.XButton1 == XnaButtonState.Pressed;
            state.X2Down = xnaState.XButton2 == XnaButtonState.Pressed;
            state.Wheel = xnaState.ScrollWheelValue;*/

#if !WINRT
            if ( !IsCursorVisible )
            {
                // Reset the mouse to the center of the screen
                SetPosition( Vector.Zero );
            }
#endif

            return XnaMouse.GetState();
        }

        private static ButtonState GetButtonState( MouseState oldState, MouseState newState, MouseButton button )
        {
            bool prevDown = GetButtonDown[button]( oldState );
            bool currDown = GetButtonDown[button]( newState );

            if ( prevDown && currDown ) return ButtonState.Down;
            if ( !prevDown && !currDown ) return ButtonState.Up;
            if ( !prevDown && currDown ) return ButtonState.Pressed;
            return ButtonState.Released;
        }

        /// <summary>
        /// Palauttaa napin tilan.
        /// </summary>
        /// <param name="button">Nappi</param>
        /// <returns></returns>
        public ButtonState GetButtonState( MouseButton button )
        {
            return GetButtonState( PrevState, CurrentState, button );
        }

        private ChangePredicate<MouseState> MakeTriggerRule( MouseButton button, ButtonState state )
        {
            if ( button == MouseButton.None || state == ButtonState.Irrelevant )
                return AlwaysTrigger;

            return delegate( MouseState prev, MouseState curr ) { return GetButtonState( prev, curr, button ) == state; };
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
