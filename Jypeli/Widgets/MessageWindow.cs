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

        ///<inheritdoc/>
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
        /// <param name="message">Viesti</param>
        public MessageWindow( string message )
        {
            Layout = new VerticalLayout { Spacing = 20, LeftPadding = 15, RightPadding = 15, TopPadding = 15, BottomPadding = 15 };

            int maxWidth = (int)Game.Screen.Width - 30;

            Message = new Label( Math.Min(maxWidth, Font.Default.MeasureSize(message).X), 100, message )
                { SizeMode = TextSizeMode.Wrapped, VerticalSizing = Sizing.Expanding };
            Add( Message );

#if !ANDROID
            OKButton = new PushButton( "OK" );
            OKButton.Clicked += new Action( Close );
            Add( OKButton );
#endif

            AddedToGame += AddListeners;
        }

        private void AddListeners()
        {
            var l1 = Game.Instance.PhoneBackButton.Listen( delegate { Close(); }, null ).InContext( this );
            var l2 = Game.Instance.TouchPanel.Listen( ButtonState.Pressed, delegate { Close(); }, null ).InContext( this );
            var l3 = Game.Instance.Keyboard.Listen( Key.Enter, ButtonState.Pressed, Close, null ).InContext( this );
            var l4 = Game.Instance.Keyboard.Listen( Key.Space, ButtonState.Pressed, Close, null ).InContext( this );
            associatedListeners.AddItems(l1, l2, l3, l4);

            foreach ( var controller in Game.Instance.GameControllers )
            {
                l1 = controller.Listen( Button.A, ButtonState.Pressed, Close, null ).InContext( this );
                l2 = controller.Listen( Button.B, ButtonState.Pressed, Close, null ).InContext( this );
                associatedListeners.AddItems(l1, l2);
            }
        }
    }
}
