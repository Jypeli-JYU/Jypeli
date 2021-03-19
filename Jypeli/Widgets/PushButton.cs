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
 * Authors: Tomi Karppinen, Tero Jäntti
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Jypeli
{
    /// <summary>
    /// Painonappi.
    /// </summary>
    public class PushButton : Label
    {
        internal enum State
        {
            Released,
            Hover,
            LeftPressed,
            RightPressed,
            Selected
        }

        private ShapeCache leftSideShape;
        private ShapeCache topSideShape;
        private ShapeCache RightSideShape;
        private ShapeCache BottomSideShape;

        private ShapeCache leftSidePressedShape;
        private ShapeCache topSidePressedShape;
        private ShapeCache RightSidePressedShape;
        private ShapeCache BottomSidePressedShape;

        private Image imageReleased;
        private Image imagePressed;
        private Image imageHover;

        private State state = State.Released;

        private bool isPressed { get { return state == State.LeftPressed || state == State.RightPressed; } }

        private Color releasedColor;
        private Color hoverColor;
        private Color pressedColor;
        private Color selectedColor;

        /// <summary>
        /// Kaikkien tulevien nappuloiden oletusväri
        /// </summary>
        public static Color defaultColor = new Color(29, 41, 81, 223);

        /// <summary>
        /// Kaikkien tulevien nappuloiden tekstin oletusväri
        /// </summary>
        public static Color defaultTextColor = new Color(250, 250, 250, 240);

        /// <summary>
        /// Kuva kun nappi on vapautettu.
        /// </summary>
        public Image ImageReleased
        {
            get { return imageReleased; }
            set
            {
                imageReleased = value;
                if ( !isPressed && !Game.Mouse.IsCursorOn( this ) )
                    Image = value;
            }
        }

        /// <summary>
        /// Kuva kun nappi on alaspainettuna.
        /// </summary>
        public Image ImagePressed
        {
            get { return imagePressed; }
            set
            {
                imagePressed = value;
                if ( isPressed )
                    Image = value;
            }
        }

        /// <summary>
        /// Kuva kun hiiren kursori on napin päällä.
        /// </summary>
        public Image ImageHover
        {
            get { return imageHover; }
            set
            {
                imageHover = value;
                if ( !isPressed && Game.Mouse.IsCursorOn( this ) )
                    Image = value;
            }
        }

        public override Vector Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
                InitializeShape();
            }
        }

        /// <summary>
        /// Nappulan oletusväri.
        /// Asettaa myös <c>hoverColor</c>, <c>selectedColor</c> ja <c>pressedColor</c> -kenttien arvot.
        /// Jos haluat itse määrittää em. tilojen värit, aseta ne tämän kentän arvon asettamisen jälkeen.
        /// </summary>
        public override Color Color
        {
            get
            {
                return base.Color;
            }
            set
            {
                releasedColor = value;
                hoverColor = Color.Lighter(value, 40);
                selectedColor = Color.Lighter(value, 40);
                pressedColor = Color.Darker(value, 40);
                base.Color = value;
            }
        }

        /// <summary>
        /// Nappulan väri kun hiiri viedään sen päälle.
        /// </summary>
        public Color HoverColor
        {
            get
            {
                return hoverColor;
            }
            set
            {
                hoverColor = value;
            }
        }

        /// <summary>
        /// Nappulan väri kun sitä klikataan.
        /// </summary>
        public Color PressedColor
        {
            get
            {
                return pressedColor;
            }
            set
            {
                pressedColor = value;
            }
        }

        /// <summary>
        /// Nappulan väri kun se on valittuna, esimerkiksi <c>MultiSelectWindow</c>issa.
        /// </summary>
        public Color SelectedColor
        {
            get
            {
                return selectedColor;
            }
            set
            {
                selectedColor = value;
            }
        }

        /// <summary>
        /// Tapahtuu kun nappia on painettu.
        /// </summary>
        public event Action Clicked;

        /// <summary>
        /// Tapahtuu kun nappia on painettu oikealla hiirenpainikkeella.
        /// </summary>
        public event Action RightClicked;

        private void TouchHover( Touch touch )
        {
            double touchX = touch.PositionOnScreen.X;
            double touchY = touch.PositionOnScreen.Y;

            if ( touchX >= Left && touchX <= Right && touchY >= Bottom && touchY <= Top )
                SetState( State.Hover );
            else if ( Game.TouchPanel.NumTouches == 1 )
                SetState( State.Released );
        }

        private void TouchRelease( Touch touch )
        {
            if ( Game.TouchPanel.NumTouches <= 1 )
                SetState( State.Released );
        }

        private void TouchClick( Touch touch )
        {
            Click();
        }

        /// <summary>
        /// Luo uuden painonapin.
        /// </summary>
        /// <param name="text">Napin teksti.</param>
        public PushButton( string text )
            : base( text )
        {
            Initialize();
        }

        /// <summary>
        /// Luo uuden painonapin.
        /// </summary>
        /// <param name="image">Napin kuva.</param>
        public PushButton( Image image )
            : base( image )
        {
            Initialize();
            this.Image = image;
        }

        /// <summary>
        /// Luo uuden painonapin.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public PushButton( double width, double height )
            : base( width, height )
        {
            Initialize();
        }

        private void Initialize()
        {
            InitializeMargins();
            InitializeShape();
            Color = defaultColor;
            TextColor = defaultTextColor;
            CapturesMouse = true;
            AddedToGame += InitializeControls;
        }

        private void InitializeMargins()
        {
            XMargin = 15;
            YMargin = 10;
        }

        /// <summary>
        /// Luo uuden painonapin omalla kuvalla.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="image">Kuva.</param>
        public PushButton( double width, double height, Image image )
            : this( width, height )
        {
            this.Image = image;
        }

        /// <summary>
        /// Luo uuden painonapin.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="text">Teksti.</param>
        public PushButton( double width, double height, string text )
            : base( width, height, text )
        {
            Initialize();
        }

        private void InitializeControls()
        {
            var l1 = Game.Mouse.ListenOn( this, MouseButton.Left, ButtonState.Pressed, SetState, null, State.LeftPressed ).InContext( this );
            var l2 = Game.Mouse.ListenOn( this, MouseButton.Left, ButtonState.Released, Release, null ).InContext( this );
            var l3 = Game.Mouse.Listen( MouseButton.Left, ButtonState.Released, Release, null ).InContext( this );

            var l4 = Game.Mouse.ListenOn( this, MouseButton.Right, ButtonState.Pressed, SetState, null, State.RightPressed ).InContext( this );
            var l5 = Game.Mouse.ListenOn( this, MouseButton.Right, ButtonState.Released, Release, null ).InContext( this );
            var l6 = Game.Mouse.Listen( MouseButton.Right, ButtonState.Released, Release, null ).InContext( this );

            var l7 = Game.Mouse.ListenMovement( 1.0, CheckHover, null ).InContext( this );

            var l8 = Game.Instance.TouchPanel.Listen( ButtonState.Down, TouchHover, null ).InContext( this );
            var l9 = Game.Instance.TouchPanel.ListenOn( this, ButtonState.Released, TouchRelease, null ).InContext( this );
            var l10 = Game.Instance.TouchPanel.Listen( ButtonState.Released, TouchRelease, null ).InContext( this );
            var l11 = Game.Instance.TouchPanel.ListenOn(  this, ButtonState.Released, TouchClick, null ).InContext( this );

            associatedListeners.AddItems(l1, l2, l3, l4, l5, l6, l7, l8, l9, l10, l11);
        }

        private void InitializeShape()
        {
            //double edgeSize = Math.Min( Width, Height ) / 5;
            double edgeSize = 5;
            double relativeHorizontalSize = edgeSize / Width;
            double relativeVerticalSize = edgeSize / Height;

            Vector topLeftOuter = new Vector( -0.5, 0.5 );
            Vector topLeftInner = new Vector( -0.5 + relativeHorizontalSize, 0.5 - relativeVerticalSize );
            Vector bottomLeftOuter = new Vector( -0.5, -0.5 );
            Vector bottomLeftInner = new Vector( -0.5 + relativeHorizontalSize, -0.5 + relativeVerticalSize );
            Vector topRightOuter = new Vector( 0.5, 0.5 );
            Vector topRightInner = new Vector( 0.5 - relativeHorizontalSize, 0.5 - relativeVerticalSize );
            Vector bottomRightOuter = new Vector( 0.5, -0.5 );
            Vector bottomRightInner = new Vector( 0.5 - relativeHorizontalSize, -0.5 + relativeVerticalSize );

            IndexTriangle[] triangles = { new IndexTriangle( 0, 1, 2 ), new IndexTriangle( 1, 3, 2 ), };

            Vector[] leftSideVertices = { topLeftOuter, topLeftInner, bottomLeftOuter, bottomLeftInner, };
            Vector[] topSideVertices = { topLeftOuter, topRightOuter, topLeftInner, topRightInner, };
            Vector[] rightSideVertices = { topRightOuter, bottomRightOuter, topRightInner, bottomRightInner, };
            Vector[] bottomSideVertices = { bottomRightOuter, bottomLeftOuter, bottomRightInner, bottomLeftInner, };

            leftSideShape = new ShapeCache( leftSideVertices, triangles );
            topSideShape = new ShapeCache( topSideVertices, triangles );
            RightSideShape = new ShapeCache( rightSideVertices, triangles );
            BottomSideShape = new ShapeCache( bottomSideVertices, triangles );

            const double scale = 1.4;

            topLeftOuter = new Vector( -0.5, 0.5 );
            topLeftInner = new Vector( -0.5 + relativeHorizontalSize / scale, 0.5 - relativeVerticalSize / scale );
            bottomLeftOuter = new Vector( -0.5, -0.5 );
            bottomLeftInner = new Vector( -0.5 + relativeHorizontalSize / scale, -0.5 + relativeVerticalSize * scale );
            topRightOuter = new Vector( 0.5, 0.5 );
            topRightInner = new Vector( 0.5 - relativeHorizontalSize * scale, 0.5 - relativeVerticalSize / scale );
            bottomRightOuter = new Vector( 0.5, -0.5 );
            bottomRightInner = new Vector( 0.5 - relativeHorizontalSize * scale, -0.5 + relativeVerticalSize * scale );

            Vector[] leftSidePressedVertices = { topLeftOuter, topLeftInner, bottomLeftOuter, bottomLeftInner, };
            Vector[] topSidePressedVertices = { topLeftOuter, topRightOuter, topLeftInner, topRightInner, };
            Vector[] rightSidePressedVertices = { topRightOuter, bottomRightOuter, topRightInner, bottomRightInner, };
            Vector[] bottomSidePressedVertices = { bottomRightOuter, bottomLeftOuter, bottomRightInner, bottomLeftInner, };

            leftSidePressedShape = new ShapeCache( leftSidePressedVertices, triangles );
            topSidePressedShape = new ShapeCache( topSidePressedVertices, triangles );
            RightSidePressedShape = new ShapeCache( rightSidePressedVertices, triangles );
            BottomSidePressedShape = new ShapeCache( bottomSidePressedVertices, triangles );
        }

        internal void SetState( State state )
        {
            this.state = state;

            switch ( state )
            {
                case State.Hover:
                    base.Color = hoverColor;
                    if ( ImageHover != null )
                        Image = ImageHover;
                    break;
                case State.RightPressed:
                case State.LeftPressed:
                    base.Color = pressedColor;
                    if ( ImagePressed != null )
                        ImagePressed = ImagePressed;
                    break;
                case State.Selected:
                    base.Color = selectedColor;
                    break;
                default:
                    base.Color = releasedColor;
                    if ( ImageReleased != null )
                        Image = ImageReleased;
                    break;
            }
        }

        public void Click()
        {
            if ( Clicked != null )
                Clicked();
        }

        public void RightClick()
        {
            if ( RightClicked != null )
                RightClicked();
        }

        /// <summary>
        /// Lisää pikanäppäimen napille.
        /// </summary>
        /// <param name="key">Näppäin</param>
        public Listener AddShortcut( Key key )
        {
            return Jypeli.Game.Instance.Keyboard.Listen( key, ButtonState.Pressed, Click, null ).InContext( this );
        }

        /// <summary>
        /// Lisää pikanäppäimen kaikille ohjaimille.
        /// </summary>
        /// <param name="button">Näppäin</param>
        public List<Listener> AddShortcut( Button button )
        {
            var listeners = new List<Listener>(Game.GameControllers.Count);
            Game.Instance.GameControllers.ForEach( c => listeners.Add(AddShortcut( c, button )) );
            return listeners;
        }

        /// <summary>
        /// Lisää pikanäppäimen yhdelle ohjaimelle.
        /// </summary>
        /// <param name="player">Peliohjaimen indeksi 0-3</param>
        /// <param name="button">Näppäin</param>
        public Listener AddShortcut( int player, Button button )
        {
            return AddShortcut( Game.Instance.GameControllers[player], button );
        }

        /// <summary>
        /// Lisää pikanäppäimen yhdelle ohjaimelle.
        /// </summary>
        /// <param name="controller">Peliohjain</param>
        /// <param name="button">Näppäin</param>
        public Listener AddShortcut( GamePad controller, Button button )
        {
            return controller.Listen( button, ButtonState.Pressed, Click, null ).InContext( this );
        }

        private void Release()
        {
            bool wasLeft = state == State.LeftPressed;
            bool wasRight = state == State.RightPressed;

            if ( Game.Mouse.IsCursorOn( this ) )
            {
                SetState( State.Hover );
            }
            else
            {
                SetState( State.Released );
                return;
            }

            if ( wasLeft ) Click();
            else if ( wasRight ) RightClick();
        }

        private void CheckHover()
        {
            if ( isPressed || state == State.Selected) return; // Ehkä voisi olla jonkinlainen lisäkorostus jos hiiri on päällä ja nappula on valittuna samanaikaisesti...
            SetState( Game.Mouse.IsCursorOn( this ) ? State.Hover : State.Released );
        }

        public override void Draw( Matrix parentTransformation, Matrix transformation )
        {
            base.Draw( parentTransformation, transformation );

            if ( Image == null )
            {
                Color color1 = Color.Lighter( Color, 20 );
                Color color2 = Color.Darker( Color, 20 );

                ShapeCache left = leftSideShape;
                ShapeCache top = topSideShape;
                ShapeCache right = RightSideShape;
                ShapeCache bottom = BottomSideShape;

                if ( isPressed )
                {
                    color1 = Color.Darker( Color, 20 );
                    color2 = Color.Lighter( Color, 20 );
                    left = leftSidePressedShape;
                    top = topSidePressedShape;
                    right = RightSidePressedShape;
                    bottom = BottomSidePressedShape;
                }

                Renderer.DrawFilledShape( left, ref transformation, color1 );
                Renderer.DrawFilledShape( top, ref transformation, color1 );
                Renderer.DrawFilledShape( right, ref transformation, color2 );
                Renderer.DrawFilledShape( bottom, ref transformation, color2 );
            }
        }
    }
}
