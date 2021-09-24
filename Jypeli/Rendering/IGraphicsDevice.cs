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
        /// Luo uuden shaderin.
        /// </summary>
        /// <param name="vert">Verteksishaderin koodi</param>
        /// <param name="frag">Fragmentshaderin koodi</param>
        /// <returns></returns>
        IShader CreateShader(string vert, string frag);

        /// <summary>
        /// Luo uuden shaderin Jypelin sisäisistä resursseista.
        /// </summary>
        /// <param name="vertPath">Verteksishaderin polku</param>
        /// <param name="fragPath">Fragmentshaderin polku</param>
        /// <returns></returns>
        IShader CreateShaderFromInternal(string vertPath, string fragPath);

        /// <summary>
        /// Tyhjentää ruudun kuvan
        /// </summary>
        /// <param name="color">Väri jolla ruutu täytetään</param>
        public void Clear(Color color);

        /// <summary>
        /// Lataa kuvan datan näytönohjaimen muistiin.
        /// </summary>
        /// <param name="image"></param>
        public void LoadImage(Image image);

        /// <summary>
        /// Päivittää kuvan datan näytönohjaimelle.
        /// </summary>
        /// <param name="image"></param>
        public void UpdateTextureData(Image image);

        /// <summary>
        /// Päivittää kuvan skaalausasetuksen näytönohjaimelle
        /// </summary>
        /// <param name="image"></param>
        public void UpdateTextureScaling(Image image);

        /// <summary>
        /// Asettaa kuvan toistamaan itseään
        /// </summary>
        /// <param name="image"></param>
        public void SetTextureToRepeat(Image image);

        /// <summary>
        /// Kiinnittää tekstuurin valmiiksi käyttöä varten.
        /// </summary>
        /// <param name="image">Tekstuuri</param>
        public void BindTexture(Image image);

        /// <summary>
        /// Kutsuttava kun ikkunan kokoa muutetaan
        /// </summary>
        /// <param name="newSize"></param>
        public void ResizeWindow(Vector newSize);

        /// <summary>
        /// Lukee ruudulla näkyvät pikselit annettuun osoittimeen
        /// </summary>
        /// <param name="data">Osoitin johon data kirjoitetaan</param>
        public unsafe void GetScreenContents(void* data);

        /// <summary>
        /// Kopioi ruudulla näkyvät pikselit annettuun kuvaan
        /// </summary>
        /// <param name="img">valmiiksi alustettu näytön kokoinen kuva</param>
        public unsafe void GetScreenContentsToImage(Image img);

        /// <summary>
        /// Piirtää valot ruudulle
        /// </summary>
        /// <param name="matrix">Kameran transformaatiomatriisi</param>
        public void DrawLights(Matrix4x4 matrix);
    }
}
