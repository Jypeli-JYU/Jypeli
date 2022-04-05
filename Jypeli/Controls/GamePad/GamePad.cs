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
using System.Diagnostics;
using Jypeli.Controls;
using Jypeli.Controls.GamePad;
using Silk.NET.Input;

namespace Jypeli
{
    /// <summary>
    /// Peliohjain.
    /// </summary>
    public class GamePad : Controller<GamePadState, Enum>
    {
        private SynchronousList<Vibration> vibrations;
        IGamepad gamepad;
        private GamePadState internalState;

        /// <summary>
        /// Vasemman tatin suuntavektori.
        /// Vaihtelee välillä (-1, -1) - (1, 1)
        /// </summary>
        public Vector LeftThumbDirection
        {
            get
            {
                return CurrentState.LeftThumbStick;
            }
        }

        /// <summary>
        /// Oikean tatin suuntavektori.
        /// Vaihtelee välillä (-1, -1) - (1, 1)
        /// </summary>
        public Vector RightThumbDirection
        {
            get
            {
                return CurrentState.RightThumbStick;
            }
        }

        /// <summary>
        /// Vasemman liipaisimen tila.
        /// Vaihtelee välillä 0 - 1.
        /// </summary>
        public double LeftTriggerState
        {
            get { return CurrentState.LeftTrigger; }
        }

        /// <summary>
        /// Oikean liipaisimen tila.
        /// Vaihtelee välillä 0 - 1.
        /// </summary>
        public double RightTriggerState
        {
            get { return CurrentState.RightTrigger; }
        }

        /// <summary>
        /// Vasemman tatin suuntavektorin viimeisin muutos (liike).
        /// </summary>
        public Vector LeftThumbChange
        {
            get
            {
                Vector v = CurrentState.LeftThumbStick - PrevState.LeftThumbStick;
                return new Vector(v.X, v.Y);
            }
        }

        /// <summary>
        /// Oikean tatin suuntavektorin viimeisin muutos (liike).
        /// </summary>
        public Vector RightThumbChange
        {
            get
            {
                Vector v = CurrentState.RightThumbStick - PrevState.RightThumbStick;
                return new Vector(v.X, v.Y);
            }
        }

        /// <summary>
        /// Vasemman liipaisimen tilan viimeisin muutos.
        /// </summary>
        public double LeftTriggerChange
        {
            get { return CurrentState.LeftTrigger - PrevState.LeftTrigger; }
        }

        /// <summary>
        /// Oikean liipaisimen tilan viimeisin muutos.
        /// </summary>
        public double RightTriggerChange
        {
            get { return CurrentState.RightTrigger - PrevState.RightTrigger; }
        }

        internal GamePad(IInputContext input, int index)
        {
            if (input.Gamepads.Count <= index)
                return;
            gamepad = input.Gamepads[index];

            vibrations = new SynchronousList<Vibration>();

            gamepad.ButtonDown += GamepadButtonDown;
            gamepad.ButtonUp += GamepadButtonUp;
            gamepad.ThumbstickMoved += GamepadThumbstickMoved;
            gamepad.TriggerMoved += GamepadTriggerMoved;
        }

        private void GamepadTriggerMoved(IGamepad arg1, Trigger arg2)
        {
            internalState.SetTrigger(arg2);
        }

        private void GamepadThumbstickMoved(IGamepad arg1, Thumbstick arg2)
        {
            internalState.SetThumbstick(arg2);
        }

        private void GamepadButtonUp(IGamepad arg1, Silk.NET.Input.Button arg2)
        {
            internalState.SetButtonUp(arg2);
        }

        private void GamepadButtonDown(IGamepad arg1, Silk.NET.Input.Button arg2)
        {
            internalState.SetButtonDown(arg2);
        }

        internal override GamePadState GetState()
        {
            return internalState;
        }

