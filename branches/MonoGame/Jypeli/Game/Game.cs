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
    public class Game : Microsoft.Xna.Framework.Game
    {
#region Fields
        // fullscreen isn't used as default, because debug mode doesn't work well with it
        private bool isFullScreenRequested = false;
		private bool windowSizeSet = false;
#endregion

#region Graphics properties
        /// <summary>
        /// XNA:n grafiikkakortti.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public static new GraphicsDevice GraphicsDevice
        {
            get
            {
                if ( Game.Instance == null ) return null;
                return ( (Microsoft.Xna.Framework.Game)Instance ).GraphicsDevice;
            }
        }

        /// <summary>
        /// XNA:n grafiikkakorttien hallintaolio.
        /// </summary>
        internal static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

        /// <summary>
        /// Onko peli kokoruututilassa.
        /// </summary>
        public bool IsFullScreen
        {
            get { return isFullScreenRequested; }
            set
            {
                if ( GraphicsDevice == null )
                {
                    // GraphicsDevice is not initialized yet.
                    isFullScreenRequested = value;
                }
                else if ( ( GraphicsDeviceManager.IsFullScreen != value ) )
                {
#if WINDOWS_PHONE
                    //Phone.ResetScreen();
#else
                    SetWindowSize( GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height, value );
#endif
                }
            }
        }

        /// <summary>
        /// Näytön dimensiot, eli koko ja reunat.
        /// </summary>
        public static ScreenView Screen { get; private set; }

        /// <summary>
        /// Tekstuurien (kuvien) reunanpehmennys skaalattaessa (oletus päällä).
        /// </summary>
        public static bool SmoothTextures { get; set; }
#endregion

#region Instance properties
        /// <summary>
        /// Käynnissä olevan pelin pääolio.
        /// </summary>
        public static Game Instance { get; private set; }
#endregion

#region Camera / level properties
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
#endregion

#region Constructor & inits
        public Game()
        {
            InitXnaContent();
            InitInstance();
            InitXnaGraphics();
            InitScreen();
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

        private void InitScreen()
        {
            Camera = new Camera();
            SmoothTextures = true;
        }

        private void InitXnaContent()
        {
            Content.RootDirectory = "Content";
        }

        private void InitControls()
        {
            Keyboard = new Keyboard();
        }
#endregion

#region Control properties
        public Keyboard Keyboard { get; private set; }
#endregion

#region XNA overrides
        /// <summary>
        /// This gets called after the GraphicsDevice has been created. So, this is
        /// the place to initialize the resources needed in the game. Except the graphics content,
        /// which should be called int LoadContent(), according to the XNA docs.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected override void Initialize()
        {
			Screen = new ScreenView(GraphicsDevice.Viewport);
            Jypeli.Graphics.Initialize();

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

            Graphics.LineBatch.Begin( ref worldMatrix );
            Graphics.Canvas.Reset();
            Paint( Graphics.Canvas );
            Graphics.LineBatch.End();

            base.Draw( gameTime );
        }

        protected override void Update( GameTime gameTime )
        {
            Keyboard.Update();
            base.Update( gameTime );
        }
#endregion

#region Jypeli overrides
        public virtual void Begin()
        {
        }

        protected virtual void Paint( Canvas canvas )
        {
        }
#endregion

        /// <summary>
        /// Asettaa ikkunan koon.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public void SetWindowSize( int width, int height )
        {
            SetWindowSize( width, height, IsFullScreen );
        }

        /// <summary>
        /// Asettaa ikkunan koon ja alustaa pelin käyttämään joko ikkunaa tai koko ruutua.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="fullscreen">Koko ruutu jos <c>true</c>, muuten ikkuna.</param>
        /// <returns></returns>
        public void SetWindowSize( int width, int height, bool fullscreen )
        {
            GraphicsDeviceManager.PreferredBackBufferWidth = width;
            GraphicsDeviceManager.PreferredBackBufferHeight = height;
            GraphicsDeviceManager.IsFullScreen = fullscreen;
            GraphicsDeviceManager.ApplyChanges();
            isFullScreenRequested = fullscreen;
			windowSizeSet = true;

            /*D
            if ( GraphicsDevice != null )
            {
                Viewport viewPort = GraphicsDevice.Viewport;
                Mouse.Viewport = viewPort;
                if ( screen != null )
                {
                    screen.viewPort = viewPort;
                }
            }*/
        }
    }
}
