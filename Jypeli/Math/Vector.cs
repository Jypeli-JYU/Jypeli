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
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.Globalization;
using System.Collections.Generic;

using Matrix = System.Numerics.Matrix4x4;

namespace Jypeli
{
    // NOTES:
    //
    // TJ: In addition to the types Vector2 from XNA and Vector2D from AdvanceMath, we
    // have a vector type of our own!
    //
    // PROS:
    // * No need to have "using AdvanceMath" in games, keeps the interface clean and simple.
    // * Has less methods and properties, again keeping the interface clean and simple.
    // * Less typing in the name :P
    //
    // CONS:
    // * Does it add overhead in execution? I don't think so, if the compiler is smart at all.
    //   Since the vectors are struct types, their handling should be done entirely in the stack.

    // MR, 2021:
    // TODO: AdvancedMath paketista voisi hiljalleen alkaa luopumaan.
    // On hieman hämmentävää kun löytyy neljää eri vektoria (Jypeli, AdvancedMath, MonoGame sekä System.Numerics).
    // Nyt kuitenkin onnistuu kaikkien noiden konversiot Jypelin kanssa.
    // MonoGame on (ehkä joskus) vaihtamassa System.Numericsin vektoreihin ja matriiseihin.

    // Matriisit voisi myös tuoda suoraan Jypelin puolelle, niille löytyy kuitenkin käyttöä sieltä sun täältä.


    /// <summary>
    /// 2D-vektori.
    /// </summary>
    [Save]
    public struct Vector
    {
        /// <summary>
        /// Nollavektori.
        /// </summary>
        public static readonly Vector Zero = new Vector( 0, 0 );

        /// <summary>
        /// Yksikkövektori.
        /// </summary>
        public static readonly Vector One = new Vector( 1, 1 );

        /// <summary>
        /// Vaakasuuntainen yksikkövektori (pituus 1, suunta oikealle).
        /// </summary>
        public static readonly Vector UnitX = new Vector( 1.0, 0.0 );

        /// <summary>
        /// Pystysuuntainen yksikkövektori (pituus 1, suunta ylös).
        /// </summary>
        public static readonly Vector UnitY = new Vector( 0.0, 1.0 );

        /// <summary>
        /// Diagonaalivektori (1,1)
        /// </summary>
        public static readonly Vector Diagonal = Vector.One;

        /// <summary>
        /// Vasen normaali.
        /// </summary>
        public Vector LeftNormal
        {
            get { return new Vector( -Y, X ); }
        }

        /// <summary>
        /// Oikea normaali.
        /// </summary>
        public Vector RightNormal
        {
            get { return new Vector( Y, -X ); }
        }

        /// <summary>
        /// Luo vektorin pituuden ja kulman perusteella.
        /// </summary>
        /// <param name="length">Pituus.</param>
        /// <param name="angle">Kulma radiaaneina.</param>
        private static Vector FromLengthAndAngle( double length, double angle )
        {
            Vector result;
            result.X = length * Math.Cos( angle );
            result.Y = length * Math.Sin( angle );
            return result;
        }

        /// <summary>
        /// Luo vektorin pituuden ja kulman perusteella.
        /// </summary>
        public static Vector FromLengthAndAngle( double length, Angle angle )
        {
            return FromLengthAndAngle( length, angle.Radians );
        }

        /// <summary>
        /// Luo vektorin kulman perusteella yksikköpituudella.
        /// </summary>
        public static Vector FromAngle( Angle angle )
        {
            return FromLengthAndAngle( 1, angle.Radians );
        }

        /// <summary>
        /// Etäisyys kahden pisteen välillä.
        /// </summary>
        public static double Distance( Vector p1, Vector p2 )
        {
            double x, y;
            x = p1.X - p2.X;
            y = p1.Y - p2.Y;
            return Math.Sqrt( (float)(x * x + y * y) );
        }

