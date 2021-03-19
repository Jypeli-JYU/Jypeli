using System;

namespace Jypeli.Widgets
{
    /// <summary>
    /// Helpompikäyttöinen parhaiden pisteiden lista.
    /// </summary>
    public class EasyHighScore
    {
        ScoreList score;
        string fileName;

        /// <summary>
        /// Listaikkuna.
        /// </summary>
        public HighScoreWindow HighScoreWindow { get; private set; }

        /// <summary>
        /// Nimensyöttöikkuna.
        /// </summary>
        public InputWindow NameInputWindow
        {
            get { return HighScoreWindow.NameInputWindow; }
        }

        /// <summary>
        /// Pisin sallittu nimen pituus.
        /// </summary>
        public int MaxNameLength
        {
            get { return HighScoreWindow.MaxNameLength; }
            set { HighScoreWindow.MaxNameLength = value; }
        }

        /// <summary>
        /// Pisteikkunan teksti.
        /// </summary>
        public string Text
        {
            get { return HighScoreWindow.Message.Text; }
            set { HighScoreWindow.Message.Text = value; }
        }

        /// <summary>
        /// Nimensyöttöikkunan teksti.
        /// </summary>
        public string EnterText
        {
            get { return HighScoreWindow.NameInputWindow.Message.Text; }
            set { HighScoreWindow.NameInputWindow.Message.Text = value; }
        }

        /// <summary>
        /// Listaikkunan väri.
        /// </summary>
        public Color Color
        {
            get
            {
                return HighScoreWindow.Color;
            }
            set
            {
                HighScoreWindow.Color = value;
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
            Vector entrySize = Font.Default.MeasureSize("XXXXXXXXXXXXXXXXXXXXXXXXX");
            int width = (int)Math.Min(Game.Screen.Width - 30.0, entrySize.X);
            int height = (int)Math.Min(Game.Screen.Height - 30.0, entrySize.Y * 20.0);
            // TODO ^ 20.0 is just a silly magic number, no real logic behind it
            // it seems to work well enough with both the medium and huge fonts, so it's
            // a fitting temporary solution until someone has the time to figure out the 
            // mess of layouts and HighScoreWindow / ScoreListWidget and the classes 
            // they're derived from

            HighScoreWindow = new HighScoreWindow(width, height, "High score", score);
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
            score = new ScoreList( score.Count, score.Reverse, 0 );
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