        private static ChangePredicate<GamePadState> MakeTriggerRule(Button button, ButtonState state)
        {
            return state switch
            {
                ButtonState.Up => delegate (GamePadState prev, GamePadState curr)
                {
                    return (curr.IsButtonUp(button));
                }
                ,

                ButtonState.Down => delegate (GamePadState prev, GamePadState curr)
                {
                    return (curr.IsButtonDown(button));
                }
                ,

                ButtonState.Pressed => delegate (GamePadState prev, GamePadState curr)
                {
                    return (prev.IsButtonUp(button) && curr.IsButtonDown(button));
                }
                ,

                ButtonState.Released => delegate (GamePadState prev, GamePadState curr)
                {
                    return (prev.IsButtonDown(button) && curr.IsButtonUp(button));
                }
                ,

                _ => AlwaysTrigger
            };
        }

        private static ChangePredicate<GamePadState> MakeTriggerRule(AnalogControl control, double moveTrigger)
        {
            return control switch
            {
                AnalogControl.LeftStick => delegate (GamePadState prev, GamePadState curr)
                {
                    double xdist = curr.LeftThumbStick.X - prev.LeftThumbStick.X;
                    double ydist = curr.LeftThumbStick.Y - prev.LeftThumbStick.Y;
                    return xdist * xdist + ydist * ydist > moveTrigger * moveTrigger;
                }
                ,

                AnalogControl.RightStick => delegate (GamePadState prev, GamePadState curr)
                {
                    double xdist = curr.RightThumbStick.X - prev.RightThumbStick.X;
                    double ydist = curr.RightThumbStick.Y - prev.RightThumbStick.Y;
                    return xdist * xdist + ydist * ydist > moveTrigger * moveTrigger;
                }
                ,

                AnalogControl.LeftTrigger => delegate (GamePadState prev, GamePadState curr)
                {
                    return Math.Abs(curr.LeftTrigger - prev.LeftTrigger) > moveTrigger;
                }
                ,

                AnalogControl.RightTrigger => delegate (GamePadState prev, GamePadState curr)
                {
                    return Math.Abs(curr.RightTrigger - prev.RightTrigger) > moveTrigger;
                }
                ,

                _ => throw new ArgumentException(control.ToString() + " is not a valid analog control for a GamePad"),
            };
        }

        private static string GetButtonName(Button b)
        {
            return b.ToString();
        }

        private static string GetAnalogName(AnalogControl a)
        {
            return a.ToString();
        }

        /// <summary>
        /// Täristää peliohjainta.
        /// </summary>
        /// <param name="leftMotor">Vasemmanpuoleisen moottorin tärinän määrä (maksimi 1).</param>
        /// <param name="rightMotor">Oikeanpuoleisen moottorin tärinän määrä (maksimi 1) .</param>
        /// <param name="leftAcceleration">Vasemmanpuoleisen moottorin tärinäkiihtyvyys (yksikköä sekunnissa).</param>
        /// <param name="rightAcceleration">Oikeanpuoleisen moottorin tärinäkiihtyvyys (yksikköä sekunnissa).</param>
        /// <param name="time">Aika, jonka tärinä kestää (sekunteina).</param>
        [Obsolete("Ei toistaiseksi toiminnassa!")]
        public void Vibrate(double leftMotor, double rightMotor, double leftAcceleration, double rightAcceleration, double time)
        {
            vibrations?.Add(new Vibration(gamepad, leftMotor, rightMotor, leftAcceleration, rightAcceleration, time));
        }

        /// <summary>
        /// Lopettaa täristyksen.
        /// </summary>
        public void StopVibration()
        {
            vibrations?.Clear();
        }

        internal void UpdateVibrations(Time time)
        {
            vibrations?.Update(time);
        }

        /// <summary>
        /// Kuuntelee peliohjaimen nappulan painalluksia.
        /// </summary>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener Listen(Button button, ButtonState state, Action handler, string helpText)
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule(button, state);
            return AddListener(rule, button, GetButtonName(button), helpText, handler);
        }

