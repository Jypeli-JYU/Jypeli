using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

/*
 * Authors: Jaakko Kosonen
 */

namespace TextFileContentExtension
{
    /// <summary>
    /// Lukee tekstitiedoston ja palauttaa rivit taulukossa.
    /// </summary>
    [ContentImporter(".txt", DisplayName = "TextFileContentExtension.TextFileImporter", DefaultProcessor = "TextFileContentExtension.TextFileContentProcessor")]
    public class TextFileImporter : ContentImporter<string[]>
    {
        public override string[] Import(string filename, ContentImporterContext context)
        {
            return System.IO.File.ReadAllLines(filename);
        }
    }
}