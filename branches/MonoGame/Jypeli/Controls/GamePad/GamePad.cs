﻿#region MIT License
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
using Jypeli.Controls;
using Jypeli.Controls.GamePad;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using XnaGamePad = Microsoft.Xna.Framework.Input.GamePad;

namespace Jypeli
{
    public class GamePad : Controller<GamePadState>
    {
        private PlayerIndex playerIndex;
        private SynchronousList<Vibration> vibrations;

        /// <summary>
        /// Vasemman tatin suuntavektori.
        /// Vaihtelee välillä (-1, -1) - (1, 1)
        /// </summary>
        public Vector LeftThumbDirection
        {
            get
            {
                Vector2 v = CurrentState.ThumbSticks.Left;
                return new Vector( v.X, v.Y );
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
                Vector2 v = CurrentState.ThumbSticks.Right;
                return new Vector( v.X, v.Y );
            }
        }

        /// <summary>
        /// Vasemman liipaisimen tila.
        /// Vaihtelee välillä 0 - 1.
        /// </summary>
        public double LeftTriggerState
        {
            get { return CurrentState.Triggers.Left; }
        }

        /// <summary>
        /// Oikean liipaisimen tila.
        /// Vaihtelee välillä 0 - 1.
        /// </summary>
        public double RightTriggerState
        {
            get { return CurrentState.Triggers.Right; }
        }

        /// <summary>
        /// Oikean tatin suuntavektorin viimeisin muutos (liike).
        /// </summary>
        public Vector LeftThumbChange
        {
            get
            {
                Vector2 v = CurrentState.ThumbSticks.Left - PrevState.ThumbSticks.Left;
                return new Vector( v.X, v.Y );
            }
        }

        /// <summary>
        /// Oikean tatin suuntavektorin viimeisin muutos (liike).
        /// </summary>
        public Vector RightThumbChange
        {
            get
            {
                Vector2 v = CurrentState.ThumbSticks.Right - PrevState.ThumbSticks.Right;
                return new Vector( v.X, v.Y );
            }
        }

        /// <summary>
        /// Vasemman liipaisimen tilan viimeisin muutos.
        /// </summary>
        public double LeftTriggerChange
        {
            get { return CurrentState.Triggers.Left - PrevState.Triggers.Left; }
        }

        /// <summary>
        /// Oikean liipaisimen tilan viimeisin muutos.
        /// </summary>
        public double RightTriggerChange
        {
            get { return CurrentState.Triggers.Right - PrevState.Triggers.Left; }
        }

        internal GamePad( PlayerIndex index )
        {
            playerIndex = index;
            vibrations = new SynchronousList<Vibration>();
        }

        internal override GamePadState GetState()
        {
            return XnaGamePad.GetState( playerIndex );
        }

        private ChangePredicate<GamePadState> MakeTriggerRule( Button b, ButtonState state )
        {
            Buttons buttons = (Buttons)b;

            switch ( state )
            {
                case ButtonState.Up:
                    return delegate( GamePadState prev, GamePadState curr ) { return ( curr.IsButtonUp( buttons ) ); };

                case ButtonState.Down:
                    return delegate( GamePadState prev, GamePadState curr ) { return ( curr.IsButtonDown( buttons ) ); };

                case ButtonState.Pressed:
                    return delegate( GamePadState prev, GamePadState curr ) { return ( prev.IsButtonUp( buttons ) && curr.IsButtonDown( buttons ) ); };

                case ButtonState.Released:
                    return delegate( GamePadState prev, GamePadState curr ) { return ( prev.IsButtonDown( buttons ) && curr.IsButtonUp( buttons ) ); };
            }

            return AlwaysTrigger;
        }

        private ChangePredicate<GamePadState> MakeTriggerRule( AnalogControl control, double moveTrigger )
        {
            switch ( control )
            {
                case AnalogControl.LeftStick:
                    return delegate( GamePadState prev, GamePadState curr )
                    {
                        double xdist = curr.ThumbSticks.Left.X - prev.ThumbSticks.Left.X;
                        double ydist = curr.ThumbSticks.Left.Y - prev.ThumbSticks.Left.Y;
                        return xdist * xdist + ydist * ydist > moveTrigger * moveTrigger;
                    };

                case AnalogControl.RightStick:
                    return delegate( GamePadState prev, GamePadState curr )
                    {
                        double xdist = curr.ThumbSticks.Right.X - prev.ThumbSticks.Right.X;
                        double ydist = curr.ThumbSticks.Right.Y - prev.ThumbSticks.Right.Y;
                        return xdist * xdist + ydist * ydist > moveTrigger * moveTrigger;
                    };

                case AnalogControl.LeftTrigger:
                    return delegate( GamePadState prev, GamePadState curr )
                    {
                        return Math.Abs( curr.Triggers.Left - prev.Triggers.Left ) > moveTrigger;
                    };

                case AnalogControl.RightTrigger:
                    return delegate( GamePadState prev, GamePadState curr )
                    {
                        return Math.Abs( curr.Triggers.Right - prev.Triggers.Right ) > moveTrigger;
                    };
            }

            throw new ArgumentException( control.ToString() + " is not a valid analog control for a GamePad" );
        }

