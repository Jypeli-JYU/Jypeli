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
using System.Collections.Generic;

namespace Jypeli
{
    /// <summary>
    /// Suunta mittarin muutokselle.
    /// </summary>
    public enum TriggerDirection
    {
        /// <summary>
        /// Mittarin arvo kasvaa.
        /// </summary>
        Up,

        /// <summary>
        /// Mittarin arvo vähenee.
        /// </summary>
        Down,
        
        /// <summary>
        /// Ei väliä suunnalla (kasvaa tai vähenee).
        /// </summary>
        Irrelevant
    };

    /// <summary>
    /// Mittari, joka mittaa erityyppisiä arvoja.
    /// Sidottavissa näyttöihin, kuten <c>ValueDisplay</c> ja <c>BarGauge</c>.
    /// </summary>
    public abstract class Meter
    {
        /// <summary>
        /// Mittarin suhteellinen arvo (minimi 0, maksimi 1)
        /// </summary>
        public abstract double RelativeValue { get; set; }

        internal abstract double GetValue();
        internal abstract double GetMinValue();
        internal abstract double GetMaxValue();

        /// <summary>
        /// Palauttaa mittarin sen todellisessa muodossa, jotta sen kaikkiin
        /// jäseniin pääsee käsiksi.
        /// </summary>
        /// <typeparam name="T">Tyyppi, jota mittari mittaa</typeparam>
        /// <returns>Mittari, joka mittaa tyyppiä T</returns>
        public Meter<T> OfType<T>() where T : struct, IComparable<T>, IEquatable<T>
        {
            Type meterType = this.GetType();

            Type[] genargs = meterType.GetGenericArguments();

            if ( genargs.Length < 1 ) throw new ArgumentException( "This meter is not typed" );
            if ( genargs[0] != typeof( T ) ) throw new ArgumentException( String.Format( "This meter measures {0}, not {1}", genargs[0].Name, typeof( T ).Name ) );

            return (Meter<T>)this;
        }
    }

    /// <summary>
    /// Mittari, joka mittaa erityyppisiä arvoja.
    /// Sidottavissa näyttöihin, kuten <c>ValueDisplay</c> ja <c>BarGauge</c>.
    /// </summary>
    [Save]
    public abstract class Meter<ValueType> : Meter where ValueType : struct, IComparable<ValueType>, IEquatable<ValueType>
    {
        private struct Trigger
        {
            public ValueType value;
            public TriggerDirection direction;
            public Action method;

            public Trigger(ValueType value, TriggerDirection direction, Action method)
            {
                this.value = value;
                this.direction = direction;
                this.method = method;
            }
        }

        private bool valueSet;

        private ValueType val;
        private ValueType minval;
        private ValueType maxval;
        private ValueType defval;

        private List<Trigger> triggers;

        /// <summary>
        /// Mittarin arvo.
        /// </summary>
        [Save]
        public ValueType Value
        {
            get { return val; }
            set
            {
                if ( value.Equals( val ) )
                    return;

                ValueType oldval = val;
                valueSet = true;

                if (value.CompareTo(minval) < 0) value = minval;
                if (value.CompareTo(maxval) > 0) value = maxval;

                val = value;
                OnChange(oldval, value);
                CheckLimits( oldval, value );
                CheckTriggers(oldval, value);
            }
        }

        /// <summary>
        /// Mittarin oletusarvo.
        /// </summary>
        public ValueType DefaultValue
        {
            get { return defval; }
            set
            {
                defval = clampValue( value, minval, maxval );
                if ( !valueSet ) Value = value;
            }
        }

        /// <summary>
        /// Mittarin pienin sallittu arvo.
        /// Kun mittari saavuttaa tämän arvon, laukeaa tapahtuma <c>LowerLimit</c>.
        /// </summary>
        public ValueType MinValue
        {
            get { return minval; }
            set { minval = value; updateBounds(); }
        }

        /// <summary>
        /// Mittarin suurin sallittu arvo.
        /// Kun mittari saavuttaa tämän arvon, laukeaa tapahtuma <c>UpperLimit</c>.
        /// </summary>
        public ValueType MaxValue
        {
            get { return maxval; }
            set { maxval = value; updateBounds(); }
        }

        #region Events

        /// <summary>
        /// Mittarin muutostapahtumankäsittelijä.
        /// </summary>
        public delegate void ChangeHandler( ValueType oldValue, ValueType newValue );

        /// <summary>
        /// Tapahtuu, kun mittarin arvo muuttuu.
        /// </summary>
        public event ChangeHandler Changed;

        /// <summary>
	    /// Tapahtuu, kun mittari saavuttaa pienimmän sallitun arvonsa.
	    /// </summary>
        public event Action LowerLimit;

	    /// <summary>
	    /// Tapahtuu, kun mittari saavuttaa suurimman sallitun arvonsa.
	    /// </summary>
	    public event Action UpperLimit;


        private void OnChange( ValueType oldValue, ValueType newValue )
        {
            if ( Changed != null )
                Changed( oldValue, newValue );
        }

