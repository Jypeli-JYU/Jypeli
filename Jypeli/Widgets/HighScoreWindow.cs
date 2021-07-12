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

namespace Jypeli.Widgets
{
    /// <summary>
    /// Parhaiden pisteiden ikkuna.
    /// </summary>
    public class HighScoreWindow : CustomQueryWindow<ScoreListWidget>
    {
        double lastScore;
        string nameStr = "";

        /// <summary>
        /// Nimensyöttöikkuna.
        /// </summary>
        public InputWindow NameInputWindow { get; private set; }

        /// <summary>
        /// Listakomponentti.
        /// </summary>
        public ScoreListWidget List
        {
            get { return QueryWidget; }
        }

        /// <summary>
        /// Pisin mahdollinen nimi, jonka listaan voi syöttää.
        /// </summary>
        public int MaxNameLength
        {
            get { return NameInputWindow.MaxCharacters; }
            set { NameInputWindow.MaxCharacters = value; }
        }

        internal override bool OkButtonOnPhone { get { return true; } }

        /// <summary>
        /// Luo uuden parhaiden pisteiden ikkunan.
        /// </summary>
        /// <param name="message">Viesti</param>
        /// <param name="list">Lista</param>
        public HighScoreWindow( string message, ScoreList list )
            : base( message )
        {
            Initialize( list );
        }

        /// <summary>
        /// Luo uuden parhaiden pisteiden ikkunan.
        /// </summary>
        /// <param name="width">Leveys</param>
        /// <param name="height">Korkeus</param>
        /// <param name="message">Viesti</param>
        /// <param name="list">Lista</param>
        public HighScoreWindow( double width, double height, string message, ScoreList list )
            : base( width, height, message )
        {
            Initialize( list );
        }

        private void Initialize( ScoreList list )
        {
            this.List.Bind( list );
            NameInputWindow = new InputWindow( "Congratulations, you got a high score of %p points! Please enter your name." );
            AddedToGame += AddControls;
        }

        /// <summary>
        /// Luo uuden parhaiden pisteiden ikkunan.
        /// Tämä versio antaa pelaajan kirjoittaa nimensä listalle jos tulos
        /// on tarpeeksi hyvä.
        /// </summary>
        /// <param name="normalMessage">Normaalisti näytettävä viesti</param>
        /// <param name="nameMessage">Viesti joka näytetään kun pelaaja pääsee listalle</param>
        /// <param name="list">Lista</param>
        /// <param name="newScore">Viimeisimmän pelin pistemäärä</param>
        public HighScoreWindow( string normalMessage, string nameMessage, ScoreList list, double newScore )
            : base( normalMessage )
        {
            Initialize( list );
            NameInputWindow.Message.Text = nameMessage;
            ShowNameInput( newScore );
        }

        /// <summary>
        /// Luo uuden parhaiden pisteiden ikkunan.
        /// Tämä versio antaa pelaajan kirjoittaa nimensä listalle jos tulos
        /// on tarpeeksi hyvä.
        /// </summary>
        /// <param name="width">Leveys</param>
        /// <param name="height">Korkeus</param>
        /// <param name="normalMessage">Normaalisti näytettävä viesti</param>
        /// <param name="nameMessage">Viesti joka näytetään kun pelaaja pääsee listalle</param>
        /// <param name="list">Lista</param>
        /// <param name="newScore">Viimeisimmän pelin pistemäärä</param>
        public HighScoreWindow( double width, double height, string normalMessage, string nameMessage, ScoreList list, double newScore )
            : base( width, height, normalMessage )
        {
            Initialize( list );
            NameInputWindow.Message.Text = nameMessage;
            ShowNameInput( newScore );
        }

        /// <summary>
        /// Näyttää nimensyöttöikkunan.
        /// </summary>
        /// <param name="newScore"></param>
        public void ShowNameInput( double newScore )
        {
            this.lastScore = newScore;

            if ( ( this.List.Items as ScoreList).Qualifies( newScore ) )
            {
                if ( IsAddedToGame ) showNameWindow();
                else AddedToGame += showNameWindow;
            }
        }

        void AddControls()
        {
            var l = Jypeli.Game.Instance.PhoneBackButton.Listen( Close, null ).InContext( this );
            associatedListeners.Add(l);
        }

        void showNameWindow()
        {
            AddedToGame -= showNameWindow;
            IsVisible = false;

            nameStr = NameInputWindow.Message.Text;
            NameInputWindow.Message.Text = String.Format( NameInputWindow.Message.Text.Replace( "%p", "{0}" ), lastScore );
            NameInputWindow.InputBox.Text = ( List.Items as ScoreList ).LastEnteredName;
            NameInputWindow.TextEntered += nameEntered;
            Game.Add( NameInputWindow );
        }

        void nameEntered( InputWindow sender )
        {
            sender.TextEntered -= nameEntered;
            NameInputWindow.Message.Text = nameStr;

            string newName = sender.InputBox.Text.Trim();
            if ( !string.IsNullOrEmpty( newName ) )
                ( List.Items as ScoreList ).Add( newName, lastScore );

            IsVisible = true;
        }

        /// <inheritdoc/>
        protected override ScoreListWidget CreateQueryWidget()
        {
            return new ScoreListWidget() { Color = Color.Transparent };
        }
    }
}
