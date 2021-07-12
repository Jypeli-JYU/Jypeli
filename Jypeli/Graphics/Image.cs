using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaV2 = Microsoft.Xna.Framework.Vector2;
using XnaColor = Microsoft.Xna.Framework.Color;
using JyColor = Jypeli.Color;
using FontStashSharp;

#if WINDOWS_STOREAPP
using ColorConverter = Jypeli.ListHelpers.Converter<Jypeli.Color, Jypeli.Color>;
using XnaColorConverter = Jypeli.ListHelpers.Converter<Microsoft.Xna.Framework.Color, Microsoft.Xna.Framework.Color>;
#else
using ColorConverter = System.Converter<Jypeli.Color, Jypeli.Color>;
using XnaColorConverter = System.Converter<Microsoft.Xna.Framework.Color, Microsoft.Xna.Framework.Color>;
#endif


namespace Jypeli
{
    /// <summary>
    /// Kuva.
    /// </summary>
    public class Image
    {

        private static int MONOGETDATAMUL = 1;
        private static int MONOGETDATAINC = 0;
        // private static const int MONOGETDATAMUL = 1;  
        // private static const int MONOGETDATAINC = 0; // tavallinen

        private Image parentImage;
        private XnaRectangle parentRectangle;

        private string assetName;
        private Texture2D xnaTexture;

        private event Action InitDimensions;
        private event Action InitTexture;

        int _width = -1;
        int _height = -1;

        private static string[] imageExtensions = { ".png", ".jpg", ".xnb"};

        /// <summary>
        /// Asetetaan bitmapin rivikorjaus Mono:n bugin (???) takia
        /// </summary>
        /// <param name="n">0 = ei korjausta, 1 = hypätään joka toinen rivi yli</param>
        public static void SetLineCorrection(int n)
        {
            if ( n == 1 ) 
            {
                MONOGETDATAMUL = 2;
                MONOGETDATAINC = 1;
                return;
            }
            if (n == 0)
            {
                MONOGETDATAMUL = 1;
                MONOGETDATAINC = 0;
                return;
            }
        }

        internal Texture2D XNATexture
        {
            get { DoInitTexture(); return xnaTexture; }
        }

        /// <summary>
        /// Kuvan yksittäisten pikselien indeksointiin
        /// </summary>
        /// <param name="row">Rivi</param>
        /// <param name="col">Sarake</param>
        /// <returns>Pikselin väri</returns>
        public Color this[int row, int col]
        {
            get
            {
                DoInitTexture();

                Color[] buffer = new Color[1];
                XnaRectangle rect = new XnaRectangle( col, row, 1, 1 );
                xnaTexture.GetData<Color>( 0, rect, buffer, 0, 1 );
                return buffer[0];
            }
            set
            {
                DoInitTexture();
                InvalidateAsset();

                if ( row < 0 || row >= xnaTexture.Height ) throw new IndexOutOfRangeException( "row" );
                if ( col < 0 || col >= XNATexture.Width ) throw new IndexOutOfRangeException( "col" );

                Color[] buffer = new Color[1] { value };
                XnaRectangle rect = new XnaRectangle( col, row, 1, 1 );

                xnaTexture.SetData<Color>( 0, rect, buffer, 0, 1 );
                UpdateTexture();
            }
        }


        /// <summary>
        /// Kuvan pikselit Color-taulukkona
        /// </summary>
        /// <param name="ox">siirtymä x-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="oy">siirtymä y-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="w">alueen leveys joka kopioidaan</param>
        /// <param name="h">lueen korkaus joka kopioidaan</param>
        /// <returns>pikselit Color-taulukkona</returns>
        public Color[,] GetData( int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue )
        {
            int ny = Height;
            if ( h < ny ) ny = h;
            if ( Height < ny + oy ) ny = Height - oy;
            int nx = Width;
            if ( w < nx ) nx = w;
            if ( Width < nx + ox ) nx = Width - ox;
            if ( nx <= 0 || ny <= 0 ) return new Color[0, 0];

            DoInitTexture();
            Color[,] bmp = new Color[ny, nx];

            XnaRectangle rect = new XnaRectangle( ox, oy, nx, ny );
            Color[] buffer = new Color[ny * nx * MONOGETDATAMUL];
            xnaTexture.GetData<Color>( 0, rect, buffer, 0, buffer.Length );
            int i = 0;
            for (int iy = 0; iy < ny; iy++)
            {
                for (int ix = 0; ix < nx; ix++)
                    bmp[iy, ix] = buffer[i++];
                i += nx * MONOGETDATAINC;
            }
            return bmp;
        }


        /// <summary>
        /// Asettaa kuvan pikselit Color-taulukosta
        /// </summary>
        /// <param name="bmp">taulukko josta pikseleitä otetaan</param>
        /// <param name="ox">siirtymä x-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="oy">siirtymä y-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="w">alueen leveys johon kopioidaan</param>
        /// <param name="h">lueen korkaus johon kopioidaan</param>
        /// <returns>pikselit Color-taulukkona</returns>
        public void SetData( Color[,] bmp, int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue )
        {
            DoInitTexture();
            InvalidateAsset();
            int ny = bmp.GetLength( 0 );
            int nx = bmp.GetLength( 1 );
            if ( ny > Height ) ny = Height;
            if ( nx > Width ) nx = Height;
            if ( ny > h ) ny = h;
            if ( nx > w ) nx = w;
            if ( Height < ny + oy ) ny = Height - oy;
            if ( Width < nx + ox ) nx = Width - ox;
            if ( nx <= 0 || ny <= 0 ) return;

            XnaRectangle rect = new XnaRectangle( ox, oy, nx, ny );
            Color[] buffer = new Color[ny * nx];
            int i = 0;
            for ( int iy = 0; iy < ny; iy++ )
                for ( int ix = 0; ix < nx; ix++ )
                    buffer[i++] = bmp[iy, ix];

            xnaTexture.SetData<Color>( 0, rect, buffer, 0, buffer.Length );
            UpdateTexture();
        }

