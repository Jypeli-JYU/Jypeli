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
