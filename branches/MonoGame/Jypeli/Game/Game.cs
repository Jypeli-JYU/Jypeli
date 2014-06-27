#region MIT License
/*
 * Copyright (c) 2013 University of Jyväskylä, Department of Mathematical
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
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using XnaColor = Microsoft.Xna.Framework.Color;
using XnaSoundEffect = Microsoft.Xna.Framework.Audio.SoundEffect;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace Jypeli
{
    [Save]
    public partial class Game : Microsoft.Xna.Framework.Game
    {
        private bool loadContentHasBeenCalled = false;
        private bool beginHasBeenCalled = false;

        /// <summary>
        /// Käynnissä olevan pelin pääolio.
        /// </summary>
        public static Game Instance { get; private set; }

        /// <summary>
        /// Phone-olio esim. puhelimen tärisyttämiseen.
        /// </summary>
        public Phone Phone { get; private set; }

        /// <summary>
        /// Kamera, joka näyttää ruudulla näkyvän osan kentästä.
        /// Kameraa voidaan siirtää, zoomata tai asettaa seuraamaan tiettyä oliota.
        /// </summary>
        [Save]
        public Camera Camera { get; set; }

		/// <summary>
		/// Pelin nimi.
		/// </summary>
		public static string Name { get; private set; }

        /// <summary>
        /// Aktiivinen kenttä.
        /// </summary>
        public Level Level { get; private set; }

        public Game()
            : base()
        {
			InitGlobals();
            InitXnaContent();
            InitXnaGraphics();
			InitStorage();
        }

		void InitGlobals ()
		{
#if WINRT
            Name = this.GetType().AssemblyQualifiedName.Split( ',' )[0];
#else
			Name = this.GetType().Assembly.FullName.Split( ',' )[0];
#endif
			Instance = this;
			Phone = new Phone();
		}

        private void InitXnaGraphics()
        {
            GraphicsDeviceManager = new GraphicsDeviceManager( this );
            GraphicsDeviceManager.PreferredDepthStencilFormat = Jypeli.Graphics.SelectStencilMode();
        }

        /// <summary>
        /// This gets called after the GraphicsDevice has been created. So, this is
        /// the place to initialize the resources needed in the game. Except the graphics content,
        /// which should be called int LoadContent(), according to the XNA docs.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected override void Initialize()
        {
            if ( !windowSizeSet )
                SetDefaultResolution();

            // Center the window on next update (aka the OpenTK fix)
            DoNextUpdate( CenterWindow );
            
            Level = new Level( this );
            base.Initialize();
        }

        /// <summary>
        /// XNA:n sisällön alustus (Initializen jälkeen)
        /// </summary>
        protected override void LoadContent()
        {
            // Graphics initialization is best done here when window size is set for certain
            InitGraphics();
            Phone.ResetScreen();
            InitControls();
            InitLayers();
            InitPhysics();
            InitDebugScreen();

            if ( InstanceInitialized != null )
                InstanceInitialized();

            base.LoadContent();
            loadContentHasBeenCalled = true;
            addMessageDisplay();
            CallBegin();
        }

        /// <summary>
        /// XNA:n piirtorutiinit.
        /// </summary>
        /// <param name="gameTime"></param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected override void Draw( GameTime gameTime )
        {
            GraphicsDevice.SetRenderTarget( Screen.RenderTarget );
			GraphicsDevice.Clear( Level.BackgroundColor.AsXnaColor() );

            if ( Level.Background.Image != null && !Level.Background.MovesWithCamera )
            {
                SpriteBatch spriteBatch = Jypeli.Graphics.SpriteBatch;
                spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend );
                spriteBatch.Draw( Level.Background.Image.XNATexture, new XnaRectangle( 0, 0, (int)Screen.Width, (int)Screen.Height ), XnaColor.White );
                spriteBatch.End();
            }

            // The world matrix adjusts the position and size of objects according to the camera angle.
            var worldMatrix =
                Matrix.CreateTranslation( (float)-Camera.Position.X, (float)-Camera.Position.Y, 0 )
                * Matrix.CreateScale( (float)Camera.ZoomFactor, (float)Camera.ZoomFactor, 1f );

            // If the background should move with camera, draw it here.
            Level.Background.Draw( worldMatrix, Matrix.Identity );

            // Draw the layers containing the GameObjects
            Layers.ForEach( l => l.Draw( Camera ) );

            // Draw on the canvas
            Graphics.Canvas.Begin( ref worldMatrix, Level );
            Paint( Graphics.Canvas );
            Graphics.Canvas.End();

            // Draw the debug information screen
            DrawDebugScreen();

            // Render the scene on screen
            Screen.Render();

            base.Draw( gameTime );
        }

        /// <summary>
        /// Nollaa kaiken.
        /// </summary>
        public virtual void ClearAll()
        {
            Level.Clear();
            ResetLayers();
            ClearTimers();
/*#if !WINDOWS_PHONE
            ClearLights();
#endif*/
            ClearControls();
            GC.Collect();
            ControlContext.Enable();
        }

        /// <summary>
        /// Aloittaa pelin kutsumalla Begin-metodia.
        /// Tärkeää: kutsu tätä, älä Beginiä suoraan, sillä muuten peli ei päivity!
        /// </summary>
        internal void CallBegin()
        {
            Begin();
            beginHasBeenCalled = true;
        }

        /// <summary>
        /// Tässä alustetaan peli.
        /// </summary>
        public virtual void Begin()
        {
        }

        /// <summary>
        /// Canvakselle piirto.
        /// </summary>
        /// <param name="canvas"></param>
        protected virtual void Paint( Canvas canvas )
        {
        }
    }
}
