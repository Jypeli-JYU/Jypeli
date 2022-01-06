#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
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
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen, Rami Pasanen.
 */

using System;
using System.Collections.Generic;

namespace Jypeli
{
    /// <summary>
    /// Satunnaisgeneraattori. Luo satunnaisia arvoja, mm. lukuja, vektoreita sekä kulmia.
    /// </summary>
    public static class RandomGen
    {
        private static Random rand = new Random();

        /// <summary>
        /// Palauttaa satunnaisen totuusarvon.
        /// </summary>
        public static bool NextBool()
        {
            return ( rand.NextDouble() >= 0.5 );
        }

        /// <summary>
        /// Palauttaa satunnaisen kokonaisluvun, joka on vähintään 0 ja pienempi kuin <code>max</code>.
        /// </summary>
        /// <returns></returns>
        public static int NextInt( int maxValue )
        {
            return rand.Next( maxValue );
        }

        /// <summary>
        /// Palauttaa satunnaisen kokonaisluvun, joka on vähintään <code>min</code> ja pienempi kuin <code>max</code>.
        /// </summary>
        public static int NextInt( int min, int max )
        {
            return rand.Next( min, max );
        }        

        /// <summary>
        /// Palauttaa satunnaisen liukuluvun parametrien <code>min</code> ja <code>max</code> väliltä.
        /// </summary>
        public static double NextDouble( double min, double max )
        {
            return min + rand.NextDouble() * ( max - min );
        }

        /// <summary>
        /// Arpoo satunnaisen kirjaimen väliltä a-z.
        /// </summary>
        /// <param name="upperCase">Käytetäänkö ISOJA KIRJAIMIA</param>
        /// <returns>Satunnainen kirjain</returns>
        public static char NextLetter(bool upperCase = false)
        {
            int startCode = upperCase ? (int)'A' : (int)'a';
            int endCode = upperCase ? (int)'Z' : (int)'z';
            return (char)NextInt( startCode, endCode + 1 );
        }

        /// <summary>
        /// Palauttaa satunnaisen suunnan.
        /// </summary>
        public static Direction NextDirection()
        {
            double randdir = rand.NextDouble();

            if ( randdir <= 0.25 )
                return Direction.Up;

            if ( randdir <= 0.5 )
                return Direction.Down;

            if ( randdir <= 0.75 )
                return Direction.Left;

            return Direction.Right;
        }

        /// <summary>
        /// Palauttaa double-taulukon.
        /// </summary>
        /// <param name="min">Pienin arvo.</param>
        /// <param name="max">Suurin arvo.</param>
        /// <param name="size">Taulukon koko.</param>
        /// <returns>Taulukko.</returns>
        public static double[] NextDoubleArray( double min, double max, int size )
        {
            double[] array = new double[size];

            for ( int i = 0; i < size; i++ )
            {
                array[i] = NextDouble( min, max );
            }

            return array;
        }

        /// <summary>
        /// Palauttaa double-taulukon.
        /// </summary>
        /// <param name="min">Pienin arvo.</param>
        /// <param name="max">Suurin arvo.</param>
        /// <param name="size">Taulukon koko.</param>
        /// <param name="maxchange">Suurin sallittu muutos kahden luvun välillä.</param>
        /// <returns>Taulukko.</returns>
        public static double[] NextDoubleArray( double min, double max, int size, int maxchange )
        {
            double[] array = new double[size];
            double curmin = min;
            double curmax = max;

            for ( int i = 0; i < size; i++ )
            {
                array[i] = NextDouble( curmin, curmax );

                curmin = MathHelper.Max((float)min, (float)(array[i] - maxchange));
                curmax = MathHelper.Min((float)max, (float)(array[i] + maxchange));
            }

            return array;
        }

        /// <summary>
        /// Palauttaa satunnaisen värin.
        /// </summary>
        /// <returns>Väri.</returns>
        public static Color NextColor()
        {
            return new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1);
        }