        /// <summary>
        /// Asettaa kuvan pikselit annetun tavutaulukon mukaan.
        ///
        /// Taulukon tavut luetaan järjestyksessä punainen, vihreä, sininen, läpinäkyvyys
        /// </summary>
        /// <param name="byteArr">Tavutaulukko</param>
        /// <param name="height">Kuvan leveys</param>
        /// <param name="width">Kuvan korkeus</param>
        public void SetData(byte[] byteArr, int height, int width)
        {
            Color[,] newColor = new Color[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int r = byteArr[4 * (i * width + j) + 0];
                    int g = byteArr[4 * (i * width + j) + 1];
                    int b = byteArr[4 * (i * width + j) + 2];
                    int a = byteArr[4 * (i * width + j) + 3];
                    newColor[i, j] = new Color(r, g, b, a);
                }
            }
            this.SetData(newColor);
        }

        /// <summary>
        /// Asettaa kuvan pikselit annetun tavutaulukon mukaan.
        ///
        /// Taulukon tavut luetaan järjestyksessä punainen, vihreä, sininen, läpinäkyvyys
        /// </summary>
        /// <param name="byteArr"></param>
        public void SetData(byte[] byteArr)
        {
            SetData(byteArr, this.Height, this.Width);
        }

        /// <summary>
        /// Kuvan pikselit byte-taulukkona.
        /// Tavut ovat järjestyksessä punainen, vihreä, sininen, läpinäkyvyys.
        /// </summary>
        /// <returns>pikselit byte-taulukkona</returns>
        public byte[] GetByteArray()
		{
			DoInitTexture();
			byte[] buffer = new byte[4 * Width * Height];
			xnaTexture.GetData<byte>( buffer );
			return buffer;
		}

        /// <summary>
        /// Palalutetaan kuvan pikselit ARGB-uint[,] -taulukkona
        /// </summary>
        /// <param name="ox">siirtymä x-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="oy">siirtymä y-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="w">alueen leveys joka kopioidaan</param>
        /// <param name="h">lueen korkaus joka kopioidaan</param>
        /// <returns>Kuvan pikselit ARGB-taulukkona</returns>
        public uint[,] GetDataUInt( int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue )
        {
            int ny = Height;
            if ( h < ny ) ny = h;
            if ( Height < ny + oy ) ny = Height - oy;
            int nx = Width;
            if ( w < nx ) nx = w;
            if ( Width < nx + ox ) nx = Width - ox;
            if ( nx <= 0 || ny <= 0 ) return new uint[0, 0];

            DoInitTexture();
            uint[,] bmp = new uint[ny, nx];

            XnaRectangle rect = new XnaRectangle( ox, oy, nx, ny );
            Color[] buffer = new Color[ny * nx * MONOGETDATAMUL];
            xnaTexture.GetData<Color>( 0, rect, buffer, 0, buffer.Length );
            int i = 0;
            for (int iy = 0; iy < ny; iy++)
            {
                for (int ix = 0; ix < nx; ix++)
                    bmp[iy, ix] = buffer[i++].ToUInt();
                i += nx * MONOGETDATAINC;
            }
            return bmp;
        }


        /// <summary>
        /// Palalutetaan kuvan pikselit ARGB-uint[][] -taulukkona
        /// </summary>
        /// <param name="ox">siirtymä x-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="oy">siirtymä y-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="w">alueen leveys joka kopioidaan</param>
        /// <param name="h">lueen korkaus joka kopioidaan</param>
        /// <returns>Kuvan pikselit ARGB-taulukkona</returns>
        public uint[][] GetDataUIntAA( int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue )
        {
            int ny = Height;
            if ( h < ny ) ny = h;
            if ( Height < ny + oy ) ny = Height - oy;
            int nx = Width;
            if ( w < nx ) nx = w;
            if ( Width < nx + ox ) nx = Width - ox;
            if ( nx <= 0 || ny <= 0 ) return new uint[0][];

            DoInitTexture();
            uint[][] bmp = new uint[ny][];

            XnaRectangle rect = new XnaRectangle( ox, oy, nx, ny );
            Color[] buffer = new Color[ny * nx * MONOGETDATAMUL];
            xnaTexture.GetData<Color>( 0, rect, buffer, 0, buffer.Length );
            int i = 0;
            for ( int iy = 0; iy < ny; iy++ )
            {
                uint[] row = new uint[nx];
                bmp[iy] = row;
                for ( int ix = 0; ix < nx; ix++ )
                    row[ix] = buffer[i++].ToUInt();
                i += nx * MONOGETDATAINC;
            }
            return bmp;
        }


