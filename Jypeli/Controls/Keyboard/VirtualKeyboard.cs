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
    /// Virtuaalinen näppäimistö.
    /// Tarkoitettu ensisijaisesti mobiilialustoille, joiden oman 
    /// virtuaalinäppäimistön käyttö MonoGamen ja Jypelin kanssa on
    /// haastavaa tai mahdotonta.
    /// </summary>
    class VirtualKeyboard : DrawableGameComponent
    {
        private const int KEY_PADDING = 5;

        private static readonly string[][] keyLines = new string[][]
        {
            new string[] {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0"},
            new string[] {"Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P"},
            new string[] {"A", "S", "D", "F", "G", "H", "J", "K", "L", ":"},
            new string[] {"Z", "X", "C", "V", "B", "N", "M", ".", ",", "->"}
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

            int keyWidth = ((Width - KEY_PADDING) / highestKeyCount) - KEY_PADDING;
            int keyHeight = ((Height - KEY_PADDING) / keyLines.Length) - KEY_PADDING;

            // Create keys
            for (int y = 0; y < keyLines.Length; y++)
            {
                int yCoord = KEY_PADDING + y * (keyHeight + KEY_PADDING);

                for (int x = 0; x < keyLines[y].Length; x++)
                {
                    int xCoord = KEY_PADDING + x * (keyWidth + KEY_PADDING);

                    var virtualKey = new VirtualKey(game, keyLines[y][x],
                        xCoord, yCoord, keyWidth, keyHeight, Font.DefaultHuge);
                    keys.Add(virtualKey);
                }
            }

            keys[keys.Count - 1].IsCharacter = false;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            whitePixelTexture = XnaRenderer.CreateTexture(GraphicsDevice, Color.White, 1, 1);
        }

        private int GetKeyCountOnSingleLine()
        {
            int highestKeyCount = int.MinValue;

            foreach (string[] line in keyLines)
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

        public VirtualKey(XnaGame game, string text,
            int x, int y, int width, int height, Font font) : this(game, text)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Font = font;
        }

        /// <summary>
        /// Määrittää, onko kyseessä normaali tekstiä syöttävä näppäin.
        /// Mikäli ei, niin näppäin katsotaan Enter-painiketta vastaavaksi
        /// (esim. sulkee tekstilaatikon).
        /// </summary>
        public bool IsCharacter { get; set; } = true;
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
            DrawRectangle(whitePixelTexture, sb, drawRectangle, 2, XnaColor.White);
            XnaRenderer.DrawStringWithShadow(sb, Text, Font.XnaFont, new Vector2(xOffset + X + 5.0f, yOffset + Y + 5.0f), XnaColor.White);
        }

        private static void DrawRectangle(Texture2D whitePixelTexture, SpriteBatch sb, XnaRectangle rect, int thickness, XnaColor color)
        {
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X, rect.Y, rect.Width, thickness), color);
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X, rect.Y + thickness, thickness, rect.Height - thickness), color);
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
            sb.Draw(whitePixelTexture, new XnaRectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
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
    }
}