        /// <summary>
        /// Pistetulo.
        /// </summary>
        /// <param name="left">Vasen vektori</param>
        /// <param name="right">Oikea vektori</param>
        /// <returns></returns>
        public static double DotProduct( Vector left, Vector right )
        {
            return left.Y * right.Y + left.X * right.X;
        }

        /// <summary>
        /// Ristitulo.
        /// Palauttaa kohtisuoraan vektoreita vastaan olevan uuden vektorin pituuden.
        /// Tuloksen merkki kertoo kumpaan suuntaan vektori osoittaa.
        /// </summary>
        /// <param name="left">Vasen vektori</param>
        /// <param name="right">Oikea vektori</param>
        /// <returns></returns>
        public static double CrossProduct( Vector left, Vector right )
        {
            return left.Magnitude * right.Magnitude * ( right.Angle - left.Angle ).Sin;
        }

        /// <summary>
        /// Kertoo kaksi vektoria komponenteittain.
        /// </summary>
        /// <param name="a">Vektori</param>
        /// <param name="b">Vektori</param>
        /// <returns>Tulovektori</returns>
        public static Vector ComponentProduct( Vector a, Vector b )
        {
            return new Vector( a.X * b.X, a.Y * b.Y );
        }

        /// <summary>
        /// Etäisyys kahden pisteen välillä
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public double Distance(Vector vector)
        {
            return Distance(this, vector);
        }

        /// <summary>
        /// Skalaariprojektio annettuun vektoriin
        /// https://en.wikipedia.org/wiki/Scalar_projection
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public double ScalarProjection(Vector vector)
        {
            return (Vector.DotProduct(this, vector) / vector.MagnitudeSquared);
        }

        /// <summary>
        /// Laskee ja palauttaa tämän pisteen etäisyyden toiseen pisteeseen.
        /// </summary>
        public Vector Project( Vector to )
        {
            return ScalarProjection( to ) * to;
        }

        /// <summary>
        /// Palauttaa uuden vektorin, jonka suunta pysyy samana, mutta pituudeksi tulee 1.0.
        /// </summary>
        /// <returns></returns>
        public Vector Normalize()
        {
            return this / this.Magnitude;
        }

        /// <summary>
        /// Kertoo vektorin matriisilla.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public Vector Transform( Matrix matrix )
        {
            return new Vector
            (
                ( X * matrix.M11 ) + ( Y * matrix.M21 ) + matrix.M41,
                ( X * matrix.M12 ) + ( Y * matrix.M22 ) + matrix.M42
            );
        }

        /// <summary>
        /// Palauttaa uuden vektorin, jossa x ja y on vaihdettu keskenään.
        /// </summary>
        /// <returns></returns>
        public Vector Transpose()
        {
            return new Vector( Y, X );
        }

        /// <summary>
        /// Palauttaa lyhimmän vektorin.
        /// </summary>
        /// <param name="vectors">Vektorit</param>
        /// <returns>Lyhin vektori.</returns>
        public static Vector Min( params Vector[] vectors )
        {
            int minIndex = 0;
            double minMagnitude = vectors[0].Magnitude;

            for ( int i = 1; i < vectors.Length; i++ )
            {
                double m = vectors[i].Magnitude;
                if ( m < minMagnitude )
                {
                    minIndex = i;
                    minMagnitude = m;
                }
            }

            return vectors[minIndex];
        }

        /// <summary>
        /// Palauttaa pisimmän vektorin.
        /// </summary>
        /// <param name="vectors">Vektorit</param>
        /// <returns>Pisin vektori.</returns>
        public static Vector Max( params Vector[] vectors )
        {
            int maxIndex = 0;
            double maxMagnitude = vectors[0].Magnitude;

            for ( int i = 1; i < vectors.Length; i++ )
            {
                double m = vectors[i].Magnitude;
                if ( m > maxMagnitude )
                {
                    maxIndex = i;
                    maxMagnitude = m;
                }
            }

            return vectors[maxIndex];
        }

