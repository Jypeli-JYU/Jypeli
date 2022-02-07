#region License
/*
Original file:

MIT License - Copyright (C) The Mono.Xna Team
The MIT License (MIT)
Portions Copyright © The Mono.Xna Team

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion
using System;
using Silk.NET.Maths;

namespace Jypeli
{
    /// <summary>
    /// Sisältää useita yleisesti matematiikkaan käytettyjä vakioita ja funktioita.
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// Vakio e (2.71828175).
        /// </summary>
        public const double E = Math.E;

        /// <summary>
        /// 10-kantainen logaritmi e:stä (0,434294481903252).
        /// </summary>
        public const double Log10E = 0.434294481903252;

        /// <summary>
        /// 2-kantainen logaritmi e:stä (1,4426950408889634).
        /// </summary>
        public const double Log2E = 1.4426950408889634;

        /// <summary>
        /// Pii (3,141592653589793).
        /// </summary>
        public const double Pi = Math.PI;

        /// <summary>
        /// Pii jaettuna kahdella (1.57079637).
        /// </summary>
        public const double PiOver2 = (Math.PI / 2.0);

        /// <summary>
        /// Pii jaettuna neljällä(0.7853982).
        /// </summary>
        public const double PiOver4 = (Math.PI / 4.0);

        /// <summary>
        /// Pii kertaa kaksi (6.28318548).
        /// </summary>
        public const double TwoPi = (Math.PI * 2.0);

        /// <summary>
        /// Pii kertaa kaksi(6.28318548).
        /// Sama kuin <see cref="TwoPi"/>.
        /// </summary>
        public const double Tau = TwoPi;

        /// <summary>
        /// Karteesinen koordinaatti yhdellä akselilla pisteelle, joka on annetun kolmion ja kahden normalisoidun barysentrisen koordinaatin määräämä.
        /// <see href="https://en.wikipedia.org/wiki/Barycentric_coordinate_system"/>
        /// </summary>
        /// <param name="value1">Kolmion ensimmäisen kulman koordinaatti halutulla akselilla.</param>
        /// <param name="value2">Kolmion toisen kulman koordinaatti halutulla akselilla.</param>
        /// <param name="value3">Kolmion kolmannen kulman koordinaatti halutulla akselilla.</param>
        /// <param name="amount1">normalisoitu barysentrinen koordinaatti b2, kulmalle 2.</param>
        /// <param name="amount2">normalisoitu barysentrinen koordinaatti b3, kulmalle 3.</param>
        /// <returns>Karteesinen koordinaatti halutulla akselilla.</returns>
        public static double Barycentric(double value1, double value2, double value3, double amount1, double amount2)
        {
            return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
        }

        /// <summary>
        /// Catmull-Rom interpolaatio annetuilla sijainneilla.
        /// <see href="https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline"/>
        /// </summary>
        /// <param name="value1">Interpolaation ensimmäinen sijainti.</param>
        /// <param name="value2">Interpolaation toisen sijainti.</param>
        /// <param name="value3">Interpolaation kolmas sijainti.</param>
        /// <param name="value4">Interpolaation neljäs sijainti.</param>
        /// <param name="amount">Painokerroin.</param>
        /// <returns>Catmull-Rom interpolaation antama sijainti.</returns>
        public static double CatmullRom(double value1, double value2, double value3, double value4, double amount)
        {
            // Using formula from http://www.mvps.org/directx/articles/catmull/
            double amountSquared = amount * amount;
            double amountCubed = amountSquared * amount;
            return (0.5 * (2.0 * value2 +
                (value3 - value1) * amount +
                (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared +
                (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
        }

        /// <summary>
        /// Rajoittaa arvon annetulle välille.
        /// </summary>
        /// <param name="value">Arvo jota rajoitetaan.</param>
        /// <param name="min">Minimiarvo. Jos <c>value</c> on pienenmpi kuin <c>min</c>, <c>min</c> palautetaan.</param>
        /// <param name="max">Maksimiarvo. Jos <c>value</c> on suurempi kuin <c>max</c>, <c>max</c> palautetaan.</param>
        /// <returns>Rajoitettu arvo.</returns>
        public static double Clamp(double value, double min, double max)
        {
            // First we check to see if we're greater than the max
            value = (value > max) ? max : value;

            // Then we check to see if we're less than the min.
            value = (value < min) ? min : value;

            // There's no check to see if min > max.
            return value;
        }

        /// <summary>
        /// Rajoittaa arvon annetulle välille.
        /// </summary>
        /// <param name="value">Arvo jota rajoitetaan.</param>
        /// <param name="min">Minimiarvo. Jos <c>value</c> on pienenmpi kuin <c>min</c>, <c>min</c> palautetaan.</param>
        /// <param name="max">Maksimiarvo. Jos <c>value</c> on suurempi kuin <c>max</c>, <c>max</c> palautetaan.</param>
        /// <returns>Rajoitettu arvo.</returns>
        public static int Clamp(int value, int min, int max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }

        /// <summary>
        /// Normalisoi kulman olemaan välillä -Pi..Pi
        /// </summary>
        /// <param name="angle">Kulma</param>
        /// <returns>Rajoitettu kulma</returns>
        public static double ClampAngle(double angle)
        {
            if (-Pi <= angle && angle < Pi) return angle;

            double rem = (angle + Pi) % (TwoPi);
            return rem + ((rem < 0) ? (Pi) : (-Pi));
        }

        /// <summary>
        /// Arvojen välinen etäisyys.
        /// </summary>
        /// <param name="value1">Ensimmäinen arvo.</param>
        /// <param name="value2">Toinen arvo.</param>
        /// <returns>Arvojen välinen etäisyys.</returns>
        public static double Distance(double value1, double value2)
        {
            return Math.Abs(value1 - value2);
        }

        /// <summary>
        /// Hermiten interpolaatio.
        /// <see href="https://en.wikipedia.org/wiki/Hermite_interpolation"/>
        /// </summary>
        /// <param name="value1">Ensimmäinen piste.</param>
        /// <param name="tangent1">Ensimmäisen pisteen tangentti.</param>
        /// <param name="value2">Toinen piste piste.</param>
        /// <param name="tangent2">Toisen pisteen tangentti.</param>
        /// <param name="amount">Painokerroin.</param>
        /// <returns>Hermiten interpolaation tulos.</returns>
        public static double Hermite(double value1, double tangent1, double value2, double tangent2, double amount)
        {
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (amount == 0f)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2 * v1 - 2 * v2 + t2 + t1) * sCubed +
                    (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared +
                    t1 * s +
                    v1;
            return result;
        }


        /// <summary>
        /// Lineaarinen interpolaatio arvojen välillä.
        /// </summary>
        /// <param name="value1">Ensimmäinen arvo.</param>
        /// <param name="value2">Toinen arvo.</param>
        /// <param name="amount">Painokerroin välillä 0..1.</param>
        /// <returns>Interpoloitu arvo.</returns> 
        /// <remarks>Toimii seuraavanlaisesti:
        /// <code>value1 + (value2 - value1) * amount</code>.
        /// Eli jos <c>amount = 0</c>palautetaan <c>value1</c>, jos <c>amount = 1</c>palautetaan <c>value2</c>.
        /// Katso myös <see cref="MathHelper.LerpPrecise"/> Joka on hieman tarkempi erikoisten rajatapauksien kohdalla.
        /// </remarks>
        public static double Lerp(double value1, double value2, double amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        /// <summary>
        /// Lineaarinen interpolaatio vektoreille
        /// </summary>
        /// <param name="value1">Ensimmäinen piste</param>
        /// <param name="value2">toinen piste</param>
        /// <param name="amount">Painokerroin välillä 0..1.</param>
        /// <returns>Interpoloitu vektori</returns>
        public static Vector Lerp(Vector value1, Vector value2, double amount)
        {
            Vector result;
            result.X = (value2.X - value1.X) * amount + value1.X;
            result.Y = (value2.Y - value1.Y) * amount + value1.Y;
            return result;
        }

        /// <summary>
        /// Lineaarinen interpolaatio vektoreille, molempien akselien interpolaatiomäärä voidaan asettaa erikseen.
        /// </summary>
        /// <param name="value1">Ensimmäinen piste</param>
        /// <param name="value2">toinen piste</param>
        /// <param name="amount">Painokerroin välillä 0..1.</param>
        /// <returns>Interpoloitu vektori</returns>
        public static Vector Lerp(Vector value1, Vector value2, Vector amount)
        {
            Vector result;
            result.X = (value2.X - value1.X) * amount.X + value1.X;
            result.Y = (value2.Y - value1.Y) * amount.Y + value1.Y;
            return result;
        }


        /// <summary>
        /// Lineaarinen interpolaatio arvojen välillä..
        /// Vähemmän tehokas, mutta tarkempi erikoistilanteiden kohdalla kuin <see cref="MathHelper.Lerp(double, double, double)"/>.
        /// </summary>
        /// <param name="value1">Ensimmäinen arvo.</param>
        /// <param name="value2">Toinen arvo.</param>
        /// <param name="amount">Painokerroin välillä 0..1.</param>
        /// <returns>Interpolated value.</returns>
        /// <remarks>Toimii seuraavanlaisesti:
        /// <code>((1 - amount) * value1) + (value2 * amount)</code>.
        /// Eli jos <c>amount = 0</c>palautetaan <c>value1</c>, jos <c>amount = 1</c>palautetaan <c>value2</c>.
        /// Tarkempi liukulukuepätarkkuuden suhteen kuin <see cref="MathHelper.Lerp(double, double, double)"/>.
        /// Jos <c>value1</c> ja <c>value2</c> välillä on hyvin suuri ero (kokoluokkaa value1=10000000000000000, value2=1),
        /// alueen rajan reunalla (esim. amount=1), <see cref="MathHelper.Lerp(double, double, double)"/> palauttaa return 0 (kun sen pitäisi palauttaa return 1).
        /// Tarkempi selitys, katso:
        /// Wikipedia Article: https://en.wikipedia.org/wiki/Linear_interpolation#Programming_language_support
        /// StackOverflow Answer: http://stackoverflow.com/questions/4353525/floating-point-linear-interpolation#answer-23716956
        /// </remarks>
        public static double LerpPrecise(double value1, double value2, double amount)
        {
            return ((1 - amount) * value1) + (value2 * amount);
        }

        /// <summary>
        /// Palauttaa suuremman arvoista.
        /// </summary>
        /// <param name="value1">Ensimmäinen arvo.</param>
        /// <param name="value2">Ensimmäinen arvo.</param>
        /// <returns>Suurempi arvo.</returns>
        public static double Max(double value1, double value2)
        {
            return value1 > value2 ? value1 : value2;
        }

        /// <summary>
        /// Palauttaa suuremman arvoista.
        /// </summary>
        /// <param name="value1">Ensimmäinen arvo.</param>
        /// <param name="value2">Ensimmäinen arvo.</param>
        /// <returns>Suurempi arvo.</returns>
        public static int Max(int value1, int value2)
        {
            return value1 > value2 ? value1 : value2;
        }

        /// <summary>
        /// Palauttaa pienemmän arvoista.
        /// </summary>
        /// <param name="value1">Ensimmäinen arvo.</param>
        /// <param name="value2">Ensimmäinen arvo.</param>
        /// <returns>Pienempi arvo.</returns>
        public static double Min(double value1, double value2)
        {
            return value1 < value2 ? value1 : value2;
        }

        /// <summary>
        /// Palauttaa pienemmän arvoista.
        /// </summary>
        /// <param name="value1">Ensimmäinen arvo.</param>
        /// <param name="value2">Ensimmäinen arvo.</param>
        /// <returns>Pienempi arvo.</returns>
        public static int Min(int value1, int value2)
        {
            return value1 < value2 ? value1 : value2;
        }

        /// <summary>
        /// Interpolaatio arvojen välillä käyttäen toisen asteen funktiota.
        /// </summary>
        /// <param name="value1">Ensimmäinen arvo.</param>
        /// <param name="value2">Toinen arvo.</param>
        /// <param name="amount">Painokerroin välillä 0..1.</param>
        /// <returns>Interpoloitu arvo.</returns>
        public static double SmoothStep(double value1, double value2, double amount)
        {
            double result = MathHelper.Clamp(amount, 0, 1);
            result = MathHelper.Hermite(value1, 0, value2, 0, result);

            return result;
        }

        /// <summary>
        /// Muuttaa radiaanit asteiksi.
        /// Sama kuin <see cref="Angle.RadianToDegree(double)"/>
        /// </summary>
        /// <param name="radians">Kulma radiaaneina.</param>
        /// <returns>Kulma asteina.</returns>
        public static double ToDegrees(double radians)
        {
            return Angle.RadianToDegree(radians);
        }

        /// <summary>
        /// Muuttaa asteet radiaaneiksi.
        /// Sama kuin <see cref="Angle.DegreeToRadian(double)"/>
        /// </summary>
        /// <param name="degrees">Kulma asteina.</param>
        /// <returns>Kulma radiaaneina.</returns>
        public static double ToRadians(double degrees)
        {
            return Angle.DegreeToRadian(degrees);
        }

        /// <summary>
        /// Asettaa kulman välille π and -π.
        /// </summary>
        /// <param name="angle">Kulma radiaaneina.</param>
        /// <returns>Rajattu kulma radiaaneina.</returns>
        public static double WrapAngle(double angle)
        {
            if ((angle > -Pi) && (angle <= Pi))
                return angle;
            angle %= TwoPi;
            if (angle <= -Pi)
                return angle + TwoPi;
            if (angle > Pi)
                return angle - TwoPi;
            return angle;
        }

        /// <summary>
        /// Onko arvo kahden potenssi.
        /// </summary>
        /// <param name="value">Arvo.</param>
        /// <returns><c>true</c> jos <c>value</c> on kahden potenssi, muuten <c>false</c>.</returns>
        public static bool IsPowerOfTwo(int value)
        {
            return (value > 0) && ((value & (value - 1)) == 0);
        }
    }
}
