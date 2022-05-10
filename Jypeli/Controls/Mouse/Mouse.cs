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
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen, Mikko Röyskö.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Jypeli.Controls;
using Silk.NET.Input;

namespace Jypeli
{
    /// <summary>
    /// Hiiri.
    /// </summary>
    public class Mouse : Controller<MouseState, MouseButton>
    {
        private static readonly Dictionary<MouseButton, Func<MouseState, bool>> GetButtonDown = new Dictionary<MouseButton, Func<MouseState, bool>>(5)
        {
            { MouseButton.Left, delegate(MouseState state) { return state.LeftButton; } },
            { MouseButton.Right, delegate(MouseState state) { return state.RightButton; } },
            { MouseButton.Middle, delegate(MouseState state) { return state.MiddleButton; } },
            { MouseButton.XButton1, delegate(MouseState state) { return state.XButton4; } }, // Tässä numerointi menee oudosti. Pidetään kulissien takana käytettävän hiiren tilan numerointi ja Jypelin näyttämä numerointi erillään
            { MouseButton.XButton2, delegate(MouseState state) { return state.XButton5; } },
            { MouseButton.XButton3, delegate(MouseState state) { return state.XButton6; } },
            { MouseButton.XButton4, delegate(MouseState state) { return state.XButton7; } },
            { MouseButton.XButton5, delegate(MouseState state) { return state.XButton8; } },
            { MouseButton.XButton6, delegate(MouseState state) { return state.XButton9; } },
            { MouseButton.XButton7, delegate(MouseState state) { return state.XButton10; } },
            { MouseButton.XButton8, delegate(MouseState state) { return state.XButton11; } },
            { MouseButton.XButton9, delegate(MouseState state) { return state.XButton12; } },
        };

        private ScreenView screen;
        private IMouse mouse;
        private MouseState internalState;

        /// <summary>
        /// Onko hiiren kursori näkyvissä
        /// </summary>
        public bool IsCursorVisible
        {
            get { return mouse.Cursor.CursorMode == CursorMode.Normal; }
            set
            {
                if (value)
                    mouse.Cursor.CursorMode = CursorMode.Normal;
                else
                    mouse.Cursor.CursorMode = CursorMode.Hidden;
            }
        }

        /// <summary>
        /// Kursorin paikka ruutukoordinaateissa.
        /// </summary>
        public Vector PositionOnScreen
        {
            get
            {
                return ScreenView.FromDisplayCoords(new Vector(mouse.Position.X, mouse.Position.Y));
            }
            set
            {
                mouse.Position = ScreenView.ToDisplayCoords(value);
            }
        }

        /// <summary>
        /// Kursorin paikka maailmankoordinaateissa.
        /// </summary>
        public Vector PositionOnWorld
        {
            get
            {
                return GetPositionOnWorld(ScreenView.FromDisplayCoords(new Vector(mouse.Position.X, mouse.Position.Y)));
            }
            set
            {
                PositionOnScreen = Game.Instance.Camera.WorldToScreen(value);
            }
        }

        /// <summary>
        /// Kursorin viimeisin liike ruutukoordinaateissa.
        /// </summary>
        public Vector MovementOnScreen
        {
            get
            {
                Matrix4x4 screenTransform = screen.GetScreenTransform();

                Vector curXna = new Vector(CurrentState.PosX, CurrentState.PosY);
                Vector prevXna = new Vector(PrevState.PosX, PrevState.PosY);
                Vector curScr = ScreenView.FromDisplayCoords(curXna);
                Vector prevScr = ScreenView.FromDisplayCoords(prevXna);
                Vector curr = curScr.Transform(screenTransform);
                Vector prev = prevScr.Transform(screenTransform);

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
                Matrix4x4 screenTransform = screen.GetScreenTransform();

                Vector curXna = new Vector(CurrentState.PosX, CurrentState.PosY);
                Vector prevXna = new Vector(PrevState.PosX, PrevState.PosY);
                Vector curScr = ScreenView.FromDisplayCoords(curXna);
                Vector prevScr = ScreenView.FromDisplayCoords(prevXna);
                Vector curr = curScr.Transform(screenTransform);
                Vector prev = prevScr.Transform(screenTransform);

                return Game.Instance.Camera.ScreenToWorld(curr) - Game.Instance.Camera.ScreenToWorld(prev);
            }
        }

