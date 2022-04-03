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
            int w = (int)Game.Screen.Width;
            int h = (int)Game.Screen.Height;
            var bytes = new byte[w * h * sizeof(Rgba32)];
            fixed (void* ptr = bytes)
                Game.GraphicsDevice.GetScreenContents(ptr);

            Image<Rgba32> img = Image<Rgba32>.LoadPixelData<Rgba32>(bytes, w, h);

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