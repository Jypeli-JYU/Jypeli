using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace Jypeli
{
    /// <summary>
    /// Reads string arrays from content.
    /// </summary>
    public class TextReader : ContentTypeReader<string[]>
    {
        /// <summary>
        /// Performs the actual reading.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="existingInstance"></param>
        /// <returns></returns>
        protected override string[] Read( ContentReader input, string[] existingInstance )
        {
            List<string> list = new List<string>();

            using ( StreamReader reader = new StreamReader( input.BaseStream ) )
            {
                while ( !reader.EndOfStream )
                {
                    string line = reader.ReadLine();
                    list.Add( line );
                }
            }

            return list.ToArray();
        }
    }
}