using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Jypeli.Tests.Graphics
{
    /// <summary>
    /// Testaa Image-luokan pikselioperaatiot ilman käynnissä olevaa peliä.
    /// </summary>
    [TestFixture]
    public class ImageTest
    {
        // 2x2 RGBA PNG. Rivi 0: punainen, vihreä. Rivi 1: sininen, puoliksi läpinäkyvä valkoinen (alpha 128).
        private const string Png2x2Base64 =
            "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAYAAABytg0kAAAAE0lEQVR42mP4z8DwHwyBNAg0AABJSQl4nFEXkgAAAABJRU5ErkJggg==";

        private static readonly Color Red = new Color(255, 0, 0, 255);
        private static readonly Color Green = new Color(0, 255, 0, 255);
        private static readonly Color Blue = new Color(0, 0, 255, 255);
        private static readonly Color HalfWhite = new Color(255, 255, 255, 128);

        private static Image LoadTestPng()
        {
            return Image.FromStream(new MemoryStream(Convert.FromBase64String(Png2x2Base64)));
        }

        private static string TempPath(string extension)
        {
            return Path.Combine(Path.GetTempPath(), "jypeli-imagetest-" + Guid.NewGuid().ToString("N") + extension);
        }

        [Test]
        public void JypeliAssemblyDoesNotReferenceImageSharp()
        {
            var references = typeof(Image).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
            Assert.That(references, Has.None.StartsWith("SixLabors"), string.Join(", ", references));
        }

        [Test]
        public void FromStream_ReadsPngDimensions()
        {
            Image img = LoadTestPng();
            Assert.AreEqual(2, img.Width);
            Assert.AreEqual(2, img.Height);
        }

        [Test]
        public void FromStream_ReadsPngPixelsWithAlpha()
        {
            Image img = LoadTestPng();
            Assert.AreEqual(Red, img[0, 0]);
            Assert.AreEqual(Green, img[0, 1]);
            Assert.AreEqual(Blue, img[1, 0]);
            Assert.AreEqual(HalfWhite, img[1, 1]);
        }

        [Test]
        public void FromStream_InvalidData_Throws()
        {
            var garbage = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            Assert.Throws<ArgumentException>(() => Image.FromStream(garbage));
        }

        [Test]
        public void Constructor_FillsWithGivenColor()
        {
            Image img = new Image(3, 2, Blue);
            Assert.AreEqual(3, img.Width);
            Assert.AreEqual(2, img.Height);
            for (int row = 0; row < 2; row++)
                for (int col = 0; col < 3; col++)
                    Assert.AreEqual(Blue, img[row, col]);
        }

        [Test]
        public void Constructor_RejectsZeroSize()
        {
            Assert.Throws<ArgumentException>(() => new Image(0, 5, Blue));
        }

        [Test]
        public void Indexer_SetThenGet()
        {
            Image img = new Image(2, 2, Blue);
            img[1, 0] = Green;
            Assert.AreEqual(Green, img[1, 0]);
            Assert.AreEqual(Blue, img[0, 0]);
        }

        [Test]
        public void Fill_ReplacesAllPixels()
        {
            Image img = LoadTestPng();
            img.Fill(Green);
            Assert.AreEqual(Green, img[0, 0]);
            Assert.AreEqual(Green, img[1, 1]);
        }

        [Test]
        public void GetByteArray_IsRowMajorRgba()
        {
            Image img = LoadTestPng();
            byte[] bytes = img.GetByteArray();
            CollectionAssert.AreEqual(new byte[]
            {
                255, 0, 0, 255,   0, 255, 0, 255,
                0, 0, 255, 255,   255, 255, 255, 128
            }, bytes);
        }

        [Test]
        public void SetData_ByteArray_RoundTrips()
        {
            Image img = new Image(2, 2, Blue);
            img.SetData(new byte[]
            {
                255, 0, 0, 255,   0, 255, 0, 255,
                0, 0, 255, 255,   255, 255, 255, 128
            });
            Assert.AreEqual(Red, img[0, 0]);
            Assert.AreEqual(Green, img[0, 1]);
            Assert.AreEqual(HalfWhite, img[1, 1]);
        }

        [Test]
        public void GetData_ReturnsRegionAsColorArray()
        {
            Image img = LoadTestPng();
            Color[,] data = img.GetData(1, 0, 1, 2);
            Assert.AreEqual(2, data.GetLength(0));
            Assert.AreEqual(1, data.GetLength(1));
            Assert.AreEqual(Green, data[0, 0]);
            Assert.AreEqual(HalfWhite, data[1, 0]);
        }

        [Test]
        public void GetDataUInt_ReturnsArgb()
        {
            Image img = LoadTestPng();
            uint[,] data = img.GetDataUInt();
            Assert.AreEqual(0xFFFF0000u, data[0, 0]);
            Assert.AreEqual(0x80FFFFFFu, data[1, 1]);
        }

        [Test]
        public void Clone_IsIndependentCopy()
        {
            Image original = LoadTestPng();
            Image copy = original.Clone();
            copy[0, 0] = Green;
            Assert.AreEqual(Red, original[0, 0]);
            Assert.AreEqual(Green, copy[0, 0]);
        }

        [Test]
        public void Flip_ReversesRowsWithoutChangingOriginal()
        {
            Image original = LoadTestPng();
            Image flipped = Image.Flip(original);
            Assert.AreEqual(Blue, flipped[0, 0]);
            Assert.AreEqual(HalfWhite, flipped[0, 1]);
            Assert.AreEqual(Red, flipped[1, 0]);
            Assert.AreEqual(Green, flipped[1, 1]);
            Assert.AreEqual(Red, original[0, 0]);
        }

        [Test]
        public void Mirror_ReversesColumnsWithoutChangingOriginal()
        {
            Image original = LoadTestPng();
            Image mirrored = Image.Mirror(original);
            Assert.AreEqual(Green, mirrored[0, 0]);
            Assert.AreEqual(Red, mirrored[0, 1]);
            Assert.AreEqual(HalfWhite, mirrored[1, 0]);
            Assert.AreEqual(Blue, mirrored[1, 1]);
            Assert.AreEqual(Red, original[0, 0]);
        }

        [Test]
        public void TileHorizontal_PlacesImagesSideBySide()
        {
            Image left = LoadTestPng();
            Image right = new Image(1, 1, Green);
            Image tiled = Image.TileHorizontal(left, right);
            Assert.AreEqual(3, tiled.Width);
            Assert.AreEqual(2, tiled.Height);
            Assert.AreEqual(Red, tiled[0, 0]);
            Assert.AreEqual(Blue, tiled[1, 0]);
            Assert.AreEqual(Green, tiled[0, 2]);
            Assert.AreEqual(0, tiled[1, 2].AlphaComponent, "Uncovered area should be transparent");
        }

        [Test]
        public void TileVertical_PlacesImagesOnTopOfEachOther()
        {
            Image top = new Image(1, 1, Green);
            Image bottom = LoadTestPng();
            Image tiled = Image.TileVertical(top, bottom);
            Assert.AreEqual(2, tiled.Width);
            Assert.AreEqual(3, tiled.Height);
            Assert.AreEqual(Green, tiled[0, 0]);
            Assert.AreEqual(0, tiled[0, 1].AlphaComponent, "Uncovered area should be transparent");
            Assert.AreEqual(Red, tiled[1, 0]);
            Assert.AreEqual(HalfWhite, tiled[2, 1]);
        }

        [Test]
        public void Area_ExtractsSubImage()
        {
            Image img = LoadTestPng();
            Image area = img.Area(1, 0, 2, 2);
            Assert.AreEqual(1, area.Width);
            Assert.AreEqual(2, area.Height);
            Assert.AreEqual(Green, area[0, 0]);
            Assert.AreEqual(HalfWhite, area[1, 0]);
        }

        [Test]
        public void Rescale_ChangesDimensions()
        {
            Image img = LoadTestPng();
            img.Rescale(5, 3);
            Assert.AreEqual(5, img.Width);
            Assert.AreEqual(3, img.Height);
        }

        [Test]
        public void Rescale_KeepsUniformColor()
        {
            Image img = new Image(2, 2, Blue);
            img.Rescale(7, 5);
            for (int row = 0; row < 5; row++)
                for (int col = 0; col < 7; col++)
                    Assert.AreEqual(Blue, img[row, col]);
        }

        [Test]
        public void Rescale_UpscaledCornersKeepSourceColors()
        {
            Image img = LoadTestPng();
            img.Rescale(8, 8);
            Assert.AreEqual(Red, img[0, 0]);
            Assert.AreEqual(Green, img[0, 7]);
            Assert.AreEqual(Blue, img[7, 0]);
            Assert.AreEqual(HalfWhite, img[7, 7]);
        }

        [Test]
        public void ReplaceColor_ReplacesExactMatches()
        {
            Image img = LoadTestPng();
            img.ReplaceColor(Red, Green);
            Assert.AreEqual(Green, img[0, 0]);
            Assert.AreEqual(Blue, img[1, 0]);
        }

        [Test]
        public void SaveAsPng_ThenFromFile_RoundTrips()
        {
            string path = TempPath(".png");
            try
            {
                LoadTestPng().SaveAsPng(path);
                Image loaded = Image.FromFile(path);
                Assert.AreEqual(2, loaded.Width);
                Assert.AreEqual(2, loaded.Height);
                Assert.AreEqual(Red, loaded[0, 0]);
                Assert.AreEqual(Green, loaded[0, 1]);
                Assert.AreEqual(Blue, loaded[1, 0]);
                Assert.AreEqual(HalfWhite, loaded[1, 1]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void SaveAsBmp_ThenFromFile_RoundTrips()
        {
            string path = TempPath(".bmp");
            try
            {
                LoadTestPng().SaveAsBmp(path);
                Image loaded = Image.FromFile(path);
                Assert.AreEqual(2, loaded.Width);
                Assert.AreEqual(2, loaded.Height);
                Assert.AreEqual(Red, loaded[0, 0]);
                Assert.AreEqual(Green, loaded[0, 1]);
                Assert.AreEqual(Blue, loaded[1, 0]);
                Assert.AreEqual(HalfWhite, loaded[1, 1]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void SaveAsBmp_ToStream_FlushesEveryByteBeforeReturning()
        {
            // Game.cs antaa tallennukselle FileStreamin, jota se ei sulje. Tiedoston on oltava
            // kokonainen heti tallennuksen jälkeen, kuten se oli ImageSharpin aikana.
            string path = TempPath(".bmp");
            FileStream fs = new FileStream(path, FileMode.Create);
            try
            {
                LoadTestPng().SaveAsBmp(fs);
                Assert.That(fs.Position, Is.GreaterThan(0), "Nothing was written to the stream");
                Assert.AreEqual(fs.Position, new FileInfo(path).Length, "Bytes are still buffered in the stream");
            }
            finally
            {
                fs.Dispose();
            }

            try
            {
                byte[] bmp = File.ReadAllBytes(path);
                int declaredSize = BitConverter.ToInt32(bmp, 2);
                Assert.AreEqual(declaredSize, bmp.Length, "BMP header size does not match the file size");
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void SaveAsPng_ToStream_FlushesEveryByteBeforeReturning()
        {
            string path = TempPath(".png");
            FileStream fs = new FileStream(path, FileMode.Create);
            try
            {
                LoadTestPng().SaveAsPng(fs);
                Assert.AreEqual(fs.Position, new FileInfo(path).Length);
                Assert.That(fs.Position, Is.GreaterThan(0));
            }
            finally
            {
                fs.Dispose();
                File.Delete(path);
            }
        }

        [Test]
        public void SaveAsJpeg_ProducesLoadableImage()
        {
            string path = TempPath(".jpg");
            try
            {
                new Image(16, 8, Blue).SaveAsJpeg(path);
                Image loaded = Image.FromFile(path);
                Assert.AreEqual(16, loaded.Width);
                Assert.AreEqual(8, loaded.Height);
                Color c = loaded[4, 4];
                Assert.That(c.BlueComponent, Is.GreaterThan(200), "JPEG should stay roughly blue");
                Assert.That(c.RedComponent, Is.LessThan(40));
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
