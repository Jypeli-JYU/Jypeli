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
 * Authors: Tomi Karppinen
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Jypeli
{
    /// <summary>
    /// Parhaiden pisteiden lista.
    /// </summary>
    [Save]
    public class ScoreList : INotifyList<ScoreItem>
    {
        [Save]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ScoreItem[] _scores;

        [Save]
        public string LastEnteredName = "";

        /// <summary>
        /// Kuinka monta nimeä listalle mahtuu.
        /// </summary>
        public int Count
        {
            get { return _scores.Length; }
        }

        public IEnumerator<ScoreItem> GetEnumerator()
        {
            int i = 0;
            while ( i < _scores.Length )
            {
                yield return _scores[i++];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Nimet ja pisteet. Indeksointi alkaa yhdestä.
        /// <example>
        /// string ykkosenNimi = Lista[1].Name;
        /// double ykkosenPisteet = Lista[1].Score;
        /// </example>
        /// </summary>
        /// <param name="position">Sijoitus listalla.</param>
        /// <returns></returns>
        public ScoreItem this[int position]
        {
            get { return _scores[position - 1]; }
            set
            {
                for ( int i = _scores.Length - 1; i > position - 1; i-- )
                    _scores[i] = _scores[i - 1];
                _scores[position - 1] = value;

                for ( int i = 0; i < _scores.Length; i++ )
                    _scores[i].Position = i + 1;

                OnChanged();
            }
        }

        /// <summary>
        /// Käänteinen järjestys, ts. pienempi tulos on parempi.
        /// </summary>
        public bool Reverse { get; private set; }

        /// <summary>
        /// Tapahtuu kun listan sisältö muuttuu.
        /// </summary>
        public event Action Changed;

        private void OnChanged()
        {
            if ( Changed != null )
                Changed();
        }

        /// <summary>
        /// Luo tyhjän, 10 sijan top-listan.
        /// </summary>
        public ScoreList()
            : this( 10, false, 0 )
        {
        }

        /// <summary>
        /// Luo uuden, tyhjän top-listan.
        /// </summary>
        /// <param name="length">Kuinka monta nimeä listalla voi olla enintään.</param>
        /// <param name="reverse">Käänteinen järjestys (false = suurempi tulos parempi, true = pienempi tulos parempi).</param>
        /// <param name="baseScore">Pohjatulos, jota parempi hyväksyttävän tuloksen on oltava.</param>
        /// <param name="defaultName">Oletusnimi tyhjille paikoille.</param>
        public ScoreList( int length, bool reverse, double baseScore, string defaultName )
        {
            if ( length < 0 ) throw new ArgumentException( "List length must be more than zero!" );
            _scores = new ScoreItem[length];
            Reverse = reverse;
            
            ScoreItem zeroItem = new ScoreItem( defaultName, baseScore );
            for ( int i = 0; i < length; i++ )
            {
                _scores[i] = zeroItem;
                _scores[i].Position = i + 1;
            }
        }

        /// <summary>
        /// Luo uuden, tyhjän top-listan.
        /// </summary>
        /// <param name="length">Kuinka monta nimeä listalla voi olla enintään.</param>
        /// <param name="reverse">Käänteinen järjestys (false = suurempi tulos parempi, true = pienempi tulos parempi).</param>
        /// <param name="baseScore">Pohjatulos, jota parempi hyväksyttävän tuloksen on oltava.</param>
        public ScoreList(int length, bool reverse, double baseScore)
             : this(length, reverse, baseScore, "-")
        {
        }

        public override int GetHashCode()
        {
            return _scores.Length;
        }

        /// <summary>
        /// Tarkistaa, onko kaksi listaa yhtäsuuret.
        /// </summary>
        /// <param name="obj">Toinen lista</param>
        /// <returns></returns>
        public override bool Equals( object obj )
        {
            var other = obj as ScoreList;
            if ( other == null || this.Count != other.Count ) return false;

            for ( int i = 0; i < _scores.Length; i++ )
            {
                if ( !_scores[i].Equals( other._scores[i] ) )
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tarkistaa, kelpaako tulos listalle.
        /// </summary>
        /// <param name="score">Tulos</param>
        /// <returns>true jos tulos riitää listalle pääsemiseksi, false jos ei.</returns>
        public bool Qualifies( double score )
        {
            return Reverse && score < _scores[_scores.Length - 1].Score ||
                    !Reverse && score > _scores[_scores.Length - 1].Score;
        }

        /// <summary>
        /// Lisää nimen ja pisteet listalle, jos tulos on tarpeeksi hyvä.
        /// </summary>
        /// <param name="name">Nimi.</param>
        /// <param name="score">Pisteet.</param>
        /// <returns>Pistesija, tai -1 jos tulos ei ole riittävä listalle pääsemiseksi.</returns>
        public int Add( string name, double score )
        {
            if ( !Qualifies( score ) )
                return -1;

            LastEnteredName = name;

            for ( int i = 1; i <= Count; i++ )
            {
                if ( !Reverse && score > this[i].Score || Reverse && score < this[i].Score )
                {
                    this[i] = new ScoreItem( name, score );
                    return i + 1;
                }
            }

            throw new InvalidOperationException( "Internal error in HighScoreList!" );
        }
    }

    /// <summary>
    /// Nimi ja pisteet.
    /// </summary>
    [Save]
    public struct ScoreItem
    {
        public int Position;

        /// <summary>
        /// Nimi
        /// </summary>
        [Save]
        public string Name;

        /// <summary>
        /// Pistemäärä
        /// </summary>
        [Save]
        public double Score;

        /// <summary>
        /// Luo uuden sijoituksen listalle.
        /// </summary>
        /// <param name="name">Nimi</param>
        /// <param name="score">Pistemäärä.</param>
        public ScoreItem( string name, double score )
        {
            Position = -1;
            Name = name;
            Score = score;
        }

        public override bool Equals( object obj )
        {
            if ( !( obj is ScoreItem ) )
                return false;

            var other = (ScoreItem)obj;
            return this.Name == other.Name && Math.Abs( this.Score - other.Score ) <= float.Epsilon;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + Score.GetHashCode();
        }
    }
}
