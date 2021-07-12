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
    /// Suuntakulma (rajoitettu -180 ja 180 asteen välille) asteina ja radiaaneina.
    /// Tietoja kulmasta: http://en.wikipedia.org/wiki/File:Degree-Radian_Conversion.svg
    /// </summary>
    public struct Angle
    {
        /// <summary>
        /// Nollakulma.
        /// </summary>
        public static readonly Angle Zero = new Angle( 0 );

        /// <summary>
        /// Suora kulma (90 astetta).
        /// </summary>
        public static readonly Angle RightAngle = new Angle( 0.5 * Math.PI );

        /// <summary>
        /// Oikokulma (180 astetta).
        /// </summary>
        public static readonly Angle StraightAngle = new Angle( Math.PI );

        /// <summary>
        /// Täysikulma (360 astetta).
        /// </summary>
        public static readonly Angle FullAngle = new Angle( 2 * Math.PI );


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
                radian = AdvanceMath.MathHelper.ClampAngle( (float)value );
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


        private Angle( double radians )
        {
            this.radian = AdvanceMath.MathHelper.ClampAngle( (float)radians );
        }

        #region Operators

        /// <summary>
        /// Laskee kaksi kulmaa yhteen.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns>Kulmien summa.</returns>
        public static Angle operator +( Angle a, Angle b )
        {
            return FromRadians( a.Radians + b.Radians );
        }

        /// <summary>
        /// Vähentää jälkimmäisen kulman ensimmäisestä.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns>Kulmien erotus.</returns>
        public static Angle operator -( Angle a, Angle b )
        {
            return FromRadians( a.GetPositiveRadians() - b.GetPositiveRadians() );
        }

        /// <summary>
        /// Ottaa kulman vastakulman.
        /// </summary>
        /// <param name="a">Kulma.</param>
        public static Angle operator -( Angle a )
        {
            return FromRadians( -a.Radians );
        }

        /// <summary>
        /// Kertoo kulman reaaliluvulla.
        /// </summary>
        /// <param name="a">Reaaliluku.</param>
        /// <param name="b">Kulma.</param>
        /// <returns>Kulma.</returns>
        public static Angle operator *( double a, Angle b )
        {
            return FromRadians( a * b.Radians );
        }

        /// <summary>
        /// Kertoo kulman reaaliluvulla.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Reaaliluku.</param>
        /// <returns>Kulma.</returns>
        public static Angle operator *( Angle a, double b )
        {
            return FromRadians( a.Radians * b );
        }

        /// <summary>
        /// Jakaa kulman reaaliluvulla.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Reaaliluku.</param>
        /// <returns>Kulma.</returns>
        public static Angle operator /( Angle a, double b )
        {
            return FromRadians( a.Radians / b );
        }

        /// <summary>
        /// Vertaa kahden kulman yhtäsuuruutta.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns>Yhtäsuuruus.</returns>
        public static bool operator ==( Angle a, Angle b )
        {
            return a.Radians == b.Radians;
        }

        /// <summary>
        /// Vertaa kahden kulman erisuuruutta.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns>Erisuuruus.</returns>
        public static bool operator !=( Angle a, Angle b )
        {
            return a.Radians != b.Radians;
        }

        /// <summary>
        /// Vertaa ensimmäisen kulman suuremmuutta toiseen.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns><c>true</c> jos suurempi, <c>false</c> jos pienempi tai yhtäsuuri.</returns>
        public static bool operator <( Angle a, Angle b )
        {
            return a.Radians < b.Radians;
        }

        /// <summary>
        /// Vertaa ensimmäisen kulman suuremmuutta/yhtäsuuruutta toiseen.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns><c>true</c> jos suurempi tai yhtäsuuri, <c>false</c> jos pienempi.</returns>
        public static bool operator <=( Angle a, Angle b )
        {
            return a.Radians <= b.Radians;
        }

        /// <summary>
        /// Vertaa ensimmäisen kulman pienemmyyttä toiseen.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns><c>true</c> jos pienempi, <c>false</c> jos suurempi tai yhtäsuuri.</returns>
        public static bool operator >( Angle a, Angle b )
        {
            return a.Radians > b.Radians;
        }

        /// <summary>
        /// Vertaa ensimmäisen kulman pienemmyyttä/yhtäsuuruutta toiseen.
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <param name="b">Kulma.</param>
        /// <returns><c>true</c> jos pienempi tai yhtäsuuri, <c>false</c> jos suurempi.</returns>
        public static bool operator >=( Angle a, Angle b )
        {
            return a.Radians >= b.Radians;
        }

        /// <summary>
        /// Poistaa kulman rajoituksen tyyppimuunnoksena.
        /// </summary>
        /// <param name="angle">Rajoitettu kulma</param>
        /// <returns>Rajoittamaton kulma</returns>
        public static explicit operator UnlimitedAngle( Angle angle )
        {
            return angle.Unlimit();
        }

        /// <summary>
        /// Laskee yhteen rajoittamattoman ja rajoitetun kulman, palauttaen rajoitetun kulman.
        /// </summary>
        /// <param name="a">Rajoittamaton kulma</param>
        /// <param name="b">Rajoitettu kulma</param>
        /// <returns>Rajoitettu kulma</returns>
        public static Angle Sum(UnlimitedAngle a, Angle b)
        {
            return Angle.FromRadians( a.Radians + b.Radians );
        }

        /// <summary>
        /// Laskee yhteen rajoitetun ja rajoittamattoman kulman, palauttaen rajoitetun kulman.
        /// </summary>
        /// <param name="a">Rajoitettu kulma</param>
        /// <param name="b">Rajoittamaton kulma</param>
        /// <returns>Rajoitettu kulma</returns>
        public static Angle Sum( Angle a, UnlimitedAngle b )
        {
            return Angle.FromRadians( a.Radians + b.Radians );
        }

        #endregion

        /// <summary>
        /// Palauttaa vastaavan rajoittamattoman kulman.
        /// </summary>
        public UnlimitedAngle Unlimit()
        {
            return UnlimitedAngle.FromRadians( this.radian );
        }

        /// <summary>
        /// Luo kulman annettujen radiaanien mukaan.
        /// </summary>
        /// <param name="radian">Radiaanit.</param>
        public static Angle FromRadians( double radian )
        {
            return new Angle( radian );
        }

        /// <summary>
        /// Luo kulman annettujen asteiden mukaan.
        /// </summary>
        /// <param name="degree">Asteet.</param>
        public static Angle FromDegrees( double degree )
        {
            return new Angle( DegreeToRadian( degree ) );
        }

        /// <summary>
        /// Muuttaa asteet radiaaneiksi.
        /// </summary>
        /// <param name="degree">Asteet.</param>
        /// <returns></returns>
        public static double DegreeToRadian( double degree )
        {
            return AdvanceMath.MathHelper.ClampAngle( (float)(degree * ( System.Math.PI / 180 )) );
        }

        /// <summary>
        /// Muuttaa radiaanit asteiksi.
        /// </summary>
        /// <param name="radian">Radiaanit.</param>
        /// <returns></returns>
        public static double RadianToDegree( double radian )
        {
            double a = AdvanceMath.MathHelper.ClampAngle( (float)radian );
            return a * ( 180 / System.Math.PI );
        }

        /// <summary>
        /// Laskee komplementtikulman (90 asteen kulman toinen puoli)
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <returns></returns>
        public static Angle Complement( Angle a )
        {
            return FromRadians( 0.5 * Math.PI - a.Radians );
        }

        /// <summary>
        /// Laskee suplementtikulman (180 asteen kulman toinen puoli)
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <returns></returns>
        public static Angle Supplement( Angle a )
        {
            return FromRadians( Math.PI - a.Radians );
        }

        /// <summary>
        /// Laskee eksplementtikulman (360 asteen kulman toinen puoli)
        /// </summary>
        /// <param name="a">Kulma.</param>
        /// <returns></returns>
        public static Angle Explement( Angle a )
        {
            return FromRadians( 2 * Math.PI - a.Radians );
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
            if ( obj is Angle )
            {
                return Double.Equals( this.Radians, ( (Angle)obj ).Radians );
            }

            return false;
        }

        /// <summary>
        /// Kulma radiaaneina
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return radian.ToString( System.Globalization.NumberFormatInfo.InvariantInfo );
        }

        /// <summary>
        /// Kulma radiaaneina annetussa muodossa
        /// </summary>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public string ToString( IFormatProvider formatProvider )
        {
            return radian.ToString( formatProvider );
        }

        /// <summary>
        /// Muodostaa kulman annetusta merkkijonosta
        /// </summary>
        /// <param name="angleStr">Kulma radiaaneina</param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public static Angle Parse( string angleStr, IFormatProvider formatProvider )
        {
            return new Angle( double.Parse( angleStr, formatProvider ) );
        }

        /// <summary>
        /// Muodostaa kulman annetusta merkkijonosta
        /// </summary>
        /// <param name="angleStr">Kulma radiaaneina</param>
        /// <returns></returns>
        public static Angle Parse( string angleStr )
        {
            return new Angle( double.Parse( angleStr, NumberFormatInfo.InvariantInfo ) );
        }

        /// <summary>
        /// Palauttaa kulman radiaaneina siten, että se on aina positiivinen.
        /// Hyödyllinen esimerkiksi ympyrän kaarien käsittelyssä.
        /// </summary>
        /// <returns>[0,2pi[</returns>
        public double GetPositiveRadians()
        {
            return Radians >= 0 ? Radians : Math.PI * 2 + Radians;
        }

        /// <summary>
        /// Palauttaa kulman asteina siten, että se on aina positiivinen.
        /// Hyödyllinen esimerkiksi ympyrän kaarien käsittelyssä.
        /// </summary>
        /// <returns>[0,360[</returns>
        public double GetPositiveDegrees()
        {
            return Degrees >= 0 ? Degrees : 360 + Degrees;
        }

        /// <summary>
        /// Kulmaa vastaava yksikkövektori
        /// </summary>
        /// <returns></returns>
        public Vector GetVector()
        {
            return Vector.FromAngle( this );
        }

        #region Arcusfunktiot

        /// <summary>
        /// Palauttaa kulman joka vastaa d:n arcus-sini.
        /// </summary>
        /// <param name="d">Lukuarvo välillä 0-1.</param>
        /// <returns>Kulma.</returns>
        public static Angle ArcSin( double d )
        {
            return new Angle( Math.Asin( d ) );
        }

        /// <summary>
        /// Palauttaa kulman joka vastaa d:n arcuskosini.
        /// </summary>
        /// <param name="d">Lukuarvo välillä 0-1.</param>
        /// <returns>Kulma.</returns>
        public static Angle ArcCos( double d )
        {
            return new Angle( Math.Acos( d ) );
        }

        /// <summary>
        /// Palauttaa kulman joka vastaa d:n arcus-tangentti.
        /// </summary>
        /// <param name="d">Lukuarvo.</param>
        /// <returns>Kulma.</returns>
        public static Angle ArcTan( double d )
        {
            return new Angle( Math.Atan( d ) );
        }

        #endregion
    }
}
