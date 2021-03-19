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

namespace Jypeli
{
    /// <summary>
    /// Rajoittamaton kulma (asteina ja radiaaneina).
    /// Tietoja kulmasta: http://en.wikipedia.org/wiki/File:Degree-Radian_Conversion.svg
    /// </summary>
    public struct UnlimitedAngle
    {
        /// <summary>
        /// Nollakulma.
        /// </summary>
        public static readonly UnlimitedAngle Zero = new UnlimitedAngle( 0 );

        /// <summary>
        /// Suora kulma (90 astetta).
        /// </summary>
        public static readonly UnlimitedAngle RightAngle = new UnlimitedAngle( 0.5 * Math.PI );

        /// <summary>
        /// Oikokulma (180 astetta).
        /// </summary>
        public static readonly UnlimitedAngle StraightAngle = new UnlimitedAngle( Math.PI );

        /// <summary>
        /// Täysikulma (360 astetta).
        /// </summary>
        public static readonly UnlimitedAngle FullAngle = new UnlimitedAngle( 2 * Math.PI );

        /// <summary>
        /// Ääretön kulma.
        /// </summary>
        public static readonly UnlimitedAngle Infinity = new UnlimitedAngle( double.PositiveInfinity );

        private double radian;

        /// <summary>
        /// Palauttaa tai asettaa kulman asteina.
        /// </summary>
        /// <value>Asteet.</value>
        public double Degrees
        {
            get
            {
                return RadianToDegree( radian );
            }
            set
            {
                radian = DegreeToRadian( value );
            }
        }

        /// <summary>
        /// Palauttaa tai asettaa kulman radiaaneina.
        /// </summary>
        /// <value>Radiaanit.</value>
        public double Radians
        {
            get { return radian; }
            set
            {
                radian = (float)value;
            }
        }

        /// <summary>
        /// Kulmaa vastaava pääilmansuunta.
        /// </summary>
        public Direction MainDirection
        {
            get
            {
                if ( radian >= -Math.PI / 4 && radian <= Math.PI / 4 ) return Direction.Right;
                if ( radian > Math.PI / 4 && radian < 3 * Math.PI / 4 ) return Direction.Up;
                if ( radian < -Math.PI / 4 && radian > -3 * Math.PI / 4 ) return Direction.Down;
                return Direction.Left;
            }
        }

        /// <summary>
        /// Kulman sini.
        /// </summary>
        public double Sin
        {
            get { return Math.Sin( this.Radians ); }
        }

        /// <summary>
        /// Kulman kosini.
        /// </summary>
        public double Cos
        {
            get { return Math.Cos( this.Radians ); }
        }

        /// <summary>
        /// Kulman tangentti.
        /// </summary>
        public double Tan
        {
            get { return Math.Tan( this.Radians ); }
        }


        private UnlimitedAngle( double radians )
        {
            this.radian = (float)radians;
        }

        #region Operators

        /// <summary>
        /// Laskee kaksi kulmaa yhteen.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns>Kulmien summa.</returns>
        public static UnlimitedAngle operator +( UnlimitedAngle a, UnlimitedAngle b )
        {
            return FromRadians( a.Radians + b.Radians );
        }

        /// <summary>
        /// Vähentää jälkimmäisen kulman ensimmäisestä.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns>Kulmien erotus.</returns>
        public static UnlimitedAngle operator -( UnlimitedAngle a, UnlimitedAngle b )
        {
            return FromRadians( a.Radians - b.Radians );
        }

        /// <summary>
        /// Ottaa kulman vastakulman.
        /// </summary>
        /// <param name="a">Kulma.</param>
        public static UnlimitedAngle operator -( UnlimitedAngle a )
        {
            return FromRadians( -a.Radians );
        }

        /// <summary>
        /// Kertoo kulman reaaliluvulla.
        /// </summary>
        /// <param name="a">Reaaliluku.</param>
        /// <param name="b">Kulma.</param>
        /// <returns>Kulma.</returns>
        public static UnlimitedAngle operator *( double a, UnlimitedAngle b )
        {
            return FromRadians( a * b.Radians );
        }

        /// <summary>
        /// Kertoo kulman reaaliluvulla.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Reaaliluku.</param>
        /// <returns>Kulma.</returns>
        public static UnlimitedAngle operator *( UnlimitedAngle a, double b )
        {
            return FromRadians( a.Radians * b );
        }