        /// <summary>
        /// Palauttaa satunnaisen vaalean värin.
        /// </summary>
        /// <returns>Satunnainen vaalea väri</returns>
        public static Color NextLightColor()
        {
            return Color.Mix( NextColor(), Color.White );
        }

        /// <summary>
        /// Palauttaa satunnaisen tumman värin.
        /// </summary>
        /// <returns>Satunnainen tumma väri</returns>
        public static Color NextDarkColor()
        {
            return Color.Mix( NextColor(), Color.Black );
        }

        /// <summary>
        /// Palauttaa satunnaisen värin olioilmentymän perusteella.
        /// Sama olio palauttaa aina saman värin.
        /// </summary>
        /// <param name="obj">Olio</param>
        /// <returns>Väri.</returns>
        /*public static Color NextColor( object obj )
        {
            var r = new Random( obj.GetHashCode() );
            return new Color( (float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), 1 );
        }*/

        /// <summary>
        /// Palauttaa satunnaisen värin.
        /// </summary>
        /// <returns>Väri.</returns>
        public static Color NextColor(Color first, Color second)
        {
            return Color.Lerp(first, second, (float)rand.NextDouble());
        }

        /// <summary>
        /// Palauttaa satunnaisen kulman.
        /// </summary>
        /// <returns>Kulma.</returns>
        public static Angle NextAngle()
        {
            return Angle.FromRadians( NextDouble( 0, 2 * Math.PI ) );
        }

        /// <summary>
        /// Palauttaa satunnaisen vektorin.
        /// </summary>
        /// <param name="minLength">Vektorin minimipituus.</param>
        /// <param name="maxLength">Vektorin maksimipituus.</param>
        /// <returns>Satunnainen vektori</returns>
        public static Vector NextVector( double minLength, double maxLength )
        {
            return Vector.FromLengthAndAngle( NextDouble( minLength, maxLength ), NextAngle() );
        }

        /// <summary>
        /// Palauttaa satunnaisen vektorin.
        /// </summary>
        /// <param name="minX">Pienin sallittu X-koordinaatti</param>
        /// <param name="minY">Pienin sallittu Y-koordinaatti</param>
        /// <param name="maxX">Suurin sallittu X-koordinaatti</param>
        /// <param name="maxY">Suurin sallittu Y-koordinaatti</param>
        /// <returns></returns>
        public static Vector NextVector( double minX, double minY, double maxX, double maxY )
        {
            return new Vector( NextDouble( minX, maxX ), NextDouble( minY, maxY ) );
        }

        /// <summary>
        /// Luodaan satunnainen vektori, jonka "piste" on suorakaiteen sisällä.
        /// </summary>
        /// <param name="rect">suorakaisen joka rajoittaa arvontaa</param>
        /// <param name="r">säde joka otetaan reunoilta sisäänpäin</param>
        /// <returns>Satunnainen vektori</returns>
        public static Vector NextVector( BoundingRectangle rect, int r = 0 )
        {
            return new Vector( NextDouble( rect.Left + r, rect.Right - r ), NextDouble( rect.Bottom + r, rect.Top - r ) );
        }

        /// <summary>
        /// Palauttaa satunnaisen kulman nollasta annettuun maksimiin.
        /// </summary>
        /// <param name="max">Maksimikulma.</param>
        /// <returns>Kulma.</returns>
        public static Angle NextAngle( Angle max )
        {
            return Angle.FromRadians( NextDouble( 0, max.Radians ) );
        }

        /// <summary>
        /// Palauttaa satunnaisen kulman tietyltä väliltä.
        /// </summary>
        /// <param name="min">Minimikulma.</param>
        /// <param name="max">Maksimikulma.</param>
        /// <returns>Kulma.</returns>
        public static Angle NextAngle( Angle min, Angle max )
        {
            double a1 = min.Radians;
            double a2 = max.Radians;
            while ( a2 < a1 ) a2 += 2 * Math.PI;
            return Angle.FromRadians( NextDouble( a1, a2 ) );
        }

