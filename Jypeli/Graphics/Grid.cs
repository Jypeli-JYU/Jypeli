using System;

namespace Jypeli
{
    /// <summary>
    /// Avustava luokka ruutukarttojen käsittelyyn.
    /// Lisää tämä olio Layerille, jos haluat viivat näkyviin.
    /// </summary>
    public class Grid
    {
        /// <summary>
        /// Ruudukon väri, oletuksena <c>Color.Green</c>
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Ruudun koko, oletuksena 10,10
        /// </summary>
        public Vector CellSize { get; set; }

        /// <summary>
        /// Muodostaa uuden ruudukon
        /// </summary>
        public Grid()
        {
            Color = Color.Green;
            CellSize = new Vector( 10, 10 );
        }

        /// <summary>
        /// Kertoo sen ruudun keskipisteen, jossa annettu piste sijaitsee
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector SnapToLines( Vector v )
        {
            Vector result;
            result.X = Math.Round( v.X / this.CellSize.X ) * this.CellSize.X;
            result.Y = Math.Round( v.Y / this.CellSize.Y ) * this.CellSize.Y;
            return result;
        }
    }
}
