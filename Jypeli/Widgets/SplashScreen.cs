using System;

namespace Jypeli.Widgets
{
    /// <summary>
    /// Aloitusruutu, joka voidaan näyttää ennen pelin käynnistämistä.
    /// </summary>
    public class SplashScreen : Window
    {
        private string _controlHelp = DefaultControlHelp;
        private string _loadingText = "Loading...";

        /// <summary>
        /// Pelin nimen näyttävä tekstikenttä.
        /// </summary>
        public Label NameLabel { get; private set; }

        /// <summary>
        /// Pelin tekijänoikeudet näyttävä tekstikenttä.
        /// </summary>
        public Label CopyrightLabel { get; private set; }

        /// <summary>
        /// Pelin tekijät näyttävä tekstikenttä.
        /// </summary>
        public Label AuthorsLabel { get; private set; }

        /// <summary>
        /// Päätekstikenttä.
        /// </summary>
        public StringListWidget TextBody { get; private set; }

        /// <summary>
        /// Aloita peli painamalla... -tekstin näyttävä tekstikenttä.
        /// Käytä ominaisuuksia ControlHelp ja LoadingText jos haluat muuttaa itse tekstiä.
        /// </summary>
        public Label StartLabel { get; private set; }

        /// <summary>
        /// Kontrolliohje (Aloita peli painamalla Enter / Xbox A).
        /// </summary>
        public string ControlHelp
        {
            get { return _controlHelp; }
            set
            {
                if (StartLabel.Text == _controlHelp) StartLabel.Text = value;
                _controlHelp = value;
            }
        }

        /// <summary>
        /// Latausteksti.
        /// </summary>
        public string LoadingText
        {
            get { return _loadingText; }
            set
            {
                if (StartLabel.Text == _loadingText) StartLabel.Text = value;
                _loadingText = value;
            }
        }

        /// <summary>
        /// Tapahtuu kun ruudusta poistutaan.
        /// Tee varsinaiset pelin alustukset tämän tapahtuman käsittelijässä.
        /// </summary>
        public event Action GameStarted;

        #region Default dimensions

        private static double DefaultWidth
        {
            get
            {
                return Game.Screen.Width;
            }
        }

        private static double DefaultTextWidth
        {
            get
            {
#if ANDROID
                return Game.Device.DisplayResolution == DisplayResolution.Small ? 300 : 500;
#else
                return 500;
#endif
            }
        }

        private static double DefaultHeight
        {
            get
            {
                return Game.Screen.Height;
            }
        }

        private static double DefaultSpacing
        {
            get
            {
                return 50;
            }
        }

        private static string DefaultControlHelp
        {
            get
            {
#if ANDROID
                return "Start the game by tapping here";
#else
                return "Start the game by pressing Enter";
#endif
            }
        }

        #endregion

        /// <summary>
        /// Alustaa aloitusruudun.
        /// </summary>
        public SplashScreen(string gameName, string authors, string copyright, string textBody)
            : base(DefaultWidth, DefaultHeight)
        {
            Layout = new VerticalLayout() { Spacing = DefaultSpacing };

            NameLabel = InitializeTextDisplay(gameName, Color.Red);
            CopyrightLabel = InitializeTextDisplay(copyright, Color.Blue);
            AuthorsLabel = InitializeTextDisplay(authors, Color.Blue);
            StartLabel = InitializeTextDisplay(ControlHelp, Color.Green);

            double targetWidth = 2 * this.Width / 3;
            NameLabel.TextScale = new Vector(targetWidth / NameLabel.TextSize.X, 2);
            NameLabel.SizeMode = TextSizeMode.Wrapped;
            CopyrightLabel.SizeMode = TextSizeMode.Wrapped;

            TextBody = new StringListWidget();
            TextBody.ItemAligment = HorizontalAlignment.Center;
            TextBody.Width = DefaultTextWidth;
            TextBody.TextColor = Color.Black;
            TextBody.Color = new Color(0, 0, 255, 4);

            TextBody.Text = textBody;

            StartLabel.SizeMode = TextSizeMode.Wrapped;

            AddedToGame += AddControls;

            Add(NameLabel);
            Add(CopyrightLabel);
            Add(AuthorsLabel);
            Add(TextBody);
            Add(StartLabel);
        }

        private void AddControls()
        {
            /*var l1 = Game.Keyboard.Listen(Key.Enter, ButtonState.Pressed, BeginLoad, null, StartLabel).InContext(this);
            var l2 = Game.Keyboard.Listen(Key.Escape, ButtonState.Pressed, Game.Instance.Exit, null).InContext(this); ;
            var l3 = Game.Mouse.Listen(MouseButton.Left, ButtonState.Down, BeginLoad, null, StartLabel).InContext(this);
            associatedListeners.AddItems(l1, l2, l3);

            for (int i = 0; i < Game.GameControllers.Count; i++)
            {
                l1 = Game.GameControllers[i].Listen(Button.A, ButtonState.Pressed, BeginLoad, null, StartLabel).InContext(this);
                l2 = Game.GameControllers[i].Listen(Button.B, ButtonState.Pressed, Game.Instance.Exit, null).InContext(this);
                associatedListeners.AddItems(l1, l2);
            }

            l1 = Game.TouchPanel.ListenOn(StartLabel, ButtonState.Pressed, delegate { BeginLoad(StartLabel); }, null).InContext(this);
            l2 = Game.PhoneBackButton.Listen(Game.Instance.Exit, null).InContext(this);
            associatedListeners.AddItems(l1, l2);*/
        }

        private void BeginLoad(Label aloitusohje)
        {
            // Don't trigger twice
            if (aloitusohje.Text == LoadingText)
                return;

            aloitusohje.TextColor = Color.Red;
            aloitusohje.Text = LoadingText;
            Timer.SingleShot(0.1, ResumeLoad);
        }

        private void ResumeLoad()
        {
            Close();
            if (GameStarted != null)
                GameStarted();
        }

        private Label InitializeTextDisplay(string text, Color textColor)
        {
            Label kentta = new Label(text);

            kentta.HorizontalAlignment = HorizontalAlignment.Center;
            kentta.VerticalAlignment = VerticalAlignment.Top;
            kentta.Width = DefaultTextWidth;
            kentta.Text = text;
            kentta.TextColor = textColor;

            return kentta;
        }
    }
}
