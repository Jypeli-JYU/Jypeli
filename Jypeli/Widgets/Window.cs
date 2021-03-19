#region MIT License
/*
 * Copyright (c) 2009-2011 University of Jyväskylä, Department of Mathematical
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
 * Authors: Tomi Karppinen, Tero Jäntti, Rami Pasanen
 */


namespace Jypeli
{
    /// <summary>
    /// Ikkuna.
    /// </summary>
    public class Window : Widget
    {
        private Color _actColor = new Color( 255, 255, 255, 200 );
        private Color _inactColor = new Color( 50, 50, 50, 50 );
        private Color _actTitle = new Color( 255, 255, 255, 100 );
        private Color _inactTitle = new Color( 128, 128, 128, 50 );

        private bool moving = false;
        private Vector movementCenter = Vector.Zero;
        private bool prevMouseVisible = true;

        /// <summary>
        /// Ikkunan väri.
        /// </summary>
        public override Color Color
        {
            get
            {
                return base.Color;
            }
            set
            {
                ActiveColor = value;
                InactiveColor = Color.Darker( value, 75 );
                base.Color = value;
            }
        }

        /// <summary>
        /// Ikkunan väri, kun ikkuna on aktiivinen.
        /// </summary>
        public Color ActiveColor
        {
            get { return _actColor; }
            set { _actColor = value; }
        }

        /// <summary>
        /// Ikkunan väri, kun ikkuna ei ole aktiivinen.
        /// </summary>
        public Color InactiveColor
        {
            get { return _inactColor; }
            set { _inactColor = value; }
        }

        /// <summary>
        /// Ikkunatapahtumien käsittelijä.
        /// </summary>
        public delegate void WindowHandler( Window sender );

        /// <summary>
        /// Tapahtuu kun ikkuna suljetaan.
        /// TODO: ClearAllin kutsuminen samalla updatella kuin Closed-eventti tapahtuu aiheuttaa StackOverflown.
        /// TODO: ClearAll ei tyhjennä HighScoreWindowia oikein.
        /// </summary>
        public event WindowHandler Closed;

        private void OnClosed()
        {
            if ( Closed != null )
                Closed( this );
        }

        /// <summary>
        /// Alustaa uuden ikkunan.
        /// </summary>
        public Window()
            : base( new VerticalLayout() )
        {
            initialize();
        }

        /// <summary>
        /// Alustaa uuden ikkunan.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public Window( double width, double height )
            : base( width, height )
        {
            SizingByLayout = false;
            Layout = new VerticalLayout();
            initialize();
        }

        /// <summary>
        /// Ikkunalla maksimikoko on siten, että se mahtuu näytölle.
        /// </summary>
        protected override Vector GetMaximumSize()
        {
            var screen = Game.Screen;
            return new Vector( screen.Width * ( 7.0 / 8.0 ), screen.Height * ( 7.0 / 8.0 ) );
        }

        private void initialize()
        {
            AddedToGame += ShowMouse;
            Removed += RestoreMouse;

            Removed += OnClosed;
            ControlContext.Activated += Window_Activated;
            ControlContext.Deactivated += Window_Deactivated;

            AddedToGame += AddControls;

            CapturesMouse = true;
            this.IsModal = true;

            Game.AssertInitialized( RefreshLayout );
        }

        private void ShowMouse()
        {
            if (!IsModal)
                return;

            if ( Game.Instance != null )
            {
                prevMouseVisible = Game.Instance.IsMouseVisible;
                Game.Instance.IsMouseVisible = true;
            }
        }

        private void RestoreMouse()
        {
            if (!IsModal)
                return;

            if ( Game.Instance != null )
                Game.Instance.IsMouseVisible = prevMouseVisible;
        }

        void AddControls()
        {
            var l1 = Game.Mouse.ListenOn( this, MouseButton.Left, ButtonState.Pressed, StartMoveWindow, null ).InContext( this );
            var l2 = Game.Mouse.Listen( MouseButton.Left, ButtonState.Down, MoveWindow, null ).InContext( this );
            var l3 = Game.Mouse.ListenOn( this, MouseButton.Left, ButtonState.Released, EndMoveWindow, null ).InContext( this );
            associatedListeners.AddItems(l1, l2, l3);
        }

        void StartMoveWindow()
        {
            if ( Game == null ) return;
            if (!CapturesMouse) return;
            foreach (var obj in Objects)
            {
                if (obj is Widget w && w.IsCapturingMouse)
                    return;
            }

            movementCenter = this.Position - Game.Mouse.PositionOnScreen;
            moving = true;
        }

        void MoveWindow()
        {
            if ( !moving ) return;
            this.Position = movementCenter + Game.Mouse.PositionOnScreen;
        }

        void EndMoveWindow()
        {
            moving = false;
        }

        void Window_Activated()
        {
            base.Color = ActiveColor;
        }

        void Window_Deactivated()
        {
            base.Color = InactiveColor;
        }

        /// <summary>
        /// Sulkee ikkunan.
        /// </summary>
        public void Close()
        {
            if ( Parent != null )
                Parent.Remove( this );
            else
                Game.Remove( this );
        }
    }
}