        /// <summary>
        /// Asetetaan kuvan pikselit ARGB-uint taulukosta
        /// </summary>
        /// <param name="bmp">taulukko josta pikselit otetaan</param>
        /// <param name="ox">siirtymä x-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="oy">siirtymä y-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="w">alueen leveys johon kopioidaan</param>
        /// <param name="h">alueen korkeus johon kopioidaan</param>
        public void SetData( uint[,] bmp, int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue )
        {
            DoInitTexture();
            InvalidateAsset();
            int ny = bmp.GetLength( 0 );
            int nx = bmp.GetLength( 1 );
            if ( ny > Height ) ny = Height;
            if ( nx > Width ) nx = Width;
            if ( ny > h ) ny = h;
            if ( nx > w ) nx = w;
            if ( Height < ny + oy ) ny = Height - oy;
            if ( Width < nx + ox ) nx = Width - ox;

            if ( nx <= 0 || ny <= 0 ) return;

            XnaRectangle rect = new XnaRectangle( ox, oy, nx, ny );
            Color[] buffer = new Color[ny * nx];
            int i = 0;
            for ( int iy = 0; iy < ny; iy++ )
                for ( int ix = 0; ix < nx; ix++ )
                    buffer[i++] = Jypeli.Color.UIntToColor( bmp[iy, ix] );
            // foreach (int c in bmp) buffer[i++] = Jypeli.Color.IntToColor(c);

            xnaTexture.SetData<Color>( 0, rect, buffer, 0, buffer.Length );
            UpdateTexture();
        }


        /// <summary>
        /// Asetetaan kuvan pikselit ARGB-uint taulukosta
        /// </summary>
        /// <param name="bmp">taulukko josta pikselit otetaan</param>
        /// <param name="ox">siirtymä x-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="oy">siirtymä y-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="w">alueen leveys johon kopioidaan</param>
        /// <param name="h">alueen korkeus johon kopioidaan</param>
        public void SetData( uint[][] bmp, int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue )
        {
            DoInitTexture();
            InvalidateAsset();
            int ny = bmp.Length;
            int nx = bmp[0].Length;
            if ( ny > Height ) ny = Height;
            if ( nx > Width ) nx = Width;
            if ( ny > h ) ny = h;
            if ( nx > w ) nx = w;
            if ( nx <= 0 || ny <= 0 ) return;

            XnaRectangle rect = new XnaRectangle( ox, oy, nx, ny );
            Color[] buffer = new Color[ny * nx];
            int i = 0;
            for ( int iy = 0; iy < ny; iy++ )
                for ( int ix = 0; ix < nx; ix++ )
                    buffer[i++] = Jypeli.Color.UIntToColor( bmp[iy][ix] );
            // foreach (int c in bmp) buffer[i++] = Jypeli.Color.IntToColor(c);

            xnaTexture.SetData<Color>( 0, rect, buffer, 0, buffer.Length );
            UpdateTexture();
        }


        /// <summary>
        /// Leveys pikseleinä.
        /// </summary>
        public int Width
        {
            get { DoInitDimensions(); return _width; }
        }

        /// <summary>
        /// Korkeus pikseleinä.
        /// </summary>
        public int Height
        {
            get { DoInitDimensions(); return _height; }
        }

        /// <summary>
        /// Nimi.
        /// </summary>
        public string Name
        {
            get { DoInitTexture(); return xnaTexture.Name; }
        }

        internal Image( int width, int height )
        {
            AssertDimensions( width, height );
            this._width = width;
            this._height = height;
            this.InitTexture += CreateNewTexture;
        }

        internal Image( string assetName )
        {
            this.assetName = assetName;
            this.InitDimensions += LoadContentTexture;
        }

        /// <summary>
        /// Kuva MonoGamen Texture2D oliosta
        /// </summary>
        /// <param name="texture"></param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public Image( Microsoft.Xna.Framework.Graphics.Texture2D texture )
        {
            AssertDimensions( texture.Width, texture.Height );
            this.xnaTexture = texture;
            this._width = texture.Width;
            this._height = texture.Height;
        }

        /// <summary>
        /// Luo uuden kuvan.
        /// </summary>
        /// <param name="width">Kuvan leveys</param>
        /// <param name="height">Kuvan korkeus</param>
        /// <param name="backColor">Kuvan taustaväri</param>
        public Image( int width, int height, Color backColor )
        {
            AssertDimensions( width, height );
            assetName = null;
            this._width = width;
            this._height = height;
            this.InitTexture += CreateNewTexture;
            this.InitTexture += delegate { this.Fill( backColor ); };
        }

        /// <summary>
        /// Luo uuden kuvan.
        /// </summary>
        /// <param name="width">Kuvan leveys</param>
        /// <param name="height">Kuvan korkeus</param>
        /// <param name="backColor">Kuvan taustaväri</param>
        public Image( double width, double height, Color backColor )
            : this( (int)Math.Round( width ), (int)Math.Round( height ), backColor )
        {
        }

        private void AssertDimensions(int width, int height)
        {
            if ( width < 1 || height < 1 )
                throw new ArgumentException( String.Format( "Image dimensions must be at least 1 x 1! (given: {0} x {1}", width, height ) );
        }

        private void DoInitDimensions()
        {
            if ( _width > 0 && _height > 0 )
                return;

            if ( InitDimensions != null )
                InitDimensions();
            else
                throw new InvalidOperationException( "Cannot initialize dimensions for image!" );
        }

        private void DoInitTexture()
        {
            DoInitDimensions();
            AssertDimensions( this.Width, this.Height );

            if ( xnaTexture != null )
                return;

            if ( InitTexture != null )
                InitTexture();
            else
                throw new InvalidOperationException( "Cannot initialize texture for image!" );
        }

        private void LoadContentTexture()
        {
            // Some duct tape around the fact that in XNA,
            // content can not be loaded before LoadContent().
            Debug.Assert( assetName != null );
            xnaTexture = LoadFile(assetName);
            _width = xnaTexture.Width;
            _height = xnaTexture.Height;
            
        }
        // TODO: Why is this path not used???
        private Texture2D LoadFile(string path)
        {
            assetName = Game.FileExtensionCheck(assetName, imageExtensions);
            FileStream fileStream = new FileStream(assetName, FileMode.Open);
            Texture2D texture = Texture2D.FromStream(Game.GraphicsDevice, fileStream);
            fileStream.Dispose();

            return texture;
        }

