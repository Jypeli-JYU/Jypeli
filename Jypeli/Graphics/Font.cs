using System;
using System.Text;
using System.IO;
using FontStashSharp;

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

        private FontSystem fontsystem;
        private DynamicSpriteFont font;
        private string name;
        private int size;
        private ContentSource source;

        private int blurAmount = 0;
        private int strokeAmount = 0;

        internal static string[] FontExtensions { get; } = { ".ttf" };

        internal FontSystem FontSystem
        {
            get { DoLoad(); return fontsystem; }
        }

        internal DynamicSpriteFont SpriteFont
        {
            get { DoLoad(); return font; }
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
                DoLoad();
            }
        }

        /// <summary>
        /// Merkin leveys.
        /// </summary>
        public double CharacterWidth
        {
            get 
            {
                return SpriteFont.MeasureString("X").X;
            } // TODO: pitäisi todellisuudessa etsiä fontin suurin merkki ja katsoa sen mitat.
        }

        /// <summary>
        /// Merkin korkeus.
        /// </summary>
        public double CharacterHeight
        {
            get
            {
                return SpriteFont.MeasureString("X").Y;
            }
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
            this.name = name;
            this.source = source;
            this.size = 25;
        }

        internal Font( string name, ContentSource source, int fontSize)
        {
            this.name = name;
            this.source = source;
            this.size = fontSize;
        }

        private void DoLoad()
        {
            if (fontsystem == null)
            {
                fontsystem = new FontSystem();
                fontsystem.AddFont(Game.ResourceContent.StreamInternalFont("Roboto-Regular.ttf"));
                font = fontsystem.GetFont(Size);
                GenerateCommonGlyphs();
            }
        }

        /// <summary>
        /// Luodaan yleisimmät merkit valmiiksi, jotta niitä ei tarvitse luoda sitä mukaa kuin niitä käytetään.
        /// Tämä samalla vie fonttitekstuurin näytönohjaimelle.
        /// </summary>
        private void GenerateCommonGlyphs()
        {
            StringBuilder s = new StringBuilder();
            for (char i =(char)32; i < 255; i++)
            {
                s.Append(i);
            }
            Renderer.DrawText(s.ToString(), Vector.Zero, this, Color.Transparent);
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
        }


        private void MergeFont(string filename)
        {
            Stream s = File.Open("Content/" + filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        /// <summary>
        /// Palauttaa annetun merkin koon tässä fontissa.
        /// </summary>
        /// <param name="c">Merkki</param>
        /// <returns>Kokovektori, nollavektori jos merkkiä ei ole määritelty</returns>
        public Vector GetCharacterSize(char c)
        {
            return SpriteFont.MeasureString(c.ToString());
        }

        /// <summary>
        /// Laskee tekstin koon fontilla.
        /// </summary>
        /// <param name="str">Teksti.</param>
        /// <returns>Vektorin, joka kertoo tekstin koon.</returns>
        public Vector MeasureSize(string str)
        {
            return SpriteFont.MeasureString(str);
        }

        /// <summary>
        /// Katkaisee merkkijonon loppupäästä niin että se sopii annettuun
        /// pikselileveyteen fontilla kirjoitettuna.
        /// </summary>
        /// <param name="str">Merkkijono</param>
        /// <param name="maxLineWidth">Maksimipikselimäärä merkkijonolle</param>
        /// <returns></returns>
        public string TruncateText(string str, double maxLineWidth)
        {
            StringBuilder builder = new StringBuilder(str);

            double realWidth = SpriteFont.MeasureString(str).X;

            while (realWidth > maxLineWidth)
            {
                builder.Remove(builder.Length - 1, 1);
                realWidth = SpriteFont.MeasureString(builder).X;
            }

            return builder.ToString();
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

                var wordWidth = MeasureSize(word.ToString()).X;

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
            // TODO: Font char spacing
            /*
            double currentWidth = -fontCollection.FontSystem.CharacterSpacing;
            int i = fromRight ? text.Length - 1 : 0;
            int step = fromRight ? -1 : 1;

            //for ( int i = 0; i < text.Length; i++ )
            while ( ( fromRight && i >= 0 ) || ( !fromRight && i < text.Length ) )
            {
                currentWidth += fontCollection.FontSystem.CharacterSpacing + GetCharacterSize( text[i] ).X;
                if ( currentWidth >= maxWidth ) return i;
                i += step;
            }

            return fromRight ? -1 : text.Length;*/
            return 5; 
        }
    }
}
