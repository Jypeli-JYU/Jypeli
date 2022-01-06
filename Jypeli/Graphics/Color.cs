using System;

using System.Globalization;
using System.Runtime.InteropServices;

namespace Jypeli
{
    /// <summary>
    /// Väri.
    /// </summary>
    [Save]
    public struct Color
    {
        /// <summary>
        /// Punainen värikomponentti välillä 0-255
        /// </summary>
        [Save]
        public byte RedComponent;

        /// <summary>
        /// Vihreä värikomponentti välillä 0-255
        /// </summary>
        [Save]
        public byte GreenComponent;

        /// <summary>
        /// Sininen värikomponentti välillä 0-255
        /// </summary>
        [Save]
        public byte BlueComponent;

        /// <summary>
        /// Läpinäkymättömyys välillä 0-255
        /// </summary>
        [Save]
        public byte AlphaComponent;

        /// <summary>
        /// Uusi väri
        /// </summary>
        /// <param name="red">Punainen värikomponentti välillä 0-255</param>
        /// <param name="green">Vihreä värikomponentti välillä 0-255</param>
        /// <param name="blue">Sininen värikomponentti välillä 0-255</param>
        public Color( byte red, byte green, byte blue )
            : this( red, green, blue, byte.MaxValue )
        {
        }

        /// <summary>
        /// Uusi väri
        /// </summary>
        /// <param name="red">Punainen värikomponentti välillä 0-255</param>
        /// <param name="green">Vihreä värikomponentti välillä 0-255</param>
        /// <param name="blue">Sininen värikomponentti välillä 0-255</param>
        /// <param name="alpha">Läpinäkymättömyys välillä 0-255</param>
        public Color( byte red, byte green, byte blue, byte alpha )
        {
            RedComponent = red;
            GreenComponent = green;
            BlueComponent = blue;
            AlphaComponent = alpha;
        }

        /// <summary>
        /// Uusi väri
        /// </summary>
        /// <param name="red">Punainen värikomponentti välillä 0-255</param>
        /// <param name="green">Vihreä värikomponentti välillä 0-255</param>
        /// <param name="blue">Sininen värikomponentti välillä 0-255</param>
        /// <param name="alpha">Läpinäkymättömyys välillä 0-255</param>
        public Color( int red, int green, int blue, int alpha )
            : this( (byte)red, (byte)green, (byte)blue, (byte)alpha )
        {
        }

        /// <summary>
        /// Uusi väri
        /// </summary>
        /// <param name="red">Punainen värikomponentti välillä 0-255</param>
        /// <param name="green">Vihreä värikomponentti välillä 0-255</param>
        /// <param name="blue">Sininen värikomponentti välillä 0-255</param>
        public Color( int red, int green, int blue )
            : this( (byte)red, (byte)green, (byte)blue, byte.MaxValue )
        {
        }

        /// <summary>
        /// Uusi väri aiemman pohjalta uudella läpinäkyvyydellä
        /// </summary>
        /// <param name="rgb">Väri</param>
        /// <param name="alpha">Läpinäkymättömyys välillä 0-255</param>
        public Color( Color rgb, byte alpha )
            : this( rgb.RedComponent, rgb.GreenComponent, rgb.BlueComponent, alpha )
        {
        }

        /// <summary>
        /// Uusi väri
        /// </summary>
        /// <param name="red">Punainen värikomponentti välillä 0-1.0</param>
        /// <param name="green">Vihreä värikomponentti välillä 0-1.0</param>
        /// <param name="blue">Sininen värikomponentti välillä 0-1.0</param>
        public Color( double red, double green, double blue )
            : this( red, green, blue, 1.0 )
        {
        }

        /// <summary>
        /// Uusi väri
        /// </summary>
        /// <param name="red">Punainen värikomponentti välillä 0-1.0</param>
        /// <param name="green">Vihreä värikomponentti välillä 0-1.0</param>
        /// <param name="blue">Sininen värikomponentti välillä 0-1.0</param>
        /// <param name="alpha">Läpinäkymättömyys välillä 0-1.0</param>
        public Color(double red, double green, double blue, double alpha)
            : this((int)(red * 255), (int)(green * 255), (int)(blue * 255), (int)(alpha * 255))
        {
        }

