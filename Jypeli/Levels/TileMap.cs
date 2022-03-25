using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Jypeli
{
    /// <summary>
    /// Ruutukartta, jonka avulla olioita voidaan helposti asettaa tasavälein ruudukkoon.
    /// Ruutukartta koostuu kirjoitusmerkeistä (<c>char</c>), joihin voi liittää
    /// aliohjelman, joka luo merkkiä vastaavan olion.
    /// </summary>
    public class TileMap : AbstractTileMap<char>
    {
        private Dictionary<char, Func<GameObject>> oldLegend = new Dictionary<char, Func<GameObject>>();
        private static string[] textExtensions = { ".txt", ".dat" };

        /// <inheritdoc/>
        protected override char Null
        {
            get { return ' '; }
        }

        /// <summary>
        /// Asettaa merkin vastaamaan aliohjelmaa, joka luo olion. Huom! Käytä tämän
        /// syntaksin kanssa metodia <c>Insert</c>.
        /// </summary>
        public Func<GameObject> this[char c]
        {
            get { return oldLegend[c]; }
            set { oldLegend[c] = value; }
        }

        /// <summary>
        /// Luo uuden ruutukartan.
        /// </summary>
        /// <param name="tiles">Kaksiulotteinen taulukko merkeistä.</param>
        public TileMap(char[,] tiles)
            : base(tiles)
        {
        }

        /// <summary>
        /// Lukee ruutukentän tiedostosta.
        /// </summary>
        /// <param name="path">Tiedoston polku.</param>
        public static TileMap FromFile(string path)
        {
            char[,] tiles = ReadFromFile(path);
            return new TileMap(tiles);
        }

        /// <summary>
        /// Lukee ruutukentän merkkijonotaulukosta.
        /// </summary>
        /// <param name="lines">Merkkijonotaulukko</param>        
        public static TileMap FromStringArray(string[] lines)
        {
            char[,] tiles = ReadFromStringArray(lines);
            return new TileMap(tiles);
        }

        /// <summary>
        /// Lukee ruutukentän Content-projektin tekstitiedostosta.
        /// </summary>
        /// <param name="assetName">Tiedoston nimi</param>        
        public static TileMap FromLevelAsset(string assetName)
        {
            char[,] tiles = ReadFromFile(assetName);
            return new TileMap(tiles);
        }

        /// <summary>
        /// Asettaa oliot kenttään aiemmin annettujen merkkien perusteella.
        /// </summary>
        public void Insert()
        {
            double h = Game.Instance.Level.Height / tiles.GetLength(0);
            double w = Game.Instance.Level.Width / tiles.GetLength(1);
            Insert(w, h);
        }

        /// <summary>
        /// Asettaa oliot kenttään aiemmin annettujen merkkien perusteella.
        /// </summary>
        /// <remarks>
        /// Huom! Tiilien asettaminen muuttaa kentän kokoa. Jos lisäät kenttään reunat
        /// tai zoomaat kameraa, tee se vasta tämän aliohjelman kutsun jälkeen.
        /// </remarks>
        /// <param name="tileWidth">Ruudun leveys.</param>
        /// <param name="tileHeight">Ruudun korkeus.</param>
        public void Insert(double tileWidth, double tileHeight)
        {
            Game game = Game.Instance;
            int width = tiles.GetLength(1);
            int height = tiles.GetLength(0);

            game.Level.Width = width * tileWidth;
            game.Level.Height = height * tileHeight;

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    char symbol = tiles[y, x];
                    if (oldLegend.ContainsKey(symbol))
                    {
                        Func<GameObject> create = oldLegend[symbol];
                        GameObject o = create();
                        o.X = game.Level.Left + (x * tileWidth) + (tileWidth / 2);
                        o.Y = game.Level.Top - (y * tileHeight) - (tileHeight / 2);
                        game.Add(o);
                    }
                }
            }
        }

        /// <summary>
        /// Lukee kentän ruudut tiedostosta.
        /// </summary>
        /// <param name="path">Tiedoston polku</param>
        /// <returns>Kentän ruudut kaksiulotteisessa taulukossa</returns>
        internal static char[,] ReadFromFile(string path)
        {
            var tileBuffer = new List<char[]>();

            using (StreamReader input = new StreamReader(Game.Device.StreamContent(path, textExtensions)))
            {
                string line;
                while ((line = input.ReadLine()) != null)
                {
                    tileBuffer.Add(line.ToCharArray());
                }
            }

            return ListToArray(tileBuffer);
        }

        internal static char[,] ReadFromStringArray(string[] lines)
        {
            var tileBuffer = new List<char[]>();

            for (int i = 0; i < lines.Length; i++)
            {

                tileBuffer.Add(lines[i].ToCharArray());

            }

            return ListToArray(tileBuffer);
        }

        private static char[,] ListToArray(List<char[]> list)
        {
            int finalWidth = list.Max(cs => cs.Length);

            char[,] tiles = new char[list.Count, finalWidth];

            for (int y = 0; y < list.Count; y++)
            {
                char[] row = list.ElementAt(y);

                for (int x = 0; x < row.Length; x++)
                {
                    tiles[y, x] = row[x];
                }
            }

            return tiles;
        }
    }
}
