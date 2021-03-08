using System;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using XnaV2 = Microsoft.Xna.Framework.Vector2;
using FontStashSharp;
using System.IO;
using Microsoft.Xna.Framework.Content;
using System.ComponentModel;
using System.Collections.Generic;

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
        /// Oletusfontti.
        /// </summary>
        public static readonly Font Default = new Font(defaultFont, ContentSource.ResourceContent, 25);

        /// <summary>
        /// Lihavoitu oletusfontti.
        /// </summary>
        public static readonly Font DefaultBold = new Font(defaultFontBold, ContentSource.ResourceContent, 25);

        private DynamicSpriteFont xnaFont;
        private FontSystem fontSystem;
        private string name;
        private int size;
        private ContentSource source;

        private int blurAmount = 0;
        private int strokeAmount = 0;

        private List<string> mergedFonts = new List<string>();

        internal DynamicSpriteFont XnaFont
        {
            get { DoLoad(); return xnaFont; }
        }

        internal FontSystem FontSystem
        {
            get { DoLoad(); return fontSystem; }
        }

        /// <summary>
        /// Fontin koko.
        /// 
        /// Käytä tätä ainoastaan itse luomiesi Font-olioiden kanssa.
        /// </summary>
        /// <remarks>
        /// Älä muuta <c>Font.Default</c>-olion kokoa!!!
        /// </remarks>
        public int Size
        {
            get { return size; }
            set
            {
                DoLoad();
                if (value <= 0) throw new Exception("Fontsize must be greater than zero.");
                size = value;
                xnaFont = fontSystem.GetFont(size);                
                fontSystem = null;
                DoLoad();
            }
        }

        /// <summary>
        /// Asettaa tekstin sumennuksen määrän.
        /// 
        /// Tekstille voi asettaa ainoastaan sumennuksen tai reunuksen, ei molempia.
        /// Tämän muuttaminen asettaa <c>StrokeAmount</c>in nollaan.
        /// </summary>
        /// <remarks>
        /// Älä aseta <c>Font.Default</c>-oliolle!
        /// </remarks>
        public int BlurAmount
        {
            get { return blurAmount; }
            set
            {
                DoLoad();
                blurAmount = value;
                strokeAmount = 0;
                fontSystem = null;
                DoLoad();
            }
        }

        /// <summary>
        /// Asettaa tekstin reunuksen paksuuden.
        /// 
        /// Tekstille voi asettaa ainoastaan sumennuksen tai reunuksen, ei molempia.
        /// Tämän muuttaminen asettaa <c>BlurAmount</c>in nollaan.
        /// </summary>
        /// <remarks>
        /// Älä aseta <c>Font.Default</c>-oliolle!
        /// </remarks>
        public int StrokeAmount
        {
            get { return strokeAmount; }
            set
            {
                DoLoad();
                strokeAmount = value;
                blurAmount = 0;
                fontSystem = null;
                DoLoad();
            }
        }

        /// <summary>
        /// Merkin leveys.
        /// </summary>
        public double CharacterWidth
        {
            get { return XnaFont.MeasureString( "X" ).X; } // TODO: pitäisi todellisuudessa etsiä fontin suurin merkki ja katsoa sen mitat.
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
            Font font = new Font("Content/" + name, ContentSource.GameContent);
            return font;
        }

        /// <summary>
        /// Luo uuden oletusfontin halutulla koolla.
        /// </summary>
        /// <param name="fontSize">Fontin koko. Oletusfontti on kokoa 25</param>
        public Font(int fontSize=25) : this(defaultFont, ContentSource.ResourceContent, fontSize)
        {
        }

        /// <summary>
        /// Luo uuden oletusfontin halutulla koolla.
        /// </summary>
        /// <param name="fontSize">Fontin koko. Oletusfontti on kokoa 25</param>
        /// <param name="bold">Onko fontti boldattu</param>
        public Font(int fontSize, bool bold) : this(bold ? defaultFontBold : defaultFont, ContentSource.ResourceContent, fontSize)
        {
        }

        /// <summary>
        /// Lataa uuden fontin contentista.
        /// </summary>
        /// <param name="name">Fontin tiedostonimi.</param>
        public Font(string name) : this(name, ContentSource.GameContent) { }

        /// <summary>
        /// Lataa uuden fontin contentista.
        /// </summary>
        /// <param name="name">Fontin tiedostonimi.</param>
        /// <param name="fontSize">Fontin koko. Oletusfontti on kokoa 25</param>
        public Font(string name, int fontSize) : this(name, ContentSource.GameContent, fontSize) { }

        internal Font(string name, ContentSource source)
        {
            this.xnaFont = null;
            this.name = name;
            this.source = source;
            this.size = 25;
        }

        internal Font( string name, ContentSource source, int fontSize)
        {
            this.xnaFont = null;
            this.name = name;
            this.source = source;
            this.size = fontSize;
        }

        internal Font( DynamicSpriteFont xnaFont )
        {
            this.xnaFont = xnaFont;
            this.name = null;
            this.source = ContentSource.ResourceContent;
        }

        private void DoLoad()
        {
            if (fontSystem == null)
            {
                fontSystem = new FontSystem(StbTrueTypeSharpFontLoader.Instance, new Texture2DManager(Game.GraphicsDevice), 1024, 1024, blurAmount, StrokeAmount);

                Stream s;
                if (this.source == ContentSource.ResourceContent) s = Game.ResourceContent.StreamInternalResource("Jypeli.Content.Fonts." + name);
                else s = File.Open(name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // Tässä pitää jostain syystä olla ReadWrite, vaikka tiedosto ainoastaan luetaan.

                fontSystem.AddFont(s);
                xnaFont = fontSystem.GetFont(size);

                mergedFonts.ForEach(name => MergeFont(name));
                
                s.Dispose();
            }
        }

        /// <summary>
        /// Lisää toisen fontin merkistön tähän fonttiin.
        /// Jos fontit sisältävät päällekkäistä merkistöä, ensimmäisenä lisätty säilyy käytettävänä ulkoasuna.
        /// </summary>
        /// <param name="filename">Contentissa olevan fonttitiedoston nimi.</param>
        public void AddFont(string filename)
        {
            DoLoad();
            MergeFont(filename);
            mergedFonts.Add(filename);
        }


        private void MergeFont(string filename)
        {
            Stream s = File.Open("Content/" + filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fontSystem.AddFont(s);
        }

        /// <summary>
        /// Palauttaa annetun merkin koon tässä fontissa.
        /// </summary>
        /// <param name="c">Merkki</param>
        /// <returns>Kokovektori, nollavektori jos merkkiä ei ole määritelty</returns>
        public Vector GetCharacterSize( char c )
        {
            return XnaFont.MeasureString(c.ToString());
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
            var xnaVector2 = XnaFont.MeasureString(str);
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
            
            double currentWidth = -xnaFont.FontSystem.CharacterSpacing;
            int i = fromRight ? text.Length - 1 : 0;
            int step = fromRight ? -1 : 1;

            //for ( int i = 0; i < text.Length; i++ )
            while ( ( fromRight && i >= 0 ) || ( !fromRight && i < text.Length ) )
            {
                currentWidth += xnaFont.FontSystem.CharacterSpacing + GetCharacterSize( text[i] ).X;
                if ( currentWidth >= maxWidth ) return i;
                i += step;
            }

            return fromRight ? -1 : text.Length;
        }
    }
}
