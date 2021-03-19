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
 * Authors: Tomi Karppinen, Tero Jäntti, Rami Pasanen
 */


using System;
using Microsoft.Xna.Framework;

// TODO: text input on Windows Phone

namespace Jypeli
{
    /// <summary>
    /// Laatikko, johon käyttäjä voi syöttää tekstiä.
    /// </summary>
    public class InputBox : Label
    {
        Timer cursorBlinkTimer;

        /// <summary>
        /// Alustaa uuden syöttökentän.
        /// </summary>
        public InputBox()
            : this(15)
        {
        }

        /// <summary>
        /// Tekstilaatikon pituus kirjaimissa.
        /// </summary>
        public int WidthInCharacters { get; set; }

        /// <summary>
        /// Suurin määrä merkkejä joita tekstilaatikkoon voi kirjoittaa.
        /// </summary>
        public int MaxCharacters { get; set; }

        /// <summary>
        /// Kursori.
        /// </summary>
        public Widget Cursor { get; set; }

        public override Vector PreferredSize
        {
            get
            {
                return new Vector( Font.CharacterWidth * WidthInCharacters + 2 * XMargin, Font.CharacterHeight + 2 * YMargin );
            }
        }

        public override Vector Size
        {
            get { return base.Size; }
            set
            {
                base.Size = value;
                if ( Cursor != null )
                    UpdateCursorPosition();
            }
        }

        public override string Text
        {
            get { return base.Text; }
            set
            {
				base.Text = value.Length > MaxCharacters ?
					base.Text = value.Substring (0, MaxCharacters) : value;
 
				UpdateCursorPosition();
            }
        }

        /// <summary>
        /// Tapahtuma tekstin muuttumiselle.
        /// </summary>
        public event Action<string> TextChanged;

        protected void OnTextChanged()
        {
            if ( TextChanged != null )
                TextChanged( Text );
        }

#if ANDROID
        private bool vkSubscribed = false;
#endif

        /// <summary>
        /// Alustaa uuden syöttökentän.
        /// </summary>
        /// <param name="characters">
        /// Kentän leveys merkkeinä. Tämä ei rajoita kirjoitettavan tekstin pituutta.
        /// <see cref="MaxCharacters"/>
        /// </param>
        public InputBox( int characters )
            : base()
        {
            MaxCharacters = int.MaxValue;
            WidthInCharacters = characters;
            HorizontalAlignment = HorizontalAlignment.Left;
            HorizontalSizing = Sizing.Expanding;
            XMargin = 7;
            YMargin = 2;
            TextColor = Color.Black;
            Color = new Color( 0, 255, 255, 150 );
            BorderColor = new Color( 200, 200, 200 );
            SizeMode = TextSizeMode.None;
            Size = PreferredSize;

            Cursor = new Widget( Font.CharacterWidth, Font.CharacterHeight );
            Cursor.Color = new Color(255, 0, 0, 100);
            Add( Cursor );
            AddedToGame += UpdateCursorPosition;

            cursorBlinkTimer = new Timer();
            cursorBlinkTimer.Interval = 0.5;
            cursorBlinkTimer.Timeout += BlinkCursor;

            AddedToGame += OnAdded;
            Removed += onRemoved;
        }
       
        private void OnAdded()
        {
            cursorBlinkTimer.Start();

            // TODO: Should also work on android
#if ANDROID
            ShowVirtualKeyboard();
#endif

            Game.Instance.Window.TextInput += InputText;
			var l = Game.Instance.Keyboard.Listen(Key.Back, ButtonState.Pressed, EraseText, null).InContext(this);
            associatedListeners.Add(l);
        }

#if ANDROID

        private void ShowVirtualKeyboard()
        {
            // For some reason OnAdded() gets called twice on Android but only once on Windows
            // when using EasyHighScore. What makes it even more odd is that when subscribing 
            // to AddedToGame from outside, the event is only fired once.
            // We use vkSubscribed as a work-around to avoid subscribing to the key events 
            // multiple times.
            Game.VirtualKeyboard.Show();
            if (!vkSubscribed)
            {
                Game.VirtualKeyboard.InputEntered += VirtualKeyboard_InputEntered;
                Game.VirtualKeyboard.EnterPressed += VirtualKeyboard_EnterPressed;
                Game.VirtualKeyboard.BackspacePressed += VirtualKeyboard_BackspacePressed;
            }
            cursorBlinkTimer.Start();
            vkSubscribed = true;
        }

        private void VirtualKeyboard_BackspacePressed(object sender, EventArgs e)
        {
            EraseText();
        }

        private void VirtualKeyboard_EnterPressed(object sender, EventArgs e)
        {
            HideVirtualKeyboard();
        }

        private void VirtualKeyboard_InputEntered(object sender, Controls.Keyboard.VirtualKeyboardInputEventArgs e)
        {
            AddText(e.Text);
        }

        private void HideVirtualKeyboard()
        {
            Game.VirtualKeyboard.Hide();
            Game.VirtualKeyboard.InputEntered -= VirtualKeyboard_InputEntered;
            Game.VirtualKeyboard.EnterPressed -= VirtualKeyboard_EnterPressed;
            Game.VirtualKeyboard.BackspacePressed -= VirtualKeyboard_BackspacePressed;
            cursorBlinkTimer.Stop();
            Cursor.IsVisible = false;
            vkSubscribed = false;
        }

#endif

        private void onRemoved()
        {
            cursorBlinkTimer.Stop();
#if ANDROID
            HideVirtualKeyboard();
#endif
            Game.Instance.Window.TextInput -= InputText;

        }

        private void BlinkCursor()
        {
            Cursor.IsVisible = !Cursor.IsVisible;
        }

        void UpdateCursorPosition()
        {
            Cursor.Left = Math.Min( -Width / 2 + XMargin + TextSize.X, Width / 2 - Font.CharacterWidth );
        }


        void InputText( object sender, TextInputEventArgs e )
        {
            if ( !this.ControlContext.Active ) return;
			if ( e.Character == 0x7F || e.Character == 0x08 ) return;

            // TODO: Ei välttämättä tarvi välittää
            /*
			if ( !this.Font.XnaFont.Characters.Contains( e.Character ) )
            {
                // Unsupported character
                return;
            }
            */
            AddText(e.Character.ToString());
        }


        void AddText(string text)
        {
            Text += text;
            OnTextChanged();
            UpdateCursorPosition();
        }

        void EraseText()
        {
            if (Text.Length == 0) return;
            Text = Text.Substring(0, Text.Length - 1);
            OnTextChanged();
        }

        public override void Draw( Matrix parentTransformation, Matrix transformation )
        {
            if ( ! IsTruncated )
                base.Draw( parentTransformation, transformation, Text );
            else
            {
                String shownText = "";

                for ( int i = Text.Length - 1; i >= 0; i-- )
                {
                    String newText = Text[i] + shownText.ToString();

                    if ( Font.XnaFont.MeasureString( newText ).X >= Width )
                        break;

                    shownText = newText;
                }

                base.Draw( parentTransformation, transformation, shownText );
            }
        }
    }
}
