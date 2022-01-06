using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Jypeli
{
    internal static class Screencap
    {
        public unsafe static Image<Rgba32> CaptureRaw()
        {
            Image<Rgba32> img = new Image<Rgba32>((int)Game.Screen.Width, (int)Game.Screen.Height);
            var a = img.TryGetSinglePixelSpan(out Span<Rgba32> pixels);
            fixed (void* ptr = pixels)
                Game.GraphicsDevice.GetScreenContents(ptr);

            img.Mutate(i => i.Flip(FlipMode.Vertical));
            return img;
        }

        public unsafe static Image Capture()
        {
            using Image<Rgba32> img = CaptureRaw();
            return new Image(img);
        }

        public static void SavePng(string filename)
        {
            using Image<Rgba32> img = CaptureRaw();
            img.SaveAsPng(filename);
        }

        public static void SaveBmp(string filename)
        {
            using Image<Rgba32> img = CaptureRaw();
            img.SaveAsBmp(filename); // TODO: Olisiko SaveAsBmpAsync parempi?
        }

        public static void SaveBmp(Stream stream)
        {
            using Image<Rgba32> img = CaptureRaw();
            img.SaveAsBmp(stream);
        }
    }
}