        /// <summary>
        /// Palauttaa satunnaisen aikavälin.
        /// </summary>
        /// <param name="minSeconds">Minimikesto sekunteina</param>
        /// <param name="maxSeconds">Maksimikesto sekunteina</param>
        /// <returns></returns>
        public static TimeSpan NextTimeSpan( double minSeconds, double maxSeconds )
        {
            return TimeSpan.FromSeconds( NextDouble( minSeconds, maxSeconds ) );
        }

        /// <summary>
        /// Palauttaa satunnaisen muodon.
        /// </summary>
        public static Shape NextShape()
        {
            return SelectOne(new Shape[] { Shape.Diamond, Shape.Ellipse,
                Shape.Heart, Shape.Hexagon, Shape.Octagon, Shape.Pentagon,
                Shape.Rectangle, Shape.Star, Shape.Triangle });
        }

        /// <summary>
        /// Palauttaa satunnaisen kokonaisluvun annettujen todennäköisyyksien
        /// perusteella.
        /// </summary>
        /// <param name="p">Todennäköisyydet. 0 = ei koskaan, 1 = varmasti, 0.5 = 50% jne.</param>
        /// <example>
        /// int luku = RandomGen.NextIntWithProbabilities( 0.4 );  // palauttaa 40% tod.näk. nollan, muuten ykkösen (60%)
        /// int luku2 = RandomGen.NextIntWithProbabilities( 0.6, 0.2 );  // palauttaa 60% tod.näk. nollan, 20% tn. ykkösen ja muuten kakkosen (40%)
        /// int luku3 = RandomGen.NextIntWithProbabilities( 0.6, 0.4 );  // palauttaa 60% tod.näk. nollan ja 40% tn. ykkösen
        /// </example>
        /// <returns>Kokonaisluku väliltä 0 - (p+1)</returns>
        public static int NextIntWithProbabilities( params double[] p )
        {
            double randomNum = rand.NextDouble();
            double accumulator = 0;

            for ( int i = 0; i < p.Length; i++ )
            {
                accumulator += p[i];
                if ( randomNum < accumulator ) return i;
            }

            return p.Length;
        }


        /// <summary>
        /// Sotkee rakenteen satunnaiseen järjestykseen
        /// </summary>
        /// <typeparam name="T">Minkä tyyppisiä alkioita sotketaan</typeparam>
        /// <param name="list">tietorakenne jossa sotkettavat alkiot</param>
        public static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count; i > 1; i--)
            {
                // Pick random element to swap.
                int j = rand.Next(i); // 0 <= j <= i-1
                // Swap.
                T tmp = list[j];
                list[j] = list[i - 1];
                list[i - 1] = tmp;
            }
        }

        /// <summary>
        /// Palauttaa yhden annetuista vaihtoehdoista.
        /// Esim. RandomGen.SelectOne&lt;string&gt;("yksi", "kaksi", "kolme");
        /// </summary>
        /// <typeparam name="T">Vaihtoehtojen tyyppi</typeparam>
        /// <param name="choices">Vaihtoehdot</param>
        /// <returns>Satunnainen vaihtoehto</returns>
        public static T SelectOne<T>( params T[] choices )
        {
            int i = NextInt( choices.Length );
            return choices[i];
        }

        /// <summary>
        /// Palauttaa yhden annetuista vaihtoehdoista.
        /// Esim. RandomGen.SelectOne&lt;string&gt;("yksi", "kaksi", "kolme");
        /// </summary>
        /// <typeparam name="T">Vaihtoehtojen tyyppi</typeparam>
        /// <param name="choices">Vaihtoehdot</param>
        /// <returns>Satunnainen vaihtoehto</returns>
        public static T SelectOne<T>( IList<T> choices )
        {
            int i = NextInt( choices.Count );
            return choices[i];
        }
    }
}
