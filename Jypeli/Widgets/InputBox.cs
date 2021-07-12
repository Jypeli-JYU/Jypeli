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
 * Authors: Tomi Karppinen, Tero Jäntti, Rami Pasanen, Mikko Röyskö
 */

using System;
using Microsoft.Xna.Framework;

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

        /// <summary>
        /// Kursorin sijainti tekstissä.
        /// </summary>
        public int CursorPos { get; set; }

        private int distFromEnd = 0;
        private int firstVisibleChar = 0;

        /// <inheritdoc/>
        public override Vector PreferredSize
        {
            get
            {
                return new Vector( Font.CharacterWidth * WidthInCharacters + 2 * XMargin, Font.CharacterHeight + 2 * YMargin );
            }
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Laatikkoon kirjoitettu teksti.
        /// Jos asetetaan teksti joka on pidempi kuin <c>MaxCharacters</c>, se katkaistaan.
        /// </summary>
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

        /// <summary>
        /// Kun tekstin sisältö muuttuu
        /// </summary>
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

            Cursor = new Widget(Font.MeasureSize("I").X/2, Font.CharacterHeight);
            Cursor.Color = new Color(255, 0, 0, 100);
            Add( Cursor );
            AddedToGame += UpdateCursorPosition;

            cursorBlinkTimer = new Timer();
            cursorBlinkTimer.Interval = 0.5;
            cursorBlinkTimer.Timeout += BlinkCursor;

            AddedToGame += OnAdded;
            Removed += OnRemoved;
        }
       
        private void OnAdded()
        {
            cursorBlinkTimer.Start();

            // TODO: Should also work on android
#if ANDROID
            ShowVirtualKeyboard();
#endif

            Game.Instance.Window.TextInput += InputText;
            associatedListeners.Add(Game.Instance.Keyboard.Listen(Key.Back, ButtonState.Pressed, EraseText, null).InContext(this));
            associatedListeners.Add(Game.Instance.Keyboard.Listen(Key.Left, ButtonState.Pressed, MoveCursor, null, -1).InContext(this));
            associatedListeners.Add(Game.Instance.Keyboard.Listen(Key.Right, ButtonState.Pressed, MoveCursor, null, 1).InContext(this));
            // TODO: Jos nuolta pitää hetken pohjassa, alkaa kursori liikkua nopeasti sivusuunnassa.
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

        private new void OnRemoved()
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

        private void UpdateCursorPosition()
        {
            Cursor.Height = Font.MeasureSize("I").Y;
            string shownText = ShownText();
            int endPos = CursorPos - firstVisibleChar;
            double strLen = Font.MeasureSize(shownText.Substring(0, endPos < 0 ? 0 : endPos > shownText.Length ? shownText.Length : endPos)).X;
            Cursor.Left = Left + strLen + Cursor.Width*2;
            
        }

        private void MoveCursor(int dir)
        {
            if (CursorPos > 0 && dir == -1)
            {
                CursorPos--;
                while (CursorPos < firstVisibleChar) {
                    ShownText();
                    distFromEnd++;
                }
            }

            if (CursorPos < Text.Length && dir == 1)
            {
                CursorPos++;
                while (CursorPos > Text.Length - distFromEnd)
                {
                    distFromEnd--;
                    ShownText();
                }
            }

            UpdateCursorPosition();
        }

        private void InputText( object sender, TextInputEventArgs e )
        {
            if ( !this.ControlContext.Active ) return;
            char input = e.Character;
            if ( input == 0x7F || input == 0x08 || input == 0x1B ) return; // delete, backspace, esc

            // TODO: Ei välttämättä tarvi välittää
            /*
			if ( !this.Font.XnaFont.Characters.Contains( e.Character ) )
            {
                // Unsupported character
                return;
            }
            */
            AddText(input.ToString());
        }


        private void AddText(string text)
        {
            Text = Text.Insert(CursorPos, text);
            CursorPos++;
            OnTextChanged();
            UpdateCursorPosition();
        }

        private void EraseText()
        {
            if (Text.Length == 0 || CursorPos == 0) return;
            Text = Text.Remove(CursorPos-1, 1);
            CursorPos--;
            OnTextChanged();
            UpdateCursorPosition();
        }

        private string ShownText()
        {
            string shownText = "";

            for ( int i = Text.Length - 1 - distFromEnd; i >= 0; i-- )
            {
                string newText = Text[i] + shownText;

                if (Font.XnaFont.MeasureString(newText).X >= Width - XMargin * 2)
                {
                    firstVisibleChar = i + 1;
                    break;
                }

                firstVisibleChar = i;
                shownText = newText;
            }

            return shownText;
        }
        
        /// <inheritdoc/>
        public override void Draw(Matrix parentTransformation, Matrix transformation)
        {
            if(!IsTruncated)
                base.Draw(parentTransformation, transformation, Text);
            else
                base.Draw(parentTransformation, transformation, ShownText());
        }
    }
}
