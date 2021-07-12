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
using FontStashSharp;
using Jypeli.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jypeli
{
    /// <summary>
    /// Kuinka tekstikentän kokoa käsitellään.
    /// </summary>
    public enum TextSizeMode
    {
        /// <summary>
        /// Tekstikentän koko on käyttäjän asettama.
        /// Ylipitkä teksti katkaistaan.
        /// </summary>
        None,

        /// <summary>
        /// Tekstikentän koko asetetaan tekstin koon mukaan.
        /// </summary>
        AutoSize,

        /// <summary>
        /// Tekstin koko asetetaan tekstikentän koon mukaan.
        /// </summary>
        StretchText,

        /// <summary>
        /// Teksti rivitetään tekstikentän leveyden mukaan.
        /// </summary>
        Wrapped,
    }


    /// <summary>
    /// Tekstikenttä.
    /// </summary>
    public class Label : BindableWidget
    {
        private Font font;
        private string title = "";
        private string originalText = "";
        private string visibleText = "";
        private TextSizeMode sizeMode = TextSizeMode.None;
        private int decimalPlaces = 2;
        private string doubleFormatString = "{0:N2}";
        private string intFormatString = "{0:D1}";
        private double _xMargin;
        private double _yMargin;
        private bool useDefaultHeight = false;
        private bool initialized = false;
        private const double DefaultWidth = 100;
        private Color[] characterColors;

        static double GetDefaultHeight()
        {
            Vector2 fontDims = Font.Default.XnaFont.MeasureString( "A" );
            return fontDims.Y;
        }

        /// <summary>
        /// Teksti.
        /// </summary>
        public virtual string Text
        {
            get { return originalText; }
            set
            {
                if ( value == null )
                    throw new ArgumentException( "Text must not be null" );
                originalText = value;
                updateSize();
            }
        }

        /// <summary>
        /// Onko tekstiä katkaistu
        /// </summary>
        public bool IsTruncated
        {
            get { return visibleText.Length < ( originalText.Length ); }
        }
        
        Vector3 _textScale = new Vector3(1, 1, 1);

        /// <summary>
        /// Tekstin skaalaus. Oletus (1,1) ; isompi suurempi.
        /// </summary>
        public Vector TextScale
        {
            get { return new Vector(_textScale.X, _textScale.Y); }
            set
            {
                _textScale = new Vector3((float)value.X, (float)value.Y, 1.0f);
                updateSize();
            }
        }


        /// <summary>
        /// Kuinka monta desimaalia näytetään, kun tekstikenttä on
        /// sidottu näyttämään desimaalilukua.
        /// </summary>
        public int DecimalPlaces
        {
            get { return decimalPlaces; }
            set
            {
                decimalPlaces = value;
                DoubleFormatString = "{0:N" + decimalPlaces + "}";
            }
        }


        /// <summary>
        /// Millä tavalla desimaalinumerot muotoillaan.
        /// Tämän asettaminen ylikirjoittaa <c>DecimalPlaces</c> asetuksen.
        /// </summary>
        public string DoubleFormatString
        {
            get { return doubleFormatString; }
            set
            {
                doubleFormatString = value;
                UpdateValue();
            }

        }


        /// <summary>
        /// Millä tavalla int numerot muotoillaan.
        /// </summary>
        /// <example>
        ///   "{0:D3}" näyttää numeron 5 muodossa 005.
        ///   teksti = "Laskuri:"; naytto.IntFormatString = " " + teksti + " {0:D2} ";  
        ///         näyttää numeron 5 muodossa " Laskuri: 05 "; // huomaa vielä tyhjää ympärillä
        /// </example>
        public string IntFormatString
        {
            get { return intFormatString; }
            set
            {
                intFormatString = value;
                UpdateValue();
            }

        }

        /// <summary>
        /// Asettaa tekstin, joka näkyy ennen kiinnitetyn mittarin arvoa.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Kuinka tekstikentän koko määräytyy.
        /// </summary>
        public TextSizeMode SizeMode
        {
            get { return sizeMode; }
            set { sizeMode = value; updateSize(); }
        }

        /// <summary>
        /// Näytettävän tekstin koko.
        /// Ei välttämättä sama kuin <c>Size</c>.
        /// </summary>
        public Vector TextSize { get; private set; }

        /// <summary>
        /// Tekstin väri.
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// Tekstin yksittäisten kirjainten väri.
        /// Jos tämä on asetettu, jätetään <c>TextColor</c>-kentän arvo huomioimatta.
        /// Taulukossa pitää olla väri jokaiselle kirjaimelle, ts. sen pituus pitää olla vähintään sama kuin tekstin.
        /// </summary>
        public Color[] CharacterColors 
        { 
            get { return characterColors; } 
            set { characterColors = value; }
        }

        /// <summary>
        /// Tekstin fontti.
        /// </summary>
        public virtual Font Font
        {
            get { return font; }
            set { font = value; updateSize(); }
        }

        /// <summary>
        /// Tekstin sijoitus vaakasuunnassa.
        /// Vaikuttaa vain, jos tekstikentän koko on suurempi kuin tekstin koko
        /// ja <c>SizeMode</c> ei ole <c>SizeMode.AutoSize</c>.
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Tekstin sijoitus pystysuunnassa.
        /// Vaikuttaa vain, jos tekstikentän koko on suurempi kuin tekstin koko
        /// ja <c>SizeMode</c> ei ole <c>SizeMode.AutoSize</c>.
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Marginaali vasemmasta/oikeasta reunasta.
        /// </summary>
        public double XMargin
        {
            get { return _xMargin; }
            set { _xMargin = value; updateSize(); }
        }

        /// <summary>
        /// Marginaali ylä-/alareunasta.
        /// </summary>
        public double YMargin 
        {
            get { return _yMargin; }
            set { _yMargin = value; updateSize(); }
        }

        /// <summary>
        /// Tekstikentän koko.
        /// Jos SizeMode on SizeMode.StretchText, teksti
        /// venytetään kentän koon mukaiseksi.
        /// </summary>
        public override Vector Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
                this.updateSize();
            }
        }

        /// <summary>
        /// Luo uuden tekstikentän. Asettaa
        /// koon tekstin mukaan.
        /// </summary>
        public Label()
            : this( DefaultWidth, 10 )
        {
            sizeMode = TextSizeMode.AutoSize;

            if ( Game.Instance != null )
            {
                this.Height = GetDefaultHeight();
                updateSize();
            }
            else
                Game.InstanceInitialized += setDefaultHeight;
        }

        /// <summary>
        /// Luo uuden tekstikentän annetulla tekstillä. Asettaa
        /// koon tekstin mukaan.
        /// </summary>
        public Label( string text )
            : this( DefaultWidth, 10, text )
        {
            sizeMode = TextSizeMode.AutoSize;

            if ( Game.Instance != null )
            {
                this.Height = GetDefaultHeight();
                updateSize();
            }
            else
                Game.InstanceInitialized += setDefaultHeight;
        }

        /// <summary>
        /// Luo uuden tekstikentän animaatiolla.
        /// </summary>
        public Label( Animation animation )
            : base( animation )
        {
            Game.AssertInitialized( Initialize );
        }

        /// <summary>
        /// Luo uuden tekstikentän.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public Label( double width, double height )
            : this( width, height, "" )
        {
        }

        /// <summary>
        /// Luo uuden tekstikentän.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="text">Teksti.</param>
        public Label( double width, double height, string text )
            : base( width, height, Shape.Rectangle )
        {
            this.sizeMode = TextSizeMode.None;
            this.originalText = text;
            Game.AssertInitialized( Initialize );
        }

        private void setDefaultHeight()
        {
            if ( !useDefaultHeight ) return;
            this.Height = GetDefaultHeight();
        }

        private void Initialize()
        {
            font = Font.Default;
            Color = Color.Transparent;
            TextColor = Color.Black;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            updateSize();
            initialized = true;
            UpdateValue();
        }

        /// <inheritdoc/>
        protected override void UpdateValue()
        {
            if ( !initialized || !Bound ) return;

            if ( Meter is IntMeter )
            {
                int newNumber = ( (IntMeter)Meter ).Value;
                Text = Title + string.Format( intFormatString, newNumber );
            }
            else if ( Meter is DoubleMeter )
            {
                double newNumber = ( (DoubleMeter)Meter ).Value;
                Text = Title + string.Format( doubleFormatString, newNumber );
            }
        }

        /// <summary>
        /// Päivittää tekstikentän/tekstin koon ja rivittää tekstin.
        /// </summary>
        private void updateSize()
        {
            DynamicSpriteFont xnaFont = this.Font.XnaFont;
            visibleText = (title.Length > 0) ? (title + ": " + originalText) : originalText;

            if ( visibleText.Length == 0 )
            {
                this.TextSize = Vector.Zero;
                NotifyParentAboutChangedSizingAttributes();
                return;
            }

            Vector2 rawTextDims = xnaFont.MeasureString( visibleText );
            Vector clientArea = new Vector( this.Width - 2 * XMargin, this.Height - 2 * YMargin );
            Vector fullTextDims = new Vector( _textScale.X * rawTextDims.X, _textScale.Y * rawTextDims.Y );
            TextSize = new Vector( fullTextDims.X, fullTextDims.Y );

            switch ( SizeMode )
            {
                case TextSizeMode.None:
                    TruncateText();
                    break;

                case TextSizeMode.StretchText:
                    _textScale = new Vector( clientArea.X / rawTextDims.X, clientArea.Y / rawTextDims.Y );
                    TextSize = clientArea;
                    break;

                case TextSizeMode.AutoSize:
                    base.Size = PreferredSize = new Vector( fullTextDims.X + 2 * XMargin, fullTextDims.Y + 2 * YMargin );
                    break;

                case TextSizeMode.Wrapped:
                    WrapText();
                    break;
            }

            NotifyParentAboutChangedSizingAttributes();
        }

        private void TruncateText()
        {
            double textWidth = this.Width - 2 * XMargin;
            if ( textWidth <= 0 )
            {
                textWidth = this.Width;
            }

            visibleText = font.TruncateText( Text, textWidth );

            Vector2 textDims = Font.XnaFont.MeasureString( visibleText );
            TextSize = new Vector( textDims.X, textDims.Y );
        }

        private void WrapText()
        {
            DynamicSpriteFont xnaFont = this.Font.XnaFont;
            Vector2 rawTextDims = xnaFont.MeasureString( visibleText );
            Vector2 fullTextDims = new Vector2( _textScale.X * rawTextDims.X, _textScale.Y * rawTextDims.Y );

            if ( Width <= 0 || fullTextDims.X <= Width )
                return;

            double hardBreak = base.Size.X - 2 * XMargin;
            double softBreak = Math.Max( hardBreak / 2, hardBreak - 5 * Font.CharacterWidth );

            visibleText = Font.WrapText( visibleText, softBreak, hardBreak );
            Vector2 textDims = xnaFont.MeasureString( visibleText );
            base.Size = PreferredSize = new Vector( base.Size.X, textDims.Y + 2 * YMargin );
            TextSize = new Vector( textDims.X, textDims.Y );
        }

        private double GetHorizontalAlignment()
        {
            switch ( HorizontalAlignment )
            {
                case HorizontalAlignment.Center:
                    return 0;
                case HorizontalAlignment.Left:
                    return ( -Width + TextSize.X ) / 2 + XMargin;
                case HorizontalAlignment.Right:
                    return ( Width - TextSize.X ) / 2 - XMargin;
                default:
                    return XMargin;
            }
        }

        private double GetVerticalAlignment()
        {
            switch ( VerticalAlignment )
            {
                case VerticalAlignment.Center:
                    return 0;
                case VerticalAlignment.Top:
                    return ( Size.Y - TextSize.Y ) / 2 - YMargin;
                case VerticalAlignment.Bottom:
                    return ( -Size.Y + TextSize.Y ) / 2 + YMargin;
                default:
                    return YMargin;
            }
        }

        /// <inheritdoc/>
        public override void Draw( Matrix parentTransformation, Matrix transformation )
        {
            Draw( parentTransformation, transformation, visibleText );
        }

        /// <inheritdoc/>
        protected void Draw( Matrix parentTransformation, Matrix transformation, string text )
        {
            Matrix m = Matrix.CreateScale( _textScale )
                * Matrix.CreateTranslation( (float)GetHorizontalAlignment(), (float)GetVerticalAlignment(), 0 )
                * Matrix.CreateRotationZ( (float)Angle.Radians )
                * Matrix.CreateTranslation( (float)Position.X, (float)Position.Y, 0 )
                * parentTransformation;

            if(CharacterColors == null)
                Renderer.DrawText( text, ref m, Font, TextColor );
            else
                Renderer.DrawText(text, ref m, Font, CharacterColors);
            base.Draw( parentTransformation, transformation );
        }
    }
}