        /// <summary>
        /// Rullan asento. Vähenee alaspäin ja kasvaa ylöspäin rullattaessa.
        /// </summary>
        public int WheelState
        {
            get
            {
                return CurrentState.ScrollY;
            }
        }

        /// <summary>
        /// Rullan sivusuuntainen asento. Vähenee vasemmalle ja kasvaa oikealle rullattaessa.
        /// </summary>
        public int WheelStateHorizontal
        {
            get
            {
                return CurrentState.ScrollX;
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
                return (CurrentState.ScrollY - PrevState.ScrollY);
            }
        }

        /// <summary>
        /// Rullan asennon muutos viime tarkistuksesta. Vähenee alaspäin ja kasvaa ylöspäin rullattaessa.
        /// Nolla jos rullaa ei ole käytetty.
        /// </summary>
        public int WheelChangeHorizontal
        {
            get
            {
                return (CurrentState.ScrollX - PrevState.ScrollX);
            }
        }

        /// <summary>
        /// Hiiren kuva. Katso myös <see cref="SetCursorImage(Image)"/>
        /// </summary>
        public MouseCursor MouseCursor
        {
            get
            {
                return (MouseCursor)mouse.Cursor.StandardCursor;
            }
            set
            {
                mouse.Cursor.StandardCursor = (StandardCursor)value;
            }
        }

        internal Mouse(ScreenView screen, IInputContext input)
        {
            this.screen = screen;
            this.CurrentState = default;
            this.internalState = default;
            this.mouse = input.Mice[0]; // TODO: Mitä tapahtuu jos koneessa on useampi hiiri kiinni?
            mouse.Scroll += MouseScroll;
            mouse.MouseDown += MouseDown;
            mouse.MouseUp += MouseUp;
            mouse.MouseMove += MouseMove;
        }

        /// <summary>
        /// Asettaa hiiren kuvan.
        /// Jos parametrina annetaan <c>null</c>, muutetaan hiiren kuva takaisin normaaliksi.
        /// </summary>
        /// <param name="img">Hiiren kuva, tai null</param>
        public void SetCursorImage(Image img)
        {
            if (img == null)
            {
                mouse.Cursor.Type = CursorType.Standard;
                return;
            }
            mouse.Cursor.Image = new Silk.NET.Core.RawImage(img.Width, img.Height, img.GetByteArray());
            mouse.Cursor.Type = CursorType.Custom;
            mouse.Cursor.HotspotX = img.Width / 2;
            mouse.Cursor.HotspotY = img.Height / 2;
        }

        private void MouseMove(IMouse arg1, Vector2 arg2)
        {
            internalState.PosX = (int)arg2.X;
            internalState.PosY = (int)arg2.Y;
        }

        private void MouseDown(IMouse arg1, Silk.NET.Input.MouseButton arg2)
        {
            internalState.SetButtonDown(arg2);
        }

        private void MouseUp(IMouse arg1, Silk.NET.Input.MouseButton arg2)
        {
            internalState.SetButtonUp(arg2);
        }

        private void MouseScroll(IMouse arg1, ScrollWheel arg2)
        {
            internalState.ScrollX += (int)arg2.X;
            internalState.ScrollY += (int)arg2.Y;
        }

        private static Vector GetPositionOnWorld(Vector pos)
        {
            return Game.Instance.Camera.ScreenToWorld(pos);
        }

        internal override MouseState GetState()
        {
            return internalState;
        }

        private static ButtonState GetButtonState(MouseState oldState, MouseState newState, MouseButton button)
        {
            bool prevDown = GetButtonDown[button](oldState);
            bool currDown = GetButtonDown[button](newState);

            if (prevDown && currDown)
                return ButtonState.Down;
            if (!prevDown && !currDown)
                return ButtonState.Up;
            if (!prevDown && currDown)
                return ButtonState.Pressed;
            return ButtonState.Released;
        }

