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

using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jypeli
{
    [Save]
    public partial class Game : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// Käynnissä olevan pelin pääolio.
        /// </summary>
        public static Game Instance { get; private set; }

        /// <summary>
        /// Kamera, joka näyttää ruudulla näkyvän osan kentästä.
        /// Kameraa voidaan siirtää, zoomata tai asettaa seuraamaan tiettyä oliota.
        /// </summary>
        [Save]
        public Camera Camera { get; set; }

        /// <summary>
        /// Aktiivinen kenttä.
        /// </summary>
        public Level Level { get; private set; }

        public Game()
            : base()
        {
            InitXnaContent();
            InitInstance();
            InitXnaGraphics();
            InitControls();
        }

        private void InitInstance()
        {
            Instance = this;
        }

        private void InitXnaGraphics()
        {
            GraphicsDeviceManager = new GraphicsDeviceManager( this );
            //GraphicsDeviceManager.PreferredDepthStencilFormat = Jypeli.Graphics.SelectStencilMode();
        }

        private void InitXnaContent()
        {
            Content.RootDirectory = "Content";
        }
      
        /// <summary>
        /// This gets called after the GraphicsDevice has been created. So, this is
        /// the place to initialize the resources needed in the game. Except the graphics content,
        /// which should be called int LoadContent(), according to the XNA docs.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected override void Initialize()
        {
#if WINDOWS_PHONE
            isFullScreenRequested = true;
            //Phone.ResetScreen();
#elif !LINUX
			// Let Linux use the default 800x480 window size, seems to work best with OpenTK
			if ( !windowSizeSet )
            	SetWindowSize( GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height, isFullScreenRequested );
#endif

            Level = new Level( this );
            //MediaPlayer = new MediaPlayer( Content );
            //addMessageDisplay();

#if DEBUG && !DISABLE_WIDGETS
            double barWidth = 20;
            double barHeight = Screen.Height;
            fpsDisplay = new Label( "00" );
            fpsDisplay.Color = Color.Gray;
            fpsDisplay.X = Level.Right - barWidth / 2 - fpsDisplay.Width;
            fpsDisplay.Y = Screen.Top - fpsDisplay.Height / 2;

            double left = Screen.Right - Layers.Count * barWidth;

            objectCountDisplays = new BarGauge[Layers.Count];

            for ( int i = 0; i < Layers.Count; i++ )
            {
                var gauge = new BarGauge( barWidth, Screen.Height );
                gauge.X = left + i * barWidth + barWidth / 2;
                gauge.Y = Screen.Center.Y;
                gauge.BarColor = Color.DarkGreen;
                gauge.BindTo( Layers[i + Layers.FirstIndex].ObjectCount );
                objectCountDisplays[i] = gauge;
            }

            Keyboard.Listen( Key.F12, ButtonState.Pressed, ToggleDebugScreen, null );
#endif

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Graphics initialization is best done here when window size is set for certain
            InitGraphics();
            
            Begin();
            base.LoadContent();
        }

        [EditorBrowsable( EditorBrowsableState.Never )]
        protected override void Draw( GameTime gameTime )
        {
            GraphicsDevice.Clear( ClearOptions.Target, Level.BackgroundColor.AsXnaColor(), 1.0f, 0 );

            // The world matrix adjusts the position and size of objects according to the camera angle.
            var worldMatrix =
                Matrix.CreateTranslation( (float)-Camera.Position.X, (float)-Camera.Position.Y, 0 )
                * Matrix.CreateScale( (float)Camera.ZoomFactor, (float)Camera.ZoomFactor, 1f );

            Graphics.Canvas.Begin( ref worldMatrix, Level );
            Paint( Graphics.Canvas );
            Graphics.Canvas.End();

            base.Draw( gameTime );
        }

        protected override void Update( GameTime gameTime )
        {
            UpdateControls();
            ExecutePendingActions();
            base.Update( gameTime );
        }

        public virtual void Begin()
        {
        }

        protected virtual void Paint( Canvas canvas )
        {
        }
    }
}
