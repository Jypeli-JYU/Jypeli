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
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */


namespace Jypeli
{
    public partial class GameObject
    {
        private Color _color = Color.White;
        private bool _textureFillsShape = false;
        private Vector _textureWrapSize = new Vector( 1, 1 );

        /// <summary>
        /// Piirretäänkö oliota ruudulle.
        /// </summary>
        [Save]
        public bool IsVisible { get; set; }

        /// <summary>
        /// Animaatio. Voi olla <c>null</c>, jolloin piirretään vain väri.
        /// </summary>
        public override Animation Animation { get; set; }

        /// <summary>
        /// Määrittää kuinka moneen kertaan kuva piirretään. Esimerkiksi (3.0, 2.0) piirtää
        /// kuvan 3 kertaa vaakasuunnassa ja 2 kertaa pystysuunnassa.
        /// </summary>
        [Save]
        public Vector TextureWrapSize
        {
            get { return _textureWrapSize; }
            set { _textureWrapSize = value; }
        }

        /// <summary>
        /// Väri, jonka värisenä olio piirretään, jos tekstuuria ei ole määritelty.
        /// </summary>
        public virtual Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        /// <summary>
        /// Jos <c>true</c>, kuva piirretään niin, ettei se mene olion muodon
        /// ääriviivojen yli. Toisin sanoen kuva piirretään tarkasti vain
        /// muodon määrittämälle alueelle.
        /// </summary>
        /// <remarks>
        /// Tämän asettaminen tekee olion piirtämisestä hitaamman. Jos
        /// muoto on yksinkertainen, harkitse voisiko kuvan piirtää niin, että
        /// läpinäkyvyyttä käyttämällä saadaan kuvasta halutun muotoinen.
        /// </remarks>
        public bool TextureFillsShape
        {
            get { return _textureFillsShape; }
            set { _textureFillsShape = value; }
        }

        /// <summary>
        /// Jättääkö olio kentän valaistuksen huomiotta.
        /// Asetettu oletuksena käyttöliittymäkomponenteilla (widget).
        /// </summary>
        [System.Obsolete("Ei käytössä")]
        public bool IgnoresLighting { get; set; }

        private void InitAppearance()
        {
            this.IsVisible = true;
        }

        private void InitAppearance( Animation animation )
        {
            this.Animation = animation;
            InitAppearance();
        }

        /// <summary>
        /// Lataa kuvan tiedostosta ja asettaa sen oliolle.
        /// </summary>
        /// <param name="file"></param>
        public void SetImage( StorageFile file )
        {
            this.Image = Image.FromStream( file.Stream );
        }

        /// <summary>
        /// Kääntää olion kuvan vaakasuunnassa.
        /// </summary>
        public void MirrorImage()
        {
            TextureWrapSize = new Vector( -TextureWrapSize.X, TextureWrapSize.Y );
        }

        /// <summary>
        /// Kääntää olion kuvan pystysuunnassa.
        /// </summary>
        public void FlipImage()
        {
            TextureWrapSize = new Vector( TextureWrapSize.X, -TextureWrapSize.Y );
        }
    }
}