        /// <summary>
        /// Palauttaa kahden tai useamman vektorin keskiarvon.
        /// </summary>
        /// <param name="vectors">Vektorit</param>
        /// <returns>Keskiarvovektori</returns>
        public static Vector Average( IEnumerable<Vector> vectors )
        {
            double sumX = 0;
            double sumY = 0;
            int count = 0;

            foreach ( var v in vectors )
            {
                sumX += v.X;
                sumY += v.Y;
                count++;
            }

            return count > 0 ? new Vector( sumX, sumY ) / count : Vector.Zero;
        }

        /// <summary>
        /// Palauttaa kahden tai useamman vektorin keskiarvon.
        /// </summary>
        /// <param name="vectors">Vektorit</param>
        /// <returns>Keskiarvovektori</returns>
        public static Vector Average( params Vector[] vectors )
        {
            double sumX = 0;
            double sumY = 0;

            foreach ( var v in vectors )
            {
                sumX += v.X;
                sumY += v.Y;
            }

            return vectors.Length > 0 ? new Vector( sumX, sumY ) / vectors.Length : Vector.Zero;
        }

        /// <summary>
        /// Vektorin X-komponentti.
        /// </summary>
        [Save] public double X;

        /// <summary>
        /// Vektorin Y-komponentti
        /// </summary>
        [Save] public double Y;

        /// <summary>
        /// Vektorin pituus.
        /// </summary>
        public double Magnitude
        {
            get { return Math.Sqrt( MagnitudeSquared ); }
        }

        /// <summary>
        /// Vektorin pituuden neliö.
        /// </summary>
        public double MagnitudeSquared
        {
            get { return X * X + Y * Y; }
        }

        /// <summary>
        /// Luo uuden vektorin komponenteista.
        /// </summary>
        /// <param name="X">X-komponentti</param>
        /// <param name="Y">Y-komponentti</param>
        public Vector( double X, double Y )
        {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Kulma radiaaneina.
        /// </summary>
        public Angle Angle
        {
            get
            {
                double a = Math.Atan2( Y, X );
                if ( a < 0 )
                    a += 2 * Math.PI;
                return Angle.FromRadians( a );
            }
        }

        /// <summary>
        /// Vektori merkkijonona muodossa (x,y)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString( NumberFormatInfo.InvariantInfo );
        }

        /// <summary>
        /// Vektori merkkijonona muodossa (x,y), jossa x ja y on
        /// muotoiltu annetun formaatin mukaisesti
        /// </summary>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public string ToString( IFormatProvider formatProvider )
        {
            string x = X.ToString( formatProvider );
            string y = Y.ToString( formatProvider );
            return string.Format( "({0},{1})", x, y );
        }

        /// <summary>
        /// Muodostaa (x,y) muodossa olevasta merkkijonsta vektorin
        /// </summary>
        /// <param name="vectorStr"></param>
        /// <returns></returns>
        public static Vector Parse( string vectorStr )
        {
            return Parse( vectorStr, NumberFormatInfo.InvariantInfo );
        }

        /// <summary>
        /// Muodostaa (x,y) muodossa olevasta merkkijonsta vektorin, jossa x ja y  on
        /// muotoiltu annetun formaatin mukaisesti
        /// </summary>
        /// <param name="vectorStr"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public static Vector Parse( string vectorStr, IFormatProvider formatProvider )
        {
            string[] splitStr = vectorStr.Split( '(', ',', ')' );

            if ( splitStr.Length != 4 )
                throw new FormatException( "Not a vector string: " + vectorStr );

            double x = double.Parse( splitStr[1], formatProvider );
            double y = double.Parse( splitStr[2], formatProvider );

            return new Vector( x, y );
        }

        /// <summary>
        /// Vektorin hajautuskoodi
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(X.GetHashCode(), Y.GetHashCode());
        }

        /// <summary>
        /// Onko annettu vektori yhtäsuuri tämän kanssa.
        /// Tosi, jos vektorien komponentit ovat <c>double.Epsilon</c>in päässä toisistaan.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (obj is not Vector) return false;
            