        /// <summary>
        /// Pakkaa kolme kokonaislukua väriä vastaavaksi kokonaisluvuksi
        /// </summary>
        /// <param name="r">värin punainen osuus (0-255)</param>
        /// <param name="g">värin vihreä osuus (0-255)</param>
        /// <param name="b">värin sininen osuus (0-255)</param>
        /// <param name="a">alpha-arvo (0-255)</param>
        /// <returns></returns>
        public static uint PackRGB( byte r, byte g, byte b, byte a = 255 )
        {
            return (uint)( ( a << 24 ) | ( r << 16 ) | ( g << 8 ) | b );
        }


        /// <summary>
        /// Pakkaa kolme kokonaislukua väriä vastaavaksi kokonaisluvuksi
        /// </summary>
        /// <param name="r">värin punainen osuus (0-255)</param>
        /// <param name="g">värin vihreä osuus (0-255)</param>
        /// <param name="b">värin sininen osuus (0-255)</param>
        /// <param name="a">alpha-arvo (0-255)</param>
        /// <returns></returns>
        public static uint PackRGB( int r, int g, int b, int a = 255 )
        {
            return (uint)( ( a << 24 ) | ( r << 16 ) | ( g << 8 ) | b );
        }

        /// <summary>
        /// Palauttaa heksakoodia vastaavan värin.
        /// </summary>
        /// <param name="hexString">Heksakoodi</param>
        /// <returns></returns>
        public static Color FromHexCode( string hexString )
        {
            if ( hexString.StartsWith( "#" ) )
                hexString = hexString.Substring( 1 );

            uint hex = uint.Parse( hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture );
            Color color = Color.White;

            if ( hexString.Length == 8 )
            {
                color.AlphaComponent = (byte)( hex >> 24 );
                color.RedComponent = (byte)( hex >> 16 );
                color.GreenComponent = (byte)( hex >> 8 );
                color.BlueComponent = (byte)( hex );
            }
            else if ( hexString.Length == 6 )
            {
                color.RedComponent = (byte)( hex >> 16 );
                color.GreenComponent = (byte)( hex >> 8 );
                color.BlueComponent = (byte)( hex );
            }
            else
            {
                throw new InvalidOperationException( "Invald hex representation of an ARGB or RGB color value." );
            }

            return color;
        }

        /// <summary>
        /// Antaa värin Paint.net -ohjelman paletista.
        /// </summary>
        /// <param name="row">Rivi (0-1)</param>
        /// <param name="col">Sarake (0-15)</param>
        /// <returns>Väri</returns>
        public static Color FromPaintDotNet( int row, int col )
        {
            if ( row < 0 || row > 1 || col < 0 || col > 15 )
                throw new ArgumentException( "Row must be between 0 and 1 and column between 0 and 15." );

            Color[,] colors = {
                {
                    Color.Black, Color.DarkGray, Color.Red, Color.Orange,
                    Color.Gold, Color.YellowGreen, Color.Harlequin, Color.BrightGreen,
                    Color.SpringGreen, Color.Cyan, Color.Azure, Color.PaintDotNetBlue,
                    Color.HanPurple, Color.Violet, Color.PaintDotNetMagenta, Color.Rose
                },
                {
                    Color.White, Color.Gray, Color.DarkRed, Color.Brown, 
                    Color.Olive, Color.BrownGreen, Color.ForestGreen, Color.DarkForestGreen, 
                    Color.DarkJungleGreen, Color.DarkCyan, Color.DarkAzure, Color.DarkBlue, 
                    Color.MidnightBlue,Color.DarkViolet, Color.Purple, Color.BloodRed
                }
            };

            return colors[row, col];
        }

        

        /// <summary>
        /// Ottaa kokonaisluvusta alpha-arvon
        /// </summary>
        /// <param name="c">luku josta otetaan</param>
        /// <returns>Alpha-arvo</returns>
        public static byte GetAlpha( uint c )
        {
            return (byte)( ( c >> 24 ) & 0xff );
        }


        /// <summary>
        /// Ottaa kokonaisluvusta alpha-arvon
        /// </summary>
        /// <param name="c">luku josta otetaan</param>
        /// <returns>Alpha-arvo</returns>
        public static byte GetAlpha( int c )
        {
            return (byte)( ( c >> 24 ) & 0xff );
        }


        /// <summary>
        /// Ottaa kokonaisluvusta punaisen värin
        /// </summary>
        /// <param name="c">luku josta otetaan</param>
        /// <returns>punaisen osuus</returns>
        public static byte GetRed( uint c )
        {
            return (byte)( ( c >> 16 ) & 0xff );
        }


