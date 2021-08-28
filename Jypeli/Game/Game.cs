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
using Jypeli.Devices;

using Silk.NET.Windowing;

using Matrix = System.Numerics.Matrix4x4;
using Jypeli.Effects;

#if ANDROID
using Android.Content.Res;
using Jypeli.Controls.Keyboard;
#endif

namespace Jypeli
{
    [Save]
    public partial class Game : GameObjectContainer, IDisposable
    {
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

        private int skipcounter;

        /// <summary>
        /// Tallennetaanko pelin kuvaa.
        /// Vie oletusresoluutiolla noin 3MB/frame
        /// </summary>
        public bool SaveOutput { get; private set; }

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

        /// <summary>
        /// Onko peli sulkeutumassa tämän päivityksen jölkeen.
        /// </summary>
        public bool Closing { get; private set; }

#if ANDROID
        /// <summary>
        /// Virtuaalinen näppäimistö.
        /// </summary>
        internal VirtualKeyboard VirtualKeyboard { get; private set; }
        public static AssetManager AssetManager { get; set; }
#endif

        /// <summary>
        /// Alustaa pelin.
        /// </summary>
        public Game()
        {
			InitGlobals();
            InitContent();
            InitWindow();
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
            if (save && !Directory.Exists("Output"))
            {
                Directory.CreateDirectory("Output");
            }

            Window.Run();
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
            //base.RunOneFrame();
            Screencap.SaveBmp(bmpOutName);
            OnExiting(this, EventArgs.Empty);
            //UnloadContent();
            Exit();
        }


		void InitGlobals ()
        {
			Name = this.GetType().Assembly.FullName.Split( ',' )[0];
			Instance = this;
            Device = Device.Create();
		}

        private void InitWindow()
        {
#if ANDROID
            var options = ViewOptions.Default;
            options.API = new GraphicsAPI(ContextAPI.OpenGLES, ContextProfile.Core, ContextFlags.Default, new APIVersion(3, 0));
            Window = Silk.NET.Windowing.Window.GetView(options);
#else
            var options = WindowOptions.Default;
            options.Size = new Silk.NET.Maths.Vector2D<int>(1024, 768);
            options.Title = Name;

            Window = Silk.NET.Windowing.Window.Create(options);
#endif
            Window.Load += LoadContent;
            Window.Update += OnUpdate;
            Window.Render += OnDraw;
            Window.Closing += OnExit;
            Window.Resize += (v) => OnResize(new Vector(v.X, v.Y));
        }

        private void InitAudio()
        {
            if(!Headless)
                AudioEnabled = true;
        }

        internal void OnResize(Vector size)
        {
            Screen.Resize(size);
        }

		internal void OnNoAudioHardwareException()
		{
			MessageDisplay.Add( "No audio hardware was detected. All sound is disabled." );
		}

        /// <summary>
        /// This gets called after the GraphicsDevice has been created. So, this is
        /// the place to initialize the resources needed in the game. Except the graphics content,
        /// which should be called int LoadContent(), according to the XNA docs.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected void Initialize()
        {
            if ( !windowPositionSet )
                CenterWindow();

            Level = new Level( this );

#if ANDROID
            VirtualKeyboard = new VirtualKeyboard(this);
            //Components.Add(VirtualKeyboard);
            VirtualKeyboard.Initialize();
            VirtualKeyboard.Hide();
#endif

            //Activated += (e, sender) => { IsActive = true; };
            //Deactivated += (e, sender) => { IsActive = false; };
        }

        /// <summary>
        /// XNA:n sisällön alustus (Initializen jälkeen)
        /// </summary>
        protected void LoadContent()
        {
            graphicsDevice = new Rendering.OpenGl.GraphicsDevice(Window); // TODO: GraphicsDeviceManager, jolle annetaan ikkuna ja asetukset tms. joka hoitaa oikean laitteen luomisen.
            // Graphics initialization is best done here when window size is set for certain
            InitGraphics();
            Device.ResetScreen();
            InitControls();
            InitLayers();
            InitDebugScreen();

            if ( InstanceInitialized != null )
                InstanceInitialized();

            AddMessageDisplay();
            Initialize();
            bl = new BasicLights();
            CallBegin();
        }

        // TODO: Tätä voisi hieman siistiä
        internal BasicLights bl;

        /// <summary>
        /// XNA:n piirtorutiinit.
        /// </summary>
        /// <param name="gameTime"></param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected void Draw(Time gameTime)
        {
            UpdateFps(gameTime);
            GraphicsDevice.SetRenderTarget(Screen.RenderTarget);
            GraphicsDevice.Clear(Level.BackgroundColor);
            
            if ( Level.Background.Image != null && !Level.Background.MovesWithCamera )
            {
                GraphicsDevice.BindTexture(Level.Background.Image);

                Graphics.BasicTextureShader.Use();
                Graphics.BasicTextureShader.SetUniform("world", Matrix.Identity);

                GraphicsDevice.DrawPrimitives(Rendering.PrimitiveType.OpenGlTriangles, Graphics.TextureVertices, 6, true);
            }
            
            // The world matrix adjusts the position and size of objects according to the camera angle.
            var worldMatrix =
                Matrix.CreateTranslation( (float)-Camera.Position.X, (float)-Camera.Position.Y, 0 )
                * Matrix.CreateScale( (float)Camera.ZoomFactor, (float)Camera.ZoomFactor, 1f );

            // If the background should move with camera, draw it here.
            Level.Background.Draw( worldMatrix, Matrix.Identity );

            // Draw the layers containing the GameObjects
            Layers.ForEach( l => l.Draw( Camera ) );

            bl.Draw(worldMatrix);

            // Draw on the canvas
            Graphics.Canvas.Begin( ref worldMatrix, Level );
            Paint( Graphics.Canvas );
            Graphics.Canvas.End();

            // Draw the debug information screen
            DrawDebugScreen();
            
            // Render the scene on screen
            Screen.Render();

            if (SaveOutput)
            {
                if (FrameCounter != 0) // Ekaa framea ei voi tallentaa?
                    if(skipcounter == 0)
                    {
                        Screencap.SaveBmp("Output/" + SavedFrameCounter + ".bmp");
                        skipcounter = FramesToSkip;
                        SavedFrameCounter++;
                    }
                    else
                    {
                        skipcounter--;
                    }

            }

            FrameCounter++;

            if (FrameCounter == TotalFramesToRun)
            {
                OnExiting(this, EventArgs.Empty);
                //UnloadContent();
                Exit();
            }
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
            AddMessageDisplay();
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

        public void OnExit()
        {
            Dispose();
        }

        /// <summary>
        /// Sulkee pelin
        /// </summary>
        public void Exit()
        {
            Closing = true;
            Window.Close();
        }

        public void Dispose()
        {
            GraphicsDevice?.Dispose();
        }
    }
}
