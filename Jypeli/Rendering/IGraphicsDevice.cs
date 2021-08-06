using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli.Rendering
{
    /// <summary>
    /// Yhteinen rajapinta eri renderöintialusotille.
    /// Ainoastaan tämän rajapinnan toteuttavan luokan pitäisi tietää, onko renderöintialustana esimerkiksi OpenGl vai WebGL
    /// </summary>
    public interface IGraphicsDevice : IDisposable
    {
        /// <summary>
        /// Käytössä olevan renderöintialustan nimi.
        /// </summary>
        public string Name {  get;}

        /// <summary>
        /// Kätössä olevan renderöintialustan versio.
        /// </summary>
        public string Version { get;}

        /// <summary>
        /// Transformaatiomatriisi kameran sijaintia ja kerroksen suhteellista siirtymää varten
        /// </summary>
        public Matrix4x4 World { get; set; }

        /// <summary>
        /// Transformaatiomatriisi kameran suuntaa varten
        /// </summary>
        public Matrix4x4 View { get; set; }

        /// <summary>
        /// Transformaatiomatriisi paikkakoordinaattien muuttamiseksi ruutukoordinaatteihin
        /// </summary>
        public Matrix4x4 Projection { get; set; }

        /// <summary>
        /// Verteksipuskurin koko. 
        /// </summary>
        public int BufferSize { get; }

        /// <summary>
        /// Piirtää verteksit joiden indeksit on jo määritetty
        /// </summary>
        /// <param name="primitivetype">Verteksien tyyppi</param>
        /// <param name="vertexBuffer">Luettelo vertekseistä</param>
        /// <param name="numIndices">Indeksien määrä</param>
        /// <param name="indexBuffer">indeksit</param>
        public void DrawIndexedPrimitives(PrimitiveType primitivetype, VertexPositionColorTexture[] vertexBuffer, uint numIndices, uint[] indexBuffer);

        /// <summary>
        /// Piirtää indeksoimattomat verteksit.
        /// </summary>
        /// <param name="primitivetype">Verteksien tyyppi</param>
        /// <param name="textureVertices">Luettelo vertekseistä</param>
        /// <param name="count">Määrä</param>
        /// <param name="normalized">Onko verteksit normalisoitu ruutukoordinaatteihin</param>
        public void DrawPrimitives(PrimitiveType primitivetype, VertexPositionColorTexture[] textureVertices, uint count, bool normalized = false);

        /// <summary>
        /// Piirtää indeksoimattomat verteksit.
        /// </summary>
        /// <param name="primitivetype">Verteksien tyyppi</param>
        /// <param name="textureVertices">Luettelo vertekseistä</param>
        /// <param name="count">Määrä</param>
        /// <param name="normalized">Onko verteksit normalisoitu ruutukoordinaatteihin</param>
        public void DrawPrimitivesInstanced(PrimitiveType primitivetype, VertexPositionColorTexture[] textureVertices, uint count, bool normalized = false);

        /// <summary>
        /// Asettaa rendertargetin.
        /// Antamalla parametriksi null poistaa asetuksen
        /// </summary>
        /// <param name="rendertarget"></param>
        public void SetRenderTarget(IRenderTarget rendertarget);
        /// <summary>
        /// Luo grafiikkalaitteelle soveltuvan rendertargetin
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        IRenderTarget CreateRenderTarget(uint width, uint height);

        /// <summary>
        /// Tyhjentää ruudun kuvan
        /// </summary>
        /// <param name="color">Väri jolla ruutu täytetään</param>
        public void Clear(Color color);

        /// <summary>
        /// Lataa kuvan datan näytönohjaimen muistiin.
        /// </summary>
        /// <param name="texture"></param>
        public void LoadImage(Image texture);

        /// <summary>
        /// Päivittää kuvan datan näytönohjaimelle.
        /// </summary>
        /// <param name="texture"></param>
        public void UpdateTextureData(Image texture);

        /// <summary>
        /// Päivittää kuvan skaalausasetuksen näytönohjaimelle
        /// </summary>
        /// <param name="texture"></param>
        public void UpdateTextureScaling(Image texture);

        /// <summary>
        /// Kiinnittää tekstuurin valmiiksi käyttöä varten.
        /// </summary>
        /// <param name="handle">Tekstuurin kahva</param>
        public void BindTexture(uint handle);

        /// <summary>
        /// Kutsuttava kun ikkunan kokoa muutetaan
        /// </summary>
        /// <param name="newSize"></param>
        public void ResizeWindow(Vector newSize);
    }
}
