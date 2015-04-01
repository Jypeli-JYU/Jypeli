using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace Jypeli.Widgets
{
    /// <summary>
    /// Helpompikäyttöinen parhaiden pisteiden lista.
    /// </summary>
    public class EasyHighScore
    {
        HighScoreWindow _hsWindow;
        ScoreList score;
        string fileName;

        /// <summary>
        /// Listaikkuna.
        /// </summary>
        public HighScoreWindow HighScoreWindow
        {
            get { return _hsWindow; }
        }

        /// <summary>
        /// Nimensyöttöikkuna.
        /// </summary>
        public InputWindow NameInputWindow
        {
            get { return _hsWindow.NameInputWindow; }
        }

        /// <summary>
        /// Pisin sallittu nimen pituus.
        /// </summary>
        public int MaxNameLength
        {
            get { return _hsWindow.MaxNameLength; }
            set { _hsWindow.MaxNameLength = value; }
        }

        /// <summary>
        /// Pisteikkunan teksti.
        /// </summary>
        public string Text
        {
            get { return _hsWindow.Message.Text; }
            set { _hsWindow.Message.Text = value; }
        }

        /// <summary>
        /// Nimensyöttöikkunan teksti.
        /// </summary>
        public string EnterText
        {
            get { return _hsWindow.NameInputWindow.Message.Text; }
            set { _hsWindow.NameInputWindow.Message.Text = value; }
        }

        /// <summary>
        /// Listaikkunan väri.
        /// </summary>
        public Color Color
        {
            get
            {
                return _hsWindow.Color;
            }
            set
            {
                _hsWindow.Color = value;
            }
        }

        /// <summary>
        /// Luo uuden parhaiden pisteiden ikkunan.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi</param>
        /// <param name="places">Pistesijojen lukumäärä</param>
        /// <param name="defaultName">Oletusnimi tyhjille paikoille.</param>
        public EasyHighScore(string fileName, int places, string defaultName)
        {
            this.score = new ScoreList(places, false, 0, defaultName);
            this.fileName = fileName;
            Game.AssertInitialized( LoadData );
            Game.AssertInitialized( InitHSWindow );
        }

        /// <summary>
        /// Luo uuden parhaiden pisteiden ikkunan.
        /// </summary>
        /// <param name="fileName">Tiedoston nimi</param>
        /// <param name="places">Pistesijojen lukumäärä</param>
        public EasyHighScore(string fileName, int places)
        {
            this.score = new ScoreList(places, false, 0);
            this.fileName = fileName;
            Game.AssertInitialized( LoadData );
            Game.AssertInitialized( InitHSWindow );
        }

        /// <summary>
        /// Luo uuden parhaiden pisteiden ikkunan.
        /// </summary>
        /// <param name="places">Pistesijojen lukumäärä</param>
        public EasyHighScore(int places)
            : this(Game.Name + "_score.xml", places)
        {
        }

        /// <summary>
        /// Luo uuden parhaiden pisteiden ikkunan kymmenellä pistesijalla.
        /// </summary>
        public EasyHighScore()
            : this(Game.Name + "_score.xml", 10)
        {
        }

        void InitHSWindow()
        {
            _hsWindow = new HighScoreWindow( "High score", score );
        }

        /// <summary>
        /// Näyttää parhaat pisteet.
        /// </summary>
        public void Show()
        {
            Game.Instance.Add( HighScoreWindow );
        }

        /// <summary>
        /// Tyhjentää parhaat pisteet.
        /// </summary>
        public void Clear()
        {
            score = new ScoreList( score._scores.Length, score.Reverse, 0 );
            Game.DataStorage.Save<ScoreList>( score, fileName );
        }

        /// <summary>
        /// Näyttää parhaat pisteet, ja jos annetut pisteet riittävät, antaa syöttää nimen listalle.
        /// Lista tallennetaan automaattisesti.
        /// </summary>
        /// <param name="newScore"></param>
        public void EnterAndShow(double newScore)
        {
            HighScoreWindow.NameInputWindow.Closed += SaveData;
            HighScoreWindow.ShowNameInput( newScore );
            Game.Instance.Add( HighScoreWindow );
        }

        private void SaveData(Window sender)
        {
            HighScoreWindow.NameInputWindow.Closed -= SaveData;
            Game.DataStorage.Save<ScoreList>(score, fileName);
        }

        private void LoadData()
        {
            score = Game.DataStorage.TryLoad<ScoreList>(score, fileName);
        }
    }
}
