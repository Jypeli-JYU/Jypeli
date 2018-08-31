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
    /// Ei tee mitään prosessointia.
    /// </summary>
    [ContentProcessor(DisplayName = "Text File - Jypeli")]
    public class TextFileContentProcessor : ContentProcessor<string[], string[]>
    {
        public override string[] Process(string[] input, ContentProcessorContext context)
        {
            return input;
        }
    }
}