        /// <summary>
        /// Jakaa kulman reaaliluvulla.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Reaaliluku.</param>
        /// <returns>Kulma.</returns>
        public static UnlimitedAngle operator /( UnlimitedAngle a, double b )
        {
            return FromRadians( a.Radians / b );
        }

        /// <summary>
        /// Vertaa kahden kulman yhtäsuuruutta.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns>Yhtäsuuruus.</returns>
        public static bool operator ==( UnlimitedAngle a, UnlimitedAngle b )
        {
            return a.Radians == b.Radians;
        }

        /// <summary>
        /// Vertaa kahden kulman erisuuruutta.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns>Erisuuruus.</returns>
        public static bool operator !=( UnlimitedAngle a, UnlimitedAngle b )
        {
            return a.Radians != b.Radians;
        }

        /// <summary>
        /// Vertaa ensimmäisen kulman suuremmuutta toiseen.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns><c>true</c> jos suurempi, <c>false</c> jos pienempi tai yhtäsuuri.</returns>
        public static bool operator <( UnlimitedAngle a, UnlimitedAngle b )
        {
            return a.Radians < b.Radians;
        }

        /// <summary>
        /// Vertaa ensimmäisen kulman suuremmuutta/yhtäsuuruutta toiseen.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns><c>true</c> jos suurempi tai yhtäsuuri, <c>false</c> jos pienempi.</returns>
        public static bool operator <=( UnlimitedAngle a, UnlimitedAngle b )
        {
            return a.Radians <= b.Radians;
        }

        /// <summary>
        /// Vertaa ensimmäisen kulman pienemmyyttä toiseen.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns><c>true</c> jos pienempi, <c>false</c> jos suurempi tai yhtäsuuri.</returns>
        public static bool operator >( UnlimitedAngle a, UnlimitedAngle b )
        {
            return a.Radians > b.Radians;
        }

        /// <summary>
        /// Vertaa ensimmäisen kulman pienemmyyttä/yhtäsuuruutta toiseen.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns><c>true</c> jos pienempi tai yhtäsuuri, <c>false</c> jos suurempi.</returns>
        public static bool operator >=( UnlimitedAngle a, UnlimitedAngle b )
        {
            return a.Radians >= b.Radians;
        }

        /// <summary>
        /// Rajoittaa kulman tyyppimuunnoksena.
        /// </summary>
        /// <param name="angle">Rajoittamaton kulma</param>
        /// <returns>Rajoitettu kulma</returns>
        public static explicit operator Angle( UnlimitedAngle angle )
        {
            return angle.Limit();
        }

        /// <summary>
        /// Laskee yhteen rajoittamattoman ja rajoitetun kulman, palauttaen rajoittamattoman kulman.
        /// </summary>
        /// <param name="a">Rajoittamaton kulma</param>
        /// <param name="b">Rajoitettu kulma</param>
        /// <returns>Rajoittamaton kulma</returns>
        public static UnlimitedAngle Sum( UnlimitedAngle a, Angle b )
        {
            return UnlimitedAngle.FromRadians( a.Radians + b.Radians );
        }

        /// <summary>
        /// Laskee yhteen rajoitetun ja rajoittamattoman kulman, palauttaen rajoittamattoman kulman.
        /// </summary>
        /// <param name="a">Rajoitettu kulma</param>
        /// <param name="b">Rajoittamaton kulma</param>
        /// <returns>Rajoittamaton kulma</returns>
        public static UnlimitedAngle Sum( Angle a, UnlimitedAngle b )
        {
            return UnlimitedAngle.FromRadians( a.Radians + b.Radians );
        }

        #endregion

        /// <summary>
        /// Palauttaa kulman rajoitettuna välille -180 ja 180 astetta.
        /// </summary>
        public Angle Limit()
        {
            return Angle.FromRadians( this.radian );
        }

        /// <summary>
        /// Luo kulman annettujen radiaanien mukaan.
        /// </summary>
        /// <param name="radian">Radiaanit.</param>
        public static UnlimitedAngle FromRadians( double radian )
        {
            return new UnlimitedAngle( radian );
        }

