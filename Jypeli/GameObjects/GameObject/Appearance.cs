#region MIT License
/*
 * Copyright (c) 2009 University of Jyv‰skyl‰, Department of Mathematical
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
 * Authors: Tero J‰ntti, Tomi Karppinen, Janne Nikkanen.
 */


namespace Jypeli
{
    public partial class GameObject
    {
        private Color _color = Color.White;
        private bool _textureFillsShape = false;
        private Vector _textureWrapSize = new Vector( 1, 1 );

        /// <summary>
        /// Piirret‰‰nkˆ oliota ruudulle.
        /// </summary>
        [Save]
        public bool IsVisible { get; set; }

        /// <summary>
        /// Animaatio. Voi olla <c>null</c>, jolloin piirret‰‰n vain v‰ri.
        /// </summary>
        public override Animation Animation { get; set; }

        /// <summary>
        /// M‰‰ritt‰‰ kuinka moneen kertaan kuva piirret‰‰n. Esimerkiksi (3.0, 2.0) piirt‰‰
        /// kuvan 3 kertaa vaakasuunnassa ja 2 kertaa pystysuunnassa.
        /// </summary>
        [Save]
        public Vector TextureWrapSize
        {
            get { return _textureWrapSize; }
            set { _textureWrapSize = value; }
        }

        /// <summary>
        /// V‰ri, jonka v‰risen‰ olio piirret‰‰n, jos tekstuuria ei ole m‰‰ritelty.
        /// </summary>
        public virtual Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        /// <summary>
        /// Jos <c>true</c>, kuva piirret‰‰n niin, ettei se mene olion muodon
        /// ‰‰riviivojen yli. Toisin sanoen kuva piirret‰‰n tarkasti vain
        /// muodon m‰‰ritt‰m‰lle alueelle.
        /// </summary>
        /// <remarks>
        /// T‰m‰n asettaminen tekee olion piirt‰misest‰ hitaamman. Jos
        /// muoto on yksinkertainen, harkitse voisiko kuvan piirt‰‰ niin, ett‰
        /// l‰pin‰kyvyytt‰ k‰ytt‰m‰ll‰ saadaan kuvasta halutun muotoinen.
        /// </remarks>
        public bool TextureFillsShape
        {
            get { return _textureFillsShape; }
            set { _textureFillsShape = value; }
        }

        /// <summary>
        /// J‰tt‰‰kˆ olio kent‰n valaistuksen huomiotta.
        /// Asetettu oletuksena k‰yttˆliittym‰komponenteilla (widget).
        /// </summary>
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

#if !DISABLE_STORAGE
        /// <summary>
        /// Lataa kuvan tiedostosta ja asettaa sen oliolle.
        /// </summary>
        /// <param name="file"></param>
        public void SetImage( StorageFile file )
        {
            this.Image = Image.FromStream( file.Stream );
        }
#endif

        /// <summary>
        /// K‰‰nt‰‰ olion kuvan vaakasuunnassa.
        /// </summary>
        public void MirrorImage()
        {
            TextureWrapSize = new Vector( -TextureWrapSize.X, TextureWrapSize.Y );
        }

        /// <summary>
        /// K‰‰nt‰‰ olion kuvan pystysuunnassa.
        /// </summary>
        public void FlipImage()
        {
            TextureWrapSize = new Vector( TextureWrapSize.X, -TextureWrapSize.Y );
        }
    }
}