        /// <summary>
        /// Kuuntelee peliohjaimen nappulan painalluksia.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri kuuntelija-aliohjelmalle</param>
        public Listener Listen<T>(Button button, ButtonState state, Action<T> handler, string helpText, T p)
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule(button, state);
            return AddListener(rule, button, GetButtonName(button), helpText, handler, p);
        }

        /// <summary>
        /// Kuuntelee peliohjaimen nappulan painalluksia.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri kuuntelija-aliohjelmalle</param>
        /// <param name="p2">2. parametri kuuntelija-aliohjelmalle</param>
        public Listener Listen<T1, T2>(Button button, ButtonState state, Action<T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule(button, state);
            return AddListener(rule, button, GetButtonName(button), helpText, handler, p1, p2);
        }

        /// <summary>
        /// Kuuntelee peliohjaimen nappulan painalluksia.
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
        public Listener Listen<T1, T2, T3>(Button button, ButtonState state, Action<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule(button, state);
            return AddListener(rule, button, GetButtonName(button), helpText, handler, p1, p2, p3);
        }

        /// <summary>
        /// Kuuntelee analogisen kontrollin (tatin tai liipaisimen) liikettä.
        /// </summary>
        /// <param name="control">Kuunneltava kontrolli</param>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <returns></returns>
        public Listener ListenAnalog(AnalogControl control, double trigger, Action<AnalogState> handler, string helpText)
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule(control, trigger);
            Action analogHandler = delegate
            { handler(GenerateAnalogState(control)); };
            return AddListener(rule, control, GetAnalogName(control), helpText, analogHandler);
        }

        private GamePadAnalogState GenerateAnalogState(AnalogControl control)
        {
            return control switch
            {
                AnalogControl.LeftStick => new GamePadAnalogState(LeftThumbDirection.Magnitude, LeftThumbChange.Magnitude, LeftThumbDirection, LeftThumbChange),
                AnalogControl.RightStick => new GamePadAnalogState(RightThumbDirection.Magnitude, RightThumbChange.Magnitude, RightThumbDirection, RightThumbChange),
                AnalogControl.LeftTrigger => new GamePadAnalogState(LeftTriggerState, LeftTriggerChange),
                AnalogControl.RightTrigger => new GamePadAnalogState(RightTriggerState, RightTriggerChange),
                _ => throw new NotImplementedException("Unsupported Controller / GamePad control for ListenAnalog: " + control.ToString()),
            };
        }

        /// <summary>
        /// Kuuntelee analogisen kontrollin (tatin tai liipaisimen) liikettä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control">Kuunneltava kontrolli</param>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p">Parametri</param>
        /// <returns></returns>
        public Listener ListenAnalog<T>(AnalogControl control, double trigger, Action<AnalogState, T> handler, string helpText, T p)
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule(control, trigger);
            Action analogHandler = delegate
            { handler(GenerateAnalogState(control), p); };
            return AddListener(rule, control, GetAnalogName(control), helpText, analogHandler);
        }

        /// <summary>
        /// Kuuntelee analogisen kontrollin (tatin tai liipaisimen) liikettä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="control">Kuunneltava kontrolli</param>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        /// <returns></returns>
        public Listener ListenAnalog<T1, T2>(AnalogControl control, double trigger, Action<AnalogState, T1, T2> handler, string helpText, T1 p1, T2 p2)
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule(control, trigger);
            Action analogHandler = delegate
            { handler(GenerateAnalogState(control), p1, p2); };
            return AddListener(rule, control, GetAnalogName(control), helpText, analogHandler);
        }

        /// <summary>
        /// Kuuntelee analogisen kontrollin (tatin tai liipaisimen) liikettä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="control">Kuunneltava kontrolli</param>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <param name="p1">1. parametri</param>
        /// <param name="p2">2. parametri</param>
        /// <param name="p3">3. parametri</param>
        /// <returns></returns>
        public Listener ListenAnalog<T1, T2, T3>(AnalogControl control, double trigger, Action<AnalogState, T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3)
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule(control, trigger);
            Action analogHandler = delegate
            { handler(GenerateAnalogState(control), p1, p2, p3); };
            return AddListener(rule, control, GetAnalogName(control), helpText, analogHandler);
        }
    }
}
