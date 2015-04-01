﻿#region MIT License
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
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Jypeli.GameObjects;

namespace Jypeli.Widgets
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ScoreItemWidget : Widget
    {
        public Label Place;
        public Label Name;
        public Label Score;

        public ScoreItemWidget()
            : base( new HorizontalLayout() )
        {
            Color = Color.Transparent;
            Place = new Label() { Color = Color.Transparent, HorizontalAlignment = HorizontalAlignment.Left };
            Name = new Label() { Color = Color.Transparent, SizeMode = TextSizeMode.None, HorizontalAlignment = HorizontalAlignment.Left, XMargin = 20, HorizontalSizing = Sizing.Expanding };
            Score = new Label() { Color = Color.Transparent, HorizontalAlignment = HorizontalAlignment.Right };
            Add( Place );
            Add( Name );
            Add( Score );
        }
    }

    /// <summary>
    /// Käyttöliittymäkomponentti, joka näyttää parhaat pisteet.
    /// </summary>
    public class ScoreListWidget : ListWidget<ScoreItem, ScoreItemWidget>
    {
        Font _font = Font.Default;
        Color _posColor = Color.Blue;
        Color _nameColor = Color.Black;
        Color _scoreColor = Color.Red;
        string _scoreFormat = "{0}";
        string _infMarker = "-";

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
                {
                    Content[i].Place.Font = value;
                    Content[i].Name.Font = value;
                    Content[i].Score.Font = value;
                }
            }
        }

        /// <summary>
        /// Sijoitusten väri.
        /// </summary>
        public Color PositionColor
        {
            get { return _posColor; }
            set
            {
                _posColor = value;
                for ( int i = 0; i < Content.ItemCount; i++ )
                    Content[i].Place.TextColor = value;
            }
        }

        /// <summary>
        /// Nimien väri.
        /// </summary>
        public Color NameColor
        {
            get { return _nameColor; }
            set
            {
                _nameColor = value;
                for ( int i = 0; i < Content.ItemCount; i++ )
                    Content[i].Name.TextColor = value;
            }
        }

        /// <summary>
        /// Pisteiden väri.
        /// </summary>
        public Color ScoreColor
        {
            get { return _scoreColor; }
            set
            {
                _scoreColor = value;
                for ( int i = 0; i < Content.ItemCount; i++ )
                    Content[i].Score.TextColor = value;
            }
        }

        /// <summary>
        /// Pisteiden muoto (oletus on {0})
        /// </summary>
        public string ScoreFormat
        {
            get { return _scoreFormat; }
            set
            {
                _scoreFormat = value;
                Reset();
            }
        }

        /// <summary>
        /// Äärettömän pistemäärän merkki. Oletus '-'
        /// </summary>
        public string InfinityMarker
        {
            get { return _infMarker; }
            set
            {
                _infMarker = value;
                Reset();
            }
        }

        /// <summary>
        /// Luo uuden ruudulla näytettävän parhaiden pisteiden listan.
        /// </summary>
        /// <param name="list">Olemassaoleva lista.</param>
        public ScoreListWidget( ScoreList list )
            : base( list )
        {
        }

        internal ScoreListWidget()
            : base( new ScoreList() )
        {
        }

        internal protected override ScoreItemWidget CreateWidget( ScoreItem item )
        {
            var w = new ScoreItemWidget();
            w.Place.Font = Font;
            w.Place.TextColor = PositionColor;
            w.Place.Text = String.Format( "{0}.", item.Position );
            w.Name.Font = Font;
            w.Name.TextColor = NameColor;
            w.Name.Text = item.Name;
            w.Score.Font = Font;
            w.Score.TextColor = ScoreColor;
            w.Score.Text = double.IsInfinity( item.Score ) ? InfinityMarker : String.Format( ScoreFormat, item.Score );
            return w;
        }
    }
}