        /// <summary>
        /// Ottaa kokonaisluvusta vihreän värin
        /// </summary>
        /// <param name="c">luku josta otetaan</param>
        /// <returns>vihreän osuus</returns>
        public static byte GetGreen( uint c )
        {
            return (byte)( ( c >> 8 ) & 0xff );
        }


        /// <summary>
        /// Ottaa kokonaisluvusta sinisen värin
        /// </summary>
        /// <param name="c">luku josta otetaan</param>
        /// <returns>sinisen osuus</returns>
        public static byte GetBlue( uint c )
        {
            return (byte)( ( c >> 0 ) & 0xff );
        }


        /// <summary>
        /// Tekee kokonaisluvusta värin
        /// </summary>
        /// <param name="c">ARGB-arvon sisältävä kokonaisluku</param>
        /// <returns></returns>
        public static Color IntToColor( int c )
        {
            return new Color( (byte)( ( c >> 16 ) & 0xff ), (byte)( ( c >> 8 ) & 0xff ),
                             (byte)( ( c ) & 0xff ), (byte)( ( c >> 24 ) & 0xff ) );
        }


        /// <summary>
        /// Tekee kokonaisluvusta värin
        /// </summary>
        /// <param name="c">ARGB-arvon sisältävä kokonaisluku</param>
        /// <returns></returns>
        public static Color UIntToColor( uint c )
        {
            return new Color( (byte)( ( c >> 16 ) & 0xff ), (byte)( ( c >> 8 ) & 0xff ),
                             (byte)( ( c ) & 0xff ), (byte)( ( c >> 24 ) & 0xff ) );
        }

        /// <summary>
        /// Laskee kahden värin (euklidisen) etäisyyden RGB-väriavaruudessa.
        /// Värikomponentit ovat välillä 0-255 joten suurin mahdollinen etäisyys
        /// (musta ja valkoinen) on noin 441,68.
        /// </summary>
        /// <returns>Etäisyys</returns>
        public static double Distance( Color a, Color b )
        {
            double rd = Math.Pow( a.RedComponent - b.RedComponent, 2 );
            double gd = Math.Pow( a.GreenComponent - b.GreenComponent, 2 );
            double bd = Math.Pow( a.BlueComponent - b.BlueComponent, 2 );
            return Math.Sqrt( rd + gd + bd );
        }

        /// <summary>
        /// Muuttaa värin ARGB-kokonaisluvuksi
        /// </summary>
        /// <returns></returns>
        public int ToInt()
        {
            return ( AlphaComponent << 24 ) + ( RedComponent << 16 ) + ( GreenComponent << 8 ) + BlueComponent;
        }

        /// <summary>
        /// Muuttaa värin RGB-kokonaisluvuksi
        /// </summary>
        /// <returns></returns>
        public int ToIntRGB()
        {
            return ( RedComponent << 16 ) + ( GreenComponent << 8 ) + BlueComponent;
        }

        /// <summary>
        /// Muuttaa värin ARGB-kokonaisluvuksi
        /// </summary>
        /// <returns></returns>
        public uint ToUInt()
        {
            return (uint)ToInt();
        }

        /// <summary>
        /// Palautetaan väri heksamerkkijonona
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString( true );
        }

        /// <summary>
        /// Palautetaan väri heksamerkkijonona.
        /// </summary>
        /// <param name="alpha">Otetaanko läpinäkyvyys mukaan (8 merkin heksakoodi)</param>
        /// <returns></returns>
        public string ToString( bool alpha )
        {
            if ( alpha )
                return ToInt().ToString( "X8" );

            return ToIntRGB().ToString( "X6" );
        }

