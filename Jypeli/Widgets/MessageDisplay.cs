#region MIT License
/*
 * Copyright (c) 2009-2011 University of Jyv‰skyl‰, Department of Mathematical
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
 * Authors: Tero J‰ntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using XnaColor = Microsoft.Xna.Framework.Color;
using FontStashSharp;

namespace Jypeli
{
    public class MessageDisplay : Widget
    {
        private struct Message
        {
            public String Text;
            public Color Color;
            public TimeSpan Expires;

            public bool Expired
            {
                get { return Expires < Game.Time.SinceStartOfGame; }
            }

            public TimeSpan TimeLeft
            {
                get { return Expired ? TimeSpan.Zero : Expires - Game.Time.SinceStartOfGame; }
            }

            public Message( string text, Color color, TimeSpan lifetime )
            {
                Text = text;
                Color = color;
                Expires = Game.Time.SinceStartOfGame + lifetime;
            }
        }

        /// <summary>
        /// Kuinka monta viesti‰ kerrallaan n‰ytet‰‰n.
        /// </summary>
        public int MaxMessageCount { get; set; }

        /// <summary>
        /// Kuinka pitk‰‰n yksi viesti n‰kyy.
        /// </summary>
        public TimeSpan MessageTime { get; set; }

        /// <summary>
        /// K‰ytett‰v‰ fontti.
        /// </summary>
        public Font Font
        {
            get { return _font; }
            set
            {
                _font = value;
                fontHeight = value.XnaFont.MeasureString( "A" ).Y;
                UpdateTexture();
            }
        }

        /// <summary>
        /// Tekstin v‰ri.
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// Tekstin taustav‰ri.
        /// </summary>
        public Color BackgroundColor
        {
            get { return bgColor; }
            set
            {
                bgColor = value;
                UpdateTexture();
            }
        }

        /// <summary>
        /// Onko n‰yttˆ reaaliaikainen (oletuksena ei)
        /// Jos on, vanhin viesti poistetaan heti jos viestien maksimim‰‰r‰
        /// tulee t‰yteen. Jos ei, viesti j‰‰ jonoon odottamaan kunnes
        /// vanhimman viestin n‰yttˆaika tulee t‰yteen.
        /// </summary>
        public bool RealTime { get; set; }

        private Color bgColor = Color.Transparent;
        private Image bgImage = null;
        private Font _font;
        private float fontHeight;
        private List<Message> messages = new List<Message>();
        private Queue<String> unseen = new Queue<string>();

        private Timer removeTimer;
        
        /// <summary>
        /// Luo uuden viestin‰ytˆn.
        /// </summary>
        public MessageDisplay()
            : base( Game.Screen.WidthSafe, Game.Screen.HeightSafe )
        {
            removeTimer = new Timer();
            removeTimer.Timeout += RemoveMessages;

            TextColor = Color.Black;
            Color = Color.Transparent;

            MaxMessageCount = 20;
            MessageTime = TimeSpan.FromSeconds( 5 );
            Font = Font.Default;

            // default position to top-left-corner
            this.Position = new Vector( Game.Screen.LeftSafe + Game.Screen.WidthSafe / 2,
                                        Game.Screen.TopSafe - Game.Screen.HeightSafe / 2 );
        }

        private void RemoveMessages()
        {
            removeTimer.Stop();

            while ( messages.Count > 0 && messages[0].Expired )
            {
                messages.RemoveAt( 0 );
                if ( unseen.Count > 0 ) Add( unseen.Dequeue() );
            }

            if ( messages.Count > 0 )
            {
                removeTimer.Interval = messages[0].TimeLeft.TotalSeconds;
                removeTimer.Start();
            }

            UpdateTexture();
        }
        
        public override void Draw( Matrix parentTransformation, Matrix transformation )
        {
            SpriteBatch spriteBatch = Graphics.SpriteBatch;
            Matrix m =
                Matrix.CreateTranslation( (float)Position.X, (float)Position.Y, 0 )
                * parentTransformation;

            spriteBatch.Begin( SpriteSortMode.Immediate, BlendState.AlphaBlend, Graphics.GetDefaultSamplerState(), DepthStencilState.None, RasterizerState.CullCounterClockwise, null, m );

            if ( bgImage != null )
                spriteBatch.Draw( bgImage.XNATexture, Vector2.Zero, XnaColor.White );

            for ( int i = 0; i < messages.Count; i++ )
            {
                Font.XnaFont.DrawText(Graphics.FontRenderer, messages[i].Text, (new Vector2(0, i * fontHeight)).ToSystemNumerics(), messages[i].Color.AsXnaColor().ToSystemDrawing());
            }

            spriteBatch.End();

            base.Draw( parentTransformation, transformation );
        }

        private void UpdateTexture()
        {
            if ( messages.Count == 0 || bgColor == Color.Transparent )
            {
                bgImage = null;
                return;
            }

            double maxW = 0;

            for ( int i = 0; i < messages.Count; i++ )
            {
                Vector2 dims = Font.XnaFont.MeasureString( messages[i].Text );
                if ( dims.X > maxW ) maxW = dims.X;
            }

            if ( maxW > 0 )
                bgImage = new Image( maxW, messages.Count * fontHeight, bgColor );
        }

        /// <summary>
        /// Lis‰‰ uuden viestin n‰kym‰‰n.
        /// </summary>
        public void Add( string message )
        {
            if ( messages.Count > MaxMessageCount )
            {
                if ( RealTime )
                {
                    messages.RemoveRange( 0, messages.Count - MaxMessageCount + 1 );
                }
                else
                {
                    unseen.Enqueue( message );
                    return;
                }
            }

            messages.Add( new Message( message, TextColor, MessageTime ) );
            UpdateTexture();

            if ( !removeTimer.Enabled )
            {
                removeTimer.Interval = MessageTime.TotalSeconds;
                removeTimer.Start();
            }
        }

        public void Add( IEnumerable<string> strings )
        {
            // TODO: optimization?
            foreach ( string s in strings )
                Add( s );
        }

        /// <summary>
        /// Lis‰‰ uuden viestin n‰kym‰‰n.
        /// </summary>
        public void Add( string message, Color color )
        {
            if ( messages.Count > MaxMessageCount )
            {
                if ( RealTime )
                {
                    messages.RemoveRange( 0, messages.Count - MaxMessageCount + 1 );
                }
                else
                {
                    unseen.Enqueue( message );
                    return;
                }
            }

            messages.Add( new Message( message, color, MessageTime ) );
            UpdateTexture();

            if ( !removeTimer.Enabled )
            {
                removeTimer.Interval = MessageTime.TotalSeconds;
                removeTimer.Start();
            }
        }

        /// <summary>
        /// Poistaa kaikki lis‰tyt viestit.
        /// </summary>
        public override void Clear()
        {
            messages.Clear();
            UpdateTexture();
        }
    }
}
