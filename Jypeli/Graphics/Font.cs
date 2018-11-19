using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaV2 = Microsoft.Xna.Framework.Vector2;
using XnaColor = Microsoft.Xna.Framework.Color;

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
        /// <summary>
        /// Oletusfontti.
        /// </summary>

#if ANDROID
        public static readonly Font Default = new Font("HugeFont", ContentSource.ResourceContent);
#else
        public static readonly Font Default = new Font( "MediumFont", ContentSource.ResourceContent );
#endif
        /// <summary>
        /// Pieni oletusfontti.
        /// </summary>
        public static readonly Font DefaultSmall = new Font( "SmallFont", ContentSource.ResourceContent );

        /// <summary>
        /// Suuri oletusfontti.
        /// </summary>
        public static readonly Font DefaultLarge = new Font( "LargeFont", ContentSource.ResourceContent );

        /// <summary>
        /// Oletusfontti.
        /// </summary>
        public static readonly Font Medium = new Font("MediumFont", ContentSource.ResourceContent);

        /// <summary>
        /// Lihavoitu oletusfontti.
        /// </summary>
        public static readonly Font DefaultBold = new Font( "MediumFontBold", ContentSource.ResourceContent );

        /// <summary>
        /// Lihavoitu pieni oletusfontti.
        /// </summary>
        public static readonly Font DefaultSmallBold = new Font( "SmallFontBold", ContentSource.ResourceContent );

        /// <summary>
        /// Lihavoitu suuri oletusfontti.
        /// </summary>
        public static readonly Font DefaultLargeBold = new Font( "LargeFontBold", ContentSource.ResourceContent );

        /// <summary>
        /// Valtava oletusfontti.
        /// </summary>
        public static readonly Font DefaultHuge = new Font("HugeFont", ContentSource.ResourceContent);

        /// <summary>
        /// Lihavoitu valtava oletusfontti.
        /// </summary>
        public static readonly Font DefaultHugeBold = new Font("HugeFontBold", ContentSource.ResourceContent);

        private SpriteFont xnaFont;
        private string name;
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
        /// Lataa uuden fontin contentista.
        /// </summary>
        /// <param name="name">Fontin tiedostonimi.</param>
        /// <returns></returns>
        public static Font FromContent(string name)
        {
            Font font = new Font(name, ContentSource.GameContent);
            font.DoLoad();
            return font;
        }

        /// <summary>
        /// Lataa uuden fontin contentista.
        /// </summary>
        /// <param name="name">Fontin tiedostonimi.</param>
        public Font(string name) : this(name, ContentSource.GameContent) { }

        internal Font( string name, ContentSource source )
        {
            this.xnaFont = null;
            this.charsizes = null;
            this.name = name;
            this.source = source;
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
                if ( source == ContentSource.ResourceContent )
                    xnaFont = Game.ResourceContent.Load<SpriteFont>( name );
                else
                    xnaFont = Game.Instance.Content.Load<SpriteFont>( name );

                charsizes = new Vector[xnaFont.Characters.Count];
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