        private static HoverState GetHoverState(MouseState oldState, MouseState newState, GameObject obj)
        {
#if ANDROID
            return HoverState.Off;
#else
            bool prevOn = IsCursorOn(Game.Screen, oldState, obj);
            bool currOn = IsCursorOn(Game.Screen, newState, obj);

            if (prevOn && currOn)
                return HoverState.On;
            if (!prevOn && !currOn)
                return HoverState.Off;
            if (!prevOn && currOn)
                return HoverState.Enter;
            return HoverState.Exit;
#endif
        }

        /// <summary>
        /// Palauttaa napin tilan.
        /// </summary>
        /// <param name="button">Nappi</param>
        /// <returns></returns>
        public ButtonState GetButtonState(MouseButton button)
        {
            return GetButtonState(PrevState, CurrentState, button);
        }

        private ChangePredicate<MouseState> MakeTriggerRule(MouseButton button, ButtonState state)
        {
            if (button == MouseButton.None || state == ButtonState.Irrelevant)
                return AlwaysTrigger;

            return delegate (MouseState prev, MouseState curr)
            {
                return GetButtonState(prev, curr, button) == state;
            };
        }

        private ChangePredicate<MouseState> MakeTriggerRule(GameObject obj, HoverState hover, MouseButton button, ButtonState state)
        {
            if (button == MouseButton.None || state == ButtonState.Irrelevant)
            {
                return delegate (MouseState prev, MouseState curr)
                {
                    if (obj == null || obj.IsDestroyed || obj.Layer == null)
                        return false;
                    return GetHoverState(prev, curr, obj) == hover;
                };
            }

            return delegate (MouseState prev, MouseState curr)
            {
                if (obj == null || obj.IsDestroyed || obj.Layer == null)
                    return false;
                return GetButtonState(prev, curr, button) == state && GetHoverState(prev, curr, obj) == hover;
            };
        }

        private ChangePredicate<MouseState> MakeTriggerRule(double moveTrigger)
        {
            return delegate (MouseState prev, MouseState curr)
            {
                double xdist = prev.PosX - curr.PosX;
                double ydist = prev.PosY - curr.PosY;
                return xdist * xdist + ydist * ydist > moveTrigger * moveTrigger;
            };
        }

        //TODO: Sivusuuntaisen skrollauksen kuuntelu, miten olisi paras toteuttaa?
        // Tämä katsoo molemmat suunnat, vai oma kuuntelija sitä varten?

        private ChangePredicate<MouseState> MakeWheelTriggerRule()
        {
            return delegate (MouseState prev, MouseState curr)
            {
                return prev.ScrollY != curr.ScrollY;
            };
        }

        private string GetButtonName(MouseButton b)
        {
            return "Mouse " + b.ToString();
        }

        private string GetButtonName(MouseButton b, GameObject obj)
        {
            if (obj != null && obj.Tag != null)
                return string.Format("{0} on {1}", GetButtonName(b), obj.Tag);

            return GetButtonName(b);
        }

        /// <summary>
        /// Onko hiiren kursori annetun olion päällä.
        /// </summary>
        private static bool IsCursorOn(ScreenView screen, MouseState state, GameObject obj)
        {
            if (obj == null || obj.Layer == null || obj.IsDestroyed)
                return false;
            return obj.IsInside(Game.Instance.Camera.ScreenToWorld(new Vector(state.PosX, state.PosY), obj.Layer));
        }

        /// <summary>
        /// Onko hiiren kursori annetun olion päällä.
        /// </summary>
        public bool IsCursorOn(GameObject obj)
        {
#if ANDROID
            return false;
#else
            if (obj == null || obj.Layer == null || obj.IsDestroyed)
                return false;
            return obj.IsInside(Game.Instance.Camera.ScreenToWorld(new Vector(CurrentState.PosX, CurrentState.PosY), obj.Layer));
#endif
        }

