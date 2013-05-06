using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jypeli
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game()
        {
            graphics = new GraphicsDeviceManager( this );
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            Begin();
            base.LoadContent();
        }

        protected override void Draw( GameTime gameTime )
        {
            GraphicsDevice.Clear( Color.Black );
            base.Draw( gameTime );
        }

        public virtual void Begin()
        {
        }
    }
}
