using System.IO;

namespace Jypeli
{
    internal static class Screencap
    {
        /// <summary>
        /// Lukee näytön (tai valitun piirtokohteen) sisällön kuvaksi.
        /// </summary>
        public unsafe static Image Capture()
        {
            int w = (int)Game.Screen.Width;
            int h = (int)Game.Screen.Height;
            var bytes = new byte[w * h * Image.BytesPerPixel];
            fixed (void* ptr = bytes)
                Game.GraphicsDevice.GetScreenContents(ptr);

            // OpenGL lukee pikselit alhaalta ylös, joten kuva on käännettävä.
            Image img = new Image(w, h, bytes);
            img.FlipVertical();
            return img;
        }

        public static void SavePng(string filename)
        {
            Capture().SaveAsPng(filename);
        }

        public static void SaveBmp(string filename)
        {
            Capture().SaveAsBmp(filename);
        }

        public static void SaveBmp(Stream stream)
        {
            Capture().SaveAsBmp(stream);
        }
    }
}