            Vector v = (Vector)obj;

            double x = v.X;
            double y = v.Y;

            return (Math.Abs(X - x) < double.Epsilon) && (Math.Abs(Y - y) < double.Epsilon);
        }

        #region operators

        /// <summary>
        /// Summaa vektorit yhteen
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Vector operator +(Vector left, Vector right)
        {
            Vector result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            return result;
        }

        /// <summary>
        /// Vähentää vektorit toisistaan
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Vector operator -(Vector left, Vector right)
        {
            Vector result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            return result;
        }

        /// <summary>
        /// Kertoo vektorin skalaarilla
        /// </summary>
        /// <param name="source"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Vector operator *(Vector source, double scalar)
        {
            Vector result;
            result.X = source.X * scalar;
            result.Y = source.Y * scalar;
            return result;
        }

        /// <summary>
        /// Kertoo vektorin skalaarilla
        /// </summary>
        /// <param name="scalar"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Vector operator *(double scalar, Vector source)
        {
            return source * scalar;
        }

        /// <summary>
        /// Jakaa vektorin skalaarilla
        /// </summary>
        /// <param name="source"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static Vector operator /(Vector source, double scalar)
        {
            // TJ: Let's not implement this using operator* in order to
            // avoid rounding errors.
            Vector result;
            result.X = source.X / scalar;
            result.Y = source.Y / scalar;
            return result;
        }

        /// <summary>
        /// Miinustaa vektorin toisesta
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Vector operator -(Vector source)
        {
            Vector result;
            result.X = -source.X;
            result.Y = -source.Y;
            return result;
        }

        /// <summary>
        /// Ovatko vektorit samat
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==( Vector left, Vector right )
        {
            return ( Math.Abs( left.X - right.X ) < double.Epsilon ) && ( Math.Abs( left.Y - right.Y ) < double.Epsilon );
        }

        /// <summary>
        /// Ovatko vektorit eri
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=( Vector left, Vector right )
        {
            return !( left == right );
        }

        #region Silk.NET conversions

        /// <summary>
        /// Muuttaa Silk.NET.Maths.Vector2D -> Jypeli.Vector
        /// </summary>
        /// <param name="v"></param>
        public static explicit operator Vector(Silk.NET.Maths.Vector2D<float> v)
        {
            return new Vector(v.X, v.Y);
        }

        #endregion
        #region System.Numerics conversions

        /// <summary>
        /// Muuttaa System.Numerics.Vector2 -> Jypeli.Vector
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator Vector(System.Numerics.Vector2 v)
        {
            return new Vector(v.X, v.Y);
        }

        /// <summary>
        /// Muuttaa Jypeli.Vector -> System.Numerics.Vector2
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator System.Numerics.Vector2(Vector v)
        {
            return new System.Numerics.Vector2((float)v.X, (float)v.Y);
        }

        /// <summary>
        /// Muuttaa System.Numerics.Vector3 -> Jypeli.Vector
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator Vector(System.Numerics.Vector3 v)
        {
            return new Vector(v.X, v.Y);
        }

        /// <summary>
        /// Muuttaa Jypeli.Vector -> System.Numerics.Vector3
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator System.Numerics.Vector3(Vector v)
        {
            return new System.Numerics.Vector3((float)v.X, (float)v.Y, 0);
        }

        /// <summary>
        /// Muuttaa System.Numerics.Vector4 -> Jypeli.Vector
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator Vector(System.Numerics.Vector4 v)
        {
            return new Vector(v.X, v.Y);
        }

        /// <summary>
        /// Muuttaa Jypeli.Vector -> System.Numerics.Vector4
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator System.Numerics.Vector4(Vector v)
        {
            return new System.Numerics.Vector4((float)v.X, (float)v.Y, 0, 0);
        }
        #endregion
        #endregion
    }
}
