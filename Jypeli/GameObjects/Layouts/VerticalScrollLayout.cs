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

using System.ComponentModel;
using System.Collections.Generic;

namespace Jypeli.GameObjects
{
    /// <summary>
    /// Asettelee widgetit päällekäin, järjestyksessä ylhäältä alas.
    /// Jos widgeteille ei ole tarpeeksi tilaa, vain osa niistä asetellaan
    /// paikalleen. Ensimmäisen näkyvän widgetin indeksi asetetaan <c>Update</c>:ssa
    /// propertyyn <c>StartIndex</c> ja viimeisen jälkeisen indeksi propertyyn
    /// <c>EndIndex</c>.
    /// </summary>
    internal class VerticalScrollLayout : ILayout
    {
        private Sizing _horizontalSizing;
        private Sizing _verticalSizing;
        private Vector _preferredSize;
        private double _heightRequestedByExpandingObjects;
        private double _heightRequestedByFixedSizeObjects;
        private double _spacing = 0;
        private double _topPadding = 0;
        private double _bottomPadding = 0;
        private double _leftPadding = 0;
        private double _rightPadding = 0;
        private double _firstVisibleItemOffset = 0;


        [EditorBrowsable(EditorBrowsableState.Never)]
        public GameObject Parent { get; set; }

        /// <summary>
        /// Olioiden väliin jäävä tyhjä tila.
        /// </summary>
        public double Spacing
        {
            get { return _spacing; }
            set { _spacing = value; NotifyParent(); }
        }

        /// <summary>
        /// Yläreunaan jäävä tyhjä tila.
        /// </summary>
        public double TopPadding
        {
            get { return _topPadding; }
            set { _topPadding = value; NotifyParent(); }
        }

        /// <summary>
        /// Alareunaan jäävä tyhjä tila.
        /// </summary>
        public double BottomPadding
        {
            get { return _bottomPadding; }
            set { _bottomPadding = value; NotifyParent(); }
        }

        /// <summary>
        /// Vasempaan reunaan jäävä tyhjä tila.
        /// </summary>
        public double LeftPadding
        {
            get { return _leftPadding; }
            set { _leftPadding = value; NotifyParent(); }
        }

        /// <summary>
        /// Oikeaan reunaan jäävä tyhjä tila.
        /// </summary>
        public double RightPadding
        {
            get { return _rightPadding; }
            set { _rightPadding = value; NotifyParent(); }
        }

        /// <summary>
        /// Ylhäältä lukien ensimmäisen piirtoalueen sisällä olevan widgetin indeksi.
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Viimeisen piirtoalueella olevan widgetin jälkeinen indeksi.
        /// </summary>
        public int EndIndex { get; set; }

        private void NotifyParent()
        {
            if (Parent != null)
            {
                Parent.NotifyParentAboutChangedSizingAttributes();
            }
        }

        public void ScrollUp(IList<GameObject> objects)
        {
            if (StartIndex > 0)
            {
                StartIndex--;
                _firstVisibleItemOffset = 0;
                Update(objects, Parent.Size);
            }
        }

        public void ScrollDown(IList<GameObject> objects)
        {
            if (StartIndex < objects.Count - 1)
            {
                StartIndex++;
                _firstVisibleItemOffset = 0;
                Update(objects, Parent.Size);
            }
        }

