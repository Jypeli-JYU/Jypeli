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
using Jypeli.GameObjects;

namespace Jypeli
{
    /// <summary>
    /// Ikkuna, joka sisältää käyttäjän määrittelemän viestin ja OK-painikkeen.
    /// Ikkunan koko määräytyy automaattisesti tekstin ja ruudun koon mukaan.
    /// </summary>
    public class MessageWindow : Window
    {
        /// <summary>
        /// Viesti.
        /// </summary>
        public Label Message { get; private set; }

        /// <summary>
        /// OK-painike
        /// </summary>
        public PushButton OKButton { get; private set; }

        public override Color Color
        {
            get
            {
                return base.Color;
            }
            set
            {
                OKButton.Color = Color.Darker( value, 40 );
                base.Color = value;
            }
        }

        /// <summary>
        /// Alustaa uuden viesti-ikkunan.
        /// </summary>
        /// <param name="question">Kysymys</param>
        public MessageWindow( string question )
        {
            Layout = new VerticalLayout { Spacing = 20, LeftPadding = 15, RightPadding = 15, TopPadding = 15, BottomPadding = 15 };

            Message = new Label( 400, 100, question ) { SizeMode = TextSizeMode.Wrapped, VerticalSizing = Sizing.Expanding };
            Add( Message );

#if !WINDOWS_PHONE && !ANDROID
            OKButton = new PushButton( "OK" );
            OKButton.Clicked += new Action( Close );
            Add( OKButton );
#endif

            AddedToGame += AddListeners;
        }

        private void AddListeners()
        {
            Game.Instance.PhoneBackButton.Listen( delegate { Close(); }, null ).InContext( this );
            Game.Instance.TouchPanel.Listen( ButtonState.Pressed, delegate { Close(); }, null ).InContext( this );
            Game.Instance.Keyboard.Listen( Key.Enter, ButtonState.Pressed, Close, null ).InContext( this );
            Game.Instance.Keyboard.Listen( Key.Space, ButtonState.Pressed, Close, null ).InContext( this );

            foreach ( var controller in Game.Instance.GameControllers )
            {
                controller.Listen( Button.A, ButtonState.Pressed, Close, null ).InContext( this );
                controller.Listen( Button.B, ButtonState.Pressed, Close, null ).InContext( this );
            }
        }
    }
}
