namespace Jypeli.Controls
{
    /// <summary>
    /// Hiiren sijainti, näppäinten tila sekä rullan asento. 
    /// </summary>
    public unsafe struct MouseState
    {
        private fixed bool buttons[13]; // Silk.NET.Input.MouseButton määrän mukaan.
        // fixed-"taulukon" käyttö on luultavasti ihan ok, tälleen meidän ei tarvitse luoda useita muuttujia tai tehdä bittimatikkaa.
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code#fixed-size-buffers
        // Tämä siis toimii arvomuuttuja, eikä viitemuuttujana, structia kopioitaessa.

        /// <summary>
        /// Hiiren X-suuntainen sijainti
        /// </summary>
        public int PosX { get; set; }

        /// <summary>
        /// Hiiren Y-suuntainen sijainti
        /// </summary>
        public int PosY { get; set; }

        /// <summary>
        /// Rullan korkeussuuntainen arvo
        /// </summary>
        public int ScrollX { get; set; }

        /// <summary>
        /// Rullan sivusuuntainen arvo.
        /// </summary>
        public int ScrollY { get; set; }

        internal void SetButtonDown(Silk.NET.Input.MouseButton button)
        {
            buttons[(int)button] = true;
        }

        internal void SetButtonUp(Silk.NET.Input.MouseButton button)
{
            buttons[(int)button] = false;
        }

        /// <summary>
        /// Onko hiiren vasen nappula alhaalla
        /// </summary>
        public bool LeftButton
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Left];
            }
        }

        /// <summary>
        /// Onko hiiren keskimmäinen nappula (eli rulla) alhaalla
        /// </summary>
        public bool MiddleButton
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Middle];
            }
        }

        /// <summary>
        /// Onko hiiren oikea nappula alhaalla
        /// </summary>
        public bool RightButton
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Right];
            }
        }

        /// <summary>
        /// Onko hiiren lisänäppäin alhaalla
        /// </summary>
        public bool XButton4
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Button4];
            }
        }

        /// <summary>
        /// Onko hiiren lisänäppäin alhaalla
        /// </summary>
        public bool XButton5
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Button5];
            }
        }

        /// <summary>
        /// Onko hiiren lisänäppäin alhaalla
        /// </summary>
        public bool XButton6
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Button6];
            }
        }

        /// <summary>
        /// Onko hiiren lisänäppäin alhaalla
        /// </summary>
        public bool XButton7
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Button7];
            }
        }

        /// <summary>
        /// Onko hiiren lisänäppäin alhaalla
        /// </summary>
        public bool XButton8
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Button8];
            }
        }

        /// <summary>
        /// Onko hiiren lisänäppäin alhaalla
        /// </summary>
        public bool XButton9
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Button9];
            }
        }

        /// <summary>
        /// Onko hiiren lisänäppäin alhaalla
        /// </summary>
        public bool XButton10
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Button10];
            }
        }

        /// <summary>
        /// Onko hiiren lisänäppäin alhaalla
        /// </summary>
        public bool XButton11
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Button11];
            }
        }

        /// <summary>
        /// Onko hiiren lisänäppäin alhaalla
        /// </summary>
        public bool XButton12
        {
            get
            {
                return buttons[(int)Silk.NET.Input.MouseButton.Button12];
            }
        }

    }
}
