﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    /// <summary>
    /// Apufunktioita listojen ja muiden tietorakenteiden käyttöön.
    /// </summary>
    public static class ListHelpers
    {
        /// <summary>
        /// Laskee minimin.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Min( this IEnumerable<double> values )
        {
            double min = double.PositiveInfinity;

            foreach ( var value in values )
            {
                if ( value < min ) min = value;
            }

            return min;
        }

        /// <summary>
        /// Laskee maksimin.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Max( this IEnumerable<double> values )
        {
            double max = double.NegativeInfinity;

            foreach ( var value in values )
            {
                if ( value > max ) max = value;
            }

            return max;
        }

        /// <summary>
        /// Laskee keskiarvon.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Average( this IEnumerable<double> values )
        {
            double sum = 0;
            int count = 0;

            foreach ( var value in values )
            {
                sum += value;
                count++;
            }

            return sum / count;
        }

#if JYPELI

        /// <summary>
        /// Laskee keskiarvon komponenteittain.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Vector Average( this IEnumerable<Vector> values )
        {
            double xsum = 0, ysum = 0;
            int count = 0;

            foreach ( var value in values )
            {
                xsum += value.X;
                ysum += value.Y;
                count++;
            }

            return new Vector( xsum / count, ysum / count );
        }

#endif

#if NETFX_CORE
        /// <summary>
        /// Tyyppi metodille joka muuttaa annetun arvon tyypistä toiseen.
        /// Löytyy kirjastosta mscorlib.dll.
        /// </summary>
        /// <typeparam name="TInput">Tyyppi josta muunnetaan.</typeparam>
        /// <typeparam name="TOutput">Tyyppi johon muunnetaan.</typeparam>
        /// <param name="input">Arvo joka muunnetaan.</param>
        /// <returns>Muunnettu arvo.</returns>
        public delegate TOutput Converter<in TInput, out TOutput>(TInput input);

        /// <summary>
        /// Suorittaa metodin kaikille kokoelman olioille.
        /// </summary>
        /// <typeparam name="T">Olioiden tyyppi</typeparam>
        /// <param name="items">Oliokokoelma</param>
        /// <param name="action">Metodi</param>
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (T item in items)
            {
                action( item );
            }
        }
#endif

        /// <summary>
        /// Muuntaa kokoelman tietyn tyyppisiä olioita kokoelmaksi toisen tyyppisiä olioita.
        /// </summary>
        /// <typeparam name="TInput">Lähtötyyppi</typeparam>
        /// <typeparam name="TOutput">Kohdetyyppi</typeparam>
        /// <param name="items">Muunnettava kokoelma</param>
        /// <param name="converter">Muunnosmetodi yhdelle oliolle</param>
        /// <returns>Muunnettu kokoelma</returns>
        public static IEnumerable<TOutput> ConvertAll<TInput, TOutput>( this IEnumerable<TInput> items, Converter<TInput, TOutput> converter )
        {
            // Huom/TK: ConvertAll<TOutput>-metodi on jo olemassa, mutta sitä ei ole toteutettu X360/WP7-alustoille.

            List<TOutput> outList = new List<TOutput>();

            foreach ( TInput item in items )
            {
                outList.Add( converter( item ) );
            }

            return outList;
        }

        public static List<T> FindAll<T>( this IEnumerable<T> items, Predicate<T> pred )
        {
            // Huom/TK: FindAll-metodi on jo olemassa, mutta sitä ei ole toteutettu X360/WP7-alustoille.

            List<T> outList = new List<T>();

            foreach ( var item in items )
            {
                if ( pred( item ) )
                    outList.Add( item );
            }

            return outList;
        }

        public static void RemoveAll<T>( this List<T> items, Predicate<T> pred )
        {
            // Huom/TK: RemoveAll-metodi on jo olemassa, mutta sitä ei ole toteutettu X360/WP7-alustoille.

            foreach ( var item in items.FindAll( pred ) )
            {
                items.Remove( item );
            }
        }

        public static T Find<T>( this List<T> items, Predicate<T> pred )
        {
            // Huom/TK: FindAll-metodi on jo olemassa, mutta sitä ei ole toteutettu X360/WP7-alustoille.

#if WINDOWS
            return items.Find( pred );
#else
            foreach ( var item in items )
            {
                if ( pred( item ) )
                    return item;
            }

            return default( T );
#endif
        }

        public static T ArrayFind<T>( T[] array, Predicate<T> pred )
        {
            // Huom/TK: FindAll-metodi on jo olemassa, mutta sitä ei ole toteutettu X360/WP7-alustoille.

#if WINDOWS
            return Array.Find( array, pred );
#else
            for (int i = 0; i < array.Length; i++)
            {
                if ( pred( array[i] ) )
                    return array[i];
            }

            return default( T );
#endif
        }

        public static IEnumerable<K> FindAll<K,V>( this Dictionary<K,V>.KeyCollection keys, Predicate<K> pred )
        {
            for ( int i = 0; i < keys.Count; i++ )
            {
                K key = keys.ElementAt( i );
                if ( pred( key ) ) yield return key;
            }
        }

        public static void ForEach<T>( this T[] array, Action<T> action )
        {
            for ( int i = 0; i < array.Length; i++ )
            {
                action( array[i] );
            }
        }
    }
}
