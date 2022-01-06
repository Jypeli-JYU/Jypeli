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
    /// Ikkuna, joka kysyy käyttäjältä kyllä tai ei -kysymyksen.
    /// </summary>
    public class YesNoWindow : MultiSelectWindow
    {
        /// <summary>
        /// Tapahtuu kun käyttäjä valitsee "kyllä"-vaihtoehdon.
        /// </summary>
        public event Action Yes;

        /// <summary>
        /// Tapahtuu kun käyttäjä valitsee "ei"-vaihtoehdon.
        /// </summary>
        public event Action No;

        private void OnYes()
        {
            if ( Yes != null ) Yes();
        }

        private void OnNo()
        {
            if ( No != null ) No();
        }

        /// <summary>
        /// Luo uuden kyselyikkunan.
        /// </summary>
        /// <param name="question">Kysymys</param>
        public YesNoWindow( string question )
            : this( question, "Yes", "No" )
        {
        }

        /// <summary>
        /// Kyllä/Ei ikkuna
        /// </summary>
        /// <param name="question">Kysymys</param>
        /// <param name="yesString">Kyllä-vaihtoehto</param>
        /// <param name="noString">Ei-vaihtoehto</param>
        public YesNoWindow( string question, string yesString, string noString )
            : base( question, yesString, noString )
        {
            AddItemHandler( 0, OnYes );
            AddItemHandler( 1, OnNo );

            Buttons[0].Color = Color.Green;
            Buttons[1].Color = Color.DarkRed;

            DefaultCancel = 1;

            AddedToGame += AddControls;
        }

        private void AddControls()
        {
            //var l1 = Buttons[0].AddShortcut(Button.A);
            //var l2 = Buttons[1].AddShortcut(Button.B);
            //associatedListeners.AddRange(l1);
            //associatedListeners.AddRange(l2);
        }
    }
}
