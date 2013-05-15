#region MIT License
/*
 * Copyright (c) 2013 University of Jyväskylä, Department of Mathematical
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
using Jypeli.Content;


#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Jypeli
{
    public partial class Game
    {
        /// <summary>
        /// Kirjaston mukana tuleva sisältö.
        /// Voidaan käyttää esimerkiksi tekstuurien lataamiseen.
        /// </summary>
        public static ResourceContentManager ResourceContent { get; private set; }

        private void InitXnaContent()
        {
            ResourceContent = new ResourceContentManager( this.Services, ContentResources.ResourceManager );
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Lataa kuvan contentista.
        /// </summary>
        /// <param name="name">Kuvan nimi (ei tarkennetta)</param>
        /// <returns>Kuva</returns>
        public static Image LoadImage( string name )
        {
            return new Image( name );
        }

        /// <summary>
        /// Lataa kuvan Jypelin resursseista.
        /// </summary>
        /// <param name="name">Kuvan nimi</param>
        /// <returns>Kuva</returns>
        public static Image LoadImageFromResources( string name )
        {
            return new Image( ResourceContent.Load<Texture2D>( name ) );
        }

        /// <summary>
        /// Lataa taulukon kuvia contentista.
        /// </summary>
        /// <param name="name">Kuvien nimet ilman tarkennetta pilkuin eroiteltuna</param>
        /// <returns>Kuvataulukko</returns>
        public static Image[] LoadImages( params string[] names )
        {
            Image[] result = new Image[names.Length];
            for ( int i = 0; i < names.Length; i++ )
                result[i] = LoadImage( names[i] );
            return result;
        }

        /// <summary>
        /// Lataa taulukon kuvia contentista.
        /// </summary>
        /// <param name="baseName">Ennen numeroa tuleva osa nimestä.</param>
        /// <param name="startIndex">Ensimmäisen kuvan numero.</param>
        /// <param name="endIndex">Viimeisen kuvan numero.</param>
        /// <param name="zeroPad">Onko numeron edessä täytenollia.</param>
        /// <returns>Kuvataulukko</returns>
        public static Image[] LoadImages( string baseName, int startIndex, int endIndex, bool zeroPad = false )
        {
            if ( startIndex > endIndex ) throw new ArgumentException( "Starting index must be smaller than ending index." );

            Image[] result = new Image[endIndex - startIndex];
            string format;

            if ( zeroPad )
            {
                int digits = endIndex.ToString().Length;
                format = "{0}{1:" + "0".Repeat( digits ) + "}";
            }
            else
            {
                format = "{0}{1}";
            }

            for ( int i = startIndex; i < endIndex; i++ )
            {
                string imgName = String.Format( format, baseName, i );
                result[i - startIndex] = LoadImage( imgName );
            }

            return result;
        }
    }
}
