using System;
using System.Collections.Generic;
using System.IO;

namespace Jypeli
{
    /// <summary>
    /// J�rjestetty lista merkkijonoja.
    /// </summary>
    public class StringList : INotifyList<string>
    {
        List<string> strings = new List<string>();


        public IEnumerator<string> GetEnumerator()
        {
            return strings.GetEnumerator();
        }

        /*
         * Since IEnumerable<T> inherits from non-generic IEnumerable (which is pretty odd...),
         * we have to provide this GetEnumerator() method as well.
         */
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        /// <summary>
        /// Merkkijono listassa.
        /// </summary>
        /// <param name="index">Merkkijonon indeksi</param>
        /// <returns>Merkkijono</returns>
        public string this[int index]
        {
            get { return strings[index]; }
            set { strings[index] = value; }
        }

        /// <summary>
        /// Jatkaa listaa oliolla, joka voi olla toinen lista, toinen merkkijono jne.
        /// </summary>
        /// <param name="a">Lista</param>
        /// <param name="b">Olio.</param>
        /// <returns>Lista jatkettuna oliolla.</returns>
        public static StringList operator +( StringList a, object b )
        {
            return new StringList( a ).Add( b );
        }

        /// <summary>
        /// Jatkaa oliota listalla. Olio voi olla toinen lista, toinen merkkijono jne.
        /// </summary>
        /// <param name="a">Olio</param>
        /// <param name="b">Lista.</param>
        /// <returns>Olio jatkettuna listalla.</returns>
        public static StringList operator +( object a, StringList b )
        {
            return new StringList( a ).Add( b );
        }

        /// <summary>
        /// Muuntaa merkkijonolistan implisiittisesti listaksi merkkijonoja.
        /// </summary>
        /// <param name="list">Merkkijonolista.</param>
        /// <returns>Lista merkkijonoja.</returns>
        public static implicit operator List<String>( StringList list )
        {
            return list.strings;
        }

        /// <summary>
        /// Muuntaa listan merkkijonoja implisiittisesti merkkijonolistaksi.
        /// </summary>
        /// <param name="list">Lista merkkijonoja.</param>
        /// <returns>Merkkijonolista.</returns>
        public static implicit operator StringList( List<String> list )
        {
            return new StringList( list );
        }

        /// <summary>
        /// Muuntaa merkkijonolistan implisiittisesti taulukoksi merkkijonoja.
        /// </summary>
        /// <param name="list">Merkkijonolista.</param>
        /// <returns>Taulukko merkkijonoja.</returns>
        public static implicit operator String[]( StringList list )
        {
            return list.strings.ToArray();
        }

        /// <summary>
        /// Muuntaa taulukon merkkijonoja implisiittisesti merkkijonolistaksi.
        /// </summary>
        /// <param name="array">Taulukko merkkijonoja.</param>
        /// <returns>Merkkijonolista.</returns>
        public static implicit operator StringList( String[] array )
        {
            return new StringList( array );
        }

        /// <summary>
        /// Voiko listaa vain lukea, ei kirjoittaa.
        /// </summary>
        public bool IsReadOnly { get { return false; } }

        /// <summary>
        /// Listan pituus.
        /// Jos asetetaan pienemm�ksi kuin nykyinen koko, ylimenev�t rivit poistetaan.
        /// Jos asetetaan suuremmaksi kuin nykyinen koko, lis�t��n tyhji� rivej�.
        /// </summary>
        public int Length
        {
            get { return strings.Count; }
            set
            {
                if ( value < strings.Count )
                {
                    strings.RemoveRange( value, strings.Count - value );
                }

                else if ( value > strings.Count )
                {
                    for ( int i = value; i < strings.Count; i++ )
                        strings.Add( "" );
                }
            }
        }

        /// <summary>
        /// Listan pituus.
        /// Jos asetetaan pienemm�ksi kuin nykyinen koko, ylimenev�t rivit poistetaan.
        /// Jos asetetaan suuremmaksi kuin nykyinen koko, lis�t��n tyhji� rivej�.
        /// </summary>
        public int Count { get { return Length; } /*set { Length = value; }*/ }

        /// <summary>
        /// Tapahtuu kun listan sis�lt� muuttuu.
        /// </summary>
        public event Action Changed;

        private void OnChanged()
        {
            if ( Changed != null )
                Changed();
        }

        /// <summary>
        /// Luo uuden tyhj�n merkkijonolistan.
        /// </summary>
        public StringList()
        {
        }

