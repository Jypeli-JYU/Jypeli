using System;
using System.Collections.Generic;
using System.Linq;

namespace Jypeli
{
    public abstract class AbstractTileMap<TileType>
    {
        public delegate void TileMethod(Vector position, double width, double height);
        public delegate void TileMethod<T1>(Vector position, double width, double height, T1 p1);
        public delegate void TileMethod<T1, T2>(Vector position, double width, double height, T1 p1, T2 p2);
        public delegate void TileMethod<T1, T2, T3>(Vector position, double width, double height, T1 p1, T2 p2, T3 p3);
        public delegate void TileMethod<T1, T2, T3, T4>(Vector position, double width, double height, T1 p1, T2 p2, T3 p3, T4 p4);
        public delegate void TileMethod<T1, T2, T3, T4, T5>(Vector position, double width, double height, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
        public delegate void TileMethod<T1, T2, T3, T4, T5, T6>(Vector position, double width, double height, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);

        public delegate void RouteMethod(List<Vector> route, double width, double height);

        struct TileMethodCall
        {
            public TileMethod method;
            public int row;
            public int col;

            public TileMethodCall(TileMethod m, int row, int col)
            {
                this.method = m;
                this.row = row;
                this.col = col;
            }

            public void Invoke(double tileWidth, double tileHeight)
            {
                double realX = Game.Instance.Level.Left + (col * tileWidth) + (tileWidth / 2);
                double realY = Game.Instance.Level.Top - (row * tileHeight) - (tileHeight / 2);
                method(new Vector(realX, realY), tileWidth, tileHeight);
            }
        }

        private List<TileMethodCall> optimized = new List<TileMethodCall>();
        protected Dictionary<TileType, TileMethod> legend = new Dictionary<TileType, TileMethod>();
        protected TileType[,] tiles;

        protected abstract TileType Null { get; }

        public AbstractTileMap(TileType[,] tiles)
        {
            if (tiles.GetLength(0) == 0 || tiles.GetLength(1) == 0)
                throw new ArgumentException("All dimensions of tiles must be at least 1");

            this.tiles = tiles;
        }

        /// <summary>
        /// Rivien määrä kentässä (pystysuoraan).
        /// </summary>
        public int RowCount
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Sarakkeiden määrä kentässä (vaakasuoraan).
        /// </summary>
        public int ColumnCount
        {
            get { return tiles.GetLength(1); }
        }

        /// <summary>
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <param name="tileSymbol">Merkki</param>
        /// <param name="f">Aliohjelma muotoa void LuoOlio(Vector paikka, double leveys, double korkeus)</param>
        public void SetTileMethod(TileType tileSymbol, TileMethod f)
        {
            legend[tileSymbol] = f;
        }

        /// <summary>
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <typeparam name="T1">Parametrin tyyppi</typeparam>
        /// <param name="tileSymbol">Merkki</param>
        /// <param name="f">Aliohjelma muotoa void LuoOlio(Vector paikka, double leveys, double korkeus)</param>
        /// <param name="p1">Parametri</param>
        public void SetTileMethod<T1>(TileType tileSymbol, TileMethod<T1> f, T1 p1)
        {
            legend[tileSymbol] = delegate (Vector p, double w, double h) { f(p, w, h, p1); };
        }

        /// <summary>
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen parametrin tyyppi</typeparam>
        /// <param name="tileSymbol">Merkki</param>
        /// <param name="f">Aliohjelma muotoa void LuoOlio(Vector paikka, double leveys, double korkeus)</param>
        /// <param name="p1">Ensimmäinen parametri</param>
        /// <param name="p2">Toinen parametri</param>
        public void SetTileMethod<T1, T2>(TileType tileSymbol, TileMethod<T1, T2> f, T1 p1, T2 p2)
        {
            legend[tileSymbol] = delegate (Vector p, double w, double h) { f(p, w, h, p1, p2); };
        }

        /// <summary>
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen parametrin tyyppi</typeparam>
        /// <typeparam name="T3">Kolmannen parametrin tyyppi</typeparam>
        /// <param name="tileSymbol">Merkki</param>
        /// <param name="f">Aliohjelma muotoa void LuoOlio(Vector paikka, double leveys, double korkeus)</param>
        /// <param name="p1">Ensimmäinen parametri</param>
        /// <param name="p2">Toinen parametri</param>
        /// <param name="p3">Kolmas parametri</param>
        public void SetTileMethod<T1, T2, T3>(TileType tileSymbol, TileMethod<T1, T2, T3> f, T1 p1, T2 p2, T3 p3)
        {
            legend[tileSymbol] = delegate (Vector p, double w, double h) { f(p, w, h, p1, p2, p3); };
        }

        /// <summary>
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen parametrin tyyppi</typeparam>
        /// <typeparam name="T3">Kolmannen parametrin tyyppi</typeparam>
        /// <typeparam name="T4">Neljännen parametrin tyyppi</typeparam>
        /// <param name="tileSymbol">Merkki</param>
        /// <param name="f">Aliohjelma muotoa void LuoOlio(Vector paikka, double leveys, double korkeus)</param>
        /// <param name="p1">Ensimmäinen parametri</param>
        /// <param name="p2">Toinen parametri</param>
        /// <param name="p3">Kolmas parametri</param>
        /// <param name="p4">Neljäs parametri</param>
        public void SetTileMethod<T1, T2, T3, T4>(TileType tileSymbol, TileMethod<T1, T2, T3, T4> f, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            legend[tileSymbol] = delegate (Vector p, double w, double h) { f(p, w, h, p1, p2, p3, p4); };
        }

        /// <summary>
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen parametrin tyyppi</typeparam>
        /// <typeparam name="T3">Kolmannen parametrin tyyppi</typeparam>
        /// <typeparam name="T4">Neljännen parametrin tyyppi</typeparam>
        /// <typeparam name="T5">Viidennen parametrin tyyppi</typeparam>
        /// <param name="tileSymbol">Merkki</param>
        /// <param name="f">Aliohjelma muotoa void LuoOlio(Vector paikka, double leveys, double korkeus)</param>
        /// <param name="p1">Ensimmäinen parametri</param>
        /// <param name="p2">Toinen parametri</param>
        /// <param name="p3">Kolmas parametri</param>
        /// <param name="p4">Neljäs parametri</param>
        /// <param name="p5">Viides parametri</param>
        public void SetTileMethod<T1, T2, T3, T4, T5>(TileType tileSymbol, TileMethod<T1, T2, T3, T4, T5> f, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            legend[tileSymbol] = delegate (Vector p, double w, double h) { f(p, w, h, p1, p2, p3, p4, p5); };
        }

        /// <summary>
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <typeparam name="T1">Ensimmäisen parametrin tyyppi</typeparam>
        /// <typeparam name="T2">Toisen parametrin tyyppi</typeparam>
        /// <typeparam name="T3">Kolmannen parametrin tyyppi</typeparam>
        /// <typeparam name="T4">Neljännen parametrin tyyppi</typeparam>
        /// <typeparam name="T5">Viidennen parametrin tyyppi</typeparam>
        /// <typeparam name="T6">Kuudennen parametrin tyyppi</typeparam>
        /// <param name="tileSymbol">Merkki</param>
        /// <param name="f">Aliohjelma muotoa void LuoOlio(Vector paikka, double leveys, double korkeus)</param>
        /// <param name="p1">Ensimmäinen parametri</param>
        /// <param name="p2">Toinen parametri</param>
        /// <param name="p3">Kolmas parametri</param>
        /// <param name="p4">Neljäs parametri</param>
        /// <param name="p5">Viides parametri</param>
        /// <param name="p6">Kuudes parametri</param>
        public void SetTileMethod<T1, T2, T3, T4, T5, T6>(TileType tileSymbol, TileMethod<T1, T2, T3, T4, T5, T6> f, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
        {
            legend[tileSymbol] = delegate (Vector p, double w, double h) { f(p, w, h, p1, p2, p3, p4, p5, p6); };
        }

        /// <summary>
        /// Kokoaa reitin useammasta ruutukentän symbolista.
        /// 
        /// Määrittää, että tietyn ruutukentän symbolin (<c>tileSymbol</c>) kohdalla
        /// kutsutaan aliohjelmaa <c>f</c>. Huom! Käytä tämän aliohjelman kanssa metodia
        /// Execute.
        /// </summary>
        /// <param name="f">Aliohjelma, muotoa void LuoReittiolio(List&lt;Vector&gt; reitti, double leveys, double korkeus)</param>
        /// <param name="tileSymbols">Ruutukentän symbolit tiedostossa joista reitti muodostuu</param>
        public void SetRouteMethod(RouteMethod f, params TileType[] tileSymbols)
        {
            if (tileSymbols == null || tileSymbols.Length < 1)
                throw new ArgumentException("Pass at least one tile symbol!");

            Vector[] vectorTable = new Vector[tileSymbols.Length];
            int vectorsAdded = 0;

            for (int i = 0; i < tileSymbols.Length; i++)
            {
                int index = i;
                legend[tileSymbols[i]] = delegate (Vector p, double w, double h)
                {
                    vectorTable[index] = p;
                    if (++vectorsAdded >= tileSymbols.Length)
                        f(vectorTable.ToList(), w, h);
                };
            }
        }

        /// <summary>
        /// Käy kentän kaikki merkit läpi ja kutsuu <c>SetTileMethod</c>-metodilla annettuja
        /// aliohjelmia kunkin merkin kohdalla.
        /// </summary>
        /// <remarks>
        /// Aliohjelmassa voi esimerkiksi luoda olion ruudun kohdalle.
        /// </remarks>
        public void Execute()
        {
            double h = Game.Instance.Level.Height / tiles.GetLength(0);
            double w = Game.Instance.Level.Width / tiles.GetLength(1);
            Execute(w, h);
        }

        /// <summary>
        /// Käy kentän kaikki merkit läpi ja kutsuu <c>SetTileMethod</c>-metodilla annettuja
        /// aliohjelmia kunkin merkin kohdalla.
        /// </summary>
        /// <remarks>
        /// Aliohjelmassa voi esimerkiksi luoda olion ruudun kohdalle.
        /// </remarks>
        /// <param name="tileWidth">Yhden ruudun leveys.</param>
        /// <param name="tileHeight">Yhden ruudun korkeus.</param>
        public void Execute(double tileWidth, double tileHeight)
        {
            Game game = Game.Instance;
            int width = tiles.GetLength(1);
            int height = tiles.GetLength(0);

            game.Level.Width = width * tileWidth;
            game.Level.Height = height * tileHeight;

            foreach (var m in optimized)
            {
                m.Invoke(tileWidth, tileHeight);
            }

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    TileMethod method = GetMethodForSymbol(tiles[y, x]);

                    if (method != null)
                    {
                        double realX = game.Level.Left + (x * tileWidth) + (tileWidth / 2);
                        double realY = game.Level.Top - (y * tileHeight) - (tileHeight / 2);
                        method(new Vector(realX, realY), tileWidth, tileHeight);
                    }
                }
            }
        }

