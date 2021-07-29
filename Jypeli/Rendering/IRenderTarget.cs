using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli.Rendering
{
    /// <summary>
    /// Renderöintitekstuuri, johon kuva piirretään
    /// </summary>
    public interface IRenderTarget
    {
        /// <summary>
        /// Leveys
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Korkeus
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Kiinnittää tämän rendertargetin näytönohjaimeen käyttöä varten.
        /// </summary>
        public void Bind();

        /// <summary>
        /// Kiinnittää tämän sisältävän tekstuurin näytönohjaimelle
        /// </summary>
        void BindTexture();

        /// <summary>
        /// Irroittaa tämän sisältävän tekstuurin näytönohjaimelle
        /// </summary>
        void UnBindTexture();

        /// <summary>
        /// Asettaa tekstuurin tietyn alueen datan
        /// </summary>
        /// <param name="data">Pikselien data</param>
        /// <param name="startX">Alkupisteen X</param>
        /// <param name="startY">Alkupisteen Y</param>
        /// <param name="width">Alueen leveys</param>
        /// <param name="height">Alueen korkeus</param>
        void SetData(byte[] data, int startX, int startY, uint width, uint height);
    }
}