        /// <summary>
        /// Kuuntelee hiiren nappulan painalluksia.
        /// </summary>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener Listen(MouseButton button, ButtonState state, Action handler, string helpText)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(button, state);
            return AddListener(rule, button, GetButtonName(button), helpText, handler);
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
        public Listener Listen<T>(MouseButton button, ButtonState state, Action<T> handler, string helpText, T p)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(button, state);
            return AddListener(rule, button, GetButtonName(button), helpText, handler, p);
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
        public Listener Listen<T1, T2>(MouseButton button, ButtonState state, Action<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(button, state);
            return AddListener(rule, button, GetButtonName(button), helpText, handler, p1, p2);
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
        public Listener Listen<T1, T2, T3>(MouseButton button, ButtonState state, Action<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(button, state);
            return AddListener(rule, button, GetButtonName(button), helpText, handler, p1, p2, p3);
        }

        /// <summary>
        /// Kuuntelee hiiren liikettä.
        /// </summary>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener ListenMovement(double trigger, Action handler, string helpText)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(trigger);
            return AddListener(rule, MouseButton.None, "Mouse movement", helpText, handler);
        }

        /// <summary>
        /// Kuuntelee hiiren liikettä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri kuuntelija-aliohjelmalle</param>
        public Listener ListenMovement<T>(double trigger, Action<T> handler, string helpText, T p)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(trigger);
            return AddListener(rule, MouseButton.None, "Mouse movement", helpText, handler, p);
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
        public Listener ListenMovement<T1, T2>(double trigger, Action<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(trigger);
            return AddListener(rule, MouseButton.None, "Mouse movement", helpText, handler, p1, p2);
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
        public Listener ListenMovement<T1, T2, T3>(double trigger, Action<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(trigger);
            return AddListener(rule, MouseButton.None, "Mouse movement", helpText, handler, p1, p2, p3);
        }

        /// <summary>
        /// Kuuntelee hiirenpainalluksia annetun peliolion päällä.
        /// </summary>
        /// <param name="obj">Olio.</param>
        /// <param name="hoverstate">Tila siitä onko kursori olion päällä, pois, menossa päälle vai poistumassa</param>
        /// <param name="button">Hiiren nappula.</param>
        /// <param name="state">Nappulan tila.</param>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        public Listener ListenOn(GameObject obj, HoverState hoverstate, MouseButton button, ButtonState state, Action handler, string helpText)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(obj, hoverstate, button, state);
            return AddListener(rule, button, GetButtonName(button, obj), helpText, handler);
        }

        /// <summary>
        /// Kuuntelee hiirenpainalluksia annetun peliolion päällä.
        /// </summary>
        /// <typeparam name="T">Tapahtuman käsittelijän parametrin tyyppi.</typeparam>
        /// <param name="obj">Olio, jonka päällä hiiren kursorin tulisi olla.</param>
        /// <param name="hoverstate">Hiiren leijumistila, jolloin painallus katsotaan tapahtuneeksi.
        /// Käytetään vain, jos hiiren nappulan tilaksi annetaan <see cref="ButtonState.Irrelevant"/></param>
        /// <param name="button">Hiiren nappula.</param>
        /// <param name="state">Nappulan tila.</param>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p">Tapahtuman käsittelijän parametri.</param>
        public Listener ListenOn<T>(GameObject obj, HoverState hoverstate, MouseButton button, ButtonState state, Action<T> handler, string helpText, T p)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(obj, hoverstate, button, state);
            return AddListener(rule, button, GetButtonName(button, obj), helpText, handler, p);
        }

