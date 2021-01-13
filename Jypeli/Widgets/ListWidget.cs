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

using System.ComponentModel;
using Microsoft.Xna.Framework;
using Jypeli.GameObjects;

namespace Jypeli
{
    [EditorBrowsable( EditorBrowsableState.Never )]
    public class ScrollableList<O> : Widget where O : Widget
    {
        public bool IsAtTop
        {
            get { return _layout.StartIndex == 0; }
        }

        public bool IsAtBottom
        {
            get { return _layout.EndIndex == _childObjects.Count; }
        }

        public int ItemCount { get { return _childObjects.Count; } }

        private VerticalScrollLayout _layout;

        public ScrollableList()
            : base( new VerticalScrollLayout() )
        {
            _layout = (VerticalScrollLayout)Layout;
            Color = Color.Transparent;
            HorizontalSizing = Sizing.Expanding;
            VerticalSizing = Sizing.Expanding;
            AddedToGame += AddListeners;
        }

        void AddListeners()
        {
#if WINDOWS_PHONE || ANDROID
            // TODO: gestures
            //Game.TouchPanel.ListenGestureOn( this, GestureType.VerticalDrag, Scroll, null ).InContext( this );
#endif
        }

#if WINDOWS_PHONE || ANDROID
        const double MaxMoves = 5;

        double velocity = 0;
        Queue<double> lastMoves = new Queue<double>();

        void Scroll( Touch touch )
        {
            if ( _childObjects == null || _childObjects.Count == 0 )
                return;

            // Works better when divided by 2. Don't know why :)
            double movement = touch.MovementOnScreen.Y / 2;

            if ( lastMoves.Count >= MaxMoves )
                lastMoves.Dequeue();
            lastMoves.Enqueue( movement );

            velocity = lastMoves.Average();

            List<GameObject> widgets = _childObjects.FindAll( o => o is Widget );
            _layout.Scroll( widgets, movement );
        }

        public override void Update( Time time )
        {
            if ( Math.Abs( velocity ) > float.Epsilon && _childObjects != null && _childObjects.Count > 0 )
            {
                List<GameObject> widgets = _childObjects.FindAll( o => o is Widget );
                _layout.Scroll( widgets, velocity );
            }

            velocity *= 0.98;
            base.Update( time );
        }
#endif

        public void ScrollUp()
        {
            _layout.ScrollUp( Objects.items );
        }

        public void ScrollDown()
        {
            _layout.ScrollDown( Objects.items );
        }

        public override void Clear()
        {
            _layout.StartIndex = 0;
            _layout.EndIndex = 0;
            base.Clear();
        }

        public O this[int index]
        {
            get { return (O)_childObjects[index]; }
            set { _childObjects[index] = value; }
        }
    }


    /// <summary>
    /// Listakomponentti. Voidaan liittää listaan, joka
    /// toteuttaa <c>INotifyList</c>-rajapinnan. Tällöin
    /// listaan tehdyt muutokset päivittyvät komponenttiin.
    /// Listaa voi vierittää, jos kaikki rivit eivät mahdu
    /// kerralla sen sisälle.
    /// </summary>
    /// <typeparam name="T">Listan alkion tyyppi.</typeparam>
    /// <typeparam name="O">Listan riviä esittävän olion tyyppi.</typeparam>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public abstract class ListWidget<T, O> : Widget
        where O : Widget
    {
#if WINDOWS
        static Image upImage = null;
        static Image downImage = null;
        static Image transparentImage = null;

        PushButton scrollUpButton = null;
        PushButton scrollDownButton = null;
#endif

        private INotifyList<T> List;

        internal protected ScrollableList<O> Content;

        /// <summary>
        /// Listan alkiot.
        /// </summary>
        public INotifyList<T> Items
        {
            get { return List; }
        }

        public ListWidget( INotifyList<T> list )
            : base( new HorizontalLayout() )
        {
            Add( Content = new ScrollableList<O> { Color = Color.Transparent } );
#if WINDOWS
            Add( CreateVerticalScrollPanel() );
#endif

            Bind( list );

            AddedToGame += AddListeners;
        }

        /// <summary>
        /// Luo annettua alkiota vastaavan listan rivin.
        /// </summary>
        internal protected abstract O CreateWidget( T item );

        private void AddListeners()
        {
            var l1 = Game.Instance.Keyboard.Listen( Key.Up, ButtonState.Pressed, scrollUp, null ).InContext( this );
            var l2 = Game.Instance.Keyboard.Listen( Key.Down, ButtonState.Pressed, scrollDown, null ).InContext( this );
            associatedListeners.AddItems(l1, l2);
        }

#if WINDOWS
        private Widget CreateVerticalScrollPanel()
        {
            Widget scrollPanel = new Widget( new VerticalLayout() ) { Color = Color.Transparent, HorizontalSizing = Sizing.FixedSize };

            if ( upImage == null )
            {
                upImage = Game.LoadImageFromResources( "UpArrow.png" );
                downImage = Game.LoadImageFromResources( "DownArrow.png" );
                transparentImage = Image.FromColor( upImage.Width, upImage.Height, Color.Transparent );
            }

            scrollUpButton = new PushButton( transparentImage );
            scrollUpButton.Clicked += scrollUp;
            scrollPanel.Add( scrollUpButton );

            scrollPanel.Add( new VerticalSpacer() );

            scrollDownButton = new PushButton( transparentImage );
            scrollDownButton.Clicked += scrollDown;
            scrollPanel.Add( scrollDownButton );

            return scrollPanel;
        }
#endif

        protected void Reset()
        {
            Content.Clear();

            foreach ( var item in List )
            {
                Content.Add( CreateWidget( item ) );
            }

#if WINDOWS
            // This is a bit tricky case. Because the clear call above doesn't take
            // effect immediately, calling UpdateLayout() uses old objects when updating
            // layout. Thus, let's just wait for at least one update to occur.
            Game.DoNextUpdate( delegate { if ( !Content.IsAtBottom ) ShowDownButton(); } );
#endif
        }

#if WINDOWS
        private void ShowUpButton()
        {
            scrollUpButton.Image = upImage;
        }
#endif

#if WINDOWS
        private void ShowDownButton()
        {
            scrollDownButton.Image = downImage;
        }
#endif

#if WINDOWS
        private void Hide(PushButton button)
        {
            button.Image = transparentImage;
        }
#endif

        private void scrollDown()
        {
            Content.ScrollDown();

#if WINDOWS
            if ( !Content.IsAtTop )
            {
                ShowUpButton();
            }

            if ( Content.IsAtBottom )
            {
                Hide( scrollDownButton );
            }
#endif
        }

        private void scrollUp()
        {
            Content.ScrollUp();

#if WINDOWS
            if ( Content.IsAtTop )
            {
                Hide( scrollUpButton );
            }
#endif
        }

        private void listChanged()
        {
            Reset();
        }

        /// <summary>
        /// Sitoo olemassaolevan listan tähän näyttöön.
        /// Kun listaa muutetaan, näytetyt arvot päivittyvät automaattisesti.
        /// </summary>
        public void Bind( INotifyList<T> list )
        {
            this.List = list;
            list.Changed += listChanged;
            Reset();
        }

        /// <summary>
        /// Poistaa yhteyden olemassaolevaan listaan.
        /// </summary>
        public void Unbind()
        {
            List.Changed -= listChanged;
        }
    }
}