        /// <summary>
        /// Listan portaaton vieritys.
        /// </summary>
        public void Scroll(IList<GameObject> objects, double amount)
        {
            if (objects.Count == 0)
                return;

            if (amount < 0)
            {
                double totalScrollingAmount = -amount;
                double spaceForCurrentObject = _firstVisibleItemOffset;

                while (spaceForCurrentObject < totalScrollingAmount)
                {
                    if (StartIndex == 0)
                    {
                        spaceForCurrentObject = 0;
                        break;
                    }

                    totalScrollingAmount -= spaceForCurrentObject;
                    StartIndex--;
                    spaceForCurrentObject = objects[StartIndex].PreferredSize.Y + Spacing;
                }

                spaceForCurrentObject -= totalScrollingAmount;
                _firstVisibleItemOffset = spaceForCurrentObject;
            }
            else
            {
                double totalScrollingAmount = -amount;
                double spaceForCurrentObject = objects[StartIndex].PreferredSize.Y + Spacing - _firstVisibleItemOffset;

                while (spaceForCurrentObject < -totalScrollingAmount)
                {
                    if (StartIndex >= objects.Count - 1)
                    {
                        spaceForCurrentObject = 0;
                        break;
                    }

                    totalScrollingAmount += spaceForCurrentObject;
                    StartIndex++;
                    spaceForCurrentObject = objects[StartIndex].PreferredSize.Y + Spacing;
                }

                spaceForCurrentObject += totalScrollingAmount;
                _firstVisibleItemOffset = objects[StartIndex].PreferredSize.Y + Spacing - spaceForCurrentObject;
            }

            Update(objects, Parent.Size);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void UpdateSizeHints(IList<GameObject> objects)
        {
            if (objects.Count == 0)
                return;

            double maxWidth = 0;
            double heightOfExpandingObjects = 0;
            double heightOfFixedSizeObjects = 0;
            Sizing horizontalSizing = Sizing.FixedSize;
            Sizing verticalSizing = Sizing.FixedSize;

            foreach (var o in objects)
            {
                if (o.PreferredSize.X > maxWidth)
                {
                    maxWidth = o.PreferredSize.X;
                }

                if (o.VerticalSizing != Sizing.FixedSize)
                {
                    verticalSizing = Sizing.Expanding;
                    heightOfExpandingObjects += o.PreferredSize.Y;
                }
                else if (o.VerticalSizing == Sizing.FixedSize)
                {
                    heightOfFixedSizeObjects += o.PreferredSize.Y;
                }

                if (o.HorizontalSizing != Sizing.FixedSize)
                {
                    horizontalSizing = Sizing.Expanding;
                }
            }

            double preferredHeight = TopPadding + heightOfExpandingObjects + heightOfFixedSizeObjects + ((objects.Count - 1) * Spacing) + BottomPadding;
            double preferredWidth = LeftPadding + maxWidth + RightPadding;

            _horizontalSizing = horizontalSizing;
            _verticalSizing = verticalSizing;
            _heightRequestedByExpandingObjects = heightOfExpandingObjects;
            _heightRequestedByFixedSizeObjects = heightOfFixedSizeObjects;
            _preferredSize = new Vector(preferredWidth, preferredHeight);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Sizing HorizontalSizing
        {
            get { return _horizontalSizing; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Sizing VerticalSizing
        {
            get { return _verticalSizing; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Vector PreferredSize
        {
            get { return _preferredSize; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Update(IList<GameObject> objects, Vector maximumSize)
        {
            double contentHeight = maximumSize.Y - (TopPadding + BottomPadding);
            double contentWidth = maximumSize.X - (LeftPadding + RightPadding);
            double contentBottomLimit = -maximumSize.Y / 2 + BottomPadding;
            double top = maximumSize.Y / 2 - TopPadding;
            double offset = _firstVisibleItemOffset;
            int i = StartIndex;

            while ((i < objects.Count) && (top >= contentBottomLimit))
            {
                GameObject o = objects[i];
                double width = o.PreferredSize.X;
                double height = o.PreferredSize.Y;

                if ((o.PreferredSize.X > contentWidth) || (o.HorizontalSizing != Sizing.FixedSize))
                {
                    width = contentWidth;
                }

                o.Size = new Vector(width, height);
                o.X = (-maximumSize.X / 2) + (LeftPadding + contentWidth / 2) + Parent.X;
                o.Y = top - height / 2 + offset + Parent.Y;

                top -= height + Spacing - offset;

                // offset is zero for all but the first object.
                offset = 0;
                i++;
            }

            EndIndex = i;
        }
    }
}
