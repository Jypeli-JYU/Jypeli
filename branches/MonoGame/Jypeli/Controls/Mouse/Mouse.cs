using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Jypeli.Controls;

namespace Jypeli
{
    using XnaMouse = Microsoft.Xna.Framework.Input.Mouse;
    using XnaButtonState = Microsoft.Xna.Framework.Input.ButtonState;
    using Microsoft.Xna.Framework.Graphics;

    public class Mouse : Controller<MouseState>
    {
        private static int wcx = 800;
        private static int wcy = 600;

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

        internal static Viewport Viewport
        {
            set
            {
                wcx = value.X + value.Width / 2;
                wcy = value.Y + value.Height / 2;
            }
        }

        /// <summary>
        /// Kursorin paikka ruutukoordinaateissa.
        /// </summary>
        public Vector PositionOnScreen
        {
            get
            {
                return new Vector( CurrentState.X - wcx, wcy - CurrentState.Y );
            }
            set
            {
                int x = wcx + (int)value.X;
                int y = wcy - (int)value.Y;
                XnaMouse.SetPosition( x, y );
                CurrentState = new MouseState(
                    x, y, CurrentState.ScrollWheelValue, CurrentState.LeftButton, CurrentState.MiddleButton,
                    CurrentState.RightButton, CurrentState.XButton1, CurrentState.XButton2
                );
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
                return new Vector( CurrentState.X - PrevState.X, PrevState.Y - CurrentState.Y );
            }
        }

        /// <summary>
        /// Kursorin viimeisin liike maailmankoordinaateissa.
        /// </summary>
        public Vector MovementOnWorld
        {
            get
            {
                Vector lastPosOnScreen = new Vector( PrevState.X - wcx, wcy - PrevState.Y );
                Vector lastPosOnWorld = Game.Instance.Camera.ScreenToWorld( PositionOnScreen );
                return PositionOnWorld - lastPosOnWorld;
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

        internal Mouse()
        {
            XnaMouse.SetPosition( wcx, wcy );
        }

        internal override MouseState GetState()
        {
            return XnaMouse.GetState();
        }

        private XnaButtonState GetXnaButtonState( MouseState state, MouseButton button )
        {
            switch (button)
            {
                case MouseButton.Left: return state.LeftButton;
                case MouseButton.Right: return state.RightButton;
                case MouseButton.Middle: return state.MiddleButton;
                case MouseButton.XButton1: return state.XButton1;
                case MouseButton.XButton2: return state.XButton2;
            }

            throw new ArgumentException( "Can't get mouse button state for button " + button.ToString() );
        }

        /// <summary>
        /// Palauttaa napin tilan (ButtonState.Down tai ButtonState.Up)
        /// </summary>
        /// <param name="button">Nappi</param>
        /// <returns></returns>
        public ButtonState GetButtonState( MouseButton button )
        {
            XnaButtonState xnastate = GetXnaButtonState( CurrentState, button );
            return xnastate == XnaButtonState.Pressed ? ButtonState.Down : ButtonState.Up;
        }

        private ChangePredicate<MouseState> MakeTriggerRule( MouseButton button, ButtonState state )
        {
            if ( button == MouseButton.None || state == ButtonState.Irrelevant )
                return AlwaysTrigger;

            switch ( state )
            {
                case ButtonState.Up:
                    return delegate( MouseState prev, MouseState curr ) { return GetXnaButtonState( curr, button ) == XnaButtonState.Released; };

                case ButtonState.Down:
                    return delegate( MouseState prev, MouseState curr ) { return GetXnaButtonState( curr, button ) == XnaButtonState.Pressed; };

                case ButtonState.Pressed:
                    return delegate( MouseState prev, MouseState curr )
                    { return GetXnaButtonState( prev, button ) == XnaButtonState.Released && GetXnaButtonState( curr, button ) == XnaButtonState.Pressed; };

                case ButtonState.Released:
                    return delegate( MouseState prev, MouseState curr )
                        { return GetXnaButtonState( prev, button ) == XnaButtonState.Pressed && GetXnaButtonState( curr, button ) == XnaButtonState.Released; };
            }

            return AlwaysTrigger;
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

        public void ListenMovement( double trigger, Action handler, string helpText )
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule( trigger );
            AddListener( rule, helpText, handler );
        }
    }
}
