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

namespace Jypeli
{
    public partial class Game
    {
        // fullscreen isn't used as default, because debug mode doesn't work well with it
        private bool isFullScreenRequested = false;
        private bool windowSizeSet = false;
        private bool windowPositionSet = false;

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
        //[EditorBrowsable(EditorBrowsableState.Never )]
        public static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }

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
#if WINDOWS_PHONE && !WINDOWS_PHONE81
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

        private void SetDefaultResolution()
        {
#if WINDOWS_STOREAPP || ANDROID
            isFullScreenRequested = true;
#else
            SetWindowSize( 1024, 768, isFullScreenRequested );
#endif
        }

        /// <summary>
        /// Asettaa ikkunan paikan. Huomaa että origo on vasemmassa yläreunassa.
        /// </summary>
        /// <param name="x">Ikkunan vasemman reunan x-koordinaatti</param>
        /// <param name="y">Ikkunan yläreunan y-koordinaatti (kasvaa alaspäin)</param>
        public void SetWindowPosition( int x, int y )
        {
            Window.Position = new Point( x, y );
            windowPositionSet = true;
        }

        /// <summary>
        /// Asettaa ikkunan ruudun keskelle.
        /// </summary>
        public void CenterWindow()
        {

            int W = (int)GraphicsDevice.DisplayMode.Width;
            int H = (int)GraphicsDevice.DisplayMode.Height;
			int w = (int)GraphicsDeviceManager.PreferredBackBufferWidth;
			int h = (int)GraphicsDeviceManager.PreferredBackBufferHeight;

            //TODO: How to do this now?
#if WINDOWS
            //int borderwidth = GetSystemMetrics( 32 ); // SM_CXFRAME
            //int titleheight = GetSystemMetrics( 30 ); // SM_CXSIZE
            //w += 2 * borderwidth;
            //h += titleheight + 2 * borderwidth;
#endif

            SetWindowPosition( ( W - w ) / 2, ( H - h ) / 2 );

        }

        /// <summary>
        /// Asettaa ikkunan koon.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public void SetWindowSize( int width, int height )
        {
            // WP have a limited set of supported resolutions
            // Use Phone.DisplayResolution instead
            // For RT, use Screen.Size to scale down from native
#if !WINDOWS_PHONE && !WINRT
            DoSetWindowSize( width, height, IsFullScreen );
#endif
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
            // WP has a limited set of supported resolutions
            // Use Phone.DisplayResolution instead
            // For RT, use Screen.Size to scale down from native
#if !WINDOWS_PHONE && !WINRT
            DoSetWindowSize( width, height, fullscreen );
#endif
        }

        /// <summary>
        /// Asettaa ikkunan koon ja alustaa pelin käyttämään joko ikkunaa tai koko ruutua.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="fullscreen">Koko ruutu jos <c>true</c>, muuten ikkuna.</param>
        /// <returns></returns>
        internal void DoSetWindowSize (int width, int height, bool fullscreen)
		{
            //TODO: DO this without previously imported dll, or is this no longer needed?

            // For high-DPI support
            //System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            //IntPtr hdc = graphics.GetHdc();
            //int logicalScreenHeight = GetDeviceCaps(hdc, (int)DeviceCap.VERTRES);
            //int physicalScreenHeight = GetDeviceCaps(hdc, (int)DeviceCap.DESKTOPVERTRES);
            //graphics.ReleaseHdc(hdc);
            //graphics.Dispose();
            
            //float scaleFactor = (float)logicalScreenHeight / (float)physicalScreenHeight;
            
            //width = (int)(width * scaleFactor);
            //height = (int)(height * scaleFactor);


            GraphicsDeviceManager.PreferredBackBufferWidth = width;
			GraphicsDeviceManager.PreferredBackBufferHeight = height;
			GraphicsDeviceManager.IsFullScreen = fullscreen;
            
            //TODO: is this needed?
#if LINUX
			//if (fullscreen) {
			//	GraphicsDeviceManager.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
			//	GraphicsDeviceManager.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
			//}
#endif

            GraphicsDeviceManager.ApplyChanges ();
			isFullScreenRequested = fullscreen;

			if (Screen != null) {
				Screen.Size = new Vector (width, height);
                //TODO: What about this?
#if LINUX
				Screen.ScaleToFit ();
#endif
            }

            windowSizeSet = true;

            if ( GraphicsDevice != null )
                CenterWindow();
        }

        /// <summary>
        /// Alustaa grafiikat. Suorita vasta kun ikkuna on lopullisessa koossaan.
        /// </summary>
        private void InitGraphics()
        {
            Viewport viewPort = GraphicsDevice.Viewport;
            Screen = new ScreenView( GraphicsDevice );
            Jypeli.Graphics.Initialize();

            Camera = new Camera();
            SmoothTextures = true;
        }
    }
}
