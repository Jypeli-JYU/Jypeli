using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

using StbImageSharp;
using StbImageWriteSharp;

using ColorConverter = System.Converter<Jypeli.Color, Jypeli.Color>;
using System.Numerics;

namespace Jypeli
{
    /// <summary>
    /// Kuvan skaalausasetus
    /// </summary>
    public enum ImageScaling
    {
        /// <summary>
        /// Lineaarinen interpolointi (Sumentaa/pehmentää)
        /// </summary>
        Linear,
        /// <summary>
        /// Lähin pikseli (Pikseligrafiikalle oikea valinta)
        /// </summary>
        Nearest
    }
    /// <summary>
    /// Kuva.
    /// </summary>
    public class Image
    {
        private string assetName;

        internal static string[] ImageExtensions { get; } = { ".png", ".jpg", ".bmp", ".gif", ".tga" };

        /// <summary>
        /// Tavuja per pikseli (RGBA).
        /// </summary>
        internal const int BytesPerPixel = 4;

        private int width;
        private int height;

        /// <summary>
        /// Kuvan pikselit rivi kerrallaan vasemmasta ylänurkasta alkaen.
        /// Jokaisesta pikselistä on neljä tavua järjestyksessä punainen, vihreä, sininen, läpinäkyvyys.
        /// </summary>
        internal byte[] data;

        /// <summary>
        /// Kuvan kahva näytönohjaimessa
        /// </summary>
        internal uint handle;

        /// <summary>
        /// Onko kuvan dataa muutettu ja se pitää viedä uudestaan näytönohjaimelle
        /// </summary>
        internal bool dirty;

        private ImageScaling scaling;

        /// <summary>
        /// Kuinka kuvan kokoa skaalataan ruudulle piirrettäessä.
        /// </summary>
        public ImageScaling Scaling
        {
            get => scaling;
            set
            {
                scaling = value;
                Game.GraphicsDevice.UpdateTextureScaling(this); // TODO: Pitäisikö tämä tehdä samoin kuin datan muokkaus, eli vasta piirtovaiheessa?
            }
        }

        /// <summary>
        /// Leveys pikseleinä.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Korkeus pikseleinä.
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Nimi.
        /// </summary>
        public string Name
        {
            get { return assetName; }
        }

        /// <summary>
        /// Kuvan koko
        /// </summary>
        public Vector Size { get => new Vector(Width, Height); }

        internal Image(int width, int height)
        {
            AssertDimensions(width, height);
            CreateNewTexture(width, height, Color.Black);
        }

        /// <summary>
        /// Luo kuvan StorageFile-oliosta.
        /// </summary>
        /// <param name="f"></param>
        public Image(StorageFile f) : this(f.Stream)
        {
        }

        internal Image(Stream s)
        {
            Load(s);
        }

        internal Image(string assetName)
        {
            using (FileStream fs = File.OpenRead(assetName))
                Load(fs);
            this.assetName = assetName;
        }

        internal Image()
        {

        }

        /// <summary>
        /// Luo kuvan valmiista RGBA-pikselidatasta. Taulukkoa ei kopioida.
        /// </summary>
        internal Image(int width, int height, byte[] rgba)
        {
            AssertDimensions(width, height);
            AssertDataLength(width, height, rgba);
            this.width = width;
            this.height = height;
            data = rgba;
        }

        /// <summary>
        /// Luo uuden kuvan.
        /// </summary>
        /// <param name="width">Kuvan leveys</param>
        /// <param name="height">Kuvan korkeus</param>
        /// <param name="backColor">Kuvan taustaväri</param>
        public Image(double width, double height, Color backColor)
            : this((int)Math.Round(width), (int)Math.Round(height), backColor)
        {
        }

        /// <summary>
        /// Luo uuden kuvan.
        /// </summary>
        /// <param name="width">Kuvan leveys</param>
        /// <param name="height">Kuvan korkeus</param>
        /// <param name="color">Kuvan väri</param>
        public Image(int width, int height, Color color)
        {
            AssertDimensions(width, height);
            assetName = null;
            CreateNewTexture(width, height, color);
        }