        private void CreateNewTexture()
        {
            this.xnaTexture = new Texture2D( Game.GraphicsDevice, Width, Height );
        }

        /// <summary>
        /// Luo kopion kuvasta
        /// </summary>
        /// <returns></returns>
        public Image Clone()
        {
            Image copy;

            if ( assetName != null )
            {
                copy = new Image( assetName );
            }
            else
            {
                copy = new Image( this.Width, this.Height );
                copy.InitTexture += delegate { CopyData( copy, this ); };
            }

            return copy;
        }

        private static void CopyData( Image dest, Image src )
        {
            src.DoInitTexture();

            int w = src.Width;
            int h = src.Height;
            XnaRectangle rect = new XnaRectangle( 0, 0, w, 1 );
            Color[] scanline = new Color[w];

            for ( rect.Y = 0; rect.Y < h; rect.Y++ )
            {
                src.xnaTexture.GetData<Color>( 0, rect, scanline, 0, w );
                dest.xnaTexture.SetData<Color>( 0, rect, scanline, 0, w );
            }
        }

        private static void CopyData( Texture2D dest, Texture2D src )
        {
            int w = src.Width;
            int h = src.Height;
            XnaRectangle rect = new XnaRectangle( 0, 0, w, 1 );
            Color[] scanline = new Color[w];

            for ( rect.Y = 0; rect.Y < h; rect.Y++ )
            {
                src.GetData<Color>( 0, rect, scanline, 0, w );
                dest.SetData<Color>( 0, rect, scanline, 0, w );
            }
        }

        private static void CopyData( Image dest, Image src, XnaRectangle destRect, XnaRectangle srcRect )
        {
            src.DoInitTexture();

            int w = srcRect.Width;
            int h = srcRect.Height;
			XnaRectangle srcScan = new XnaRectangle( srcRect.X, srcRect.Y, w, 1 );
			XnaRectangle destScan = new XnaRectangle( destRect.X, destRect.Y, w, 1 );
			Color[] scanline = new Color[w];

			for ( int i = 0; i < h; i++ )
            {
				src.xnaTexture.GetData<Color>( 0, srcScan, scanline, 0, w );
				dest.xnaTexture.SetData<Color>( 0, destScan, scanline, 0, w );
				srcScan.Y += MONOGETDATAINC;
				destScan.Y += MONOGETDATAINC;
            }
        }

        /// <summary>
        /// Suorittaa annetun pikselioperaation koko kuvalle.
        /// </summary>
        /// <param name="operation">Aliohjelma, joka ottaa värin ja palauttaa värin</param>
        public void ApplyPixelOperation( ColorConverter operation )
        {
            XnaColorConverter newOp = delegate( XnaColor c )
            {
                return operation( new Color( c ) ).AsXnaColor();
            };

            ApplyPixelOperation( newOp );
        }

        /// <summary>
        /// Suorittaa annetun pikselioperaation koko kuvalle.
        /// </summary>
        /// <param name="operation">Aliohjelma, joka ottaa värin ja palauttaa värin</param>
        internal void ApplyPixelOperation( XnaColorConverter operation )
        {
            DoInitTexture();
            InvalidateAsset();

            XnaRectangle scanRect = new XnaRectangle( 0, 0, xnaTexture.Width, 1 );
            XnaColor[] scanline = new XnaColor[xnaTexture.Width];

            for ( scanRect.Y = 0; scanRect.Y < xnaTexture.Height; scanRect.Y++ )
            {
                xnaTexture.GetData<XnaColor>( 0, scanRect, scanline, 0, xnaTexture.Width );

                for ( int j = 0; j < xnaTexture.Width; j++ )
                {
                    scanline[j] = operation( scanline[j] );
                }

                xnaTexture.SetData<XnaColor>( 0, scanRect, scanline, 0, xnaTexture.Width );
            }

            UpdateTexture();
        }

        /// <summary>
        /// Tekee uuden lokaalin instanssin kuvan tekstuurista ja poistaa
        /// viitteen assettiin josta kuva on luotu.
        /// Kutsu tätä metodia aina kun kuvan dataa muutetaan.
        /// </summary>
        private void InvalidateAsset()
        {
            if ( assetName == null )
                return;

            Texture2D oldTex = xnaTexture;
            CreateNewTexture();
            CopyData( xnaTexture, oldTex );
            assetName = null;
        }

        private void UpdateTexture()
        {
            Game.DoNextUpdate( DoUpdateTexture );
        }

        private void DoUpdateTexture()
        {
            if ( parentImage != null )
            {
                XnaRectangle srcRect = new XnaRectangle( 0, 0, Width, Height );
                CopyData( parentImage, this, parentRectangle, srcRect );
                parentImage.UpdateTexture();
            }
        }

        #region static methods

#if !WINDOWS_STOREAPP
        /// <summary>
        /// Lataa kuvan tiedostosta. Kuvan ei tarvitse olla lisättynä
        /// Content-projektiin.
        /// </summary>
        /// <param name="path">Tiedoston polku.</param>
        public static Image FromFile( string path )
        {
            StreamReader sr = new StreamReader( path );
            Image img = new Image( Texture2D.FromStream( Game.GraphicsDevice, sr.BaseStream ) );
            return img;
        }
#endif

        ///// <summary>
        ///// Lataa kuvan tiedostosta. Kuvan ei tarvitse olla lisättynä
        ///// Content-projektiin.
        ///// </summary>
        ///// <param name="path">Tiedosto.</param>
        //public static Image FromFile( StorageFile file )
        //{
        //    return FromStream( file.Stream );
        //}

