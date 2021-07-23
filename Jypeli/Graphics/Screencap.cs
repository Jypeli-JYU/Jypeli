using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Jypeli
{
	internal static class Screencap
	{
	    public static void WriteBmp( Stream outStream, Jypeli.Image image )
	    {
			// TODO: Kuva tulee ylösalaisin
			byte[] buffer = image.GetByteArray();

	        BinaryWriter writer = new BinaryWriter(outStream);
	        {
	            // Magic
	            writer.Write('B');
	            writer.Write('M');

	            // Size of the BMP file
	            //writer.Write( (int)(54 + buffer.Length) );
	            writer.Write((int)(122 + buffer.Length));

	            // Unused
	            writer.Write( (int)0 );

	            // Offset of pixel array
	            //writer.Write( (int)0x36 );
	            writer.Write((int)0x7A);

	            // DIB header length
	            //writer.Write( (int)0x28 );
	            writer.Write( (int)108 );

	            // Image dimensions
	            writer.Write( image.Width );
	            writer.Write( image.Height );

	            // Number of color planes
	            writer.Write( (short)1 );

	            // Bits per pixel
	            writer.Write( (short)32 );

	            // No compression, specify bitfields
	            //writer.Write( (int)0 );
	            writer.Write( (int)3 );

	            // Size of raw bitmap data
	            writer.Write( (int)buffer.Length );

	            // Print resolution as pixels per meter (this is 72 dpi)
	            writer.Write( (int)2835 );
	            writer.Write( (int)2835 );

	            // Number of colors in the palette
	            writer.Write( (int)0 );

	            // Number of important colors
	            writer.Write( (int)0 );

	            // Bit masks: red, green, blue, alpha (big endian)
	            writer.Write( new byte[] { 0xFF, 0x00, 0x00, 0x00 } );
	            writer.Write( new byte[] { 0x00, 0xFF, 0x00, 0x00 } );
	            writer.Write( new byte[] { 0x00, 0x00, 0xFF, 0x00 } );
	            writer.Write( new byte[] { 0x00, 0x00, 0x00, 0xFF } );

	            // Color space: "Win"
	            writer.Write(' ');
	            writer.Write('n');
	            writer.Write('i');
	            writer.Write('W');

	            // Unused (color space endpoints & gamma)
	            writer.Write( new byte[36] );
	            writer.Write( (int)0 );
	            writer.Write( (int)0 );
	            writer.Write( (int)0 );

                // Bitmap data
				writer.Write(buffer);

			}

	        // Do not call Close or Dispose! It closes the stream too.
	        writer.Flush();
	    }

	    public static void EncodePng(Stream outStream, Stream inStream)
	    {
			var encoders = ImageCodecInfo.GetImageEncoders();

	        Bitmap bmp = new Bitmap(inStream);
	        bmp.Save(outStream, ImageFormat.Png);
	    }

	    public static void EncodeJpeg(Stream outStream, Stream inStream)
	    {
	        var bitmap = new Bitmap(inStream);

	        ImageCodecInfo jpgEncoder = GetCodecByDescription("JPEG");
	        Encoder encoder2 = System.Drawing.Imaging.Encoder.Quality;
	        EncoderParameters parameters = new System.Drawing.Imaging.EncoderParameters(1);
	        EncoderParameter parameter = new EncoderParameter(encoder2, 50L);
	        parameters.Param[0] = parameter;

	        //System.IO.Stream stream = new MemoryStream();
	        //bitmap.Save(stream, jpgEncoder, parameters);
	        bitmap.Save(outStream, jpgEncoder, parameters);

	        /*var bytes = ((MemoryStream)stream).ToArray();
	        System.IO.Stream inputStream = new MemoryStream(bytes);
	        Bitmap fromDisk = new Bitmap(@"C:\Temp\TestJPEG.jpg");
	        Bitmap fromStream = new Bitmap(inputStream);*/
	    }

		/// <summary>
		/// Tallentaa kuvan jpg-muodossa.
		/// </summary>
		/// <param name="fname">Tallennettavan tiedoston nimi</param>
		/// <param name="img">Tallennettava kuva</param>
		public static void SaveJPG(string fname, Image img)
        {
            img.SaveAsJpeg(fname);
		}

	    private static ImageCodecInfo GetCodecByDescription( string desc )
	    {
	        foreach ( var codec in ImageCodecInfo.GetImageEncoders().FindAll(x => x.FormatDescription == "JPEG") )
	        {
	            return codec;
	        }

	        throw new IOException(desc + " codec not found");
	    }
	}
}