        /// <summary>
        /// Luo kulman annettujen asteiden mukaan.
        /// </summary>
        /// <param name="degree">Asteet.</param>
        public static UnlimitedAngle FromDegrees( double degree )
        {
            return new UnlimitedAngle( DegreeToRadian( degree ) );
        }

        /// <summary>
        /// Muuttaa asteet radiaaneiksi.
        /// </summary>
        /// <param name="degree">Asteet.</param>
        /// <returns></returns>
        public static double DegreeToRadian( double degree )
        {
            return (float)( degree * ( System.Math.PI / 180 ) );
        }

        /// <summary>
        /// Muuttaa radiaanit asteiksi.
        /// </summary>
        /// <param name="radian">Radiaanit.</param>
        /// <returns></returns>
        public static double RadianToDegree( double radian )
        {
            return radian * ( 180 / Math.PI );
        }

        /// <summary>
        /// Palauttaa kulmaa yksilöivän luvun, tässä tapauksessa kulman asteluvun.
        /// </summary>
        /// <returns>
        /// Kokonaisluku.
        /// </returns>
        public override int GetHashCode()
        {
            return Convert.ToInt32( Degrees );
        }

        /// <summary>
        /// Tarkistaa kahden kulman yhtäsuuruuden. Jos parametrinä annetaan jotain muuta kuin kulma, tulos on aina epätosi.
        /// </summary>
        /// <param name="obj">Toinen kulma.</param>
        /// <returns></returns>
        public override bool Equals( object obj )
        {
            if ( obj is UnlimitedAngle )
            {
                return Double.Equals( this.Radians, ( (UnlimitedAngle)obj ).Radians );
            }

            else if ( obj is Angle )
            {
                return Double.Equals( this.Radians, ( (Angle)obj ).Radians );
            }

            return false;
        }

        public override string ToString()
        {
            return radian.ToString( System.Globalization.NumberFormatInfo.InvariantInfo );
        }

        public string ToString( IFormatProvider formatProvider )
        {
            return radian.ToString( formatProvider );
        }

        public static UnlimitedAngle Parse( string angleStr, IFormatProvider formatProvider )
        {
            return new UnlimitedAngle( double.Parse( angleStr, formatProvider ) );
        }

        public static UnlimitedAngle Parse( string angleStr )
        {
            return new UnlimitedAngle( double.Parse( angleStr, NumberFormatInfo.InvariantInfo ) );
        }

        /// <summary>
        /// Palauttaa kulman radiaaneina siten, että se on aina positiivinen.
        /// Hyödyllinen esimerkiksi ympyrän kaarien käsittelyssä.
        /// </summary>
        /// <returns>]0,2pi]</returns>
        public double GetPositiveRadians()
        {
            return Radians > 0 ? Radians : Math.PI * 2 + Radians;
        }

        /// <summary>
        /// Palauttaa kulman asteina siten, että se on aina positiivinen.
        /// Hyödyllinen esimerkiksi ympyrän kaarien käsittelyssä.
        /// </summary>
        /// <returns>]0,360]</returns>
        public double GetPositiveDegrees()
        {
            double deg = Degrees;
            while ( deg < 0 ) deg += 360;
            return deg;
        }

        public Vector GetVector()
        {
            return Vector.FromAngle( Limit() );
        }

        #region Arcusfunktiot

        /// <summary>
        /// Palauttaa kulman joka vastaa d:n arcus-sini.
        /// </summary>
        /// <param name="d">Lukuarvo välillä 0-1.</param>
        /// <returns>Kulma.</returns>
        public static UnlimitedAngle ArcSin( double d )
        {
            return new UnlimitedAngle( Math.Asin( d ) );
        }

        /// <summary>
        /// Palauttaa kulman joka vastaa d:n arcuskosini.
        /// </summary>
        /// <param name="d">Lukuarvo välillä 0-1.</param>
        /// <returns>Kulma.</returns>
        public static UnlimitedAngle ArcCos( double d )
        {
            return new UnlimitedAngle( Math.Acos( d ) );
        }

        /// <summary>
        /// Palauttaa kulman joka vastaa d:n arcus-tangentti.
        /// </summary>
        /// <param name="d">Lukuarvo.</param>
        /// <returns>Kulma.</returns>
        public static UnlimitedAngle ArcTan( double d )
        {
            return new UnlimitedAngle( Math.Atan( d ) );
        }

        #endregion
    }
}
