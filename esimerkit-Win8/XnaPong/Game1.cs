using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Pong : Microsoft.Xna.Framework.Game
{
	GraphicsDeviceManager graphics;
	SpriteBatch spriteBatch;
	
	Rectangle maila1alue;
	Rectangle maila2alue;
	Rectangle palloAlue;
	
	SpriteFont fontti;
	Texture2D mailanKuva;
	Texture2D pallonKuva;
	
	Vector2 pallonNopeus = Vector2.Zero;
	int pisteetVasen = 0;
	int pisteetOikea = 0;
	
	public Pong()
	{
		graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		
		graphics.PreferredBackBufferWidth = 800;
		graphics.PreferredBackBufferHeight = 480;
		graphics.IsFullScreen = false;
	}
	
	protected override void Initialize()
	{
		maila1alue = new Rectangle(
			32,
			GraphicsDevice.Viewport.Bounds.Height / 2 - 64,
			32,
			128);
		maila2alue = new Rectangle(
			GraphicsDevice.Viewport.Bounds.Width - 64,
			GraphicsDevice.Viewport.Bounds.Height / 2 - 64,
			32,
			128);
		palloAlue = new Rectangle(
			GraphicsDevice.Viewport.Bounds.Width / 2 - 16,
			GraphicsDevice.Viewport.Bounds.Height / 2 - 16,
			32,
			32);
		
		pallonNopeus = new Vector2(3, 3);
		
		base.Initialize();
	}
	
	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(GraphicsDevice);
		mailanKuva = Content.Load<Texture2D>("maila");
		pallonKuva = Content.Load<Texture2D>("pallo");
		fontti = Content.Load<SpriteFont>("ScoreFont");
	}
	
	protected override void Update(GameTime gameTime)
	{
		LiikutaPalloa();
		LiikutaMailoja();
		KasitteleTormaykset();
		LaskePisteita();
		
		base.Update(gameTime);
	}
	
	private void LiikutaPalloa()
	{
		palloAlue.X += (int)pallonNopeus.X;
		palloAlue.Y += (int)pallonNopeus.Y;
	}
	
	private void LiikutaMailoja()
	{
		KeyboardState kb = Keyboard.GetState();
		
		if (kb.IsKeyDown(Keys.W) && maila1alue.Y > 0)
		{
			// Vasen maila liikkuu ylös
			maila1alue.Y -= 10;
		}
		else if (kb.IsKeyDown(Keys.S) && maila1alue.Y + maila1alue.Height < GraphicsDevice.Viewport.Bounds.Height)
		{
			// Vasen maila liikkuu alas
			maila1alue.Y += 10;
		}
		
		if (kb.IsKeyDown(Keys.Up) && maila2alue.Y > 0)
		{
			// Oikea maila liikkuu ylös
			maila2alue.Y -= 10;
		}
		else if (kb.IsKeyDown(Keys.Down) && maila2alue.Y + maila2alue.Height < GraphicsDevice.Viewport.Bounds.Height)
		{
			// Oikea maila liikkuu alas
			maila2alue.Y += 10;
		}
	}
	
	private void KasitteleTormaykset()
	{
		if (palloAlue.Y < 0 ||
		    palloAlue.Y + palloAlue.Height > GraphicsDevice.Viewport.Bounds.Height)
		{
			// Käännä Y-nopeus jos törmätään ylä- tai alareunaan
			pallonNopeus.Y = -pallonNopeus.Y;
		}
		
		if (palloAlue.Intersects(maila2alue) ||
		    palloAlue.Intersects(maila1alue))
		{
			// Käännä X-nopeus jos törmätään mailaan
			pallonNopeus.X = -pallonNopeus.X;
		}
	}
	
	private void LaskePisteita()
	{
		if (palloAlue.X < 0)
		{
			// Pallo vasemman reunan vasemmalla puolella -> piste pelaajalle 2
			pisteetOikea++;
			Initialize(); // re-init the ball
		}
		else if (palloAlue.X + palloAlue.Width > GraphicsDevice.Viewport.Bounds.Width) // blue scores
		{
			// Pallo oikean reunan oikealla puolella -> piste pelaajalle 1
			pisteetVasen++;
			Initialize();
		}
	}
	
	protected override void Draw(GameTime gameTime)
	{
		// Taustaväri musta
		GraphicsDevice.Clear(Color.Black);
		
		// Piirretään läpinäkyvyys päällä
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		
		// Piirretään mailat ja pallo
		spriteBatch.Draw(mailanKuva, maila1alue, Color.White);
		spriteBatch.Draw(mailanKuva, maila2alue, Color.White);
		spriteBatch.Draw(pallonKuva, palloAlue, Color.White);
		
		// Lopetetaan piirtäminen
		spriteBatch.End();
		
		string pisteTeksti = pisteetVasen.ToString() + " - " + pisteetOikea.ToString();
		Vector2 pistePaikka = new Vector2(GraphicsDevice.Viewport.Bounds.Width / 2 - 25, 10.0f);
		
		// Piirretään pisteet
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		spriteBatch.DrawString(fontti, pisteTeksti, pistePaikka, Color.Yellow);
		spriteBatch.End();
	}
}
