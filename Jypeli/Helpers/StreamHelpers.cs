using System.IO;

namespace Jypeli
{
    /// <summary>
    /// Apufunktioita virtojen käyttöön.
    /// </summary>
    public static class StreamHelpers
    {
        /// <summary>
        /// Kopioi virran sisällön toiseen virtaan.
        /// Sama kuin Stream.CopyTo (C# ver 4), mutta toimii myös vanhemmilla versioilla.
        /// </summary>
        /// <param name="input">Mistä kopioidaan</param>
        /// <param name="output">Mihin kopioidaan</param>
        public static void CopyStreamTo( this Stream input, Stream output )
        {
            byte[] buffer = new byte[32768];
            int read;

            while ( ( read = input.Read( buffer, 0, buffer.Length ) ) > 0 )
            {
                output.Write( buffer, 0, read );
            }
        }
    }
}
