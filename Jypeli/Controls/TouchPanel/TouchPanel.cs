using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Jypeli
{
    using XnaTouchPanel = Microsoft.Xna.Framework.Input.Touch.TouchPanel;

    public delegate void TouchHandler( Touch touch );

    /// <summary>
    /// Kosketusnäyttö.
    /// </summary>
    public class TouchPanel : Controls.Controller
    {
        protected static readonly Predicate<Touch> AlwaysTrigger = delegate { return true; };

        private ScreenView screen;
        private TouchPanelCapabilities caps;
        private List<Touch> touches;
        private List<Touch> newTouches;

        private List<TouchListener> DownListeners = new List<TouchListener>();
        private List<TouchListener> PressListeners = new List<TouchListener>();
        private List<TouchListener> ReleaseListeners = new List<TouchListener>();

        /// <summary>
        /// Onko kosketusnäyttö kytketty.
        /// </summary>
        public bool IsConnected
        {
            get { return caps.IsConnected; }
        }

        /// <summary>
        /// Kuinka monta yhtäaikaista kosketusta näyttö tukee.
        /// </summary>
        public int MaxTouches
        {
            get { return caps.MaximumTouchCount; }
        }

        internal TouchPanel( ScreenView screen )
        {
            this.screen = screen;
            this.caps = XnaTouchPanel.GetCapabilities();
            this.touches = new List<Touch>( caps.MaximumTouchCount );
            this.newTouches = new List<Touch>( caps.MaximumTouchCount );
        }
        
        public void Update()
        {
            var xnaTouches = XnaTouchPanel.GetState();

            for ( int i = 0; i < xnaTouches.Count; i++ )
            {
                Touch prevTouch = touches.Find( s => s.Id == xnaTouches[i].Id );
                Touch thisTouch = prevTouch != null ? prevTouch : new Touch( screen, xnaTouches[i] );

                newTouches.Add( thisTouch );
                DownListeners.ForEach( dl => dl.CheckAndInvoke( thisTouch ) );

                if ( prevTouch == null )
                {
                    // New touch
                    PressListeners.ForEach( dl => dl.CheckAndInvoke( thisTouch ) );
                }
                else
                {
                    // Existing touch
                    touches.Remove( thisTouch );
                    thisTouch.Update( xnaTouches[i] );
                }
            }

            for ( int i = 0; i < touches.Count; i++ )
            {
                // Released touch
                ReleaseListeners.ForEach( dl => dl.CheckAndInvoke( touches[i] ) );
            }

            touches.Clear();
            var empty = touches;
            touches = newTouches;
            newTouches = empty;
        }

        public void Clear()
        {
            DownListeners.Clear();
            PressListeners.Clear();
            ReleaseListeners.Clear();
        }
        
        private List<TouchListener> GetList( ButtonState state )
        {
            switch ( state )
            {
                case ButtonState.Down: return DownListeners;
                case ButtonState.Pressed: return PressListeners;
                case ButtonState.Released: return ReleaseListeners;
            }

            throw new ArgumentException( "Button state is not supported" );
        }

        private void AddListener( List<TouchListener> list, Predicate<Touch> rule, string helpText, Delegate handler, params object[] args )
        {
            list.Add( new TouchListener( rule, helpText, handler, args ) );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä.
        /// </summary>
        /// <param name="state">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public void Listen( ButtonState state, TouchHandler handler, string helpText )
        {
            AddListener( GetList( state ), AlwaysTrigger, helpText, handler );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri</param>
        public void Listen<T>( ButtonState state, TouchHandler handler, string helpText, T p )
        {
            AddListener( GetList( state ), AlwaysTrigger, helpText, handler, p );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="state">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        public void Listen<T1, T2>( ButtonState state, TouchHandler handler, string helpText, T1 p1, T2 p2 )
        {
            AddListener( GetList( state ), AlwaysTrigger, helpText, handler, p1, p2 );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="state">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        /// <param name="p3">3. parametri</param>
        public void Listen<T1, T2, T3>( ButtonState state, TouchHandler handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            AddListener( GetList( state ), AlwaysTrigger, helpText, handler, p1, p2, p3 );
        }
    }
}