        /// <summary>
        /// Lataa kuvan tiedostovirrasta.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Image FromStream( Stream stream )
        {
            return new Image( Texture2D.FromStream( Game.GraphicsDevice, stream ) );
        }

#if WINDOWS
        /// <summary> 
        /// Lataa kuvan Internetistä. 
        /// </summary> 
        /// <param name="url">Kuvan URL-osoite</param> 
        /// <returns>Kuva</returns> 
        public static Image FromURL( string url )
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create( url );
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();

            MemoryStream memStream = new MemoryStream();
            resStream.CopyTo( memStream );

            Image img = new Image( Texture2D.FromStream( Game.GraphicsDevice, memStream ) );
            return img;
        }
#endif

        /// <summary>
        /// Luo tähtitaivaskuvan.
        /// </summary>
        /// <param name="width">Tekstuurin leveys.</param>
        /// <param name="height">Tekstuurin korkeus.</param>
        /// <param name="stars">Tähtien määrä.</param>
        /// <param name="transparent">Onko tausta läpinäkyvä vai ei (jolloin siitä tulee täysin musta)</param>
        /// <returns>Tekstuuri.</returns>
        public static Image CreateStarSky( int width, int height, int stars, bool transparent = false)
        {
            XnaColor[] textureColors = new XnaColor[width * height];

            // Background, black or transparent
            int i = 0;
            for ( int ver = 0; ver < height; ver++ )
            {
                for ( int hor = 0; hor < width; hor++ )
                {
                    if (transparent) textureColors[i++] = XnaColor.Transparent;
                    else textureColors[i++] = XnaColor.Black;
                }
            }

            // Random stars
            for ( int j = 0; j < stars; j++ )
            {
                int star = RandomGen.NextInt( 0, width * height );
                int size = RandomGen.NextInt( 1, 5 );

                for ( int k = 0; k < size / 2; k++ )
                {
                    XnaColor starcolor = RandomGen.NextColor( Jypeli.Color.White, new Color( 192, 192, 192, 255 ) ).AsXnaColor();

                    if ( star + k < textureColors.Length )
                        textureColors[star + k] = starcolor;

                    if ( size % 2 != 0 || size == 2 )
                        continue;

                    int nextStar = star + k + width;

                    if ( nextStar < ( width * height ) )
                    {
                        textureColors[nextStar] = starcolor;
                    }
                }
            }

            //Texture2D newTexture = new Texture2D( Game.GraphicsDevice, width, height, 1, TextureUsage.None, SurfaceFormat.Color );
            Texture2D newTexture = new Texture2D( Game.GraphicsDevice, width, height, false, SurfaceFormat.Color );
            newTexture.SetData<XnaColor>( textureColors );

            return new Image( newTexture );
        }

        /// <summary>
        /// Luo kuvan tekstistä.
        /// </summary>
        /// <param name="text">Teksti josta kuva luodaan</param>
        /// <param name="font">Fontti</param>
        /// <param name="textColor">Tekstin väri</param>
        /// <param name="backgroundColor">Tekstin taustaväri</param>
        /// <returns>Teksti kuvana</returns>
        public static Image FromText( string text, Font font, Color textColor, Color backgroundColor )
        {
            if ( text == null )
                text = "";

            var spriteBatch = new SpriteBatch( Game.GraphicsDevice );
            var device = spriteBatch.GraphicsDevice;

            XnaV2 textDims = font.XnaFont.MeasureString( text );
            int textw = ( textDims.X > 1 ) ? Convert.ToInt32( textDims.X ) : 1;
            int texth = ( textDims.Y > 1 ) ? Convert.ToInt32( textDims.Y ) : 1;

            //RenderTarget2D rt = new RenderTarget2D( device, textw, texth, 1, device.DisplayMode.Format );
            RenderTarget2D rt = new RenderTarget2D( device, textw, texth, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8 );

            //device.SetRenderTarget( 0, rt );
            device.SetRenderTarget( rt );
            device.Clear( ClearOptions.Target | ClearOptions.DepthBuffer, backgroundColor.AsXnaColor(), 1.0f, 0 );

            spriteBatch.Begin();
            font.XnaFont.DrawText(Graphics.FontRenderer, text, XnaV2.Zero.ToSystemNumerics(), textColor.AsXnaColor().ToSystemDrawing());
            spriteBatch.End();

            //device.SetRenderTarget( 0, null );
            device.SetRenderTarget( null );

            //return new Image( rt.GetTexture() );
            return new Image( rt );
        }

        /// <summary>
        /// Piirtää tekstiä kuvan päälle.
        /// </summary>
        /// <param name="img">Kuva jonka päälle piirretään</param>
        /// <param name="text">Piirrettävä teksti</param>
        /// <param name="position">Piirtokohta (origo keskellä kuvaa)</param>
        /// <param name="font">Fontti</param>
        /// <param name="textColor">Tekstin väri</param>
        /// <param name="backgroundColor">Tekstin taustaväri</param>
        /// <returns>Kuva tekstin kanssa</returns>
        public static Image DrawTextOnImage( Image img, string text, Vector position, Font font, Color textColor, Color backgroundColor )
        {
            if ( text == null )
                text = "";

            var spriteBatch = new SpriteBatch( Game.GraphicsDevice );
            var device = spriteBatch.GraphicsDevice;

            XnaV2 textDims = font.XnaFont.MeasureString( text );
            int textw = ( textDims.X > 1 ) ? Convert.ToInt32( textDims.X ) : 1;
            int texth = ( textDims.Y > 1 ) ? Convert.ToInt32( textDims.Y ) : 1;

            //RenderTarget2D rt = new RenderTarget2D( device, textw, texth, 1, device.DisplayMode.Format );
            RenderTarget2D rt = new RenderTarget2D( device, img.Width, img.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8 );

            //device.SetRenderTarget( 0, rt );
            device.SetRenderTarget( rt );
            device.Clear( ClearOptions.Target | ClearOptions.DepthBuffer, backgroundColor.AsXnaColor(), 1.0f, 0 );

            float xpos = 0.5f * ( img.Width - textw ) + (float)position.X;
            float ypos = 0.5f * ( img.Height - texth ) - (float)position.Y;

            spriteBatch.Begin();
            spriteBatch.Draw( img.XNATexture, rt.Bounds, XnaColor.White );
            font.XnaFont.DrawText(Graphics.FontRenderer, text, (new XnaV2(xpos, ypos)).ToSystemNumerics(), textColor.AsXnaColor().ToSystemDrawing());
            spriteBatch.End();

            //device.SetRenderTarget( 0, null );
            device.SetRenderTarget( null );


            //return new Image( rt.GetTexture() );
            return new Image( rt );
        }

        /// <summary>
        /// Piirtää tekstiä kuvan päälle keskelle kuvaa.
        /// </summary>
        /// <param name="img">Kuva jonka päälle piirretään</param>
        /// <param name="text">Piirrettävä teksti</param>
        /// <param name="font">Fontti</param>
        /// <param name="textColor">Tekstin väri</param>
        /// <returns>Kuva tekstin kanssa</returns>
        public static Image DrawTextOnImage( Image img, string text, Font font, Color textColor )
        {
            return DrawTextOnImage( img, text, Vector.Zero, font, textColor, Jypeli.Color.Transparent );
        }

        /// <summary>
        /// Luo pystysuuntaisen liukuväritetyn kuvan.
        /// </summary>
        /// <param name="imageWidth">kuvan leveys.</param>
        /// <param name="imageHeight">kuvan korkeus.</param>
        /// <param name="lowerColor">Alareunassa käytettävä väri.</param>
        /// <param name="upperColor">Yläreunassa käytettävä väri.</param>
        /// <returns>Väritetty kuva.</returns>
        public static Image FromGradient( int imageWidth, int imageHeight, Color lowerColor, Color upperColor )
        {
            XnaColor lower = lowerColor.AsXnaColor();
            XnaColor upper = upperColor.AsXnaColor();
            XnaColor[] textureColors = new XnaColor[imageWidth * imageHeight];
            int i = 0;

            for ( int ver = 0; ver < imageHeight; ver++ )
            {
                for ( int hor = 0; hor < imageWidth; hor++ )
                {
                    textureColors[i++] = XnaColor.Lerp( upper, lower, ( (float)ver / (float)imageHeight ) );
                }
            }

            Texture2D newTexture = new Texture2D( Game.GraphicsDevice, imageWidth, imageHeight, false, SurfaceFormat.Color );
            newTexture.SetData<XnaColor>( textureColors );
            return new Image( newTexture );
        }

        /// <summary>
        /// Luo yksivärisen kuvan.
        /// </summary>
        /// <param name="imageWidth">Kuvan leveys.</param>
        /// <param name="imageHeight">Kuvan korkeus.</param>
        /// <param name="color">Kuvan väri.</param>
        /// <returns>Väritetty kuva.</returns>
        public static Image FromColor( int imageWidth, int imageHeight, Color color )
        {
            return Image.FromGradient( imageWidth, imageHeight, color, color );
        }

        private static XnaColor[] MirrorLine( XnaColor[] scanline, int width )
        {
            XnaColor[] res = new XnaColor[width];
            int l = 0;
            int r = width - 1;

            while ( l < r )
            {
                res[l] = scanline[r];
                res[r] = scanline[l];
                l++; r--;
            }

            if ( l == r )
            {
                // Center pixel
                res[l] = scanline[l];
            }

            return res;
        }

        /// <summary>
        /// Peilaa kuvan X-suunnassa.
        /// </summary>
        /// <param name="image">Peilattava kuva.</param>
        /// <returns>Peilattu kuva.</returns>
        public static Image Mirror( Image image )
        {
            Texture2D newTex = new Texture2D( image.XNATexture.GraphicsDevice, image.Width, image.Height, false, image.XNATexture.Format );
            XnaColor[] scanline = new XnaColor[image.Width];
            var scanRect = new XnaRectangle( 0, 0, image.Width, 1 );

            for ( scanRect.Y = 0; scanRect.Y < image.Height; scanRect.Y++ )
            {
                image.XNATexture.GetData<XnaColor>( 0, scanRect, scanline, 0, image.Width );
                scanline = MirrorLine( scanline, image.Width );
                newTex.SetData<XnaColor>( 0, scanRect, scanline, 0, image.Width );
            }

            return new Image( newTex );
        }

        /// <summary>
        /// Peilaa kuvat X-suunnassa.
        /// </summary>
        /// <param name="images">Peilattavat kuvat.</param>
        /// <returns>Peilatut kuvat.</returns>
        public static Image[] Mirror( Image[] images )
        {
            Image[] result = new Image[images.Length];
            for ( int i = 0; i < images.Length; i++ )
                result[i] = Mirror( images[i] );
            return result;
        }

        /// <summary>
        /// Peilaa kuvan Y-suunnassa.
        /// </summary>
        /// <param name="image">Peilattava kuva.</param>
        /// <returns>Peilattu kuva.</returns>
        public static Image Flip( Image image )
        {
            Texture2D newTex = new Texture2D( image.XNATexture.GraphicsDevice, image.Width, image.Height, false, image.XNATexture.Format );
            XnaColor[] scanlineUpper = new XnaColor[image.Width];
            XnaColor[] scanlineLower = new XnaColor[image.Width];
            var scanRectUpper = new XnaRectangle( 0, 0, image.Width, 1 );
            var scanRectLower = new XnaRectangle( 0, 0, image.Width, 1 );

            for ( int i = 0; i < image.Height / 2; i++ )
            {
                scanRectUpper.Y = i;
                scanRectLower.Y = image.Height - 1 - i;

                image.XNATexture.GetData<XnaColor>( 0, scanRectUpper, scanlineUpper, 0, image.Width );
                image.XNATexture.GetData<XnaColor>( 0, scanRectLower, scanlineLower, 0, image.Width );

                newTex.SetData<XnaColor>( 0, scanRectUpper, scanlineLower, 0, image.Width );
                newTex.SetData<XnaColor>( 0, scanRectLower, scanlineUpper, 0, image.Width );
            }

            if ( image.Height % 2 == 1 )
            {
                // Center line
                scanRectUpper.Y = image.Height / 2;
                image.XNATexture.GetData<XnaColor>( 0, scanRectUpper, scanlineUpper, 0, image.Width );
                newTex.SetData<XnaColor>( 0, scanRectUpper, scanlineUpper, 0, image.Width );
            }

            return new Image( newTex );
        }

        /// <summary>
        /// Peilaa kuvat Y-suunnassa.
        /// </summary>
        /// <param name="images">Peilattavat kuvat.</param>
        /// <returns>Peilatut kuvat.</returns>
        public static Image[] Flip( Image[] images )
        {
            Image[] result = new Image[images.Length];
            for ( int i = 0; i < images.Length; i++ )
                result[i] = Flip( images[i] );
            return result;
        }

        /// <summary>
        /// Värittää kuvan.
        /// </summary>
        /// <param name="image">Väritettävä kuva.</param>
        /// <param name="color">Väri, jolla väritetään.</param>
        /// <returns>Väritetty kuva.</returns>
        public static Image Color( Image image, Color color )
        {
            Texture2D newTex = new Texture2D( image.XNATexture.GraphicsDevice, image.Width, image.Height, false, image.XNATexture.Format );
            XnaColor[] scanline = new XnaColor[image.Width];
            var scanRect = new XnaRectangle( 0, 0, image.Width, 1 );
            XnaColor xnaColor = color.AsXnaColor();

            for ( scanRect.Y = 0; scanRect.Y < image.Height; scanRect.Y++ )
            {
                image.XNATexture.GetData<XnaColor>( 0, scanRect, scanline, 0, image.Width );

                for ( int i = 0; i < image.Width; i++ )
                {
                    if ( scanline[i].A < 255 )
                    {
                        scanline[i].R = (byte)( ( 255 - scanline[i].A ) * xnaColor.R + scanline[i].A * scanline[i].R );
                        scanline[i].G = (byte)( ( 255 - scanline[i].A ) * xnaColor.G + scanline[i].A * scanline[i].G );
                        scanline[i].B = (byte)( ( 255 - scanline[i].A ) * xnaColor.B + scanline[i].A * scanline[i].B );

                        if ( scanline[i].A > 10 )
                        {
                            scanline[i].A = color.AlphaComponent;
                        }
                    }
                }

                newTex.SetData<XnaColor>( 0, scanRect, scanline, 0, image.Width );
            }

            return new Image( newTex );
        }

        /// <summary>
        /// Värittää kuvat.
        /// </summary>
        /// <param name="images">Väritettävät kuvat.</param>
        /// <param name="color">Väri, jolla väritetään.</param>
        /// <returns>Väritetyt kuvat.</returns>
        public static Image[] Color( Image[] images, Color color )
        {
            Image[] result = new Image[images.Length];
            for ( int i = 0; i < images.Length; i++ )
                result[i] = Color( images[i], color );
            return result;
        }

        // TODO: On hyvin hämäävä nimi...
        /// <summary>
        /// Muuttaa kuvan jokaisen pikselin <c>alpha</c>-arvon vastaamaan annettua.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static Image Color( Image image, byte alpha )
        {
            Texture2D newTex = new Texture2D( image.XNATexture.GraphicsDevice, image.Width, image.Height, false, image.XNATexture.Format );
            XnaColor[] scanline = new XnaColor[image.Width];
            var scanRect = new XnaRectangle( 0, 0, image.Width, 1 );

            for ( scanRect.Y = 0; scanRect.Y < image.Height; scanRect.Y++ )
            {
                image.XNATexture.GetData<XnaColor>( 0, scanRect, scanline, 0, image.Width );
                for ( int i = 0; i < image.Width; i++ )
                {
                    scanline[i].A = alpha;
                }
                newTex.SetData<XnaColor>( 0, scanRect, scanline, 0, image.Width );
            }
            return new Image( newTex );
        }

        /// <summary>
        /// Yhditää kaksi kuvaa olemaan vierekkäin uudessa kuvassa.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Image TileHorizontal( Image left, Image right )
        {
            if ( left.Height != right.Height ) throw new InvalidOperationException( "Cannot tile two images with different height" );

            left.DoInitTexture();
            right.DoInitTexture();

            XnaRectangle leftRect = new XnaRectangle( 0, 0, left.Width, left.Height );
            XnaRectangle rightSrc = new XnaRectangle( 0, 0, right.Width, right.Height );
            XnaRectangle rightDest = new XnaRectangle( left.Width, 0, right.Width, right.Height );

            Image tiled = new Image( left.Width + right.Width, left.Height );
            tiled.InitTexture += delegate { CopyData( tiled, left, leftRect, leftRect ); };
            tiled.InitTexture += delegate { CopyData( tiled, right, rightDest, rightSrc ); };

            return tiled;
        }

        /// <summary>
        /// Yhdistää kaksi kuvaa olemaan päällekkäin uudessa kuvassa
        /// </summary>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public static Image TileVertical( Image top, Image bottom )
        {
            if ( top.Width != bottom.Width ) throw new InvalidOperationException( "Cannot tile two images with different width" );

            top.DoInitTexture();
            bottom.DoInitTexture();

            XnaRectangle topRect = new XnaRectangle( 0, 0, top.Width, top.Height );
            XnaRectangle botSrc = new XnaRectangle( 0, 0, bottom.Width, bottom.Height );
            XnaRectangle botDest = new XnaRectangle( 0, top.Height, bottom.Width, bottom.Height );

            Image tiled = new Image( top.Width, top.Height + bottom.Height );
            tiled.InitTexture += delegate { CopyData( tiled, top, topRect, topRect ); };
            tiled.InitTexture += delegate { CopyData( tiled, bottom, botDest, botSrc ); };

            return tiled;
        }

        #endregion

        /// <summary>
        /// Leikkaa kuvasta palan ja palauttaa sen uutena kuvana
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public Image Area( int left, int top, int right, int bottom )
        {
            int width = right - left;
            int height = bottom - top;

            if ( width <= 0 ) throw new ArgumentException( "Left coordinate must be less than right coordinate" );
            if ( height <= 0 ) throw new ArgumentException( "Top coordinate must be less than bottom coordinate" );

            XnaRectangle srcRect = new XnaRectangle( left, top, width, height );
            XnaRectangle destRect = new XnaRectangle( 0, 0, width, height );

            Image areaImage = new Image( width, height );
            areaImage.parentImage = this;
            areaImage.parentRectangle = srcRect;
            areaImage.InitTexture += delegate { CopyData( areaImage, this, destRect, srcRect ); };
            return areaImage;
        }

        /// <summary>
        /// Täyttää kuvan värillä
        /// </summary>
        /// <param name="backColor"></param>
        public void Fill( Color backColor )
        {
            DoInitTexture();
            InvalidateAsset();

            XnaRectangle rect = new XnaRectangle( 0, 0, xnaTexture.Width, 1 );
            Color[] scanline = new Color[xnaTexture.Width];

            for ( int i = 0; i < xnaTexture.Width; i++ )
                scanline[i] = backColor;

            for ( rect.Y = 0; rect.Y < xnaTexture.Height; rect.Y++ )
                xnaTexture.SetData<Color>( 0, rect, scanline, 0, xnaTexture.Width );

            UpdateTexture();
        }

        /// <summary>
        /// Korvaa värin toisella värillä.
        /// </summary>
        /// <param name="src">Korvattava väri.</param>
        /// <param name="dest">Väri jolla korvataan.</param>
        /// <param name="tolerance">Kuinka paljon korvattava väri voi poiketa annetusta.</param>
        /// <param name="blend">Sävytetäänkö korvattavaa väriä sen mukaan kuinka kaukana se on alkuperäisestä väristä</param>
        /// <param name="exactAlpha">Vaaditaanko täsmälleen sama läpinäkyvyys ennen kuin korvataan</param>
        public void ReplaceColor( Color src, Color dest, double tolerance, bool blend, bool exactAlpha = false )
        {
            XnaColor srcColor = src.AsXnaColor();
            XnaColor destColor = dest.AsXnaColor();
            XnaColorConverter op = delegate( XnaColor c )
            {
                if ( exactAlpha && c.A != srcColor.A )
                    return c;

                if ( JyColor.Distance( c, srcColor ) <= tolerance )
                {
                    if ( !blend ) return destColor;
                    Vector3 srcDist = new Vector3( c.R - srcColor.R, c.G - srcColor.G, c.B - srcColor.B );
                    return new XnaColor( destColor.ToVector3() + srcDist );
                }

                return c;
            };

            ApplyPixelOperation( op );
        }

        /// <summary>
        /// Korvaa värin toisella värillä.
        /// </summary>
        /// <param name="src">Korvattava väri</param>
        /// <param name="dest">Väri jolla korvataan</param>
        public void ReplaceColor( Color src, Color dest )
        {
            XnaColor srcColor = src.AsXnaColor();
            XnaColor destColor = dest.AsXnaColor();
            XnaColorConverter op = delegate( XnaColor c )
            {
                return c == srcColor ? destColor : c;
            };

            ApplyPixelOperation( op );
        }

        /// <summary>
        /// Palauttaa kuvan jpeg-muodossa, jossa se voidaan esimerkiksi tallentaa
        /// DataStorage.Export -metodilla.
        /// </summary>
        /// <returns></returns>
        public Stream AsJpeg()
        {
            DoInitTexture();
            MemoryStream jpegStream = new MemoryStream();
            XNATexture.SaveAsJpeg( jpegStream, Width, Height );
            jpegStream.Seek( 0, SeekOrigin.Begin );
            return jpegStream;
        }

        /// <summary>
        /// Palauttaa kuvan png-muodossa, jossa se voidaan esimerkiksi tallentaa
        /// DataStorage.Export -metodilla.
        /// </summary>
        /// <returns></returns>
        public Stream AsPng()
        {
            DoInitTexture();
            MemoryStream pngStream = new MemoryStream();
            XNATexture.SaveAsPng( pngStream, Width, Height );
            pngStream.Seek( 0, SeekOrigin.Begin );
            return pngStream;
        }
    }
}

