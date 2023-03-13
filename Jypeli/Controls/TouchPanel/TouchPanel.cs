using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Jypeli.Controls;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Jypeli
{

    public delegate void TouchHandler(Touch touch);
    public delegate void TouchHandler<T>(Touch touch, T p);
    public delegate void TouchHandler<T1, T2>(Touch touch, T1 p1, T2 p2);
    public delegate void TouchHandler<T1, T2, T3>(Touch touch, T1 p1, T2 p2, T3 p3);

    /// <summary>
    /// Kosketusnäyttö.
    /// </summary>
    public class TouchPanel : IController
    {
        // Androidin DispatchTouchEvent tapahtuu eri säikeessä kuin missä pelin päivityssilmukka on
        internal readonly object TouchLock = new object();
        internal List<RawTouch> RawTouches { get; set; } = new List<RawTouch>();

        protected static readonly Predicate<Touch> AlwaysTrigger = delegate { return true; };

        private List<Touch> touches = new List<Touch>();
        private List<Gesture> gestures = new List<Gesture>();

        private readonly SynchronousList<TouchListener> DownListeners = new SynchronousList<TouchListener>();
        private readonly SynchronousList<TouchListener> PressListeners = new SynchronousList<TouchListener>();
        private readonly SynchronousList<TouchListener> ReleaseListeners = new SynchronousList<TouchListener>();
        private readonly SynchronousList<TouchListener> GestureListeners = new SynchronousList<TouchListener>(); // TODO: Gestures

        private ListenContext _snipContext = null;
        private ListenContext _pinchContext = null;


        /// <summary>
        /// Onko kosketusnäyttö kytketty.
        /// Toistaiseksi Androidilla palauttaa aina true, muuten false.
        /// </summary>
        public bool IsConnected
        {
            get 
            {
#if ANDROID
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Kuinka monta kosketusta näytöllä on aktiivisena.
        /// </summary>
        public int ActiveChannels
        {
            get { return touches.Count + gestures.Count; }
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
        /// Toistaiseksi palauttaa aina 10.
        /// </summary>
        public int MaxTouches
        {
            get { return 10; }
        }

        /// <summary>
        /// Kuinka monta elettä näytöllä on aktiivisena.
        /// </summary>
        public int NumGestures
        {
            get { return gestures.Count; }
        }

        /// <summary>
        /// Seurataanko kosketusta kameralla.
        /// </summary>
        public bool FollowSnipping
        {
            get { return _snipContext != null; }
            set
            {
                TouchHandler handler = delegate (Touch t)
                { Game.Instance.Camera.Position -= t.MovementOnWorld; };
                ContextHandler op = delegate (ListenContext ctx)
                { Listen(ButtonState.Down, handler, "Move around").InContext(ctx); };
                setContext(ref _snipContext, value, op);
            }
        }

        /// <summary>
        /// Zoomataanko kameralla kun käyttäjä tekee nipistyseleen
        /// </summary>
        public bool FollowPinching
        {
            get { return _pinchContext != null; }
            set
            {
                TouchHandler handler = delegate (Touch t)
                {
                    Gesture g = (Gesture)t;
                    Game.Instance.Camera.Zoom(g.WorldDistanceAfter.Magnitude / g.WorldDistanceBefore.Magnitude);
                };
                ContextHandler op = delegate (ListenContext ctx)
                { ListenGesture(GestureType.Pinch, handler, "Zoom"); };
                setContext(ref _pinchContext, value, op);
            }
        }

        delegate void ContextHandler(ListenContext ctx);

        private void setContext(ref ListenContext context, bool enable, ContextHandler operation)
        {
            if ((context != null) == enable)
                return;
            if (enable)
            {
                context = Game.Instance.ControlContext.CreateSubcontext();
                context.Active = true;
                operation(context);
            }
            else
            {
                context.Destroy();
                context = null;
            }
        }

        internal TouchPanel()
        {
        }

        /// <summary>
        /// Kosketetaako oliota.
        /// </summary>
        private static bool IsBeingTouched(ScreenView screen, Vector touchOnScreen, GameObject obj)
        {
            if (obj == null || obj.Layer == null || obj.IsDestroyed)
                return false;
            return obj.IsInside(Game.Instance.Camera.ScreenToWorld(touchOnScreen, obj.Layer));
        }

        private static HoverState GetHoverState(Touch touch, GameObject obj)
        {
            bool prevOn = IsBeingTouched(Game.Screen, touch._previousPosition, obj);
            bool currOn = IsBeingTouched(Game.Screen, touch._position, obj);

            if (prevOn && currOn)
                return HoverState.On;
            if (!prevOn && !currOn)
                return HoverState.Off;
            if (!prevOn && currOn)
                return HoverState.Enter;
            return HoverState.Exit;
        }

        private Predicate<Touch> MakeTriggerRule(GameObject obj, HoverState hover)
        {
            return delegate (Touch touch)
            {
                if (obj == null || obj.IsDestroyed || obj.Layer == null)
                    return false;
                return GetHoverState(touch, obj) == hover;
            };
        }

        public void Update()
        {
            UpdateTouches();
            UpdateGestures();
        }

        public void UpdateTouches()
        {
            lock (TouchLock)
            {
                RawTouches = RawTouches.OrderBy(r => r.Action).ToList();
                foreach (var rawTouch in RawTouches)
                {
                    Touch prevTouch = touches.Find(s => s.Id == rawTouch.Id);
                    Touch thisTouch = prevTouch != null ? prevTouch : new Touch(rawTouch);

                    if (rawTouch.Action == TouchAction.Down)
                    {
                        // Joskus hyvin nopealla spämmillä voi tulla sama kosketus tuplana.
                        if (prevTouch != null)
                            continue;
                        touches.Add(thisTouch);
                        PressListeners.ForEach(dl => dl.CheckAndInvoke(thisTouch));
                    }
                    if (rawTouch.Action == TouchAction.Move)
                    {
                        thisTouch.Update(rawTouch);
                        DownListeners.ForEach(dl => dl.CheckAndInvoke(thisTouch));
                    }
                    if(rawTouch.Action == TouchAction.Up)
                    {
                        int removed = touches.RemoveAll(t => t.Id == rawTouch.Id);
                        if(removed > 0)
                            ReleaseListeners.ForEach(dl => dl.CheckAndInvoke(thisTouch));
                    }
                }

                DownListeners.UpdateChanges();
                PressListeners.UpdateChanges();
                ReleaseListeners.UpdateChanges();

                RawTouches.Clear();
            }
        }

        public void UpdateGestures()
        {
            var samples = new List<Gesture>();

            /*if ( XnaTouchPanel.EnabledGestures == XnaGestureType.None )
                return;

            while ( XnaTouchPanel.IsGestureAvailable )
                samples.Add( new Gesture( XnaTouchPanel.ReadGesture() ) );
            */
            this.GestureListeners.UpdateChanges();
            this.gestures = samples;
        }

        public void Clear()
        {
            DownListeners.Clear();
            PressListeners.Clear();
            ReleaseListeners.Clear();
            GestureListeners.Clear();
        }

        public IEnumerable<string> GetHelpTexts()
        {
            foreach (var l in PressListeners)
            {
                if (l.HelpText != null)
                    yield return String.Format("TouchPanel Press", l.HelpText);
            }

            foreach (var l in DownListeners)
            {
                if (l.HelpText != null)
                    yield return String.Format("TouchPanel Down", l.HelpText);
            }

            foreach (var l in ReleaseListeners)
            {
                if (l.HelpText != null)
                    yield return String.Format("TouchPanel Release", l.HelpText);
            }
        }

        private SynchronousList<TouchListener> GetList(ButtonState state)
        {
            switch (state)
            {
                case ButtonState.Down:
                    return DownListeners;
                case ButtonState.Pressed:
                    return PressListeners;
                case ButtonState.Released:
                    return ReleaseListeners;
            }

            throw new ArgumentException("Button state is not supported");
        }

        private Listener AddListener(SynchronousList<TouchListener> list, Predicate<Touch> rule, string helpText, Delegate handler, params object[] args)
        {
            var l = new TouchListener(rule, Game.Instance.ControlContext, helpText, handler, args);
            list.Add(l);
            return l;
        }

        private Listener AddGestureListener(Predicate<Gesture> rule, string helpText, Delegate handler, params object[] args)
        {
            Predicate<Touch> touchRule = (Touch t) => t is Gesture && rule((Gesture)t);
            var l = new TouchListener(touchRule, Game.Instance.ControlContext, helpText, handler, args);
            GestureListeners.Add(l);
            return l;
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä.
        /// </summary>
        /// <param name="state">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener Listen(ButtonState state, TouchHandler handler, string helpText)
        {
            return AddListener(GetList(state), AlwaysTrigger, helpText, handler);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri</param>
        public Listener Listen<T>(ButtonState state, TouchHandler<T> handler, string helpText, T p)
        {
            return AddListener(GetList(state), AlwaysTrigger, helpText, handler, p);
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
        public Listener Listen<T1, T2>(ButtonState state, TouchHandler<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            return AddListener(GetList(state), AlwaysTrigger, helpText, handler, p1, p2);
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
        public Listener Listen<T1, T2, T3>(ButtonState state, TouchHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            return AddListener(GetList(state), AlwaysTrigger, helpText, handler, p1, p2, p3);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener ListenOn(GameObject obj, HoverState hoverstate, ButtonState buttonstate, TouchHandler handler, string helpText)
        {
            Predicate<Touch> rule = MakeTriggerRule(obj, hoverstate);
            return AddListener(GetList(buttonstate), rule, helpText, handler);
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
        public Listener ListenOn<T>(GameObject obj, HoverState hoverstate, ButtonState buttonstate, TouchHandler<T> handler, string helpText, T p)
        {
            Predicate<Touch> rule = MakeTriggerRule(obj, hoverstate);
            return AddListener(GetList(buttonstate), rule, helpText, handler, p);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        public Listener ListenOn<T1, T2>(GameObject obj, HoverState hoverstate, ButtonState buttonstate, TouchHandler<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            Predicate<Touch> rule = MakeTriggerRule(obj, hoverstate);
            return AddListener(GetList(buttonstate), rule, helpText, handler, p1, p2);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        /// <param name="p3">3. parametri</param>
        public Listener ListenOn<T1, T2, T3>(GameObject obj, HoverState hoverstate, ButtonState buttonstate, TouchHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            Predicate<Touch> rule = MakeTriggerRule(obj, hoverstate);
            return AddListener(GetList(buttonstate), rule, helpText, handler, p1, p2, p3);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttöä olion päällä.
        /// </summary>
        /// <param name="obj">Olio.</param>
        /// <param name="buttonstate">Kosketuksen tila</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener ListenOn(GameObject obj, ButtonState buttonstate, TouchHandler handler, string helpText)
        {
            return ListenOn(obj, HoverState.On, buttonstate, handler, helpText);
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
        public Listener ListenOn<T>(GameObject obj, ButtonState buttonstate, TouchHandler<T> handler, string helpText, T p)
        {
            return ListenOn(obj, HoverState.On, buttonstate, handler, helpText, p);
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
        public Listener ListenOn<T1, T2>(GameObject obj, ButtonState buttonstate, TouchHandler<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            return ListenOn(obj, HoverState.On, buttonstate, handler, helpText, p1, p2);
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
        public Listener ListenOn<T1, T2, T3>(GameObject obj, ButtonState buttonstate, TouchHandler<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            return ListenOn(obj, HoverState.On, buttonstate, handler, helpText, p1, p2, p3);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä.
        /// </summary>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener ListenGesture(GestureType type, TouchHandler handler, string helpText)
        {
            //XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return AddGestureListener((Gesture g) => g.GestureType == type, helpText, handler);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä olion päällä.
        /// </summary>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener ListenGestureOn(GameObject obj, HoverState hoverstate, GestureType type, TouchHandler handler, string helpText)
        {
            Predicate<Touch> hover = MakeTriggerRule(obj, hoverstate);
            return AddGestureListener((Gesture g) => g.GestureType == type && hover(g), helpText, handler);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä olion päällä.
        /// </summary>
        /// <param name="obj">Olio.</param>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener ListenGestureOn(GameObject obj, GestureType type, TouchHandler handler, string helpText)
        {
            return ListenGestureOn(obj, HoverState.On, type, handler, helpText);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        public Listener ListenGesture<T1>(GestureType type, TouchHandler handler, string helpText, T1 p1)
        {
            //XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return AddGestureListener((Gesture g) => g.GestureType == type, helpText, handler, p1);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä olion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        public Listener ListenGestureOn<T1>(GameObject obj, HoverState hoverstate, GestureType type, TouchHandler handler, string helpText, T1 p1)
        {
            Predicate<Touch> hover = MakeTriggerRule(obj, hoverstate);
            return AddGestureListener((Gesture g) => g.GestureType == type && hover(g), helpText, handler, p1);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä olion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        public Listener ListenGestureOn<T1>(GameObject obj, GestureType type, TouchHandler handler, string helpText, T1 p1)
        {
            return ListenGestureOn(obj, HoverState.On, type, handler, helpText, p1);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        public Listener ListenGesture<T1, T2>(GestureType type, TouchHandler handler, string helpText, T1 p1, T2 p2)
        {
            //XnaTouchPanel.EnabledGestures |= type;
            return AddGestureListener((Gesture g) => g.GestureType == type, helpText, handler, p1, p2);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä olion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        public Listener ListenGestureOn<T1, T2>(GameObject obj, HoverState hoverstate, GestureType type, TouchHandler handler, string helpText, T1 p1, T2 p2)
        {
            Predicate<Touch> hover = MakeTriggerRule(obj, hoverstate);
            return AddGestureListener((Gesture g) => g.GestureType == type && hover(g), helpText, handler, p1, p2);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä olion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        public Listener ListenGestureOn<T1, T2>(GameObject obj, GestureType type, TouchHandler handler, string helpText, T1 p1, T2 p2)
        {
            return ListenGestureOn(obj, HoverState.On, type, handler, helpText, p1);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        /// <param name="p3">3. parametri</param>
        public Listener ListenGesture<T1, T2, T3>(GestureType type, TouchHandler handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            //XnaTouchPanel.EnabledGestures |= (XnaGestureType)type;
            return AddGestureListener((Gesture g) => g.GestureType == type, helpText, handler, p1, p2);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä olion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        /// <param name="p3">3. parametri</param>
        public Listener ListenGestureOn<T1, T2, T3>(GameObject obj, HoverState hoverstate, GestureType type, TouchHandler handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            Predicate<Touch> hover = MakeTriggerRule(obj, hoverstate);
            return AddGestureListener((Gesture g) => g.GestureType == type && hover(g), helpText, handler, p1, p2);
        }

        /// <summary>
        /// Kuuntelee kosketusnäyttön elettä olion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="obj">Olio.</param>
        /// <param name="type">Kosketusele</param>
        /// <param name="handler">Aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        /// <param name="p3">3. parametri</param>
        public Listener ListenGestureOn<T1, T2, T3>(GameObject obj, GestureType type, TouchHandler handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            return ListenGestureOn(obj, HoverState.On, type, handler, helpText, p1);
        }
    }
}
