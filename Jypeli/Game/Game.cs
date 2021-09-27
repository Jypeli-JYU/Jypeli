#region MIT License
/*
 * Copyright (c) 2018 University of Jyväskylä, Department of Mathematical
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
using System.IO;


#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen, Rami Pasanen.
 */

using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Jypeli.Devices;

using XnaColor = Microsoft.Xna.Framework.Color;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
#if ANDROID
using Jypeli.Controls.Keyboard;
#endif

namespace Jypeli
{
    [Save]
    public partial class Game : Microsoft.Xna.Framework.Game, GameObjectContainer
    {
        private bool loadContentHasBeenCalled = false;
        private bool beginHasBeenCalled = false;

        /// <summary>
        /// Kuinka monen pelinpäivityksen jälkeen peli suljetaan automaattisesti.
        /// Jos 0, peli pyörii ikuisesti
        /// </summary>
        public int TotalFramesToRun { get; private set; }

        /// <summary>
        /// Kuinka monta pelinpäivitystä on tähän mennessä ajettu.
        /// </summary>
        public int FrameCounter { get; private set; }

        /// <summary>
        /// Kuinka monta pelinpäivitystä on tähän mennessä tallennettu.
        /// </summary>
        public int SavedFrameCounter { get; private set; }

        /// <summary>
        /// Kuinka monenen framen yli hypätään peliä nauhoittaessa.
        /// </summary>
        public int FramesToSkip { get; private set; }

        private int skipcounter = 0;

        /// <summary>
        /// Tallennetaanko pelin kuvaa.
        /// Vie oletusresoluutiolla noin 3MB/frame
        /// </summary>
        public bool SaveOutput { get; private set; }

        /// <summary>
        /// Kirjoitetaanko kuvatiedosto standarditulosteeseen jos <see cref="SaveOutput"/> on päällä.
        /// </summary>
        public bool SaveOutputToConsole { get; private set; }

        /// <summary>
        /// Ajetaanko peli ilman ääntä (esim. TIMissä)
        /// </summary>
        public bool Headless { get; private set; }

        /// <summary>
        /// Käynnissä olevan pelin pääolio.
        /// </summary>
        public static Game Instance { get; private set; }

        /// <summary>
        /// Laite jolla peliä pelataan.
        /// </summary>
        public static Device Device { get; private set; }

        /// <summary>
        /// Tietovarasto, johon voi tallentaa tiedostoja pidempiaikaisesti.
        /// Sopii esimerkiksi pelitilanteen lataamiseen ja tallentamiseen.
        /// </summary>
        public static FileManager DataStorage { get { return Device.Storage; } }

        /// <summary>
        /// Onko käytössä Farseer-fysiikkamoottori
        /// HUOM: Tämä saattaa poistua tulevaisuudessa jos/kun siitä tehdään ainut vaihtoehto.
        /// </summary>
        public bool FarseerGame { get; set; }

        /// <summary>
        /// Phone-olio esim. puhelimen tärisyttämiseen.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete( "Käytä Device-oliota" )]
        public Device Phone
        {
            get { return Device; }
        }

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
        /// Voiko ääniä soittaa.
        /// </summary>
        public static bool AudioEnabled { get; private set; }

        /// <summary>
        /// Aktiivinen kenttä.
        /// </summary>
        public Level Level { get; private set; }
        
        private Stream CurrentFrameStream => !SaveOutputToConsole ? new FileStream(Path.Combine("Output", $"{SavedFrameCounter}.bmp"), FileMode.Create) : standardOutStream.Value;

        private readonly Lazy<Stream> standardOutStream = new Lazy<Stream>(Console.OpenStandardOutput);
        

#if ANDROID
        /// <summary>
        /// Virtuaalinen näppäimistö.
        /// </summary>
        internal VirtualKeyboard VirtualKeyboard { get; private set; }
#endif

