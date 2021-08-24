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

namespace Jypeli
{
    /// <summary>
    /// Abstrakti kyselyikkuna
    /// </summary>
    /// <typeparam name="W"></typeparam>
    public abstract class CustomQueryWindow<W> : Window where W : Widget
    {
        internal virtual bool OkButtonOnPhone { get { return false; } }

        /// <summary>
        /// Viesti tai kysymys.
        /// </summary>
        public Label Message { get; private set; }

        /// <summary>
        /// Kysymyskomponentti.
        /// </summary>
        public W QueryWidget { get; private set; }

        /// <summary>
        /// OK-painike
        /// </summary>
        public PushButton OKButton { get; private set; }

        /// <inheritdoc/>
        public override Color Color
        {
            get { return base.Color; }
            set
            {
                QueryWidget.Color = value;

#if ANDROID
                if ( OkButtonOnPhone )
#endif
                {
                    OKButton.Color = Color.Darker( value, 40 );
                }
                base.Color = value;
            }
        }

        /// <summary>
        /// Alustaa uuden kyselyikkunan.
        /// </summary>
        /// <param name="message">Viesti tai kysymys</param>
        public CustomQueryWindow( string message )
        {
            Game.AssertInitialized<string>( Initialize, message );
        }

        /// <summary>
        /// Alustaa uuden kyselyikkunan kiinteän kokoiseksi.
        /// </summary>
        /// <param name="width">Ikkunan leveys</param>
        /// <param name="height">Ikkunan korkeus</param>
        /// <param name="message">Viesti tai kysymys</param>
        public CustomQueryWindow( double width, double height, string message )
            : base( width, height )
        {
            Game.AssertInitialized<string>( Initialize, message );
        }

        private void Initialize( string message )
        {
            Layout = new VerticalLayout { Spacing = 20, LeftPadding = 15, RightPadding = 15, TopPadding = 15, BottomPadding = 15 };

//#if ANDROID
            int maxWidth = (int)Game.Screen.Width - 30;
            Message = new Label(Math.Min(maxWidth, Font.Default.MeasureSize(message).X), 50, message)
                { SizeMode = TextSizeMode.Wrapped, VerticalSizing = Sizing.Expanding };
//#else
// Wrapped text and layouts don't work that well together... :/
// A simple workaround:
//            Message = new Label( 600, 100, message );
//            Message.SizeMode = TextSizeMode.Wrapped;
//            Message.HorizontalAlignment = HorizontalAlignment.Left;
//            Message.VerticalAlignment = VerticalAlignment.Top;
//#endif
            Add(Message);
            
            QueryWidget = CreateQueryWidget();
            Add( QueryWidget );

#if ANDROID
            if ( OkButtonOnPhone )
#endif
            {
                // By adding some space, we make it harder to accidently hit the OK button, especially on the phone.
                // Buttonrow is created in order to move the button to the right edge, for the same reason.
                Add( new VerticalSpacer { PreferredSize = new Vector( 1, 20 ), VerticalSizing = Sizing.FixedSize } );
                Add( CreateButtonRow() );
            }

            AddedToGame += AddListeners;
        }

        private Widget CreateButtonRow()
        {
            // Button row with only one button :)
            Widget buttonRow = new Widget( new HorizontalLayout() ) { Color = Color.Transparent };
            buttonRow.Add( new HorizontalSpacer() );

            OKButton = new PushButton( "OK" );
#if ANDROID
            if ( Game.Device.DisplayResolution == DisplayResolution.Large )
                OKButton.TextScale = new Vector(2, 2);
            else if ( Game.Device.DisplayResolution == DisplayResolution.HD720 )
                OKButton.TextScale = new Vector( 3, 3 );
#endif
            OKButton.Clicked += new Action(Close);
            buttonRow.Add( OKButton );

            return buttonRow;
        }

        /// <summary>
        /// Luo widgetin kyselyikkunan käyttöön
        /// </summary>
        /// <returns></returns>
        protected abstract W CreateQueryWidget();

        private void AddListeners()
        {
            Listener l;
#if ANDROID
            l = Game.Instance.TouchPanel.Listen(ButtonState.Pressed, delegate{ Close(); }, null).InContext(this);
#else
            l = Game.Instance.Keyboard.Listen(Key.Enter, ButtonState.Pressed, OKButton.Click, null).InContext(this);
#endif
            associatedListeners.Add(l);
        }
    }
}