        protected TileMethod GetMethodForSymbol(TileType symbol)
        {
            if (symbol.Equals(Null)) return null;

            foreach (var key in legend.Keys)
            {
                if (SymbolEquals(key, symbol))
                    return legend[key];
            }

            return null;
        }

        protected virtual bool SymbolEquals(TileType a, TileType b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Optimoi annetut ruudut niin, että useammat vierekkäiset oliot yhdistetään
        /// isommiksi olioiksi. Älä käytä esim. kerättäville esineille.
        /// </summary>
        /// <param name="symbols">Optimoitavat symbolit</param>
        public void Optimize(params TileType[] symbols)
        {
            for (int i = 0; i < symbols.Length; i++)
                Optimize(symbols[i]);
        }

        /// <summary>
        /// Optimoi annetut ruudut niin, että useammat vierekkäiset oliot yhdistetään
        /// isommiksi olioiksi. Älä käytä esim. kerättäville esineille.
        /// </summary>
        /// <param name="sym">Optimoitava symboli</param>
        public void Optimize(TileType sym)
        {
            TileMethod method = GetMethodForSymbol(sym);

            if (method == null)
                throw new ArgumentException("Symbol " + sym + " not added, cannot optimize.");

            for (int row = 0; row < RowCount; row++)
            {
                for (int col = 0; col < ColumnCount; col++)
                {
                    int w, h;
                    GetGreatestMatchingRectangle(sym, row, col, out w, out h);

                    if (w > 0 && h > 0)
                    {
                        TileMethod newMethod = delegate (Vector oldPos, double oldWidth, double oldHeight)
                        {
                            Vector oldAdjust = new Vector(-oldWidth, oldHeight);
                            Vector newAdjust = new Vector(oldWidth * w, -oldHeight * h);
                            Vector newPos = oldPos + (oldAdjust + newAdjust) / 2;
                            method(newPos, oldWidth * w, oldHeight * h);
                        };
                        optimized.Add(new TileMethodCall(newMethod, row, col));
                        SetArea(row, col, w, h, Null);
                        col += w;
                    }
                }
            }
        }

        private void GetGreatestMatchingRectangle(TileType sym, int row, int col, out int width, out int height)
        {
            for (width = 0; width < ColumnCount - col; width++)
            {
                if (!SymbolEquals(tiles[row, col + width], sym))
                    break;
            }

            if (width == 0)
            {
                height = 0;
                return;
            }

            for (height = 1; height < RowCount - row; height++)
            {
                if (!RowEquals(row + height, col, width, sym))
                    break;
            }
        }

        private bool RowEquals(int row, int col, int length, TileType sym)
        {
            for (int i = col; i < col + length; i++)
            {
                if (!SymbolEquals(tiles[row, i], sym)) return false;
            }

            return true;
        }

        private void SetArea(int row, int col, int width, int height, TileType sym)
        {
            for (int j = row; j < row + height; j++)
            {
                for (int i = col; i < col + width; i++)
                {
                    tiles[j, i] = sym;
                }
            }
        }

        /// <summary>
        /// Palauttaa annetun dimension pituuden (merkkeinä, ei pikseleinä).
        /// </summary>
        /// <param name="dimension">Dimensio. 0 antaa kentän korkeuden, 1 leveyden.</param>
        /// <returns>Annetun dimension koko</returns>
        public int GetLength(int dimension)
        {
            return tiles.GetLength(dimension);
        }

        /// <summary>
        /// Palauttaa ruudussa olevan symbolin.
        /// </summary>
        /// <param name="row">Rivi</param>
        /// <param name="col">Sarake</param>
        /// <returns>symbolin</returns>
        public TileType GetTile(int row, int col)
        {
            return tiles[row, col];
        }

        /// <summary>
        /// Asettaa ruudussa olevan symbolin.
        /// </summary>
        /// <param name="row">Rivi</param>
        /// <param name="col">Sarake</param>
        /// <param name="c">Uusi merkki</param>
        /// <returns>symbolin</returns>
        public void SetTile(int row, int col, TileType c)
        {
            tiles[row, col] = c;
        }

        /// <summary>
        /// Muuttaa luontialiohjelman tekemän olion kokoa.
        /// </summary>
        /// <param name="m">Luontialiohjelma</param>
        /// <param name="newWidth">Uusi leveys oliolle</param>
        /// <param name="newHeight">Uusi korkeus oliolle</param>
        /// <returns></returns>
        public static TileMethod ChangeSize(TileMethod m, double newWidth, double newHeight)
        {
            return new TileMethod(delegate (Vector p, double w, double h) { m(p, newWidth, newHeight); });
        }

        /// <summary>
        /// Muuttaa luontialiohjelman tekemän olion kokoa tietyllä kertoimilla.
        /// </summary>
        /// <param name="m">Luontialiohjelma</param>
        /// <param name="widthMultiplier">Kerroin alkuperäiselle leveydelle</param>
        /// <param name="heightMultiplier">Kerroin alkuperäiselle korkeudelle</param>
        /// <returns></returns>
        public static TileMethod ChangeSizeMultiplier(TileMethod m, double widthMultiplier, double heightMultiplier)
        {
            return new TileMethod(delegate (Vector p, double w, double h) { m(p, w * widthMultiplier, h * heightMultiplier); });
        }
    }
}
