#region MIT License
/*
 * Copyright (c) 2009-2011 University of Jyväskylä, Department of Mathematical
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
 * Authors: Tomi Karppinen, Tero Jäntti
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

#if WINDOWS_PHONE
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
#endif

namespace Jypeli
{
    /// <summary>
    /// Ikkuna, joka sisältää käyttäjän määrittelemän kysymyksen, tekstinsyöttökentän ja
    /// OK-painikkeen. Ikkunan koko määräytyy automaattisesti tekstin ja ruudun koon mukaan.
    /// </summary>
    public class InputWindow : CustomQueryWindow<InputBox>
    {
        internal override bool OkButtonOnPhone { get { return true; } }

        /// <summary>
        /// Suurin määrä merkkejä joita tekstilaatikkoon voi kirjoittaa.
        /// </summary>
        public int MaxCharacters
        {
            get { return InputBox.MaxCharacters; }
            set { InputBox.MaxCharacters = value; }
        }

        /// <summary>
        /// Vastauslaatikko.
        /// </summary>
        public InputBox InputBox
        {
            get { return QueryWidget; }
        }

        /// <summary>
        /// Näytetäänkö ikkuna puhelimella. Oletuksena false, jolloin näytetään vain
        /// puhelimen oma tekstinsyöttöikkuna.
        /// </summary>
        public bool ShowWindowOnPhone { get; set; }

        /// <summary>
        /// Syöttöikkunatapahtumien käsittelijä.
        /// </summary>
        public delegate void InputWindowHandler( InputWindow sender );

        /// <summary>
        /// Tapahtuu kun käyttäjä on syöttänyt tekstin ja painanut OK / sulkenut ikkunan.
        /// </summary>
        public event InputWindowHandler TextEntered;

        private void OnTextEntered()
        {
            if ( TextEntered != null )
                TextEntered(this);
        }

        /// <summary>
        /// Alustaa uuden tekstinkyselyikkunan.
        /// </summary>
        /// <param name="question">Kysymys</param>
        public InputWindow( string question )
            : base( question )
        {
            Closed += new WindowHandler( InputWindow_Closed );
            AddedToGame += AddListeners;
        }

        /// <summary>
        /// Alustaa uuden tekstinkyselyikkunan.
        /// </summary>
        /// <param name="width">Ikkunan leveys</param>
        /// <param name="height">Ikkunan korkeus</param>
        /// <param name="question">Kysymys</param>
        public InputWindow( double width, double height, string question )
            : base( width, height, question )
        {
            Closed += new WindowHandler( InputWindow_Closed );
        }

        protected override InputBox CreateQueryWidget()
        {
            //double widthInChars = Width / Font.Default.CharacterWidth;
            //return new InputBox( (int)widthInChars - 1 );
            return new InputBox( 40 );
        }

        static void InputWindow_Closed( Window sender )
        {
            ((InputWindow)sender).OnTextEntered();
        }

        private void Cancel()
        {
            InputBox.Text = "";
            Close();
        }

        private void AddListeners()
        {
#if WINDOWS_PHONE
            if ( !ShowWindowOnPhone )
            {
                if ( !Guide.IsVisible )
                    Guide.BeginShowKeyboardInput( PlayerIndex.One, "", Message.Text, InputBox.Text, TouchTextEntered, this );
            }
#endif

            Game.Instance.Keyboard.Listen( Key.Enter, ButtonState.Pressed, Close, null ).InContext( this );
            Game.Instance.Keyboard.Listen( Key.Escape, ButtonState.Pressed, Cancel, null ).InContext( this );
            Game.Instance.PhoneBackButton.Listen( Cancel, null ).InContext( this );
        }

#if WINDOWS_PHONE
        void TouchTextEntered( IAsyncResult result )
        {
            string typedText = Guide.EndShowKeyboardInput( result );
            InputBox.Text = typedText != null ? typedText : "";
            Close();
        }
#endif
    }
}