        private void Load(Stream s)
        {
            ImageResult result;
            try
            {
                result = ImageResult.FromStream(s, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Could not load image: " + e.Message, e);
            }

            width = result.Width;
            height = result.Height;
            data = result.Data;
        }

        private void CreateNewTexture(int width, int height, Color color)
        {
            this.width = width;
            this.height = height;
            data = new byte[width * height * BytesPerPixel];
            FillData(color);
        }

        private void FillData(Color color)
        {
            for (int i = 0; i < data.Length; i += BytesPerPixel)
            {
                data[i] = color.RedComponent;
                data[i + 1] = color.GreenComponent;
                data[i + 2] = color.BlueComponent;
                data[i + 3] = color.AlphaComponent;
            }
        }

        private int Offset(int row, int col)
        {
            if ((uint)row >= (uint)height)
                throw new ArgumentOutOfRangeException(nameof(row), row, $"Row must be between 0 and {height - 1}");
            if ((uint)col >= (uint)width)
                throw new ArgumentOutOfRangeException(nameof(col), col, $"Column must be between 0 and {width - 1}");
            return (row * width + col) * BytesPerPixel;
        }

        private Color ColorAt(int offset)
        {
            return new Color(data[offset], data[offset + 1], data[offset + 2], data[offset + 3]);
        }

        private uint ArgbAt(int offset)
        {
            return (uint)(data[offset + 3] << 24 | data[offset] << 16 | data[offset + 1] << 8 | data[offset + 2]);
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
                return ColorAt(Offset(row, col));
            }
            set
            {
                int i = Offset(row, col);
                data[i] = value.RedComponent;
                data[i + 1] = value.GreenComponent;
                data[i + 2] = value.BlueComponent;
                data[i + 3] = value.AlphaComponent;
                dirty = true;
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
        public Color[,] GetData(int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue)
        {
            int ny = Height;
            if (h < ny)
                ny = h;
            if (Height < ny + oy)
                ny = Height - oy;
            int nx = Width;
            if (w < nx)
                nx = w;
            if (Width < nx + ox)
                nx = Width - ox;
            if (nx <= 0 || ny <= 0)
                return new Color[0, 0];

            Color[,] bmp = new Color[ny, nx];

            for (int i = oy; i < oy + ny; i++)
            {
                for (int j = ox; j < ox + nx; j++)
                {
                    bmp[i - oy, j - ox] = ColorAt(Offset(i, j));
                }
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
        /// <param name="h">alueen korkaus johon kopioidaan</param>
        /// <returns>pikselit Color-taulukkona</returns>
        public void SetData(Color[,] bmp, int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue)
        {
            int ny = bmp.GetLength(0);
            int nx = bmp.GetLength(1);
            if (ny > Height)
                ny = Height;
            if (nx > Width)
                nx = Height;
            if (ny > h)
                ny = h;
            if (nx > w)
                nx = w;
            if (Height < ny + oy)
                ny = Height - oy;
            if (Width < nx + ox)
                nx = Width - ox;
            if (nx <= 0 || ny <= 0)
                return;

            // TODO: Onko indeksointioperaatio kuinka hidas/nopea verrattuna muihin tapoihin muokata kuvaa?
            // TODO: Testaa kaikki Set/GetData metodit...

            for (int iy = oy; iy < ny; iy++)
            {
                for (int ix = ox; ix < nx; ix++)
                {
                    this[iy, ix] = bmp[iy - oy, ix - ox];
                }
            }
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
            AssertDimensions(width, height);
            AssertDataLength(width, height, byteArr);
            this.width = width;
            this.height = height;
            data = (byte[])byteArr.Clone();
            dirty = true;
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
            dirty = true;
        }

        /// <summary>
        /// Kuvan pikselit byte-taulukkona.
        /// Tavut ovat järjestyksessä punainen, vihreä, sininen, läpinäkyvyys.
        /// </summary>
        /// <returns>pikselit byte-taulukkona</returns>
        public byte[] GetByteArray()
        {
            return (byte[])data.Clone();
        }

        /// <summary>
        /// Palalutetaan kuvan pikselit ARGB-uint[,] -taulukkona
        /// </summary>
        /// <param name="ox">siirtymä x-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="oy">siirtymä y-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="w">alueen leveys joka kopioidaan</param>
        /// <param name="h">alueen korkaus joka kopioidaan</param>
        /// <returns>Kuvan pikselit ARGB-taulukkona</returns>
        public uint[,] GetDataUInt(int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue)
        {
            int ny = Height;
            if (h < ny)
                ny = h;
            if (Height < ny + oy)
                ny = Height - oy;
            int nx = Width;
            if (w < nx)
                nx = w;
            if (Width < nx + ox)
                nx = Width - ox;
            if (nx <= 0 || ny <= 0)
                return new uint[0, 0];

            uint[,] bmp = new uint[ny, nx];

            for (int i = oy; i < oy + ny; i++)
            {
                for (int j = ox; j < ox + nx; j++)
                {
                    bmp[i - oy, j - ox] = ArgbAt(Offset(i, j));
                }
            }

            return bmp;
        }


        /// <summary>
        /// Palalutetaan kuvan pikselit ARGB-uint[][] -taulukkona
        /// </summary>
        /// <param name="ox">siirtymä x-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="oy">siirtymä y-suunnassa vasemmasta ylänurkasta</param>
        /// <param name="w">alueen leveys joka kopioidaan</param>
        /// <param name="h">alueen korkaus joka kopioidaan</param>
        /// <returns>Kuvan pikselit ARGB-taulukkona</returns>
        public uint[][] GetDataUIntAA(int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue)
        {
            int ny = Height;
            if (h < ny)
                ny = h;
            if (Height < ny + oy)
                ny = Height - oy;
            int nx = Width;
            if (w < nx)
                nx = w;
            if (Width < nx + ox)
                nx = Width - ox;
            if (nx <= 0 || ny <= 0)
                return Array.Empty<uint[]>();

            uint[][] bmp = new uint[ny][];

            for (int i = oy; i < oy + ny; i++)
            {
                bmp[i - oy] = new uint[nx];
                for (int j = ox; j < ox + nx; j++)
                {
                    bmp[i - oy][j - ox] = ArgbAt(Offset(i, j));
                }
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
        public void SetData(uint[,] bmp, int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue)
        {
            int ny = bmp.GetLength(0);
            int nx = bmp.GetLength(1);
            if (ny > Height)
                ny = Height;
            if (nx > Width)
                nx = Width;
            if (ny > h)
                ny = h;
            if (nx > w)
                nx = w;
            if (Height < ny + oy)
                ny = Height - oy;
            if (Width < nx + ox)
                nx = Width - ox;

            if (nx <= 0 || ny <= 0)
                return;

            for (int iy = oy; iy < ny; iy++)
            {
                for (int ix = ox; ix < nx; ix++)
                {
                    this[iy, ix] = Color.UIntToColor(bmp[iy - oy, ix - ox]);
                }
            }
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
        public void SetData(uint[][] bmp, int ox = 0, int oy = 0, int w = int.MaxValue, int h = int.MaxValue)
        {
            int ny = bmp.Length;
            int nx = bmp[0].Length;
            if (ny > Height)
                ny = Height;
            if (nx > Width)
                nx = Width;
            if (ny > h)
                ny = h;
            if (nx > w)
                nx = w;
            if (nx <= 0 || ny <= 0)
                return;

            for (int iy = oy; iy < ny; iy++)
            {
                for (int ix = ox; ix < nx; ix++)
                {
                    this[iy, ix] = Color.UIntToColor(bmp[iy - oy][ix - ox]);
                }
            }
            UpdateTexture();
        }

        private static void AssertDimensions(int width, int height)
        {
            if (width < 1 || height < 1)
                throw new ArgumentException(String.Format("Image dimensions must be at least 1 x 1! (given: {0} x {1}", width, height));
        }

        private static void AssertDataLength(int width, int height, byte[] rgba)
        {
            if (rgba == null)
                throw new ArgumentNullException(nameof(rgba));
            long expected = (long)width * height * BytesPerPixel;
            if (rgba.Length != expected)
                throw new ArgumentException($"Pixel data length {rgba.Length} does not match {width} x {height} x {BytesPerPixel} = {expected}");
        }

        /// <summary>
        /// Luo kopion kuvasta
        /// </summary>
        /// <returns></returns>
        public Image Clone()
        {
            Image copy = new Image();
            copy.width = width;
            copy.height = height;
            copy.data = (byte[])data.Clone();
            copy.scaling = scaling;

            return copy;
        }

        /// <summary>
        /// Suorittaa annetun pikselioperaation koko kuvalle.
        /// </summary>
        /// <param name="operation">Aliohjelma, joka ottaa värin ja palauttaa värin</param>
        public void ApplyPixelOperation(ColorConverter operation)
        {
            Color[,] data = GetData();

            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    data[i, j] = operation(data[i, j]);
                }
            }
            SetData(data);
            UpdateTexture();
        }

        private void UpdateTexture()
        {
            dirty = true;
        }

        /// <summary>
        /// Kääntää kuvan ylösalaisin paikallaan.
        /// </summary>
        internal void FlipVertical()
        {
            int stride = width * BytesPerPixel;
            byte[] tmp = new byte[stride];
            for (int top = 0, bottom = height - 1; top < bottom; top++, bottom--)
            {
                Buffer.BlockCopy(data, top * stride, tmp, 0, stride);
                Buffer.BlockCopy(data, bottom * stride, data, top * stride, stride);
                Buffer.BlockCopy(tmp, 0, data, bottom * stride, stride);
            }
            dirty = true;
        }

        /// <summary>
        /// Peilaa kuvan vaakasuunnassa paikallaan.
        /// </summary>
        internal void FlipHorizontal()
        {
            for (int row = 0; row < height; row++)
            {
                int rowStart = row * width * BytesPerPixel;
                for (int left = 0, right = width - 1; left < right; left++, right--)
                {
                    int a = rowStart + left * BytesPerPixel;
                    int b = rowStart + right * BytesPerPixel;
                    for (int k = 0; k < BytesPerPixel; k++)
                    {
                        byte t = data[a + k];
                        data[a + k] = data[b + k];
                        data[b + k] = t;
                    }
                }
            }
            dirty = true;
        }

        /// <summary>
        /// Kopioi toisen kuvan pikselit tämän kuvan päälle annettuun kohtaan.
        /// Kohdealueen ulkopuolelle jäävät pikselit jätetään huomiotta.
        /// </summary>
        private void Blit(Image source, int destX, int destY)
        {
            int copyWidth = Math.Min(source.width, width - destX);
            int copyHeight = Math.Min(source.height, height - destY);
            if (copyWidth <= 0 || copyHeight <= 0)
                return;

            int rowBytes = copyWidth * BytesPerPixel;
            for (int row = 0; row < copyHeight; row++)
            {
                Buffer.BlockCopy(source.data, row * source.width * BytesPerPixel, data, ((destY + row) * width + destX) * BytesPerPixel, rowBytes);
            }
            dirty = true;
        }

        #region static methods


        /// <summary>
        /// Lataa kuvan tiedostosta.
        /// </summary>
        /// <param name="path">Tiedoston polku päätteineen.</param>
        public static Image FromFile(string path)
        {
            Image img = new Image(path);
            return img;
        }

        /// <summary>
        /// Lataa kuvan tiedostovirrasta.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Image FromStream(Stream stream)
        {
            return new Image(stream);
        }

        /// <summary>
        /// Lataa kuvan Internetistä.
        /// </summary>
        /// <param name="url">Kuvan URL-osoite</param>
        /// <returns>Kuva</returns>
        public static Image FromURL(string url)
        {
            var req = FileManager.Client.GetAsync(url);
            req.Wait();
            using (Stream s = req.Result.Content.ReadAsStream())
                return new Image(s);
        }

        /// <summary>
        /// Luo tähtitaivaskuvan.
        /// </summary>
        /// <param name="width">Tekstuurin leveys.</param>
        /// <param name="height">Tekstuurin korkeus.</param>
        /// <param name="stars">Tähtien määrä.</param>
        /// <param name="transparent">Onko tausta läpinäkyvä vai ei (jolloin siitä tulee täysin musta)</param>
        /// <returns>Tekstuuri.</returns>
        public static Image CreateStarSky(int width, int height, int stars, bool transparent = false)
        {
            Image img = new Image(width, height, transparent ? Color.Transparent : Color.Black);

            // Random stars
            for (int j = 0; j < stars; j++)
            {
                int px = RandomGen.NextInt(0, width);
                int py = RandomGen.NextInt(0, height);

                int radius = RandomGen.NextInt(2, 10) / 2;
                Color starcolor = RandomGen.NextColor(Color.White, new Color(192, 192, 192, 255));

                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        if (x * x + y * y <= radius * radius && px + x > 0 && px + x < width && py + y > 0 && py + y < height)
                        {
                            img[py + y, px + x] = starcolor;
                        }

                    }
                }
            }
            return img;
        }

        /// <summary>
        /// Luo kuvan tekstistä.
        /// </summary>
        /// <param name="text">Teksti josta kuva luodaan</param>
        /// <param name="font">Fontti</param>
        /// <param name="textColor">Tekstin väri</param>
        /// <param name="backgroundColor">Tekstin taustaväri</param>
        /// <returns>Teksti kuvana</returns>
        public static Image FromText(string text, Font font, Color textColor, Color backgroundColor)
        {
            if (text == null)
                text = "";

            var device = Game.GraphicsDevice;

            Vector textDims = font.MeasureSize(text);
            int textw = (textDims.X > 1) ? Convert.ToInt32(textDims.X) : 1;
            int texth = (textDims.Y > 1) ? Convert.ToInt32(textDims.Y) : 1;

            Rendering.IRenderTarget rt = Game.GraphicsDevice.CreateRenderTarget((uint)textw, (uint)texth);

            device.SetRenderTarget(rt);
            device.Clear(backgroundColor);

            Matrix4x4 ProjectionMatrix = Matrix4x4.CreateOrthographic(
                textw,
                texth,
                1, 2
            );

            Matrix4x4 temp = Graphics.ViewProjectionMatrix;
            Graphics.ViewProjectionMatrix = ProjectionMatrix;

            Renderer.DrawText(text, Vector.Zero + new Vector(0, texth / 2), font, textColor, Vector.One);
            Graphics.CustomBatch.Flush(); // TODO: Joku DrawTextImmediately tms. Voiko tämä mennä jossain tilanteissa nyt pieleen?

            Graphics.ViewProjectionMatrix = temp;

            Image img = new Image(textw, texth);
            device.GetScreenContentsToImage(img);

            device.SetRenderTarget(null);

            return Flip(img);
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
        public static Image DrawTextOnImage(Image img, string text, Vector position, Font font, Color textColor, Color backgroundColor)
        {
            if (text == null)
                text = "";

            var device = Game.GraphicsDevice;

            Vector textDims = font.MeasureSize(text);
            int textw = (textDims.X > 1) ? Convert.ToInt32(textDims.X) : 1;
            int texth = (textDims.Y > 1) ? Convert.ToInt32(textDims.Y) : 1;

            Rendering.IRenderTarget rt = Game.GraphicsDevice.CreateRenderTarget((uint)img.Width, (uint)img.Height);

            device.SetRenderTarget(rt);
            device.Clear(backgroundColor);

            Matrix4x4 ProjectionMatrix = Matrix4x4.CreateOrthographic(
                img.Width,
                img.Height,
                1, 2
            );

            Matrix4x4 temp = Graphics.ViewProjectionMatrix;
            Graphics.ViewProjectionMatrix = ProjectionMatrix;

            Renderer.DrawImage(Matrix4x4.Identity, img, new Rendering.TextureCoordinates(), Vector.Zero, img.Size, 0);
            Renderer.DrawText(text, position + new Vector(0, texth / 2), font, textColor, Vector.One);
            Graphics.CustomBatch.Flush();

            Graphics.ViewProjectionMatrix = temp;

            Image tex = new Image(img.Width, img.Height);
            device.GetScreenContentsToImage(tex);

            device.SetRenderTarget(null);

            return Flip(tex);
        }

        /// <summary>
        /// Piirtää tekstiä kuvan päälle keskelle kuvaa.
        /// </summary>
        /// <param name="img">Kuva jonka päälle piirretään</param>
        /// <param name="text">Piirrettävä teksti</param>
        /// <param name="font">Fontti</param>
        /// <param name="textColor">Tekstin väri</param>
        /// <returns>Kuva tekstin kanssa</returns>
        public static Image DrawTextOnImage(Image img, string text, Font font, Color textColor)
        {
            return DrawTextOnImage(img, text, Vector.Zero, font, textColor, Jypeli.Color.Transparent);
        }

        //TODO: Ehkä mielummin CreateGradient...
        /// <summary>
        /// Luo pystysuuntaisen liukuväritetyn kuvan.
        /// </summary>
        /// <param name="imageWidth">kuvan leveys.</param>
        /// <param name="imageHeight">kuvan korkeus.</param>
        /// <param name="lowerColor">Alareunassa käytettävä väri.</param>
        /// <param name="upperColor">Yläreunassa käytettävä väri.</param>
        /// <returns>Väritetty kuva.</returns>
        public static Image FromGradient(int imageWidth, int imageHeight, Color lowerColor, Color upperColor)
        {
            Image img = new Image(imageWidth, imageHeight);

            for (int ver = 0; ver < imageHeight; ver++)
            {
                for (int hor = 0; hor < imageWidth; hor++)
                {
                    img[ver, hor] = Color.Lerp(lowerColor, upperColor, (float)ver / (float)imageHeight);
                }
            }

            return img;
        }

        /// <summary>
        /// Luo yksivärisen kuvan.
        /// </summary>
        /// <param name="width">Kuvan leveys.</param>
        /// <param name="height">Kuvan korkeus.</param>
        /// <param name="color">Kuvan väri.</param>
        /// <returns>Väritetty kuva.</returns>
        public static Image FromColor(int width, int height, Color color)
        {
            return new Image(width, height, color);
        }

        /// <summary>
        /// Peilaa kuvan X-suunnassa.
        /// </summary>
        /// <param name="image">Peilattava kuva.</param>
        /// <returns>Peilattu kuva.</returns>
        public static Image Mirror(Image image)
        {
            Image img = image.Clone();
            img.FlipHorizontal();
            return img;
        }

        /// <summary>
        /// Peilaa kuvat X-suunnassa.
        /// </summary>
        /// <param name="images">Peilattavat kuvat.</param>
        /// <returns>Peilatut kuvat.</returns>
        public static Image[] Mirror(Image[] images)
        {
            Image[] result = new Image[images.Length];
            for (int i = 0; i < images.Length; i++)
                result[i] = Mirror(images[i]);
            return result;
        }

        // TODO: Näissä on tyhmä nimi
        /// <summary>
        /// Peilaa kuvan Y-suunnassa.
        /// </summary>
        /// <param name="image">Peilattava kuva.</param>
        /// <returns>Peilattu kuva.</returns>
        public static Image Flip(Image image)
        {
            Image img = image.Clone();
            img.FlipVertical();
            return img;
        }

        /// <summary>
        /// Peilaa kuvat Y-suunnassa.
        /// </summary>
        /// <param name="images">Peilattavat kuvat.</param>
        /// <returns>Peilatut kuvat.</returns>
        public static Image[] Flip(Image[] images)
        {
            Image[] result = new Image[images.Length];
            for (int i = 0; i < images.Length; i++)
                result[i] = Flip(images[i]);
            return result;
        }

        /// <summary>
        /// Yhditää kaksi kuvaa olemaan vierekkäin uudessa kuvassa.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Image TileHorizontal(Image left, Image right)
        {
            int width = left.Width + right.Width;
            int height = Math.Max(left.Height, right.Height);

            Image img = new Image(width, height, Color.Transparent);
            img.Blit(left, 0, 0);
            img.Blit(right, left.Width, 0);
            return img;
        }

        /// <summary>
        /// Yhdistää kaksi kuvaa olemaan päällekkäin uudessa kuvassa
        /// </summary>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public static Image TileVertical(Image top, Image bottom)
        {
            int width = Math.Max(top.Width, bottom.Width);
            int height = top.Height + bottom.Height;

            Image img = new Image(width, height, Color.Transparent);
            img.Blit(top, 0, 0);
            img.Blit(bottom, 0, top.Height);
            return img;
        }

        /// <summary>
        /// Skaalaa kuvan annettuun resoluutioon.
        /// Käyttää lineaarista interpolointia, paitsi jos kuvan <see cref="Scaling"/> on <see cref="ImageScaling.Nearest"/>.
        /// </summary>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public void Rescale(int newWidth, int newHeight)
        {
            AssertDimensions(newWidth, newHeight);
            if (newWidth == width && newHeight == height)
                return;

            byte[] result = scaling == ImageScaling.Nearest
                ? ResampleNearest(newWidth, newHeight)
                : ResampleBilinear(newWidth, newHeight);

            width = newWidth;
            height = newHeight;
            data = result;
            dirty = true;
        }

        private byte[] ResampleNearest(int newWidth, int newHeight)
        {
            byte[] result = new byte[newWidth * newHeight * BytesPerPixel];
            for (int y = 0; y < newHeight; y++)
            {
                int srcY = Math.Min(height - 1, (int)(((long)y * height) / newHeight));
                for (int x = 0; x < newWidth; x++)
                {
                    int srcX = Math.Min(width - 1, (int)(((long)x * width) / newWidth));
                    Buffer.BlockCopy(data, (srcY * width + srcX) * BytesPerPixel, result, (y * newWidth + x) * BytesPerPixel, BytesPerPixel);
                }
            }
            return result;
        }

        private byte[] ResampleBilinear(int newWidth, int newHeight)
        {
            // Interpolointi tehdään alfalla kerrotuilla väreillä, jotta läpinäkyvien pikselien värit eivät vuoda viereisiin pikseleihin.
            byte[] result = new byte[newWidth * newHeight * BytesPerPixel];
            double scaleX = (double)width / newWidth;
            double scaleY = (double)height / newHeight;

            for (int y = 0; y < newHeight; y++)
            {
                double srcY = Math.Clamp((y + 0.5) * scaleY - 0.5, 0, height - 1);
                int y0 = (int)srcY;
                int y1 = Math.Min(y0 + 1, height - 1);
                double fy = srcY - y0;

                for (int x = 0; x < newWidth; x++)
                {
                    double srcX = Math.Clamp((x + 0.5) * scaleX - 0.5, 0, width - 1);
                    int x0 = (int)srcX;
                    int x1 = Math.Min(x0 + 1, width - 1);
                    double fx = srcX - x0;

                    double w00 = (1 - fx) * (1 - fy);
                    double w10 = fx * (1 - fy);
                    double w01 = (1 - fx) * fy;
                    double w11 = fx * fy;

                    int i00 = (y0 * width + x0) * BytesPerPixel;
                    int i10 = (y0 * width + x1) * BytesPerPixel;
                    int i01 = (y1 * width + x0) * BytesPerPixel;
                    int i11 = (y1 * width + x1) * BytesPerPixel;

                    double a00 = data[i00 + 3] * w00;
                    double a10 = data[i10 + 3] * w10;
                    double a01 = data[i01 + 3] * w01;
                    double a11 = data[i11 + 3] * w11;
                    double alpha = a00 + a10 + a01 + a11;

                    int o = (y * newWidth + x) * BytesPerPixel;
                    if (alpha > 0)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            double premultiplied = data[i00 + c] * a00 + data[i10 + c] * a10 + data[i01 + c] * a01 + data[i11 + c] * a11;
                            result[o + c] = (byte)Math.Clamp((int)Math.Round(premultiplied / alpha), 0, 255);
                        }
                    }
                    result[o + 3] = (byte)Math.Clamp((int)Math.Round(alpha), 0, 255);
                }
            }
            return result;
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
        public Image Area(int left, int top, int right, int bottom)
        {
            int width = right - left;
            int height = bottom - top;

            if (width <= 0)
                throw new ArgumentException("Left coordinate must be less than right coordinate");
            if (height <= 0)
                throw new ArgumentException("Top coordinate must be less than bottom coordinate");

            Color[,] data = new Color[height, width];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    data[j, i] = this[top + j, left + i];
                }
            }

            Image img = new Image(width, height);
            img.SetData(data);

            return img;
        }

        /// <summary>
        /// Täyttää kuvan värillä
        /// </summary>
        /// <param name="backColor"></param>
        public void Fill(Color backColor)
        {
            FillData(backColor);

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
        public void ReplaceColor(Color src, Color dest, double tolerance, bool blend, bool exactAlpha = false)
        {
            Color op(Color c)
            {
                if (exactAlpha && c.AlphaComponent != src.AlphaComponent)
                    return c;

                if (Color.Distance(c, src) <= tolerance)
                {
                    if (!blend)
                        return dest;
                    return Color.Mix(c, dest);
                }

                return c;
            }

            ApplyPixelOperation(op);
        }

        /// <summary>
        /// Korvaa värin toisella värillä.
        /// </summary>
        /// <param name="src">Korvattava väri</param>
        /// <param name="dest">Väri jolla korvataan</param>
        public void ReplaceColor(Color src, Color dest)
        {
            Color op(Color c)
            {
                return c == src ? dest : c;
            }

            ApplyPixelOperation(op);
        }

        /// <summary>
        /// Tallentaa kuvan jpg-muodossa
        /// </summary>
        /// <param name="path">Tiedoston nimi</param>
        public void SaveAsJpeg(string path)
        {
            using (FileStream fs = File.Create(path))
                SaveAsJpeg(fs);
        }

        /// <summary>
        /// Tallentaa kuvan jpg-muodossa tietovirtaan
        /// </summary>
        /// <param name="stream">Tietovirta johon kuva kirjoitetaan</param>
        public void SaveAsJpeg(Stream stream)
        {
            new ImageWriter().WriteJpg(data, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream, 90);
            stream.Flush();
        }

        /// <summary>
        /// Tallentaa kuvan png-muodossa
        /// </summary>
        /// <param name="path">Tiedoston nimi</param>
        public void SaveAsPng(string path)
        {
            using (FileStream fs = File.Create(path))
                SaveAsPng(fs);
        }

        /// <summary>
        /// Tallentaa kuvan png-muodossa tietovirtaan
        /// </summary>
        /// <param name="stream">Tietovirta johon kuva kirjoitetaan</param>
        public void SaveAsPng(Stream stream)
        {
            new ImageWriter().WritePng(data, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            stream.Flush();
        }

        /// <summary>
        /// Tallentaa kuvan bmp-muodossa
        /// </summary>
        /// <param name="path">Tiedoston nimi</param>
        public void SaveAsBmp(string path)
        {
            using (FileStream fs = File.Create(path))
                SaveAsBmp(fs);
        }

        /// <summary>
        /// Tallentaa kuvan bmp-muodossa tietovirtaan
        /// </summary>
        /// <param name="stream">Tietovirta johon kuva kirjoitetaan</param>
        public void SaveAsBmp(Stream stream)
        {
            new ImageWriter().WriteBmp(data, width, height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            stream.Flush();
        }
    }
}