        /// <summary>
        /// Luo uuden merkkijonolistan olemassaolevan kopiona.
        /// </summary>
        /// <param name="source">Olemassaoleva merkkijonolista.</param>
        public StringList( StringList source )
        {
            strings.AddRange( source.strings );
        }

        /// <summary>
        /// Luo uuden merkkijonolistan olemassaolevan kopiona.
        /// </summary>
        /// <param name="source">Olemassaoleva merkkijonolista.</param>
        public StringList( List<string> source )
        {
            strings.AddRange( source );
        }

        /// <summary>
        /// Luo uuden merkkijonolistan oliosta.
        /// </summary>
        /// <param name="source">Olio.</param>
        public StringList( object source )
        {
            Add( source );
        }

        /// <summary>
        /// Luo uuden merkkijonolistan taulukosta tai parametrina
        /// annetuista merkkijonoista.
        /// </summary>
        /// <param name="source">Merkkijonot taulukkona tai parametreina.</param>
        public StringList( params string[] source )
        {
            strings.AddRange( source );
        }

#if JYPELI
        /// <summary>
        /// Lukee merkkijonolistan Content-projektin tekstitiedostosta.
        /// </summary>
        /// <param name="assetName">Tiedoston nimi</param>        
        public static StringList FromAsset( string assetName )
        {
            return new StringList( Game.Instance.Content.Load<string[]>( assetName ) );
        }
#endif

        /// <summary>
        /// Lukee merkkijonolistan tietovirrasta.
        /// </summary>
        /// <param name="stream">Luettava virta.</param>
        internal StringList AssignFrom( Stream stream )
        {
            using ( StreamReader input = new StreamReader( stream ) )
            {
                string line;
                while ( ( line = input.ReadLine() ) != null )
                {
                    strings.Add( line );
                }
            }

            OnChanged();
            return this;
        }

#if !WINDOWS_STOREAPP
        /// <summary>
        /// Lukee merkkijonolistan tiedostosta.
        /// Huom. toimii vain PC:ll�, k�yt� mieluummin
        /// FromAsset-metodia jos vain mahdollista.
        /// </summary>
        /// <param name="path">Tiedoston polku.</param>
        public static StringList FromFile( string path )
        {
            StringList result = new StringList();

            using ( StreamReader input = File.OpenText( path ) )
            {
                string line;
                while ( ( line = input.ReadLine() ) != null )
                {
                    result.Add( line );
                }
            }

            return result;
        }
#endif

        /// <summary>
        /// Lis�� yhden tai useamman rivin merkkijonolistaan.
        /// </summary>
        /// <param name="lines">Rivi(t)</param>
        /// <returns>Lista itse</returns>
        public StringList Add( params string[] lines )
        {
            strings.AddRange( lines );
            OnChanged();
            return this;
        }

        /// <summary>
        /// Lis�� toisen merkkijonolistan t�m�n per��n.
        /// </summary>
        /// <param name="list">Merkkijonolista</param>
        /// <returns>Lista itse</returns>
        public StringList Add( StringList list )
        {
            strings.AddRange( list.strings );
            OnChanged();
            return this;
        }

        /// <summary>
        /// Lis�� toisen merkkijonolistan t�m�n per��n.
        /// </summary>
        /// <param name="list">Lista merkkijonoja.</param>
        /// <returns>Lista itse</returns>
        public StringList Add( List<String> list )
        {
            strings.AddRange( list );
            OnChanged();
            return this;
        }

        /// <summary>
        /// Lis�� olion merkkijonolistan per��n.
        /// </summary>
        /// <param name="obj">Olio.</param>
        /// <returns>Lista itse</returns>
        public StringList Add( object obj )
        {
            if ( obj is StringList ) return this.Add( (StringList)obj );
            if ( obj is ICollection<object> )
            {
                foreach ( object element in (ICollection<object>)obj )
                {
                    this.Add( element );
                }
            }

            return this.Add( obj.ToString() );
        }

        /// <summary>
        /// Poistaa yhden tai useamman rivin.
        /// Kaikki rivin ilmentym�t poistetaan.
        /// </summary>
        /// <param name="lines">Poistettava(t) rivi(t)</param>
        /// <returns>Lista itse</returns>
        public StringList RemoveAll( params string[] lines )
        {
            for ( int i = 0; i < lines.Length; i++ )
            {
                while ( strings.Remove( lines[i] ) ) ;
            }

            OnChanged();
            return this;
        }

