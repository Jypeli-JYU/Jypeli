using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private static readonly char[][] keyLines = new char[][]
        {
            new char[] {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0'},
            new char[] {'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P'},
            new char[] {'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', ':'},
            new char[] {'Z', 'X', 'C', 'V', 'B', 'N', 'M', '<', '>', '.'}
        };

        public VirtualKeyboard(Microsoft.Xna.Framework.Game game, Game jypeliGame) : base(game)
        {
            this.game = game;
        }

        private Microsoft.Xna.Framework.Game game;
        private SpriteBatch spriteBatch;

        private List<VirtualKey> keys;

        public override void Initialize()
        {
            base.Initialize();

            keys = new List<VirtualKey>();
            int totalWidth = Game.GraphicsDevice.Viewport.Width;
            int totalHeight = Game.GraphicsDevice.Viewport.Height;

            int highestKeyCount = GetKeyCountOnSingleLine();

            int keyWidth = ((totalWidth - KEY_PADDING) / highestKeyCount) - KEY_PADDING;
            int keyHeight = ((totalHeight - KEY_PADDING) / keyLines.Length) - KEY_PADDING;

            // Create keys
            for (int y = 0; y < keyLines.Length; y++)
            {
                int yCoord = KEY_PADDING + y * (keyHeight + KEY_PADDING);

                for (int x = 0; x < keyLines[y].Length; x++)
                {
                    int xCoord = KEY_PADDING + x * (keyWidth + KEY_PADDING);

                    var virtualKey = new VirtualKey(game, keyLines[y][x],
                        xCoord, yCoord, keyWidth, keyHeight, Font.DefaultLarge);
                    keys.Add(virtualKey);
                }
            }

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private int GetKeyCountOnSingleLine()
        {
            int highestKeyCount = int.MinValue;

            foreach (char[] line in keyLines)
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
            // TODO draw keys

            base.Draw(gameTime);
        }
    }

    /// <summary>
    /// Yksittäinen näppäin virtuaalisessa näppäimistössä.
    /// </summary>
    internal class VirtualKey : DrawableGameComponent
    {
        public VirtualKey(Microsoft.Xna.Framework.Game game, char character) : base(game)
        {
            Character = character;
        }

        public VirtualKey(Microsoft.Xna.Framework.Game game, char character,
            int x, int y, int width, int height, Font font) : this(game, character)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Font = font;
        }

        public char Character { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Font Font { get; set; }

        public override void Draw(GameTime gameTime)
        {
            // TODO draw

            base.Draw(gameTime);
        }
    }
}
