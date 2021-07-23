using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontStashSharp;

namespace Jypeli.Controls.Keyboard
{
    /// <summary>
    /// Virtuaalinen näppäimistö.
    /// Tarkoitettu ensisijaisesti mobiilialustoille, joiden oman 
    /// virtuaalinäppäimistön käyttö MonoGamen ja Jypelin kanssa on
    /// haastavaa tai mahdotonta.
    /// </summary>
    class VirtualKeyboard// : DrawableGameComponent
    {
        internal const int KEY_PADDING = 5;

        private static readonly VirtualKeyInfo[][] keyLines = new VirtualKeyInfo[][]
        {
            new VirtualKeyInfo[] {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0", new VirtualKeyInfo("<=", "", 1.0, VirtualKeyType.Backspace)},
            new VirtualKeyInfo[] {"Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "Å"},
            new VirtualKeyInfo[] {"A", "S", "D", "F", "G", "H", "J", "K", "L", "Ö", "Ä"},
            new VirtualKeyInfo[] {"Z", "X", "C", "V", "B", "N", "M", ".", ",", "-", "*"},
            new VirtualKeyInfo[] {new VirtualKeyInfo("Space", " ", 7), new VirtualKeyInfo("= >", "", 4, VirtualKeyType.Enter)},
        };

        public VirtualKeyboard(Game jypeliGame)// : base(jypeliGame)
        {
            this.game = jypeliGame;
        }

        public event EventHandler<VirtualKeyboardInputEventArgs> InputEntered;
        public event EventHandler EnterPressed;
        public event EventHandler BackspacePressed;

        private Game game;
        //private SpriteBatch spriteBatch;

        private List<VirtualKey> keys;

        private int Y;
        private int X = 0;
        private int Width;
        private int Height;

        private Image whitePixelTexture;

        public bool Visible { get; private set; }
        public bool Enabled { get; private set; }

        //private MouseState prevMouseState;

        public void Initialize()
        {
            //base.Initialize();

            keys = new List<VirtualKey>();
            Width = 200;//Game.GraphicsDevice.Viewport.Width;
            Height = 200;//Game.GraphicsDevice.Viewport.Height / 2;

            Y = 200;//Game.GraphicsDevice.Viewport.Height - Height;

            int highestKeyCount = GetKeyCountOnSingleLine();

            int baseKeyWidth = (Width - KEY_PADDING * (1 + highestKeyCount)) / highestKeyCount;
            int keyHeight = ((Height - KEY_PADDING) / keyLines.Length) - KEY_PADDING;

            // Create keys
            for (int y = 0; y < keyLines.Length; y++)
            {
                int yCoord = KEY_PADDING + y * (keyHeight + KEY_PADDING);
                int xCoord = KEY_PADDING;

                for (int x = 0; x < keyLines[y].Length; x++)
                {
                    VirtualKeyInfo keyInfo = keyLines[y][x];

                    var virtualKey = new VirtualKey(game, keyInfo.DisplayString, keyInfo.Value,
                        xCoord, yCoord, (int)(baseKeyWidth * keyInfo.WidthMultiplier), keyHeight,
                        new Font(60), keyInfo.KeyType);
                    xCoord += virtualKey.Width;
                    xCoord += KEY_PADDING;

                    keys.Add(virtualKey);
                }
            }

            //spriteBatch = new SpriteBatch(GraphicsDevice);
            //whitePixelTexture = XnaRenderer.CreateTexture(GraphicsDevice, Color.White, 1, 1);
        }

        private int GetKeyCountOnSingleLine()
        {
            int highestKeyCount = int.MinValue;

            foreach (VirtualKeyInfo[] line in keyLines)
            {
                if (highestKeyCount < line.Length)
                    highestKeyCount = line.Length;
            }

            return highestKeyCount;
        }

        /// <summary>
        /// Piilottaa virtuaalisen näppäimistön.
        /// </summary>
        public void Hide()
        {
            Visible = false;
            Enabled = false;
        }

        /// <summary>
        /// Avaa virtuaalisen näppäimistön.
        /// </summary>
        public void Show()
        {
            Visible = true;
            Enabled = true;
        }

        /// <summary>
        /// Käsittelee näppäimen painalluksen.
        /// </summary>
        /// <param name="key">Näppäin.</param>
        private void HandleKeyPress(VirtualKey key)
        {
            switch (key.Type)
            {
                case VirtualKeyType.Enter:
                    EnterPressed?.Invoke(this, EventArgs.Empty);
                    break;
                case VirtualKeyType.Backspace:
                    BackspacePressed?.Invoke(this, EventArgs.Empty);
                    break;
                case VirtualKeyType.Normal:
                default:
                    InputEntered?.Invoke(this, new VirtualKeyboardInputEventArgs(key.Value));
                    break;
            }

            key.Pressed();
        }

        /*
        public override void Update(Time gameTime)
        {
            bool checkForKeyPress;

#if NETCOREAPP
            MouseState mouseState = XnaMouse.GetState();

            checkForKeyPress = prevMouseState.LeftButton == XnaButtonState.Pressed &&
                mouseState.LeftButton == XnaButtonState.Released;
#endif

#if ANDROID
            TouchCollection touchCollection = XnaTouchPanel.GetState();
            checkForKeyPress = touchCollection.Count > 0 && touchCollection[0].State == TouchLocationState.Released;
            // TODO multi-touch support?
            Vector2 touchPos = touchCollection.Count > 0 ? touchCollection[0].Position : Vector2.Zero;
#endif

            foreach (VirtualKey key in keys)
            {
                if (checkForKeyPress)
                {
#if WINDOWS
                    if (mouseState.Y > Y + key.Y && mouseState.Y < Y + key.Y + key.Height &&
                        mouseState.X > X + key.X && mouseState.X < X + key.X + key.Width)
                    {
                        HandleKeyPress(key);
                        checkForKeyPress = false;
                    }
#endif

#if ANDROID
                    if (touchPos.Y > Y + key.Y && touchPos.Y < Y + key.Y + key.Height &&
                        touchPos.X > X + key.X && touchPos.X < X + key.X + key.Width)
                    {
                        HandleKeyPress(key);
                        checkForKeyPress = false;
                    }
#endif
                }

                key.Update(gameTime);
            }

#if WINDOWS
            prevMouseState = mouseState;
#endif

            base.Update(gameTime);
        }
        */
        /*
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied,
                null, null, null, null, null);
            XnaRenderer.FillRectangle(whitePixelTexture, spriteBatch, 
                new XnaRectangle(X, Y, Width, Height), XnaColor.White);
            foreach (VirtualKey key in keys)
            {
                key.Draw(whitePixelTexture, spriteBatch, X, Y);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }*/
    }

    /// <summary>
    /// Yksittäinen näppäin virtuaalisessa näppäimistössä.
    /// </summary>
    internal class VirtualKey
    {
        private static Color backgroundColorIdle = Color.Gray;
        private static Color textColorIdle = Color.White;
        private static Color backgroundColorPressed = Color.White;
        private static Color textColorPressed = Color.Red;

        private const float HIGHLIGHT_TIME_ON_PRESS = 0.125f;

        public VirtualKey(Game game, string text)
        {
            UIText = text;
        }

        public VirtualKey(Game game, string text, string value,
            int x, int y, int width, int height, Font font, VirtualKeyType type) : this(game, text)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Font = font;
            Value = value;
            Type = type;
        }

        /// <summary>
        /// Näppäimen tyyppi.
        /// </summary>
        public VirtualKeyType Type { get; private set; }

        /// <summary>
        /// Teksti, joka syötetään näppäintä painettaessa.
        /// </summary>
        public string Value { get; private set; }
        
        /// <summary>
        /// Käyttöliittymässä näppäimen kohdalla näytetty teksti.
        /// </summary>
        public string UIText { get; private set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Font Font { get; set; }

        private double highlightTime;

        private Color backgroundColor = backgroundColorIdle;
        private Color textColor = textColorIdle;

        /// <summary>
        /// Kutsutaan, kun näppäintä on painettu.
        /// </summary>
        public void Pressed()
        {
            highlightTime = HIGHLIGHT_TIME_ON_PRESS;
            backgroundColor = backgroundColorPressed;
            textColor = textColorPressed;
        }
        /*
        /// <summary>
        /// Päivittää näppäimen tilaa.
        /// </summary>
        /// <param name="gameTime">Kertoo kuluneen peliajan.</param>
        public void Update(GameTime gameTime)
        {
            if (highlightTime > 0.0)
            {
                highlightTime -= gameTime.ElapsedGameTime.TotalSeconds;
                if (highlightTime < 0.0)
                {
                    backgroundColor = backgroundColorIdle;
                    textColor = textColorIdle;
                }
            }
        }
        
        /// <summary>
        /// Piirtää näppäimen.
        /// </summary>
        public void Draw(Texture2D whitePixelTexture, SpriteBatch sb, int xOffset, int yOffset)
        {
            var drawRectangle = new XnaRectangle(xOffset + X, yOffset + Y, Width, Height);
            XnaRenderer.FillRectangle(whitePixelTexture, sb, drawRectangle, backgroundColor);
            XnaRenderer.DrawRectangle(whitePixelTexture, sb, drawRectangle, 2, new XnaColor(196, 196, 196, 255));
            XnaRenderer.DrawStringWithShadow(sb, UIText, Font.XnaFont, 
                new Vector2(xOffset + X + VirtualKeyboard.KEY_PADDING, yOffset + Y + VirtualKeyboard.KEY_PADDING), textColor);
        }*/
    }
    /*
    /// <summary>
    /// Sisältää näppäimistön piitämiseen käytettyjä metodeja.
    /// </summary>
    internal static class XnaRenderer
    {
        public static void FillRectangle(Texture2D whitePixelTexture, SpriteBatch sb, XnaRectangle rect, XnaColor color)
        {
            sb.Draw(whitePixelTexture, rect, color);
        }

        public static Texture2D CreateTexture(GraphicsDevice gd, Color color, int width, int height)
        {
            Texture2D texture = new Texture2D(gd, width, height, false, SurfaceFormat.Color);

            Color[] colorArray = new Color[width * height];

            for (int i = 0; i < colorArray.Length; i++)
                colorArray[i] = color;

            texture.SetData(colorArray);

            return texture;
        }

        public static void DrawStringWithShadow(SpriteBatch sb, string text, DynamicSpriteFont font, Vector2 location, XnaColor color)
        {
            font.DrawText(Graphics.FontRenderer, text, (new Vector2(location.X + 1f, location.Y + 1f)).ToSystemNumerics(), XnaColor.Black.ToSystemDrawing());
            font.DrawText(Graphics.FontRenderer, text, location.ToSystemNumerics(), color.ToSystemDrawing());
        }

        public static void DrawRectangle(Texture2D whitePixelTexture, SpriteBatch sb, XnaRectangle rect, int thickness, XnaColor color)
        {
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X, rect.Y, rect.Width, thickness), color);
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X, rect.Y + thickness, thickness, rect.Height - thickness), color);
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
        }
    }
    */

    /// <summary>
    /// Näppäimen tyyppi.
    /// </summary>
    internal enum VirtualKeyType
    {
        /// <summary>
        /// Näppäin syöttää tekstiä kuin "tavallinen" näppäin.
        /// </summary>
        Normal,

        /// <summary>
        /// Näppäin toimii Enter-painikkeena.
        /// </summary>
        Enter,

        /// <summary>
        /// Näppäin toimii Backspace-painikkeena.
        /// </summary>
        Backspace
    }


    /// <summary>
    /// Tietue, joka sisältää olennaiset tiedot näppäimestä sen luontia varten.
    /// </summary>
    internal struct VirtualKeyInfo
    {
        public VirtualKeyInfo(string displayString, string value, double widthMultiplier = 1.0, VirtualKeyType keyType = VirtualKeyType.Normal) : this()
        {
            DisplayString = displayString;
            WidthMultiplier = widthMultiplier;
            KeyType = keyType;
            Value = value;
        }

        public static implicit operator VirtualKeyInfo(char c)
        {
            return new VirtualKeyInfo(c.ToString(), c.ToString());
        }

        public static implicit operator VirtualKeyInfo(string s)
        {
            return new VirtualKeyInfo(s, s);
        }

        public string DisplayString { get; private set; }
        public double WidthMultiplier { get; private set; }
        public VirtualKeyType KeyType { get; private set; }
        public string Value { get; private set; }
    }


    /// <summary>
    /// Tavallisen näppäimen (ei Enter tai Backspace) painalluksesta syntyvän tapahtuman tiedot.
    /// </summary>
    internal class VirtualKeyboardInputEventArgs : EventArgs
    {
        public VirtualKeyboardInputEventArgs(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Syötetty teksti.
        /// </summary>
        public string Text { get; private set; }
    }
}
