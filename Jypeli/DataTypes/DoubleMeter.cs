using System;
using System.Collections.Generic;

namespace Jypeli
{
    /// <summary>
    /// Mittari, joka mittaa double-tyyppisiä arvoja.
    /// Sidottavissa näyttöihin, kuten <c>ValueDisplay</c> ja <c>BarGauge</c>.
    /// </summary>
    public class DoubleMeter : Meter<double>
    {
        private List<Operation> operations = new List<Operation>();

        /// <inheritdoc/>
        public override double RelativeValue
        {
            get { return (Value - MinValue) / (MaxValue - MinValue); }
            set { Value = MinValue + value * (MaxValue - MinValue); }
        }

        /// <summary>
        /// Mittari, joka mittaa double-tyyppisiä arvoja.
        /// </summary>
        /// <param name="defaultValue">Oletusarvo</param>
        public DoubleMeter(double defaultValue)
            : base(defaultValue, 0.0, double.MaxValue)
        {
        }

        /// <summary>
        /// Mittari, joka mittaa double-tyyppisiä arvoja.
        /// </summary>
        /// <param name="defaultValue">Oletusarvo</param>
        /// <param name="minValue">Minimiarvo</param>
        /// <param name="maxValue">Maksimiarvo</param>
        public DoubleMeter(double defaultValue, double minValue, double maxValue)
            : base(defaultValue, minValue, maxValue)
        {
        }

        /// <summary>
        /// Antaa mittariolion <c>m</c> arvon, kun mittaria käytetään
        /// sellaisessa yhteydessä, jossa vaaditaan tavallista <c>double</c>-
        /// tyyppistä liukulukua.
        /// </summary>
        public static implicit operator double(DoubleMeter m)
        {
            return m.Value;
        }

        /// <summary>
        /// Lisää jotain mittarin arvoon. Sama kuin Value-ominaisuuteen lisääminen,
        /// mutta helpompi käyttää tapahtumakäsittelijöissä.
        /// </summary>
        /// <param name="change">Lisättävä luku</param>
        public void AddValue(double change)
        {
            Value += change;
        }

        /// <summary>
        /// Kertoo mittarin arvon jollakin. Sama kuin Value-ominaisuuden kertominen,
        /// mutta helpompi käyttää tapahtumakäsittelijöissä.
        /// </summary>
        /// <param name="multiplier">Uusi arvo</param>
        public void MultiplyValue(double multiplier)
        {
            Value *= multiplier;
        }

        /// <summary>
        /// Lisää tietyn summan mittariin tasaisesti tietyn ajan sisällä.
        /// </summary>
        /// <param name="change">Kuinka paljon lisätään</param>
        /// <param name="seconds">Aika joka lisämiseen kuluu</param>
        /// <param name="onComplete">Aliohjelma, joka suoritetaan kun lisäys on valmis.</param>
        /// <returns>Operation-tyyppinen muuttuja, jolla voi hallita tapahtuvaa muutosta</returns>
        public Operation AddOverTime(double change, double seconds, Action onComplete)
        {
            Operation op = AddOverTime(change, seconds);
            op.Finished += onComplete;
            return op;
        }

        /// <summary>
        /// Lisää tietyn summan mittariin tasaisesti tietyn ajan sisällä.
        /// </summary>
        /// <param name="change">Kuinka paljon lisätään</param>
        /// <param name="seconds">Aika joka lisämiseen kuluu</param>
        /// <returns>Operation-tyyppinen muuttuja, jolla voi hallita tapahtuvaa muutosta</returns>
        public Operation AddOverTime(double change, double seconds)
        {
            Operation op = new DoubleMeterAddOperation(this, change, seconds);
            op.Finished += delegate
            { operations.Remove(op); };
            op.Stopped += delegate
            { operations.Remove(op); };
            operations.Add(op);
            return op;
        }

        /// <summary>
        /// Pysäyttää AddOverTime-metodilla tehtävät lisäykset mittariin.
        /// </summary>
        public void Stop()
        {
            while (operations.Count > 0)
                operations[0].Stop();
        }

        internal override double GetValue()
        {
            return Value;
        }

        internal override double GetMinValue()
        {
            return MinValue;
        }

        internal override double GetMaxValue()
        {
            return MaxValue;
        }
    }
}