        private string GetButtonName( Button b )
        {
            return String.Format( "GamePad{0} {1}", playerIndex.ToString(), b.ToString() );
        }

        private string GetAnalogName( AnalogControl a )
        {
            return String.Format( "GamePad{0} {1}", playerIndex.ToString(), a.ToString() );
        }

        /// <summary>
        /// Täristää peliohjainta.
        /// </summary>
        /// <param name="leftMotor">Vasemmanpuoleisen moottorin tärinän määrä (maksimi 1).</param>
        /// <param name="rightMotor">Oikeanpuoleisen moottorin tärinän määrä (maksimi 1) .</param>
        /// <param name="leftAcceleration">Vasemmanpuoleisen moottorin tärinäkiihtyvyys (yksikköä sekunnissa).</param>
        /// <param name="rightAcceleration">Oikeanpuoleisen moottorin tärinäkiihtyvyys (yksikköä sekunnissa).</param>
        /// <param name="time">Aika, jonka tärinä kestää (sekunteina).</param>
        public void Vibrate( double leftMotor, double rightMotor, double leftAcceleration, double rightAcceleration, double time )
        {
            vibrations.Add( new Vibration( leftMotor, rightMotor, leftAcceleration, rightAcceleration, time ) );
        }

        /// <summary>
        /// Lopettaa täristyksen.
        /// </summary>
        public void StopVibration()
        {
            vibrations.Clear();
        }

        internal void UpdateVibrations( Time time )
        {
            vibrations.Update( time );
            Vibration.Execute( playerIndex, vibrations );
        }

        /// <summary>
        /// Kuuntelee peliohjaimen nappulan painalluksia.
        /// </summary>
        /// <param name="button">Nappi</param>
        /// <param name="state">Napin tila</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        public Listener Listen( Button button, ButtonState state, Action handler, string helpText )
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule( button, state );
            return AddListener( rule, GetButtonName( button ), helpText, handler );
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
        public Listener Listen<T>( Button button, ButtonState state, Action<T> handler, string helpText, T p )
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule( button, state );
            return AddListener( rule, GetButtonName( button ), helpText, handler, p );
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
        public Listener Listen<T1, T2>( Button button, ButtonState state, Action<T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule( button, state );
            return AddListener( rule, GetButtonName( button ), helpText, handler, p1, p2 );
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
        public Listener Listen<T1, T2, T3>( Button button, ButtonState state, Action<T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule( button, state );
            return AddListener( rule, GetButtonName( button ), helpText, handler, p1, p2, p3 );
        }

        /// <summary>
        /// Kuuntelee analogisen kontrollin (tatin tai liipaisimen) liikettä.
        /// </summary>
        /// <param name="control">Kuunneltava kontrolli</param>
        /// <param name="trigger">Raja jonka liikkeen tulee ylittää</param>
        /// <param name="handler">Kuuntelija-aliohjelma</param>
        /// <param name="helpText">Ohjeteksti</param>
        /// <returns></returns>
        public Listener ListenAnalog( AnalogControl control, double trigger, Action<AnalogState> handler, string helpText )
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule( control, trigger );
            return AddListener( rule, GetAnalogName( control ), helpText, handler );
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
        public Listener ListenAnalog<T>( AnalogControl control, double trigger, Action<AnalogState, T> handler, string helpText, T p )
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule( control, trigger );
            return AddListener( rule, GetAnalogName( control ), helpText, handler, p );
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
        public Listener ListenAnalog<T1, T2>( AnalogControl control, double trigger, Action<AnalogState, T1, T2> handler, string helpText, T1 p1, T2 p2 )
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule( control, trigger );
            return AddListener( rule, GetAnalogName( control ), helpText, handler, p1, p2 );
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
        public Listener ListenAnalog<T1, T2, T3>( AnalogControl control, double trigger, Action<AnalogState, T1, T2, T3> handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            ChangePredicate<GamePadState> rule = MakeTriggerRule( control, trigger );
            return AddListener( rule, GetAnalogName( control ), helpText, handler, p1, p2, p3 );
        }
    }
}
