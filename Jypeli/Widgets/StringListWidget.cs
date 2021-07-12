#region MIT License
/*
 * Copyright (c) 2009-2011 University of Jyväskylä, Department of Mathematical
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
 * Authors: Tomi Karppinen, Tero Jäntti
 */

using System;
using Microsoft.Xna.Framework;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;

namespace Jypeli
{
    /// <summary>
    /// Käyttöliittymäkomponentti, joka näyttää listan merkkijonoja.
    /// </summary>
    public class StringListWidget : ListWidget<string, Label>
    {
        Font _font = Font.Default;
        Color _textColor = Color.Black;
        HorizontalAlignment _hAlignment = HorizontalAlignment.Left;

        /// <summary>
        /// Tekstifontti.
        /// </summary>
        public Font Font
        {
            get { return _font; }
            set
            {
                _font = value;
                for ( int i = 0; i < Content.ItemCount; i++ )
                    Content[i].Font = value;
            }
        }

        /// <summary>
        /// Tekstin väri.
        /// </summary>
        public Color TextColor
        {
            get { return _textColor; }
            set
            {
                _textColor = value;
                for ( int i = 0; i < Content.ItemCount; i++ )
                    Content[i].TextColor = value;
            }
        }

        /// <summary>
        /// Listan alkioiden sijoitus vaakasuunnassa.
        /// </summary>
        public HorizontalAlignment ItemAligment
        {
            get { return _hAlignment; }
            set
            {
                _hAlignment = value;
                for ( int i = 0; i < Content.ItemCount; i++ )
                    Content[i].HorizontalAlignment = value;
            }
        }

        /// <summary>
        /// Kaikki listan alkiot rivinvaihdoilla erotettuna.
        /// </summary>
        public string Text
        {
            get
            {
                StringBuilder result = new StringBuilder();

                foreach ( var item in Items )
                {
                    result.Append( item ).Append( "\n" );
                }

                return result.RemoveLast( 2 ).ToString();
            }
            set
            {
                if ( Width == 0 )
                    throw new InvalidOperationException( "You must set the list width before assigning text!" );

                DynamicSpriteFont xnaFont = this.Font.XnaFont;
                Vector2 textDims = xnaFont.MeasureString( value );
                double softWidth = 4 * Font.CharacterWidth < Width ? Width - 4 * Font.CharacterWidth : Width;
                string wrapped = Font.WrapText( value, softWidth, Width );

                StringList newList = new StringList( wrapped.Split( '\n' ) );
                this.Bind( newList );
            }
        }

        /// <summary>
        /// Luo uuden (tyhjän) merkkijonolistakomponentin, joka on sidottu olemassaolevaan
        /// listaan.
        /// </summary>
        /// <param name="list">Olemassaoleva lista.</param>
        public StringListWidget( StringList list )
            : base( list )
        {
        }

        /// <summary>
        /// Luo uuden (tyhjän) merkkijonolistakomponentin.
        /// </summary>
        public StringListWidget()
            : base( new StringList() )
        {
            SizingByLayout = false;
        }

        /// <inheritdoc/>
        internal protected override Label CreateWidget( string item )
        {
            return new Label( item )
            { 
                HorizontalSizing = Sizing.Expanding,
                Font = Font,
                Color = Color.Transparent,
                SizeMode = TextSizeMode.None,
                TextColor = TextColor,
                HorizontalAlignment = ItemAligment,
            };
        }
    }
}
