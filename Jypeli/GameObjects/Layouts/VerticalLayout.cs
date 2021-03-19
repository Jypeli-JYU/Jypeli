#region MIT License
/*
 * Copyright (c) 2009-2012 University of Jyväskylä, Department of Mathematical
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

using System.Collections.Generic;
using System.ComponentModel;

namespace Jypeli
{
    /// <summary>
    /// Asettelee widgetit päällekäin, järjestyksessä ylhäältä alas.
    /// </summary>
    public class VerticalLayout : ILayout
    {
        private Sizing _horizontalSizing;
        private Sizing _verticalSizing;
        private Vector _preferredSize;
        private double _spaceRequestedByExpandingObjects;
        private double _spaceRequestedByFixedSizeObjects;
        private double _spacing = 0;
        private double _topPadding = 0;
        private double _bottomPadding = 0;
        private double _leftPadding = 0;
        private double _rightPadding = 0;

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

        private void NotifyParent()
        {
            if (Parent != null)
            {
                Parent.NotifyParentAboutChangedSizingAttributes();
            }
        }

        /// <summary>
        /// Luo uuden asettelijan.
        /// </summary>
        public VerticalLayout()
        {
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

                if (o.VerticalSizing == Sizing.Expanding)
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
            _spaceRequestedByExpandingObjects = heightOfExpandingObjects;
            _spaceRequestedByFixedSizeObjects = heightOfFixedSizeObjects;
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
            double preferredContentHeight = PreferredSize.Y - (TopPadding + BottomPadding);
            double contentWidth = maximumSize.X - (LeftPadding + RightPadding);
            double fixedScale = 1.0;
            double expandingScale = 0.0;
            double top = maximumSize.Y / 2 - TopPadding;
            double availableSpaceForObjects = contentHeight - (objects.Count - 1) * Spacing;

            if ((availableSpaceForObjects < _spaceRequestedByFixedSizeObjects) && (_spaceRequestedByFixedSizeObjects > 0))
            {
                // Not enough space for all the fixed-size objects, they must be shrinked.
                fixedScale = availableSpaceForObjects / _spaceRequestedByFixedSizeObjects;

                // No space is left for expanding-size objects.
                expandingScale = 0;
            }
            else if ((maximumSize.Y < PreferredSize.Y) && (_spaceRequestedByExpandingObjects > 0))
            {
                // Not as much space as was requested.

                fixedScale = 1;

                // The expanding objects must be shrinked.
                double availableSpaceForExpandingObjects = availableSpaceForObjects - _spaceRequestedByFixedSizeObjects;
                expandingScale = availableSpaceForExpandingObjects / _spaceRequestedByExpandingObjects;
            }
            else
            {
                // Enough space is available.

                fixedScale = 1;

                if (_spaceRequestedByExpandingObjects > 0)
                {
                    // The expanding-size objects shall use up all the extra space that is available.
                    expandingScale = (availableSpaceForObjects - _spaceRequestedByFixedSizeObjects) / _spaceRequestedByExpandingObjects;
                }
            }

            foreach (var o in objects)
            {
                double scale = (o.VerticalSizing == Sizing.FixedSize) ? fixedScale : expandingScale;
                double height = o.PreferredSize.Y * scale;
                double width = o.PreferredSize.X;

                if ((o.PreferredSize.X > contentWidth) || (o.HorizontalSizing == Sizing.Expanding))
                {
                    width = contentWidth;
                }

                o.Size = new Vector(width, height);
                o.X = (-maximumSize.X / 2) + (LeftPadding + contentWidth / 2) + Parent.X;
                o.Y = top - height / 2 + Parent.Y;

                top -= height + Spacing;
            }
        }
    }
}