        /// <summary>
        /// Kuuntelee hiirenpainalluksia annetun peliolion päällä.
        /// </summary>
        /// <typeparam name="T1">Tapahtuman käsittelijän ensimmäisen parametrin tyyppi.</typeparam>
        /// <typeparam name="T2">Tapahtuman käsittelijän toisen parametrin tyyppi.</typeparam>
        /// <param name="obj">Olio, jonka päällä hiiren kursorin tulisi olla.</param>
        /// <param name="hoverstate">Hiiren leijumistila, jolloin painallus katsotaan tapahtuneeksi.
        /// Käytetään vain, jos hiiren nappulan tilaksi annetaan <see cref="ButtonState.Irrelevant"/></param>
        /// <param name="button">Hiiren nappula.</param>
        /// <param name="state">Nappulan tila.</param>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Tapahtuman käsittelijän ensimmäinen parametri.</param>
        /// <param name="p2">Tapahtuman käsittelijän toinen parametri.</param>
        public Listener ListenOn<T1, T2>(GameObject obj, HoverState hoverstate, MouseButton button, ButtonState state, Action<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(obj, hoverstate, button, state);
            return AddListener(rule, button, GetButtonName(button, obj), helpText, handler, p1, p2);
        }

        /// <summary>
        /// Kuuntelee hiirenpainalluksia annetun peliolion päällä.
        /// </summary>
        /// <typeparam name="T1">Tapahtuman käsittelijän ensimmäisen parametrin tyyppi.</typeparam>
        /// <typeparam name="T2">Tapahtuman käsittelijän toisen parametrin tyyppi.</typeparam>
        /// <typeparam name="T3">Tapahtuman käsittelijän kolmannen parametrin tyyppi.</typeparam>
        /// <param name="obj">Olio, jonka päällä hiiren kursorin tulisi olla.</param>
        /// <param name="hoverstate">Hiiren leijumistila, jolloin painallus katsotaan tapahtuneeksi.
        /// Käytetään vain, jos hiiren nappulan tilaksi annetaan <see cref="ButtonState.Irrelevant"/></param>
        /// <param name="button">Hiiren nappula.</param>
        /// <param name="state">Nappulan tila.</param>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">Tapahtuman käsittelijän ensimmäinen parametri.</param>
        /// <param name="p2">Tapahtuman käsittelijän toinen parametri.</param>
        /// <param name="p3">Tapahtuman käsittelijän kolmas parametri.</param>
        public Listener ListenOn<T1, T2, T3>(GameObject obj, HoverState hoverstate, MouseButton button, ButtonState state, Action<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(obj, hoverstate, button, state);
            return AddListener(rule, button, GetButtonName(button, obj), helpText, handler, p1, p2, p3);
        }

        /// <summary>
        /// Kuuntelee hiiren rullaa
        /// </summary>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <returns></returns>
        public Listener ListenWheel(Action handler, string helpText) => ListenWheelGeneric(handler, helpText, null);

        /// <summary>
        /// Kuuntelee hiiren rullaa
        /// </summary>
        /// <typeparam name="T">Tapahtuman käsittelijän parametrin tyyppi.</typeparam>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Tapahtuman käsittelijän parametri.</param>
        /// <returns></returns>
        public Listener ListenWheel<T>(Action<T> handler, string helpText, T p) => ListenWheelGeneric(handler, helpText, p);

        /// <summary>
        /// Kuuntelee hiiren rullaa
        /// </summary>
        /// <typeparam name="T1">Tapahtuman käsittelijän ensimmäisen parametrin tyyppi.</typeparam>
        /// <typeparam name="T2">Tapahtuman käsittelijän toise parametrin tyyppi.</typeparam>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">Tapahtuman käsittelijän ensimmäinen parametri.</param>
        /// <param name="p2">Tapahtuman käsittelijän toinen parametri.</param>
        /// <returns></returns>
        public Listener ListenWheel<T1, T2>(Action<T1, T2> handler, string helpText, T1 p1, T2 p2) => ListenWheelGeneric(handler, helpText, p1, p2);

