using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnaGame = Microsoft.Xna.Framework.Game;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Jypeli.Controls.Keyboard
{
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
    /// Virtuaalinen näppäimistö.
    /// Tarkoitettu ensisijaisesti mobiilialustoille, joiden oman 
    /// virtuaalinäppäimistön käyttö MonoGamen ja Jypelin kanssa on
    /// haastavaa tai mahdotonta.
    /// </summary>
    class VirtualKeyboard : DrawableGameComponent
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

        public VirtualKeyboard(Game jypeliGame) : base(jypeliGame)
        {
            this.game = jypeliGame;
        }

        private Game game;
        private SpriteBatch spriteBatch;

        private List<VirtualKey> keys;

        private int Y;
        private int X = 0;
        private int Width;
        private int Height;

        private Texture2D whitePixelTexture;

        public override void Initialize()
        {
            base.Initialize();

            keys = new List<VirtualKey>();
            Width = Game.GraphicsDevice.Viewport.Width;
            Height = Game.GraphicsDevice.Viewport.Height / 2;

            Y = Game.GraphicsDevice.Viewport.Height - Height;

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
                        xCoord, yCoord, (int)(baseKeyWidth * keyInfo.WidthMultiplier), keyHeight, Font.DefaultHuge);
                    xCoord += virtualKey.Width;
                    xCoord += KEY_PADDING;

                    keys.Add(virtualKey);
                }
            }

            spriteBatch = new SpriteBatch(GraphicsDevice);
            whitePixelTexture = XnaRenderer.CreateTexture(GraphicsDevice, Color.White, 1, 1);
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

        public override void Update(GameTime gameTime)
        {
            // TODO handle input
            base.Update(gameTime);
        }

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
        }
    }

    /// <summary>
    /// Yksittäinen näppäin virtuaalisessa näppäimistössä.
    /// </summary>
    internal class VirtualKey
    {
        public VirtualKey(XnaGame game, string text)
        {
            Text = text;
        }

        public VirtualKey(XnaGame game, string text, string value,
            int x, int y, int width, int height, Font font) : this(game, text)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Font = font;
            Value = value;
        }

        /// <summary>
        /// Mikäli tämä on asetettu, näppäin katsotaan Enter-painikkeeksi.
        /// </summary>
        public bool IsEnter { get; private set; }
        public string Value { get; private set; }
        public string Text { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Font Font { get; set; }

        public void Draw(Texture2D whitePixelTexture, SpriteBatch sb, int xOffset, int yOffset)
        {
            var drawRectangle = new XnaRectangle(xOffset + X, yOffset + Y, Width, Height);
            XnaRenderer.FillRectangle(whitePixelTexture, sb, drawRectangle, XnaColor.Gray);
            XnaRenderer.DrawRectangle(whitePixelTexture, sb, drawRectangle, 2, XnaColor.White);
            XnaRenderer.DrawStringWithShadow(sb, Text, Font.XnaFont, 
                new Vector2(xOffset + X + VirtualKeyboard.KEY_PADDING, yOffset + Y + VirtualKeyboard.KEY_PADDING), XnaColor.White);
        }

        
    }

    /// <summary>
    /// Sisältää näppäimistön piitämiseen käytettyjä metodeja.
    /// </summary>
    internal static class XnaRenderer
    {
        internal static void FillRectangle(Texture2D whitePixelTexture, SpriteBatch sb, XnaRectangle rect, XnaColor color)
        {
            sb.Draw(whitePixelTexture, rect, color);
        }

        internal static Texture2D CreateTexture(GraphicsDevice gd, Color color, int width, int height)
        {
            Texture2D texture = new Texture2D(gd, width, height, false, SurfaceFormat.Color);

            Color[] colorArray = new Color[width * height];

            for (int i = 0; i < colorArray.Length; i++)
                colorArray[i] = color;

            texture.SetData(colorArray);

            return texture;
        }

        internal static void DrawStringWithShadow(SpriteBatch sb, string text, SpriteFont font, Vector2 location, XnaColor color)
        {
            sb.DrawString(font, text, new Vector2(location.X + 1f, location.Y + 1f), XnaColor.Black);
            sb.DrawString(font, text, location, color);
        }

        internal static void DrawRectangle(Texture2D whitePixelTexture, SpriteBatch sb, XnaRectangle rect, int thickness, XnaColor color)
        {
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X, rect.Y, rect.Width, thickness), color);
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X, rect.Y + thickness, thickness, rect.Height - thickness), color);
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
        }
    }
}