        private void CheckLimits( ValueType oldValue, ValueType newValue )
        {
            if ( LowerLimit != null && newValue.CompareTo( minval ) <= 0 )
            {
                LowerLimit();
                return;
            }

            if ( UpperLimit != null && newValue.CompareTo( maxval ) >= 0 )
            {
                UpperLimit();
                return;
            }
        }

        private void CheckTriggers( ValueType oldValue, ValueType newValue )
        {
            if ( triggers == null ) return;

            foreach ( Trigger t in triggers )
            {
                if ( t.direction != TriggerDirection.Down && oldValue.CompareTo( t.value ) < 0 && newValue.CompareTo( t.value ) >= 0 )
                    t.method();
                
                else if ( t.direction != TriggerDirection.Up && oldValue.CompareTo( t.value ) > 0 && newValue.CompareTo( t.value ) <= 0 )
                    t.method();
            }
        }

        #endregion

        /// <summary>
        /// Luo uuden mittarin.
        /// </summary>
        /// <param name="defaultVal">Oletusarvo.</param>
        /// <param name="minVal">Pienin sallittu arvo.</param>
        /// <param name="maxVal">Suurin sallittu arvo.</param>
        public Meter( ValueType defaultVal, ValueType minVal, ValueType maxVal )
        {
            minval = minVal;
            maxval = maxVal;
            defval = defaultVal;
            val = defaultVal;
            updateBounds();
        }

        /// <summary>
        /// Luo uuden mittarin kopiona parametrina annetusta.
        /// </summary>
        /// <param name="src">Kopioitava mittari.</param>
        public Meter( Meter<ValueType> src )
        {
            minval = src.minval;
            maxval = src.maxval;
            defval = src.defval;
            val = src.val;
            updateBounds();
        }

        /// <summary>
        /// Palauttaa mittarin arvon oletusarvoonsa.
        /// </summary>
        public void Reset()
        {
            Value = DefaultValue;
        }

        /// <summary>
        /// Asettaa mittarille arvon. Sama kuin Value-ominaisuuteen sijoitus,
        /// mutta helpompi käyttää tapahtumakäsittelijöissä.
        /// </summary>
        /// <param name="value">Uusi arvo</param>
        public void SetValue( ValueType value )
        {
            Value = value;
        }

        /// <summary>
        /// Lisää mittarille rajan, jonka yli mentäessä laukaistaan aliohjelma.
        /// </summary>
        /// <param name="value">Mittarin arvo</param>
        /// <param name="direction">Suunta (TriggerDirection.Irrelevant, TriggerDirection.Up tai TriggerDirection.Down)</param>
        /// <param name="method">Aliohjelma, jota kutsutaan.</param>
        public void AddTrigger( ValueType value, TriggerDirection direction, Action method )
        {
            if ( triggers == null ) triggers = new List<Trigger>();
            triggers.Add( new Trigger( value, direction, method ) );
        }

        /// <summary>
        /// Lisää mittarille rajan, jonka yli mentäessä laukaistaan aliohjelma.
        /// </summary>
        /// <param name="value">Mittarin arvo</param>
        /// <param name="direction">Suunta (TriggerDirection.Irrelevant, TriggerDirection.Up tai TriggerDirection.Down)</param>
        /// <param name="method">Aliohjelma, jota kutsutaan (parametrina mittarin arvo).</param>
        public void AddTrigger( ValueType value, TriggerDirection direction, Action<ValueType> method )
        {
            AddTrigger( value, direction, delegate() { method( this.Value ); } );
        }

        /// <summary>
        /// Poistaa kaikki tietylle arvolle asetetut raja-arvotapahtumat.
        /// </summary>
        /// <param name="value">Arvo</param>
        public void RemoveTriggers( ValueType value )
        {
            if ( triggers == null ) return;
            triggers.RemoveAll( t => t.value.Equals( value ) );
        }

        /// <summary>
        /// Poistaa kaikki raja-arvotapahtumat, jotka kutsuvat tiettyä aliohjelmaa.
        /// </summary>
        /// <param name="method">Aliohjelma</param>
        public void RemoveTriggers( Action method )
        {
            if ( triggers == null ) return;
            triggers.RemoveAll( t => t.method == method );
        }

        /// <summary>
        /// Poistaa kaikki raja-arvotapahtumat.
        /// </summary>
        public void ClearTriggers()
        {
            if ( triggers == null ) return;
            triggers.Clear();
        }

        private static ValueType clampValue( ValueType v, ValueType min, ValueType max )
        {
            if ( v.CompareTo( min ) < 0 )
                return min;

            if ( v.CompareTo( max ) > 0 )
                return max;

            return v;
        }

        private static void clampValue( ref ValueType v, ValueType min, ValueType max )
        {
            if ( v.CompareTo( min ) < 0 )
                v = min;

            else if ( v.CompareTo( max ) > 0 )
                v = max;
        }

        private void updateBounds()
        {
            clampValue( ref val, minval, maxval );
            clampValue( ref defval, minval, maxval );
        }

        /// <summary>
        /// Palauttaa mittarin arvon merkkijonona.
        /// </summary>
        public override String ToString()
        {
            return Value.ToString();
        }
    }    
}