        /// <summary>
        /// Poistaa listassa m��ritellyt rivit.
        /// Kaikki rivin ilmentym�t poistetaan.
        /// </summary>
        /// <param name="list">Lista joka sis�lt�� poistettavat rivit</param>
        /// <returns>Lista itse</returns>
        public StringList RemoveAll( List<string> list )
        {
            for ( int i = 0; i < list.Count; i++ )
            {
                while ( strings.Remove( list[i] ) ) ;
            }

            OnChanged();
            return this;
        }

        /// <summary>
        /// Poistaa toisessa listassa m��ritellyt rivit.
        /// Kaikki rivin ilmentym�t poistetaan.
        /// </summary>
        /// <param name="list">Lista joka sis�lt�� poistettavat rivit</param>
        /// <returns>Lista itse</returns>
        public StringList RemoveAll( StringList list )
        {
            return RemoveAll( list.strings );
        }

        /// <summary>
        /// Poistaa yhden tai useamman rivin.
        /// Vain ensimm�inen ilmentym� poistetaan.
        /// </summary>
        /// <param name="lines">Poistettava(t) rivi(t)</param>
        /// <returns>Lista itse</returns>
        public StringList RemoveFirst( params string[] lines )
        {
            for ( int i = 0; i < lines.Length; i++ )
            {
                strings.Remove( lines[i] );
            }

            OnChanged();
            return this;
        }

        /// <summary>
        /// Poistaa listassa m��ritellyt rivit.
        /// Vain ensimm�inen ilmentym� poistetaan.
        /// </summary>
        /// <param name="list">Lista joka sis�lt�� poistettavat rivit</param>
        /// <returns>Lista itse</returns>
        public StringList RemoveFirst( List<string> list )
        {
            for ( int i = 0; i < list.Count; i++ )
            {
                strings.Remove( list[i] );
            }

            OnChanged();
            return this;
        }

        /// <summary>
        /// Poistaa toisessa listassa m��ritellyt rivit.
        /// Vain ensimm�inen ilmentym� poistetaan.
        /// </summary>
        /// <param name="list">Lista joka sis�lt�� poistettavat rivit</param>
        /// <returns>Lista itse</returns>
        public StringList RemoveFirst( StringList list )
        {
            return RemoveFirst( list.strings );
        }

        /// <summary>
        /// Poistaa yhden tai useamman rivin.
        /// Vain viimeinen ilmentym� poistetaan.
        /// </summary>
        /// <param name="lines">Poistettava(t) rivi(t)</param>
        /// <returns>Lista itse</returns>
        public StringList RemoveLast( params string[] lines )
        {
            for ( int i = lines.Length - 1; i >= 0; i-- )
            {
                strings.Remove( lines[i] );
            }

            OnChanged();
            return this;
        }

        /// <summary>
        /// Poistaa listassa m��ritellyt rivit.
        /// Vain ensimm�inen ilmentym� poistetaan.
        /// </summary>
        /// <param name="list">Lista joka sis�lt�� poistettavat rivit</param>
        /// <returns>Lista itse</returns>
        public StringList RemoveLast( List<string> list )
        {
            for ( int i = list.Count - 1; i >= 0; i-- )
            {
                strings.Remove( list[i] );
            }

            OnChanged();
            return this;
        }

        /// <summary>
        /// Poistaa toisessa listassa m��ritellyt rivit.
        /// Vain ensimm�inen ilmentym� poistetaan.
        /// </summary>
        /// <param name="list">Lista joka sis�lt�� poistettavat rivit</param>
        /// <returns>Lista itse</returns>
        public StringList RemoveLast( StringList list )
        {
            return RemoveLast( list.strings );
        }

        /// <summary>
        /// Tarkistaa, l�ytyyk� rivi listasta.
        /// </summary>
        /// <param name="line">Etsitt�v� rivi.</param>
        /// <returns>true jos l�ytyy</returns>
        public bool Contains( string line )
        {
            for ( int i = 0; i < strings.Count; i++ )
            {
                if ( strings[i] == line ) return true;
            }

            return false;
        }

        /// <summary>
        /// Palauttaa listasta ensimm�isen annetulla merkkijonolla
        /// alkavan merkkijonon.
        /// </summary>
        /// <param name="line">Etsitt�v�n rivin alku.</param>
        /// <returns>L�ydetty merkkijono tai null jos ei l�ytynyt.</returns>
        public string FirstBeginningWith( string line )
        {
            for ( int i = 0; i < strings.Count; i++ )
            {
                if ( strings[i].Length < line.Length ) continue;
                if ( strings[i].Substring( 0, line.Length ) == line ) return strings[i];
            }

            return null;
        }

        /// <summary>
        /// Tyhjent�� listan.
        /// </summary>
        public void Clear()
        {
            strings.Clear();
            OnChanged();
        }
    }
}
