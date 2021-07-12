using System;
using System.Text;

namespace Jypeli
{
    /// <summary>
    /// Sisältää avustusmetodeja merkkijonojen käsittelyyn.
    /// </summary>
    public static class StringHelpers
    {

        /// <summary>
        /// Vertaa kahta oliota, jotka ovat joko merkkijonoja tai StringBuildereita, merkki kerrallaan.
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <returns></returns>
        public static bool StringEquals(object o1, object o2)
        {
            if ( o1 == o2 ) return true;

            string s1 = null;
            string s2 = null;
            
            if ( o1 is string ) s1 = (string)o1;
            else if (o1 is StringBuilder) s1 = ((StringBuilder)o1).ToString();
            else return false;
            
            if ( o2 is string ) s2 = (string)o2;
            else if (o2 is StringBuilder) s2 = ((StringBuilder)o2).ToString();
            else return false;

            return s1.Equals(s2);
        }

        /// <summary>
        /// Poistaa merkkijonon lopusta tietyn määrän merkkejä.
        /// Jos merkkijono on lyhyempi kuin poistettava määrä, poistetaan mitä pystytään.
        /// </summary>
        /// <param name="builder">StringBuilder-olio</param>
        /// <param name="chars">Poistettavien merkkien määrä</param>
        /// <returns></returns>
        public static StringBuilder RemoveLast(this StringBuilder builder, int chars)
        {
            if ( builder.Length <= chars )
            {
                builder.Length = 0;
                return builder;
            }

            builder.Length -= chars;
            return builder;
        }

        /// <summary>
        /// Kirjoittaa osamerkkijonon toiseen StringBuilderiin.
        /// </summary>
        /// <param name="src">StringBuilder josta luetaan</param>
        /// <param name="dest">StringBuilder johon kirjoitetaan</param>
        /// <param name="start">Ensimmäinen merkki osajonosta</param>
        /// <param name="length">Osajonon pituus</param>
        public static void Substring( this StringBuilder src, StringBuilder dest, int start, int length )
        {
            for ( int i = start; i < length; i++ )
            {
                dest.Append(src[i]);
            }
        }

        /// <summary>
        /// Palauttaa osamerkkijonon toisena StringBuilderina.
        /// </summary>
        /// <param name="builder">StringBuilder-instanssi</param>
        /// <param name="start">Ensimmäinen merkki osajonosta</param>
        /// <param name="length">Osajonon pituus</param>
        /// <returns>Osamerkkijono-StringBuilder</returns>
        public static StringBuilder Substring( this StringBuilder builder, int start, int length )
        {
            StringBuilder result = new StringBuilder();
            builder.Substring( start, length );
            return result;
        }

        /// <summary>
        /// Poistaa koko StringBuilderin sisällön ja kirjoittaa sen toiseen StringBuilderiin.
        /// </summary>
        /// <param name="src">StringBuilder josta otetaan</param>
        /// <param name="dest">StringBuilder johon kirjoitetaan</param>
        public static void PutTo( this StringBuilder src, StringBuilder dest )
        {
            dest.Append( src );
            src.Clear();
        }

        /// <summary>
        /// Poistaa osamerkkijonon ja kirjoittaa sen toiseen StringBuilderiin.
        /// </summary>
        /// <param name="src">StringBuilder josta otetaan</param>
        /// <param name="dest">StringBuilder johon kirjoitetaan</param>
        /// <param name="start">Ensimmäinen merkki osajonosta</param>
        /// <param name="length">Osajonon pituus</param>
        public static void PutTo( this StringBuilder src, StringBuilder dest, int start, int length )
        {
            src.Substring( dest, start, length );
            src.Remove( start, length );
        }

        /// <summary>
        /// Poistaa osamerkkijonon ja palauttaa sen toisena StringBuilderina..
        /// </summary>
        /// <param name="builder">StringBuilder-instanssi</param>
        /// <param name="start">Ensimmäinen merkki osajonosta</param>
        /// <param name="length">Osajonon pituus</param>
        /// <returns>Osamerkkijono-StringBuilder</returns>
        public static StringBuilder PutTo( this StringBuilder builder, int start, int length )
        {
            StringBuilder removed = builder.Substring( start, length );
            builder.Remove( start, length );
            return removed;
        }

        /// <summary>
        /// StringBuilderin indeksi jossa annettu funktio on ensimmäisen kerran tosi.
        /// </summary>
        /// <param name="builder">StringBuilder</param>
        /// <param name="pred">Funktio</param>
        /// <returns></returns>
        public static int IndexForWhich( this StringBuilder builder, Predicate<char> pred )
        {
            for ( int i = 0; i < builder.Length; i++ )
            {
                if ( pred( builder[i] ) )
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Missä indeksissä annettu merkki sijaitsee
        /// </summary>
        /// <param name="builder">StringBuilder</param>
        /// <param name="c">Merkki</param>
        /// <returns></returns>
        public static int IndexOf( this StringBuilder builder, char c )
        {
            return builder.IndexForWhich( ch => ch == c );
        }

        /// <summary>
        /// Poistaa kaiken joka tulee ennen ehdon toteutumista
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pred"></param>
        public static void RemoveLeading( this StringBuilder builder, Predicate<char> pred )
        {
            int i = 0;
            for ( i = 0; i < builder.Length; i++ )
            {
                if ( !pred( builder[i] ) )
                    break;
            }
            
            if (i > 0)
                builder.Remove( 0, i );
        }

        /// <summary>
        /// Poistaa kaiken joka tulee ehdon toteutumisen jälkeen
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pred"></param>
        public static void RemoveTrailing( this StringBuilder builder, Predicate<char> pred )
        {
            int i = 0;
            for ( i = builder.Length - 1; i >= 0; i-- )
            {
                if ( !pred( builder[i] ) )
                    break;
            }

            if (i < builder.Length - 1)
                builder.Remove( i + 1, builder.Length - i - 1 );
        }

        /// <summary>
        /// Poistaa tyhjät merkit alusta ja lopusta
        /// </summary>
        /// <param name="builder"></param>
        public static void Trim( this StringBuilder builder )
        {
            builder.RemoveLeading( c => Char.IsWhiteSpace( c ) );
            builder.RemoveTrailing( c => Char.IsWhiteSpace( c ) );
        }

        /// <summary>
        /// Ottaa ensimmäisen sanan ja lisää sen toiseen StringBuilderiin
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public static void TakeFirstWord( this StringBuilder src, StringBuilder dest )
        {
            int ws = src.IndexForWhich( c => char.IsWhiteSpace( c ) );
            if ( ws < 0 ) src.PutTo( dest );
            else src.PutTo( dest, 0, ws + 1 );
        }

        /// <summary>
        /// Toistaa merkkijonon annetun määrän verran.
        /// </summary>
        /// <param name="s">Toistettava merkkijono</param>
        /// <param name="times">Kuinka monta kertaa toistetaan</param>
        /// <returns>Toistettu merkkijono</returns>
        public static string Repeat( this string s, int times )
        {
            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < times; i++)
                sb.Append( s );

            return sb.ToString();
        }

        /// <summary>
        /// Lisää merkkijonon jokaisen merkkijonotaulukon alkion alkuun
        /// </summary>
        /// <param name="s">Alkuihin lisättävä merkkijono</param>
        /// <param name="ends">Merkkijonot</param>
        /// <returns></returns>
        public static string[] Append( this string s, params object[] ends )
        {
            string[] results = new string[ends.Length];

            for ( int i = 0; i < ends.Length; i++ )
            {
                results[i] = s + ends[i];
            }

            return results;
        }
    }
}
