using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jypeli
{
    public partial class Game
    {
        // fullscreen isn't used as default, because debug mode doesn't work well with it
        private bool isFullScreenRequested = false;
        private bool windowSizeSet = false;

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
        }

        /// <summary>
        /// Alustaa grafiikat. Suorita vasta kun ikkuna on lopullisessa koossaan.
        /// </summary>
        private void InitGraphics()
        {
            Viewport viewPort = GraphicsDevice.Viewport;
            Mouse.Viewport = viewPort;
            Screen = new ScreenView( viewPort );
            Jypeli.Graphics.Initialize();

            Camera = new Camera();
            SmoothTextures = true;
        }
    }
}
