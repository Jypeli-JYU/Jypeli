using System;
using System.IO;
using System.Reflection;

namespace Jypeli.Content
{
    /// <summary>
    /// Hallitsee pelin tiedostojen lataamista.
    /// </summary>
    public class JypeliContentManager
    {
        /// <summary>
        /// Polku josta tiedostoja ladataan.
        /// Oletuksena Content
        /// </summary>
        public string ContentPath { get; set; } = "Content";

        /// <summary>
        /// Polku Jypelin sisäisten resurssien lataamiseen.
        /// Tätä tuskin kannattaa muuttaa.
        /// </summary>
        public string InternalResourcePath { get; set; } = "Jypeli.Content.";

        /// <summary>
        /// Jypelin sisäisten resurssien nimet.
        /// </summary>
        public string[] InternalResources { get => Assembly.GetExecutingAssembly().GetManifestResourceNames(); }

        /// <summary>
        /// Avaa tietovirran Jypelin mukana tulevaan tiedostoon.
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public Stream StreamInternalResource(string assetName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(InternalResourcePath + assetName);
            return stream;
        }

        /// <summary>
        /// Lataa kuvatiedoston Jypelin sisäisistä resursseista
        /// </summary>
        /// <param name="assetName">Tiedoston nimi</param>
        /// <returns></returns>
        public Image LoadInternalImage(string assetName)
        {
            return Image.FromStream(StreamInternalResource("Images." + assetName));
        }

        /// <summary>
        /// Lataa äänitiedoston Jypelin sisäisistä resursseista
        /// </summary>
        /// <param name="assetName">Tiedoston nimi</param>
        /// <returns></returns>
        public SoundEffect LoadInternalSoundEffect(string assetName)
        {
            return new SoundEffect(assetName, StreamInternalResource("Sounds." + assetName));
        }

        /// <summary>
        /// Lataa fonttitiedoston Jypelin sisäisistä resursseista
        /// </summary>
        /// <param name="assetName">Tiedoston nimi</param>
        /// <returns></returns>
        public Stream StreamInternalFont(string assetName)
        {
            return StreamInternalResource("Fonts." + assetName);
        }

        /// <summary>
        /// Lataa tekstitiedoston Jypelin sisäisistä resursseista
        /// </summary>
        /// <param name="assetName">Tiedoston nimi</param>
        /// <returns></returns>
        public string LoadInternalText(string assetName)
        {
            return new StreamReader(StreamInternalResource(assetName)).ReadToEnd();
        }
    }
}