		/// <summary>
		/// Alustaa pelin.
		/// </summary>
        public Game()
            : base()
        {
			InitGlobals();
            InitXnaContent();
            InitXnaGraphics();
            InitAudio();
        }

        /// <summary>
        /// Ajaa pelin. Kutsutaan Ohjelma.cs:stä.
        /// </summary>
        /// <param name="headless">Ajetaanko ohjelma headless-moodissa. Käytetään TIMissä</param>
        /// <param name="save">Tallentaako peli jokaisen framen omaan kuvatiedostoon</param>
        /// <param name="frames">Kuinka monen pelipäivityksen jälkeen peli suljetaan</param>
        /// <param name="skip">Kuinka mones frame tallennetaan peliä kuvatessa, ts. arvo 1 tarkoittaa että joka toinen frame tallennetaan</param>
        public void Run(bool headless = false, bool save = false, int frames = 0, int skip = 1)
        {
            if (frames < 0) throw new ArgumentException("n must be greater than 0!");
            TotalFramesToRun = frames;
            SaveOutput = save;
            Headless = headless;
            FramesToSkip = skip;

            ApplyCMDArgs();

            if (SaveOutput && !Directory.Exists("Output"))
            {
                Directory.CreateDirectory("Output");
            }
            base.Run();
        }

        private void ApplyCMDArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("--save"))
            {
                if (bool.TryParse(args[Array.IndexOf(args, "--save") + 1], out bool save))
                    SaveOutput = save;
                else
                    throw new ArgumentException("Invalid value for --save");
            }
            if (args.Contains("--framesToRun"))
            {
                if (int.TryParse(args[Array.IndexOf(args, "--framesToRun") + 1], out int frames))
                    TotalFramesToRun = frames;
                else
                    throw new ArgumentException("Invalid value for --framesToRun");
            }
            if (args.Contains("--headless"))
            {
                if (bool.TryParse(args[Array.IndexOf(args, "--headless") + 1], out bool headless))
                    Headless = headless;
                else
                    throw new ArgumentException("Invalid value for --headless");
            }
            if (args.Contains("--skipFrames"))
            {
                if (int.TryParse(args[Array.IndexOf(args, "--skipFrames") + 1], out int skip))
                    FramesToSkip = skip;
                else
                    throw new ArgumentException("Invalid value for --skipFrames");
            }
            if (args.Contains("--saveToStdout"))
            {
                if (bool.TryParse(args[Array.IndexOf(args, "--saveToStdout") + 1], out bool saveToStdout))
                    SaveOutputToConsole = saveToStdout;
                else
                    throw new ArgumentException("Invalid value for --saveToStdout");
            }
        }

        internal static void DisableAudio()
        {
            AudioEnabled = false;
        }

		/// <summary>
		/// Ajaa yhden päivityksen ja tallentaa ruudun tiedostoon.
		/// </summary>
		/// <param name="bmpOutName">Bmp file to write to.</param>
		public void RunOneFrame( string bmpOutName )
        {
            base.RunOneFrame();
            FileStream screenFile = new FileStream( bmpOutName, FileMode.Create );
            Screencap.WriteBmp( screenFile, Screen.Image );
            screenFile.Close();
			OnExiting(this, EventArgs.Empty);
			UnloadContent();
			Exit();
        }


		void InitGlobals ()
        {
			Name = this.GetType().Assembly.FullName.Split( ',' )[0];
			Instance = this;
            Device = Device.Create();
		}

        private void InitXnaGraphics()
        {
            GraphicsDeviceManager = new GraphicsDeviceManager( this );
            GraphicsDeviceManager.PreferredDepthStencilFormat = Jypeli.Graphics.SelectStencilMode();

#if ANDROID
            GraphicsDeviceManager.PreferredBackBufferWidth = 800;
            GraphicsDeviceManager.PreferredBackBufferHeight = 480;
#endif
        }

        private void InitAudio()
        {
            if(!Headless)
                AudioEnabled = true;
        }

		internal void OnNoAudioHardwareException()
		{
			MessageDisplay.Add( "No audio hardware was detected. All sound is disabled." );
            DisableAudio();
            //TODO: Can this still happen?
#if WINDOWS
			MessageDisplay.Add( "You might need to install OpenAL drivers." );
			MessageDisplay.Add( "Press Ctrl+Alt+I to try downloading and installing them now." );

			Keyboard.Listen( Key.I, ButtonState.Pressed, TryInstallOpenAL, null );
#endif
        }

