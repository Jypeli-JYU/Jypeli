#region MIT License
/*
 * Copyright (c) 2009-2011 University of Jyv‰skyl‰, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tomi Karppinen, Tero J‰ntti
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Jypeli.GameObjects;

namespace Jypeli
{
    /// <summary>
    /// Ikkuna, joka sis‰lt‰‰ listan merkkijonoja.
    /// </summary>
    public class StringListWindow : CustomQueryWindow<StringListWidget>
    {
        /// <summary>
        /// Listakomponentti.
        /// </summary>
        public StringListWidget List
        {
            get { return QueryWidget; }
        }

        internal override bool OkButtonOnPhone { get { return true; } }

        public StringListWindow( string question )
            : base( question )
        {
            Game.AssertInitialized( AddControls );
        }

        /// <summary>
        /// Luo uuden merkkijonolistaikkunan ja asettaa sille kiinte‰n koon.
        /// </summary>
        /// <param name="question">Kysymys</param>
        public StringListWindow( double width, double height, string question )
            : base( width, height, question )
        {
            Game.AssertInitialized( AddControls );
        }

        protected override StringListWidget CreateQueryWidget()
        {
            return new StringListWidget() { HorizontalSizing = Sizing.Expanding, VerticalSizing = Sizing.Expanding, Color = Color.Transparent };
        }

        void AddControls()
        {
            Jypeli.Game.Instance.PhoneBackButton.Listen( Close, null ).InContext( this );
        }
    }
}