        /// <summary>
        /// Kuuntelee hiiren rullaa
        /// </summary>
        /// <typeparam name="T1">Tapahtuman käsittelijän ensimmäisen parametrin tyyppi.</typeparam>
        /// <typeparam name="T2">Tapahtuman käsittelijän toise parametrin tyyppi.</typeparam>
        /// <typeparam name="T3">Tapahtuman käsittelijän kolmannen parametrin tyyppi.</typeparam>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">Tapahtuman käsittelijän ensimmäinen parametri.</param>
        /// <param name="p2">Tapahtuman käsittelijän toinen parametri.</param>
        /// <param name="p3">Tapahtuman käsittelijän kolmas parametri.</param>
        /// <returns></returns>
        public Listener ListenWheel<T1, T2, T3>(Action<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3) => ListenWheelGeneric(handler, helpText, p1, p2, p3);

        /// <summary>
        /// Kuuntelee hiiren rullaa.
        /// </summary>
        /// <param name="handler">Aliohjelma, joka käsittelee </param>
        /// <param name="helpText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Listener ListenWheelGeneric(Delegate handler, string helpText, params object[] parameters)
        {
            // Olisi selkeämpää antaa vain tämä käyttäjän käyttöön, mutta koska kaikki
            // muut Jypelin kuuntelijat myös vaativat parametrien tyyppien määrittelemistä,
            // niin vaaditaan sitten hiiren rullankin kanssa jotta toimintamalli on yhtenäinen.
            // Toki tyypin määrittelyssä on oppilaille suunnatussa kirjastossa se hyvä puoli,
            // että se tarkistaa että määritellyt tyypit (esim. T1 ja T2) eivät poikkea
            // parametrien tyypeistä (ellei parametrien tyyppejä haeta automaattisesti, mikä
            // yleensä tapahtuu).
            ChangePredicate<MouseState> rule = MakeWheelTriggerRule();
            return AddListener(rule, MouseButton.None, "Mouse wheel", helpText, handler, parameters);
        }

        /// <summary>
        /// Kuuntelee hiirenpainalluksia annetun peliolion päällä.
        /// </summary>
        /// <param name="obj">Olio, jonka päällä hiiren kursorin tulisi olla.</param>
        /// <param name="button">Hiiren nappula.</param>
        /// <param name="state">Nappulan tila.</param>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        public Listener ListenOn(GameObject obj, MouseButton button, ButtonState state, Action handler, string helpText)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(obj, HoverState.On, button, state);
            return AddListener(rule, button, GetButtonName(button, obj), helpText, handler);
        }

        /// <summary>
        /// Kuuntelee hiirenpainalluksia annetun peliolion päällä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Olio, jonka päällä hiiren kursorin tulisi olla.</param>
        /// <param name="button">Hiiren nappula.</param>
        /// <param name="state">Nappulan tila.</param>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p"></param>
        public Listener ListenOn<T>(GameObject obj, MouseButton button, ButtonState state, Action<T> handler, string helpText, T p)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(obj, HoverState.On, button, state);
            return AddListener(rule, button, GetButtonName(button, obj), helpText, handler, p);
        }

        /// <summary>
        /// Kuuntelee hiirenpainalluksia annetun peliolion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="obj">Olio, jonka päällä hiiren kursorin tulisi olla.</param>
        /// <param name="button">Hiiren nappula.</param>
        /// <param name="state">Nappulan tila.</param>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Listener ListenOn<T1, T2>(GameObject obj, MouseButton button, ButtonState state, Action<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(obj, HoverState.On, button, state);
            return AddListener(rule, button, GetButtonName(button, obj), helpText, handler, p1, p2);
        }

        /// <summary>
        /// Kuuntelee hiirenpainalluksia annetun peliolion päällä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="obj">Olio, jonka päällä hiiren kursorin tulisi olla.</param>
        /// <param name="button">Hiiren nappula.</param>
        /// <param name="state">Nappulan tila.</param>
        /// <param name="handler">Tapahtuman käsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public Listener ListenOn<T1, T2, T3>(GameObject obj, MouseButton button, ButtonState state, Action<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            ChangePredicate<MouseState> rule = MakeTriggerRule(obj, HoverState.On, button, state);
            return AddListener(rule, button, GetButtonName(button, obj), helpText, handler, p1, p2, p3);
        }
    }
}