        /// <summary>
        /// Väri System.Drawing.Color tyyppinä
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Color ToSystemDrawing()
        {
            return System.Drawing.Color.FromArgb(AlphaComponent, RedComponent, GreenComponent, BlueComponent);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool operator ==( Color c1, Color c2 )
        {
            return c1.RedComponent == c2.RedComponent
                && c1.GreenComponent == c2.GreenComponent
                && c1.BlueComponent == c2.BlueComponent
                && c1.AlphaComponent == c2.AlphaComponent;
        }


        public static bool operator ==( Color c1, uint i2 )
        {
            return c1.ToUInt() == i2;
        }

        public static bool operator !=( Color c1, uint i2 )
        {
            return c1.ToUInt() != i2;
        }

        public static bool operator ==( Color c1, int i2 )
        {
            return c1.ToUInt() == (uint)i2;
        }

        public static bool operator !=( Color c1, int i2 )
        {
            return c1.ToUInt() != (uint)i2;
        }

        public override int GetHashCode()
        {
            return ToInt();
        }

        public override bool Equals( Object c2 )
        {
            if ( c2 is Color ) return this == (Color)c2;
            if ( c2 is uint ) return this == (uint)c2;
            if ( c2 is int ) return this == (uint)c2;
            return false;
        }

        public static bool operator !=( Color c1, Color c2 )
        {
            return !( c1 == c2 );
        }


        /// <summary>
        /// Lineaarinen interpolaatio värien välillä
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static Color Lerp(Color value1, Color value2, double amount)
        {
            float x = MathHelper.Clamp((float)amount, 0, 1);
            return new Color(
                (byte)MathHelper.Lerp(value1.RedComponent, value2.RedComponent, x),
                (byte)MathHelper.Lerp(value1.GreenComponent, value2.GreenComponent, x),
                (byte)MathHelper.Lerp(value1.BlueComponent, value2.BlueComponent, x),
                (byte)MathHelper.Lerp(value1.AlphaComponent, value2.AlphaComponent, x));
        }

        internal System.Numerics.Vector4 ToNumerics()
        {
            return new System.Numerics.Vector4(RedComponent / 255f, GreenComponent / 255f, BlueComponent / 255f, AlphaComponent / 255f);
        }

        /// <summary>
        /// Antaa tummemman värin. Vähentaa jokaista kolmea osaväriä arvon <c>howMuch</c>
        /// verran.
        /// </summary>
        /// <param name="c">Alkuperäinen väri.</param>
        /// <param name="howMuch">Kuinka paljon tummempi (positiivinen luku).</param>
        /// <returns>Tummempi väri.</returns>
        public static Color Darker( Color c, int howMuch )
        {
            int r = (int)c.RedComponent - howMuch;
            int g = (int)c.GreenComponent - howMuch;
            int b = (int)c.BlueComponent - howMuch;
            if ( r < 0 ) r = 0;
            if ( g < 0 ) g = 0;
            if ( b < 0 ) b = 0;
            return new Color( (byte)r, (byte)g, (byte)b, c.AlphaComponent );
        }

        /// <summary>
        /// Antaa kirkkaamman värin. Kasvattaa jokaista kolmea osaväriä arvon <c>howMuch</c>
        /// verran.
        /// </summary>
        /// <param name="c">Alkuperäinen väri.</param>
        /// <param name="howMuch">Kuinka paljon vaaleampi.</param>
        /// <returns>Vaaleampi väri.</returns>
        public static Color Lighter( Color c, int howMuch )
        {
            int r = (int)c.RedComponent + howMuch;
            int g = (int)c.GreenComponent + howMuch;
            int b = (int)c.BlueComponent + howMuch;
            if ( r > byte.MaxValue ) r = byte.MaxValue;
            if ( g > byte.MaxValue ) g = byte.MaxValue;
            if ( b > byte.MaxValue ) b = byte.MaxValue;
            return new Color( (byte)r, (byte)g, (byte)b, c.AlphaComponent );
        }
        /// <summary>
        /// Sekoittaa kahta tai useampaa väriä.
        /// </summary>
        /// <param name="colors">Värit parametreina.</param>
        /// <returns>Sekoitettu väri</returns>
        public static Color Mix( params Color[] colors )
        {
            if (colors.Length == 0)
                throw new ArgumentException("Color.Average needs at least one argument");

            double[] sums = new double[4];

            for (int i = 0; i < colors.Length; i++)
            {
                sums[0] += colors[i].RedComponent / 255.0;
                sums[1] += colors[i].GreenComponent / 255.0;
                sums[2] += colors[i].BlueComponent / 255.0;
                sums[3] += colors[i].AlphaComponent / 255.0;
            }

            return new Color(50, 50, 50);/*
                sums[0] / colors.Length,
                sums[1] / colors.Length,
                sums[2] / colors.Length,
                sums[3] / colors.Length
            );*/
        }

        /// <summary>
        /// Tuhkanharmaa.
        /// </summary>
        public static readonly Color AshGray = new Color( 178, 190, 181, 255 );

        /// <summary>
        /// Vedensininen.
        /// </summary>
        public static readonly Color Aqua = new Color( 0, 255, 255, 255 );

        /// <summary>
        /// Akvamariini.
        /// </summary>
        public static readonly Color Aquamarine = new Color( 127, 255, 212, 255 );

        /// <summary>
        /// Asuurinsininen.
        /// </summary>
        public static readonly Color Azure = new Color( 0, 148, 255, 255 );

        /// <summary>
        /// Beessi.
        /// </summary>
        public static readonly Color Beige = new Color( 245, 245, 220, 255 );

        /// <summary>
        /// Musta.
        /// </summary>
        public static readonly Color Black = new Color( 0, 0, 0, 255 );

        /// <summary>
        /// Verenpunainen.
        /// </summary>
        public static readonly Color BloodRed = new Color( 127, 0, 55, 255 );

        /// <summary>
        /// Sininen.
        /// </summary>
        public static readonly Color Blue = new Color( 0, 0, 255, 255 );

        /// <summary>
        /// Siniharmaa.
        /// </summary>
        public static readonly Color BlueGray = new Color( 102, 153, 204, 255 );

        /// <summary>
        /// Kirkkaan vihreä.
        /// </summary>
        public static readonly Color BrightGreen = new Color( 0, 255, 33, 255 );

        /// <summary>
        /// Ruskea.
        /// </summary>
        public static readonly Color Brown = new Color( 127, 51, 0, 255 );

        /// <summary>
        /// Ruskeanvihreä.
        /// </summary>
        public static readonly Color BrownGreen = new Color( 91, 127, 0, 255 );

        /// <summary>
        /// Karmiininpunainen.
        /// </summary>
        public static readonly Color Crimson = new Color( 220, 20, 60, 255 );

        /// <summary>
        /// Syaani.
        /// </summary>
        public static readonly Color Cyan = new Color( 0, 255, 255, 255 );

        /// <summary>
        /// Hiilenmusta.
        /// </summary>
        public static readonly Color Charcoal = new Color( 54, 69, 79, 255 );

        /// <summary>
        /// Tumma asuuri.
        /// </summary>
        public static readonly Color DarkAzure = new Color( 0, 74, 127, 255 );

        /// <summary>
        /// Tumma ruskea.
        /// </summary>
        public static readonly Color DarkBrown = new Color( 92, 64, 51, 255 );

        /// <summary>
        /// Tumma sininen.
        /// </summary>
        public static readonly Color DarkBlue = new Color( 0, 19, 127, 255 );

        /// <summary>
        /// Tumma syaani.
        /// </summary>
        public static readonly Color DarkCyan = new Color( 0, 127, 127, 255 );

        /// <summary>
        /// Tumma metsänvihreä.
        /// </summary>
        public static readonly Color DarkForestGreen = new Color( 0, 127, 14 );

        /// <summary>
        /// Tumma harmaa.
        /// </summary>
        public static readonly Color DarkGray = new Color( 64, 64, 64, 255 );

        /// <summary>
        /// Tumma vihreä.
        /// </summary>
        public static readonly Color DarkGreen = new Color( 0, 100, 0, 255 );

        /// <summary>
        /// Tumma viidakonvihreä.
        /// </summary>
        public static readonly Color DarkJungleGreen = new Color( 0, 127, 70, 255 );

        /// <summary>
        /// Tumma oranssi / ruskea.
        /// </summary>
        public static readonly Color DarkOrange = Color.Brown;

        /// <summary>
        /// Tumma punainen.
        /// </summary>
        public static readonly Color DarkRed = new Color( 127, 0, 0, 255 );

        /// <summary>
        /// Tumma turkoosi.
        /// </summary>
        public static readonly Color DarkTurquoise = new Color( 0, 206, 209, 255 );

        /// <summary>
        /// Tumma violetti.
        /// </summary>
        public static readonly Color DarkViolet = new Color( 87, 0, 127, 255 );

        /// <summary>
        /// Tumma keltainen (oliivi).
        /// </summary>
        public static readonly Color DarkYellow = Color.Olive;

        /// <summary>
        /// Tumma keltavihreä (ruskeanvihreä).
        /// </summary>
        public static readonly Color DarkYellowGreen = Color.BrownGreen;

        /// <summary>
        /// Smaragdinvihreä.
        /// </summary>
        public static readonly Color Emerald = new Color( 80, 200, 120, 255 );

        /// <summary>
        /// Metsänvihreä.
        /// </summary>
        public static readonly Color ForestGreen = new Color( 38, 127, 0 );

        /// <summary>
        /// Fuksia (pinkki)
        /// </summary>
        public static readonly Color Fuchsia = new Color( 255, 0, 255, 255 );

        /// <summary>
        /// Kulta.
        /// </summary>
        public static readonly Color Gold = new Color( 255, 216, 0, 255 );

        /// <summary>
        /// Harmaa.
        /// </summary>
        public static readonly Color Gray = new Color( 128, 128, 128, 255 );

        /// <summary>
        /// Vihreä.
        /// </summary>
        public static readonly Color Green = new Color( 0, 128, 0, 255 );

        /// <summary>
        /// Keltavihreä.
        /// </summary>
        public static readonly Color GreenYellow = new Color( 173, 255, 47, 255 );

        /// <summary>
        /// Sinipurppurainen väri Han-dynastian ajoilta.
        /// </summary>
        public static readonly Color HanPurple = new Color( 72, 0, 255, 255 );

        /// <summary>
        /// Harlekiini (hieman keltaisella sävytetty kirkas vihreä).
        /// </summary>
        public static readonly Color Harlequin = new Color( 76, 255, 0, 255 );

        /// <summary>
        /// Pinkki.
        /// </summary>
        public static readonly Color HotPink = new Color( 255, 105, 180, 255 );

        /// <summary>
        /// Norsunluu.
        /// </summary>
        public static readonly Color Ivory = new Color( 255, 255, 240, 255 );

        /// <summary>
        /// Viidakonvihreä.
        /// </summary>
        public static readonly Color JungleGreen = new Color( 41, 171, 135, 255 );

        /// <summary>
        /// Laventeli.
        /// </summary>
        public static readonly Color Lavender = new Color( 220, 208, 255, 255 );

        /// <summary>
        /// Vaalea sininen.
        /// </summary>
        public static readonly Color LightBlue = new Color( 173, 216, 230, 255 );

        /// <summary>
        /// Vaalea syaani.
        /// </summary>
        public static readonly Color LightCyan = new Color( 224, 255, 255, 255 );

        /// <summary>
        /// Vaalea harmaa.
        /// </summary>
        public static readonly Color LightGray = new Color( 211, 211, 211, 255 );

        /// <summary>
        /// Vaalea vihreä.
        /// </summary>
        public static readonly Color LightGreen = new Color( 144, 238, 144, 255 );

        /// <summary>
        /// Vaalea vaaleanpunainen.
        /// </summary>
        public static readonly Color LightPink = new Color( 255, 182, 193, 255 );

        /// <summary>
        /// Vaalea keltainen.
        /// </summary>
        public static readonly Color LightYellow = new Color( 255, 255, 224, 255 );

        /// <summary>
        /// Limetti.
        /// </summary>
        public static readonly Color Lime = new Color( 0, 255, 0, 255 );

        /// <summary>
        /// Limetinvihreä.
        /// </summary>
        public static readonly Color LimeGreen = new Color( 50, 205, 50, 255 );

        /// <summary>
        /// Magenta (pinkki)
        /// </summary>
        public static readonly Color Magenta = new Color( 255, 0, 255, 255 );

        /// <summary>
        /// Viininpunainen.
        /// </summary>
        public static readonly Color Maroon = new Color( 128, 0, 0, 255 );

        /// <summary>
        /// Tummahko sininen.
        /// </summary>
        public static readonly Color MediumBlue = new Color( 0, 0, 205, 255 );

        /// <summary>
        /// Tummahko purppura.
        /// </summary>
        public static readonly Color MediumPurple = new Color( 147, 112, 219, 255 );

        /// <summary>
        /// Tummahko turkoosi.
        /// </summary>
        public static readonly Color MediumTurquoise = new Color( 72, 209, 204, 255 );

        /// <summary>
        /// Tummahko punavioletti.
        /// </summary>
        public static readonly Color MediumVioletRed = new Color( 199, 21, 133, 255 );

        /// <summary>
        /// Keskiyön sininen.
        /// </summary>
        public static readonly Color MidnightBlue = new Color( 33, 0, 127, 255 );

        /// <summary>
        /// Mintunvihreä.
        /// </summary>
        public static readonly Color Mint = new Color( 62, 180, 137, 255 );

        /// <summary>
        /// Laivastonsininen.
        /// </summary>
        public static readonly Color Navy = new Color( 0, 0, 128, 255 );

        /// <summary>
        /// Oliivi (tumma keltainen).
        /// </summary>
        public static readonly Color Olive = new Color( 127, 106, 0, 255 );

        /// <summary>
        /// Oranssi.
        /// </summary>
        public static readonly Color Orange = new Color( 255, 106, 0, 255 );

        /// <summary>
        /// Punaoranssi.
        /// </summary>
        public static readonly Color OrangeRed = new Color( 255, 69, 0, 255 );

        /// <summary>
        /// Paint.NETin sininen väri.
        /// </summary>
        public static readonly Color PaintDotNetBlue = new Color( 0, 38, 255, 255 );

        /// <summary>
        /// Paint.NETin magenta (pinkki) väri.
        /// </summary>
        public static readonly Color PaintDotNetMagenta = new Color( 255, 0, 220, 255 );

        /// <summary>
        /// Vaaleanpunainen.
        /// </summary>
        public static readonly Color Pink = new Color( 255, 192, 203, 255 );

        /// <summary>
        /// Purppura.
        /// </summary>
        public static readonly Color Purple = new Color( 127, 0, 110, 255 );

        /// <summary>
        /// Tumma magenta (purppura).
        /// </summary>
        public static readonly Color DarkMagenta = Color.Purple;

        /// <summary>
        /// Punainen.
        /// </summary>
        public static readonly Color Red = new Color( 255, 0, 0, 255 );

        /// <summary>
        /// Rose (punainen).
        /// </summary>
        public static readonly Color Rose = new Color( 255, 0, 110, 255 );

        /// <summary>
        /// Rose-pinkki.
        /// </summary>
        public static readonly Color RosePink = new Color( 251, 204, 231, 255 );

        /// <summary>
        /// Rubiininpunainen.
        /// </summary>
        public static readonly Color Ruby = new Color( 224, 17, 95, 255 );

        /// <summary>
        /// Lohenpunainen.
        /// </summary>
        public static readonly Color Salmon = new Color( 250, 128, 114, 255 );

        /// <summary>
        /// Merensininen.
        /// </summary>
        public static readonly Color SeaGreen = new Color( 46, 139, 87, 255 );

        /// <summary>
        /// Hopea.
        /// </summary>
        public static readonly Color Silver = new Color( 192, 192, 192, 255 );

        /// <summary>
        /// Taivaansininen.
        /// </summary>
        public static readonly Color SkyBlue = new Color( 135, 206, 235, 255 );

        /// <summary>
        /// Saviliuskeensininen.
        /// </summary>
        public static readonly Color SlateBlue = new Color( 106, 90, 205, 255 );

        /// <summary>
        /// Saviliuskeenharmaa.
        /// </summary>
        public static readonly Color SlateGray = new Color( 112, 128, 144, 255 );

        /// <summary>
        /// Lumenvalkoinen.
        /// </summary>
        public static readonly Color Snow = new Color( 255, 250, 250, 255 );

        /// <summary>
        /// Kevään vihreä.
        /// </summary>
        public static readonly Color SpringGreen = new Color( 0, 255, 144, 255 );

        /// <summary>
        /// Sinivihreä.
        /// </summary>
        public static readonly Color Teal = new Color( 0, 128, 128, 255 );

        /// <summary>
        /// Läpinäkyvä väri.
        /// </summary>
        public static readonly Color Transparent = new Color( 0,0,0,0 );

        /// <summary>
        /// Turkoosi.
        /// </summary>
        public static readonly Color Turquoise = new Color( 64, 224, 208, 255 );

        /// <summary>
        /// Ultramariini (tumma sininen).
        /// </summary>
        public static readonly Color Ultramarine = new Color( 18, 10, 143, 255 );

        /// <summary>
        /// Violetti.
        /// </summary>
        public static readonly Color Violet = new Color( 178, 0, 255, 255 );

        /// <summary>
        /// Luonnonvalkoinen.
        /// </summary>
        public static readonly Color Wheat = new Color( 245, 222, 179, 255 );

        /// <summary>
        /// Valkoinen.
        /// </summary>
        public static readonly Color White = new Color( 255, 255, 255, 255 );

        /// <summary>
        /// Keltainen.
        /// </summary>
        public static readonly Color Yellow = new Color( 255, 255, 0, 255 );

        /// <summary>
        /// Keltavihreä.
        /// </summary>
        public static readonly Color YellowGreen = new Color( 182, 255, 0, 255 );
    }
}
