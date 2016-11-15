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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli.Controls;
using Microsoft.Xna.Framework;

namespace Jypeli
{
    public partial class Game : ControlContexted
    {
        private ListenContext _context;
        private List<Controller> _controllers;

        /// <summary>
        /// Näppäimistö.
        /// </summary>
        public Keyboard Keyboard { get; private set; }

        /// <summary>
        /// Hiiri.
        /// </summary>
        public Mouse Mouse { get; private set; }

        /// <summary>
        /// Kosketusnäyttö
        /// </summary>
        public TouchPanel TouchPanel { get; private set; }

        /// <summary>
        /// Puhelimen takaisin-näppäin.
        /// </summary>
        public BackButton PhoneBackButton { get; private set; }

        /// <summary>
        /// Lista kaikista peliohjaimista järjestyksessä.
        /// </summary>
        public List<GamePad> GameControllers { get; private set; } 

        /// <summary>
        /// Ensimmäinen peliohjain.
        /// </summary>
        public GamePad ControllerOne { get { return GameControllers[0]; } }

        /// <summary>
        /// Toinen peliohjain.
        /// </summary>
        public GamePad ControllerTwo { get { return GameControllers[1]; } }

        /// <summary>
        /// Kolmas peliohjain.
        /// </summary>
        public GamePad ControllerThree { get { return GameControllers[2]; } }

        /// <summary>
        /// Neljäs peliohjain.
        /// </summary>
        public GamePad ControllerFour { get { return GameControllers[3]; } }

        /// <summary>
        /// Pelin pääohjainkonteksti.
        /// </summary>
        public ListenContext ControlContext
        {
            get { return Instance._context; }
        }

        public bool IsModal
        {
            get { return false; }
        }

#if WINDOWS_PHONE || ANDROID
        public new bool IsMouseVisible
        {
            get { return false; }
            set { }
        }
#endif

        private void InitControls()
        {
            _context = new ListenContext() { Active = true };

            Keyboard = new Keyboard();
            Mouse = new Mouse( Screen );
            PhoneBackButton = new BackButton();
            TouchPanel = new TouchPanel( Screen );

            GameControllers = new List<GamePad>( 4 );
            GameControllers.Add( new GamePad( PlayerIndex.One ) );
            GameControllers.Add( new GamePad( PlayerIndex.Two ) );
            GameControllers.Add( new GamePad( PlayerIndex.Three ) );
            GameControllers.Add( new GamePad( PlayerIndex.Four ) );

            _controllers = new List<Controller>();
            _controllers.Add( Keyboard );
#if !WINDOWS_PHONE && !ANDROID
            _controllers.Add( Mouse );
#endif
            _controllers.Add( TouchPanel );
#if WINDOWS_PHONE || ANDROID
            _controllers.Add( PhoneBackButton );
#endif

#if WINDOWS || LINUX
            _controllers.AddRange( GameControllers );
#endif

            IsMouseVisible = true;
        }

        private void UpdateControls( Time gameTime )
        {
            _controllers.ForEach( c => c.Update() );
            //_gamePads.ForEach( g => g.UpdateVibrations( gameTime ) );
        }

        /// <summary>
        /// Poistaa kaikki ohjainkuuntelijat.
        /// </summary>
        public void ClearControls()
        {
            _controllers.ForEach( c => c.Clear() );
        }

        /// <summary>
        /// Näyttää kontrollien ohjetekstit.
        /// </summary>
        public void ShowControlHelp()
        {
            _controllers.ForEach( c => MessageDisplay.Add( c.GetHelpTexts() ) );
        }

        /// <summary>
        /// Näyttää kontrollien ohjetekstit tietylle ohjaimelle.
        /// </summary>
        public void ShowControlHelp( Controller controller )
        {
            MessageDisplay.Add( controller.GetHelpTexts() );
        }

        private void ActivateObject( ControlContexted obj )
        {
            obj.ControlContext.Active = true;

            if ( obj.IsModal )
            {
                Game.Instance.ControlContext.SaveFocus();
                Game.Instance.ControlContext.Active = false;

                foreach ( Layer l in Layers )
                {
                    foreach ( IGameObject lo in l.Objects )
                    {
                        ControlContexted co = lo as ControlContexted;
                        if ( lo == obj || co == null )
                            continue;

                        co.ControlContext.SaveFocus();
                        co.ControlContext.Active = false;
                    }
                }
            }
        }

        private void DeactivateObject( ControlContexted obj )
        {
            obj.ControlContext.Active = false;

            if ( obj.IsModal )
            {
                Game.Instance.ControlContext.RestoreFocus();

                foreach ( Layer l in Layers )
                {
                    foreach ( IGameObject lo in l.Objects )
                    {
                        ControlContexted co = lo as ControlContexted;
                        if ( lo == obj || co == null )
                            continue;

                        co.ControlContext.RestoreFocus();
                    }
                }
            }
        }
    }
}
