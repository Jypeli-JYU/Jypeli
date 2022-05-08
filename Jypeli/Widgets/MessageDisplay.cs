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
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.Collections.Generic;

using Matrix = System.Numerics.Matrix4x4;

namespace Jypeli
{
    /// <summary>
    /// Viestikenttä, jolla voi laittaa tekstiä ruudulle.
    /// Tätä sinun tuskin tarvitsee itse muodostaa.
    /// </summary>
    public class MessageDisplay : Widget
    {
        private struct Message
        {
            public String Text;
            public Color Color;
            public TimeSpan Expires;

            public bool Expired
            {
                get { return Expires <= Game.Time.SinceStartOfGame; }
            }

            public TimeSpan TimeLeft
            {
                get { return Expired ? TimeSpan.Zero : Expires - Game.Time.SinceStartOfGame; }
            }

            public Message(string text, Color color, TimeSpan lifetime)
            {
                Text = text;
                Color = color;
                Expires = Game.Time.SinceStartOfGame + lifetime;
            }
        }

        /// <summary>
        /// Kuinka monta viestiä kerrallaan näytetään.
        /// </summary>
        public int MaxMessageCount { get; set; }

        /// <summary>
        /// Kuinka pitkään yksi viesti näkyy.
        /// </summary>
        public TimeSpan MessageTime { get; set; }

        /// <summary>
        /// Käytettävä fontti.
        /// </summary>
        public Font Font
        {
            get { return _font; }
            set
            {
                _font = value;
                fontHeight = _font.MeasureSize("A").Y;
                UpdateSizeAndPosition();
            }
        }

        /// <summary>
        /// Tekstin väri.
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// Tekstin taustaväri.
        /// </summary>
        public Color BackgroundColor
        {
            get { return bgColor; }
            set
            {
                bgColor = value;
            }
        }

        /// <summary>
        /// Onko näyttö reaaliaikainen (oletuksena ei)
        /// Jos on, vanhin viesti poistetaan heti jos viestien maksimimäärä
        /// tulee täyteen. Jos ei, viesti jää jonoon odottamaan kunnes
        /// vanhimman viestin näyttöaika tulee täyteen.
        /// </summary>
        public bool RealTime { get; set; }

        private Color bgColor = Color.Transparent;
        private Font _font;
        private double fontHeight;
        private List<Message> messages = new List<Message>();
        private Queue<String> unseen = new Queue<string>();

        private Timer removeTimer;

        /// <summary>
        /// Luo uuden viestinäytön.
        /// </summary>
        public MessageDisplay()
            : base(0, 0)
        {
            removeTimer = new Timer();
            removeTimer.Timeout += RemoveMessages;

            TextColor = Color.Black;
            Color = Color.Transparent;

            MaxMessageCount = 20;
            MessageTime = TimeSpan.FromSeconds(5);
            Font = Font.Default;

            // default position to top-left-corner
            this.Position = new Vector(Game.Screen.LeftSafe + Game.Screen.WidthSafe / 2,
                                        Game.Screen.TopSafe - Game.Screen.HeightSafe / 2);
        }

        private void RemoveMessages()
        {
            removeTimer.Stop();

            while (messages.Count > 0 && messages[0].Expired)
            {
                messages.RemoveAt(0);
                if (unseen.Count > 0)
                    Add(unseen.Dequeue());
            }

            if (messages.Count > 0)
            {
                removeTimer.Interval = messages[0].TimeLeft.TotalSeconds;
                removeTimer.Start();
            }

            UpdateSizeAndPosition();
        }

        /// <inheritdoc/>
        public override void Draw(Matrix parentTransformation, Matrix transformation)
        {
            if (messages.Count == 0)
                return;
            Graphics.FontRenderer.Begin();
            for (int i = 0; i < Math.Min(messages.Count, MaxMessageCount); i++)
            {
                Font.SpriteFont.DrawText(Graphics.FontRenderer, messages[i].Text, Position - new Vector(Width / 2, i * fontHeight - Height / 2), messages[i].Color.ToSystemDrawing());
            }
            base.Draw(parentTransformation, transformation);
        }

        private void UpdateSizeAndPosition()
        {
            if (messages.Count == 0)
                Color = Color.Transparent;
            else
                Color = bgColor;

            double maxW = 0;
            double height = 0;

            for (int i = 0; i < Math.Min(messages.Count, MaxMessageCount); i++)
            {
                Vector dims = Font.SpriteFont.MeasureString(messages[i].Text);
                maxW = Math.Max(maxW, dims.X);
                height = Math.Max(height, dims.Y);
            }

            if (maxW > 0)
                Size = new Vector(maxW, fontHeight * Math.Min(messages.Count, MaxMessageCount));

            Position = new Vector(-Game.Screen.Width / 2 + Width / 2, Game.Screen.Height / 2 - Height / 2); // TODO: Tää on huono
        }

        /// <summary>
        /// Lisää uuden viestin näkymään.
        /// </summary>
        public void Add(string message)
        {
            if (messages.Count > MaxMessageCount)
            {
                if (RealTime)
                {
                    messages.RemoveRange(0, messages.Count - MaxMessageCount + 1);
                }
                else
                {
                    unseen.Enqueue(message);
                    return;
                }
            }

            messages.Add(new Message(message, TextColor, MessageTime));
            UpdateSizeAndPosition();

            if (!removeTimer.Enabled)
            {
                removeTimer.Interval = MessageTime.TotalSeconds;
                removeTimer.Start();
            }
        }

        /// <summary>
        /// Lisää useita tekstirivejä viestinäkymään
        /// </summary>
        /// <param name="strings"></param>
        public void Add(IEnumerable<string> strings)
        {
            // TODO: optimization?
            foreach (string s in strings)
                Add(s);
        }

        /// <summary>
        /// Lisää uuden viestin näkymään.
        /// </summary>
        public void Add(string message, Color color)
        {
            if (messages.Count > MaxMessageCount)
            {
                if (RealTime)
                {
                    messages.RemoveRange(0, messages.Count - MaxMessageCount + 1);
                }
                else
                {
                    unseen.Enqueue(message);
                    return;
                }
            }

            messages.Add(new Message(message, color, MessageTime));
            UpdateSizeAndPosition();

            if (!removeTimer.Enabled)
            {
                removeTimer.Interval = MessageTime.TotalSeconds;
                removeTimer.Start();
            }
        }

        /// <summary>
        /// Poistaa kaikki lisätyt viestit.
        /// </summary>
        public override void Clear()
        {
            messages.Clear();
            UpdateSizeAndPosition();
        }
    }
}