#if WINDOWS
        private void TryInstallOpenAL()
        {
            if ( !Keyboard.IsCtrlDown() || !Keyboard.IsAltDown() )
                return;

            MessageDisplay.Add( "Starting download of OpenAL installer..." );
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create( "https://github.com/Mono-Game/MonoGame.Dependencies/raw/master/oalinst.exe" );
            request.BeginGetResponse( OpenALDownloaded, request );
        }

        private void OpenALDownloaded(IAsyncResult result)
        {
            HttpWebResponse response = ( result.AsyncState as HttpWebRequest ).EndGetResponse( result ) as HttpWebResponse;

            if ( response == null )
            {
                MessageDisplay.Add( "Download failed." );
                return;
            }

            MessageDisplay.Add( "Download completed. Launching installer..." );
            Stream resStream = response.GetResponseStream();

            string fileName = Path.Combine( Path.GetTempPath(), "oalinst.exe" );
            FileStream fs = new FileStream( fileName, FileMode.Create, FileAccess.Write );
            resStream.CopyTo( fs );
            fs.Close();

            var startInfo = new System.Diagnostics.ProcessStartInfo( fileName );
            startInfo.Verb = "runas";
            var process = System.Diagnostics.Process.Start( startInfo );
            process.WaitForExit();

            MessageDisplay.Add( "Installation complete. Trying to enable sound..." );
            InitAudio();
        }
#endif

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

            if ( !windowPositionSet )
                CenterWindow();

            Level = new Level( this );
            base.Initialize();

#if ANDROID
            VirtualKeyboard = new VirtualKeyboard(this);
            Components.Add(VirtualKeyboard);
            VirtualKeyboard.Initialize();
            VirtualKeyboard.Hide();
#endif

            //Activated += (e, sender) => { IsActive = true; };
            //Deactivated += (e, sender) => { IsActive = false; };
        }

        /// <summary>
        /// XNA:n sisällön alustus (Initializen jälkeen)
        /// </summary>
        protected override void LoadContent()
        {
            // Graphics initialization is best done here when window size is set for certain
            InitGraphics();
            Device.ResetScreen();
            InitControls();
            InitLayers();
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
            //Console.WriteLine(gameTime.ElapsedGameTime.Milliseconds);
            UpdateFps(gameTime);
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

            if (SaveOutput)
            {
                if (FrameCounter != 0) // Ekaa framea ei voi tallentaa?
                    if(skipcounter == 0)
                    {
                        Screencap.WriteBmp(CurrentFrameStream, Screen.Image);
                        skipcounter = FramesToSkip;
                        SavedFrameCounter++;
                    }
                    else
                    {
                        skipcounter--;
                    }
            }

            if (TotalFramesToRun != 0 && FrameCounter == TotalFramesToRun)
            {
                OnExiting(this, EventArgs.Empty);
                UnloadContent();
                Exit();
            }

            FrameCounter++;
        }

        /// <summary>
        /// Tuhoaa kaikki pelioliot, ajastimet ja näppäinkuuntelijat, sekä resetoi kameran.
        /// </summary>
        public virtual void ClearAll()
        {
            Level.Clear();
            ResetLayers();
            ClearTimers();
            ClearLights();
            ClearControls();
            GC.Collect();
            ControlContext.Enable();
            addMessageDisplay();
            Camera.Reset();
            IsPaused = false;
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
