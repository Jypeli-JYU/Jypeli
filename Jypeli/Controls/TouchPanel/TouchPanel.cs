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
    public delegate void TouchHandler<T>( Touch touch, T p );
    public delegate void TouchHandler<T1, T2>( Touch touch, T1 p1, T2 p2 );
    public delegate void TouchHandler<T1, T2, T3>( Touch touch, T1 p1, T2 p2, T3 p3 );

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
        /// Kuinka monta kosketusta tällä hetkellä ruudulla.
        /// </summary>
        public int NumTouches
        {
            get { return touches.Count; }
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

			try
			{
                this.caps = XnaTouchPanel.GetCapabilities();
			}
			catch (TypeLoadException)
            {
                this.caps = new TouchPanelCapabilities();
			}

            this.touches = new List<Touch>( caps.MaximumTouchCount );
            this.newTouches = new List<Touch>( caps.MaximumTouchCount );
        }

        /// <summary>
        /// Kosketetaako oliota.
        /// </summary>
        private static bool IsBeingTouched( ScreenView screen, Vector touchOnScreen, GameObject obj )
        {
            if ( obj == null || obj.Layer == null || obj.IsDestroyed ) return false;
            return obj.IsInside( Game.Instance.Camera.ScreenToWorld( touchOnScreen, obj.Layer ) );
        }

        private static HoverState GetHoverState( Touch touch, GameObject obj )
        {
            bool prevOn = IsBeingTouched( Game.Screen, touch.PrevPositionOnScreen, obj );
            bool currOn = IsBeingTouched( Game.Screen, touch.PositionOnScreen, obj );

            if ( prevOn && currOn ) return HoverState.On;
            if ( !prevOn && !currOn ) return HoverState.Off;
            if ( !prevOn && currOn ) return HoverState.Enter;
            return HoverState.Exit;
        }

        private Predicate<Touch> MakeTriggerRule( GameObject obj, HoverState hover )
        {
            return delegate( Touch touch )
            {
                if ( obj == null || obj.IsDestroyed || obj.Layer == null ) return false;
                return GetHoverState( touch, obj ) == hover;
            };
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

        public IEnumerable<string> GetHelpTexts()
        {
            foreach ( var l in PressListeners )
            {
                if ( l.HelpText != null )
                    yield return String.Format( "TouchPanel Press", l.HelpText );
            }

            foreach ( var l in DownListeners )
            {
                if ( l.HelpText != null )
                    yield return String.Format( "TouchPanel Down", l.HelpText );
            }

            foreach ( var l in ReleaseListeners )
            {
                if ( l.HelpText != null )
                    yield return String.Format( "TouchPanel Release", l.HelpText );
            }
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

        private Listener AddListener( List<TouchListener> list, Predicate<Touch> rule, string helpText, Delegate handler, params object[] args )
        {
            var l = new TouchListener( rule, Game.Instance.ControlContext, helpText, handler, args );
            list.Add( l );
            return l;
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä.
        /// </summary>
        /// <param name="state">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener Listen( ButtonState state, TouchHandler handler, string helpText )
        {
            return AddListener( GetList( state ), AlwaysTrigger, helpText, handler );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri</param>
        public Listener Listen<T>( ButtonState state, TouchHandler handler, string helpText, T p )
        {
            return AddListener( GetList( state ), AlwaysTrigger, helpText, handler, p );
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
        public Listener Listen<T1, T2>( ButtonState state, TouchHandler handler, string helpText, T1 p1, T2 p2 )
        {
            return AddListener( GetList( state ), AlwaysTrigger, helpText, handler, p1, p2 );
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
        public Listener Listen<T1, T2, T3>( ButtonState state, TouchHandler handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            return AddListener( GetList( state ), AlwaysTrigger, helpText, handler, p1, p2, p3 );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener ListenOn( GameObject obj, HoverState hoverstate, ButtonState buttonstate, TouchHandler handler, string helpText )
        {
            Predicate<Touch> rule = MakeTriggerRule( obj, hoverstate );
            return AddListener( GetList( buttonstate ), rule, helpText, handler );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri</param>
        public Listener ListenOn<T>( GameObject obj, HoverState hoverstate, ButtonState buttonstate, TouchHandler<T> handler, string helpText, T p )
        {
            Predicate<Touch> rule = MakeTriggerRule( obj, hoverstate );
            return AddListener( GetList( buttonstate ), rule, helpText, handler, p );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        public Listener ListenOn<T1, T2>( GameObject obj, HoverState hoverstate, ButtonState buttonstate, TouchHandler<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            Predicate<Touch> rule = MakeTriggerRule( obj, hoverstate );
            return AddListener( GetList( buttonstate ), rule, helpText, handler, p1, p2 );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        /// <param name="p3">3. parametri</param>
        public Listener ListenOn<T1, T2, T3>( GameObject obj, HoverState hoverstate, ButtonState buttonstate, TouchHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            Predicate<Touch> rule = MakeTriggerRule( obj, hoverstate );
            return AddListener( GetList( buttonstate ), rule, helpText, handler, p1, p2, p3 );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <param name="obj">Olio.</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener ListenOn( GameObject obj, ButtonState buttonstate, TouchHandler handler, string helpText )
        {
            return ListenOn( obj, HoverState.On, buttonstate, handler, helpText );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri</param>
        public Listener ListenOn<T>( GameObject obj, ButtonState buttonstate, TouchHandler<T> handler, string helpText, T p )
        {
            return ListenOn( obj, HoverState.On, buttonstate, handler, helpText, p );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        public Listener ListenOn<T1, T2>( GameObject obj, ButtonState buttonstate, TouchHandler<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            return ListenOn( obj, HoverState.On, buttonstate, handler, helpText, p1, p2 );
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        /// <param name="p3">3. parametri</param>
        public Listener ListenOn<T1, T2, T3>( GameObject obj, ButtonState buttonstate, TouchHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            return ListenOn( obj, HoverState.On, buttonstate, handler, helpText, p1, p2, p3 );
        }
    }
}
