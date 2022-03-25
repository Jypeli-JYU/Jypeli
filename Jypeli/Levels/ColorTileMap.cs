using System;

namespace Jypeli
{
    /// <summary>
    /// Ruutukartta, jonka avulla olioita voidaan helposti asettaa tasavälein ruudukkoon.
    /// Ruutukartta koostuu kirjoitusmerkeistä (<c>char</c>), joihin voi liittää
    /// aliohjelman, joka luo merkkiä vastaavan olion.
    /// </summary>
    public class ColorTileMap : AbstractTileMap<Color>
    {
        private double _tolerance = 30;

        /// <inheritdoc/>
        protected override Color Null
        {
            get { return Color.Transparent; }
        }

        /// <summary>
        /// Väritoleranssi. Mitä pienempi toleranssi, sitä tarkemmin eri värit erotellaan toisistaan.
        /// Nollatoleranssilla värit on annettava tarkkoina rgb-koodeina, suuremmilla toleransseilla
        /// riittää "sinne päin".
        /// </summary>
        public double ColorTolerance
        {
            get { return _tolerance; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Tolerance must not be negative.");
                _tolerance = value;
            }
        }

        /// <summary>
        /// Luo uuden ruutukartan.
        /// </summary>
        /// <param name="img">Kuva, jossa jokainen pikseli vastaa oliota.</param>
        public ColorTileMap(Image img)
            : base(img.GetData())
        {
        }

        /// <summary>
        /// Luo uuden ruutukartan.
        /// </summary>
        /// <param name="assetName">Kuvatiedoston nimi.</param>
        public ColorTileMap(string assetName)
            : this(Game.LoadImage(assetName))
        {
        }

        /// <summary>
        /// Lukee ruutukentän Content-projektin kuvatiedostosta.
        /// </summary>
        /// <param name="assetName">Tiedoston nimi</param>        
        public static ColorTileMap FromLevelAsset(string assetName)
        {
            return new ColorTileMap(Game.LoadImage(assetName));
        }

        /// <summary>
        /// Vastaavatko alkiot toisiaan
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        protected virtual bool ItemEquals(Color a, Color b)
        {
            return (a.AlphaComponent == b.AlphaComponent && Color.Distance(a, b) <= ColorTolerance);
        }

        /// <summary>
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <param name="hexCode">Heksakoodi värille</param>
        /// <param name="method">Aliohjelma</param>
        public void SetTileMethod(string hexCode, TileMethod method)
        {
            SetTileMethod(Color.FromHexCode(hexCode), method);
        }

        /// <summary>
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <param name="hexCode">Heksakoodi värille</param>
        /// <param name="method">Aliohjelma</param>
        /// <param name="p1">Parametri</param>
        public void SetTileMethod<T1>(string hexCode, TileMethod<T1> method, T1 p1)
        {
            SetTileMethod(Color.FromHexCode(hexCode), method, p1);
        }

        /// <summary>
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <param name="hexCode">Heksakoodi värille</param>
        /// <param name="method">Aliohjelma</param>
        /// <param name="p1">Parametri</param>
        /// <param name="p2">Parametri</param>
        public void SetTileMethod<T1, T2>(string hexCode, TileMethod<T1, T2> method, T1 p1, T2 p2)
        {
            SetTileMethod(Color.FromHexCode(hexCode), method, p1, p2);
        }

        /// <summary>
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <param name="hexCode">Heksakoodi värille</param>
        /// <param name="method">Aliohjelma</param>
        /// <param name="p1">Parametri</param>
        /// <param name="p2">Parametri</param>
        /// <param name="p3">Parametri</param>
        public void SetTileMethod<T1, T2, T3>(string hexCode, TileMethod<T1, T2, T3> method, T1 p1, T2 p2, T3 p3)
        {
            SetTileMethod(Color.FromHexCode(hexCode), method, p1, p2, p3);
        }
    }
}
