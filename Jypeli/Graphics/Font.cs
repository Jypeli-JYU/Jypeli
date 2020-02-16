using System;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using XnaV2 = Microsoft.Xna.Framework.Vector2;
using SpriteFontPlus;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace Jypeli
{
    internal enum ContentSource
    {
        GameContent,
        ResourceContent
    }


    /// <summary>
    /// Fontti.
    /// </summary>
    public class Font
    {
        private static string defaultFont = "Roboto-Regular.ttf";
        private static string defaultFontBold = "Roboto-Bold.ttf";

        /// <summary>
        /// Pieni oletusfontti.
        /// </summary>
        [Obsolete("Käytä oletusfonttia ja aseta fontin koko Font.SetFontSize()-metodilla")]
        public static readonly Font DefaultSmall = new Font(defaultFont, ContentSource.ResourceContent, 15);

        /// <summary>
        /// Oletusfontti.
        /// </summary>
        public static readonly Font Default = new Font(defaultFont, ContentSource.ResourceContent, 25);

        /// <summary>
        /// Suuri oletusfontti.
        /// </summary>
        [Obsolete("Käytä oletusfonttia ja aseta fontin koko Font.SetFontSize()-metodilla")]
        public static readonly Font DefaultLarge = new Font(defaultFont, ContentSource.ResourceContent, 40);

        /// <summary>
        /// Valtava oletusfontti.
        /// </summary>
        [Obsolete("Käytä oletusfonttia ja aseta fontin koko Font.SetFontSize()-metodilla")]
        public static readonly Font DefaultHuge = new Font(defaultFont, ContentSource.ResourceContent, 60);

        /// <summary>
        /// Lihavoitu pieni oletusfontti.
        /// </summary>
        [Obsolete("Käytä oletusfonttia ja aseta fontin koko Font.SetFontSize()-metodilla")]
        public static readonly Font DefaultSmallBold = new Font(defaultFontBold, ContentSource.ResourceContent, 15);

        /// <summary>
        /// Lihavoitu oletusfontti.
        /// </summary>
        public static readonly Font DefaultBold = new Font(defaultFontBold, ContentSource.ResourceContent, 25);

        /// <summary>
        /// Lihavoitu suuri oletusfontti.
        /// </summary>
        [Obsolete("Käytä oletusfonttia ja aseta fontin koko Font.SetFontSize()-metodilla")]
        public static readonly Font DefaultLargeBold = new Font(defaultFontBold, ContentSource.ResourceContent, 40);

        /// <summary>
        /// Lihavoitu valtava oletusfontti.
        /// </summary>
        [Obsolete("Käytä oletusfonttia ja aseta fontin koko Font.SetFontSize()-metodilla")]
        public static readonly Font DefaultHugeBold = new Font(defaultFontBold, ContentSource.ResourceContent, 60);

        private SpriteFont xnaFont;
        private string name;
        private int fontSize;
        private ContentSource source;
        private Vector[] charsizes;

        internal SpriteFont XnaFont
        {
            get { DoLoad(); return xnaFont; }
        }

        /// <summary>
        /// Merkin leveys.
        /// </summary>
        public double CharacterWidth
        {
            get { return XnaFont.MeasureString( "X" ).X; }
        }

        /// <summary>
        /// Merkin korkeus.
        /// </summary>
        public double CharacterHeight
        {
            get { return XnaFont.MeasureString( "X" ).Y; }
        }

        /// <summary>
        /// Fontin koko
        /// </summary>
        /// <returns></returns>
        public int GetFontSize()
        {
            return fontSize;
        }

        /// <summary>
        /// Asettaa fontin koon
        /// </summary>
        /// <param name="value"></param>
        public void SetFontSize(int value)
        {
            if (value <= 0) throw new Exception("Fontsize must be greater than zero.");
            fontSize = value;
            xnaFont = null;
            DoLoad();
        }
        /// <summary>
        /// Lataa uuden fontin contentista.
        /// </summary>
        /// <param name="name">Fontin tiedostonimi.</param>
        /// <returns></returns>
        public static Font FromContent(string name)
        {
            Font font = new Font("Content/" + name, ContentSource.GameContent);
            font.DoLoad();
            return font;
        }

        /// <summary>
        /// Lataa uuden fontin contentista.
        /// </summary>
        /// <param name="name">Fontin tiedostonimi.</param>
        public Font(string name) : this(name, ContentSource.GameContent) { }

        internal Font(string name, ContentSource source)
        {
            this.xnaFont = null;
            this.charsizes = null;
            this.name = name;
            this.source = source;
            this.fontSize = 30;
        }

        internal Font( string name, ContentSource source, int fontSize)
        {
            this.xnaFont = null;
            this.charsizes = null;
            this.name = name;
            this.source = source;
            this.fontSize = fontSize;
        }

        internal Font( SpriteFont xnaFont )
        {
            this.xnaFont = xnaFont;
            this.charsizes = new Vector[xnaFont.Characters.Count];
            this.name = null;
            this.source = ContentSource.ResourceContent;
        }

        private void DoLoad()
        {
            if ( xnaFont == null )
            {
                Stream s;
                if (this.source == ContentSource.ResourceContent) s = Game.ResourceContent.StreamInternalResource("Jypeli.Content.Fonts." + name);
                else s = new FileStream(name, FileMode.Open);
                var fontBakeResult = TtfFontBaker.Bake(s,
                    fontSize,
                    2048, // TODO: Mikä on hyvä arvo tähän?
                    2048,
                    new[]
                    {
                        CharacterRange.BasicLatin,
                        CharacterRange.Latin1Supplement,
                        CharacterRange.LatinExtendedA,
                    }
                );

                xnaFont = fontBakeResult.CreateSpriteFont(Game.GraphicsDevice);
            }

        }

        /// <summary>
        /// Palauttaa annetun merkin koon tässä fontissa.
        /// </summary>
        /// <param name="c">Merkki</param>
        /// <returns>Kokovektori, nollavektori jos merkkiä ei ole määritelty</returns>
        public Vector GetCharacterSize( char c )
        {
            int index = XnaFont.Characters.IndexOf( c );
            if ( index < 0 ) return Vector.Zero;

            if ( charsizes[index] == Vector.Zero )
            {
                XnaV2 xnaSize = XnaFont.MeasureString( c.ToString() );
                charsizes[index] = new Vector( xnaSize.X, xnaSize.Y );
            }

            return charsizes[index];
        }

        /// <summary>
        /// Katkaisee merkkijonon loppupäästä niin että se sopii annettuun
        /// pikselileveyteen fontilla kirjoitettuna.
        /// </summary>
        /// <param name="str">Merkkijono</param>
        /// <param name="maxLineWidth">Maksimipikselimäärä merkkijonolle</param>
        /// <returns></returns>
        public string TruncateText( string str, double maxLineWidth )
        {
            StringBuilder builder = new StringBuilder( str );
            double realWidth = XnaFont.MeasureString( str ).X;

            while ( realWidth > maxLineWidth )
            {
                builder.Remove( builder.Length - 1, 1 );
                realWidth = XnaFont.MeasureString( builder ).X;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Laskee tekstin koon fontilla.
        /// </summary>
        /// <param name="str">Teksti.</param>
        /// <returns>Vektorin, joka kertoo tekstin koon.</returns>
        public Vector MeasureSize(string str)
        {
            DoLoad();
            var xnaVector2 = xnaFont.MeasureString(str);
            return new Vector(xnaVector2.X, xnaVector2.Y);
        }

        private static void appendLine(StringBuilder dest, StringBuilder line)
        {
            if ( dest.Length > 0 ) dest.Append( "\n" );
            line.RemoveLeading( c => Char.IsWhiteSpace( c ) );
            line.PutTo( dest );
        }

        /// <summary>
        /// Rivittää tekstin.
        /// </summary>
        /// <param name="text">Rivitettävä teksti.</param>
        /// <param name="softLineWidth">Leveys jonka jälkeen seuraava sana rivitetään seuraavalle riville.</param>
        /// <param name="hardLineWidth">Leveys jonka jälkeen sana katkaistaan keskeltä.</param>
        public string WrapText( string text, double softLineWidth, double hardLineWidth )
        {
            if ( softLineWidth <= 0 || hardLineWidth <= 0 )
            {
                throw new ArgumentException( "Width must be positive." );
            }

            StringBuilder src = new StringBuilder( text );
            StringBuilder word = new StringBuilder();
            StringBuilder line = new StringBuilder();
            StringBuilder dest = new StringBuilder();
            double lineWidth = 0;

            while ( src.Length > 0 || word.Length > 0 )
            {
                if (word.Length == 0)
                    src.TakeFirstWord( word );

                var wordWidth = XnaFont.MeasureString( word ).X;

                if ( lineWidth + wordWidth > softLineWidth )
                {
                    appendLine( dest, line );
                    word.PutTo( line );
                    lineWidth = 0;
                }
                else if (lineWidth + wordWidth > hardLineWidth)
                {
                    int wi = FindWrapIndex(word, hardLineWidth - lineWidth, false);
                    word.PutTo(line, 0, wi + 1);
                    appendLine(dest, line);
                    lineWidth = 0;
                }
                else
                {
                    word.PutTo( line );
                    lineWidth += wordWidth;
                }
            }

            if ( line.Length > 0 )
                appendLine( dest, line );

            return dest.ToString();
        }

        /// <summary>
        /// Etsii katkaisuindeksin merkkijonolle merkki kerrallaan.
        /// Välilyönneillä ei ole erikoisasemaaa.
        /// </summary>
        /// <param name="text">Merkkijono</param>
        /// <param name="maxWidth">Maksimileveys pikseleinä</param>
        /// <param name="fromRight">Oikealta vasemmalle (oletus vasemmalta oikealle)</param>
        /// <returns>Katkaisukohdan indeksi</returns>
        private int FindWrapIndex( StringBuilder text, double maxWidth, bool fromRight )
        {
            double currentWidth = -XnaFont.Spacing;
            int i = fromRight ? text.Length - 1 : 0;
            int step = fromRight ? -1 : 1;

            //for ( int i = 0; i < text.Length; i++ )
            while ( ( fromRight && i >= 0 ) || ( !fromRight && i < text.Length ) )
            {
                currentWidth += XnaFont.Spacing + GetCharacterSize( text[i] ).X;
                if ( currentWidth >= maxWidth ) return i;
                i += step;
            }

            return fromRight ? -1 : text.Length;
        }
    }
}
