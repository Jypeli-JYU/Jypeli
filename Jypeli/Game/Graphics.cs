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
using Jypeli.Rendering;
using Silk.NET.Windowing;

namespace Jypeli
{
    public partial class Game
    {
        internal IView Window;
        private static IGraphicsDevice graphicsDevice;

        /// <summary>
        /// Grafiikkalaite joka hoitaa kuvan piirtämisen ruudulle.
        /// </summary>
        public static IGraphicsDevice GraphicsDevice
        {
            get
            {
                if (Instance == null)
                    return null;
                return graphicsDevice;
            }
        }

        /// <summary>
        /// Onko peli kokoruututilassa.
        /// </summary>
        public bool IsFullScreen
        {
#if DESKTOP
            get { return ((IWindow)Window).WindowState == WindowState.Fullscreen; }
            set { ((IWindow)Window).WindowState = value ? WindowState.Fullscreen : WindowState.Normal; }
#elif ANDROID
            get { return true; }
            set { }
#endif
        }

        /// <summary>
        /// Näytön dimensiot, eli koko ja reunat.
        /// </summary>
        public static ScreenView Screen { get; private set; }

        /// <summary>
        /// Tekstuurien (kuvien) reunanpehmennys skaalattaessa (oletus päällä).
        /// </summary>
        [Obsolete("Käytä kuva-olion Scaling ominaisuutta")]
        public static bool SmoothTextures { get; set; }

        /// <summary>
        /// Asettaa ikkunan paikan. Huomaa että origo on vasemmassa yläreunassa.
        /// </summary>
        /// <param name="x">Ikkunan vasemman reunan x-koordinaatti</param>
        /// <param name="y">Ikkunan yläreunan y-koordinaatti (kasvaa alaspäin)</param>
        public void SetWindowPosition( int x, int y )
        {
#if DESKTOP
            ((IWindow)Window).Position = new Silk.NET.Maths.Vector2D<int>(x, y);
#endif
        }

        /// <summary>
        /// Asettaa ikkunan ruudun keskelle.
        /// </summary>
        public void CenterWindow()
        {
            // Onko mahdollista että arvoa ei ole?
            if (!Window.VideoMode.Resolution.HasValue)
                return;

            int W = Window.VideoMode.Resolution.Value.X;
            int H = Window.VideoMode.Resolution.Value.Y;
            int w = Window.Size.X;
            int h = Window.Size.Y;

            SetWindowPosition((W - w) / 2, (H - h) / 2);

        }

        /// <summary>
        /// Asettaa ikkunan koon.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public void SetWindowSize(int width, int height)
        {
            SetWindowSize(width, height, IsFullScreen);
        }

        /// <summary>
        /// Asettaa ikkunan koon ja alustaa pelin käyttämään joko ikkunaa tai koko ruutua.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="fullscreen">Koko ruutu jos <c>true</c>, muuten ikkuna.</param>
        /// <returns></returns>
        public void SetWindowSize(int width, int height, bool fullscreen)
        {
            IsFullScreen = fullscreen;

            if (Screen != null) // Ei pitäisi ikinä olla null.
            {
                Screen.Size = new Vector(width, height);
            }

            OnResize(new Vector(width, height));

        }

        /// <summary>
        /// Alustaa grafiikat. Suorita vasta kun ikkuna on lopullisessa koossaan.
        /// </summary>
        private void InitGraphics()
        {
            //Viewport viewPort = GraphicsDevice.Viewport;
            Screen = new ScreenView();
            Graphics.Initialize();
            Camera = new Camera();
        }
    }
}
