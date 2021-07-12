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
    /// Asettelee widgetit riviin vaakasuunnassa.
    /// </summary>
    public class HorizontalLayout : ILayout
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

        /// <summary>
        /// Asettelijan omistaja
        /// </summary>
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
        public HorizontalLayout()
        {
        }

        /// <summary>
        /// Päivittää kappaleiden kokovihjeet
        /// </summary>
        /// <param name="objects"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void UpdateSizeHints(IList<GameObject> objects)
        {
            if (objects.Count == 0)
                return;

            double maxHeight = 0;
            double widthOfExpandingObjects = 0;
            double widthOfFixedSizeObjects = 0;
            Sizing horizontalSizing = Sizing.FixedSize;
            Sizing verticalSizing = Sizing.FixedSize;

            foreach (var o in objects)
            {
                if (o.PreferredSize.Y > maxHeight)
                {
                    maxHeight = o.PreferredSize.Y;
                }

                if (o.HorizontalSizing == Sizing.Expanding)
                {
                    widthOfExpandingObjects += o.PreferredSize.X;
                    horizontalSizing = Sizing.Expanding;
                }
                else if (o.HorizontalSizing == Sizing.FixedSize)
                {
                    widthOfFixedSizeObjects += o.PreferredSize.X;
                }

                if (o.VerticalSizing != Sizing.FixedSize)
                {
                    verticalSizing = Sizing.Expanding;
                }
            }

            double preferredWidth = LeftPadding + widthOfExpandingObjects + widthOfFixedSizeObjects + ((objects.Count - 1) * Spacing) + RightPadding;
            double preferredHeight = TopPadding + maxHeight + BottomPadding;

            _horizontalSizing = horizontalSizing;
            _verticalSizing = verticalSizing;
            _spaceRequestedByExpandingObjects = widthOfExpandingObjects;
            _spaceRequestedByFixedSizeObjects = widthOfFixedSizeObjects;
            _preferredSize = new Vector(preferredWidth, preferredHeight);
        }

        /// <summary>
        /// Leveyssuuntainen koon asettelija
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Sizing HorizontalSizing
        {
            get { return _horizontalSizing; }
        }

        /// <summary>
        /// Pystysuuntainen koon asettelija
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Sizing VerticalSizing
        {
            get { return _verticalSizing; }
        }

        /// <summary>
        /// Olion suosima koko
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Vector PreferredSize
        {
            get { return _preferredSize; }
        }

        /// <summary>
        /// Päivittää olioiden koon ja sijainnin
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="maximumSize"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Update(IList<GameObject> objects, Vector maximumSize)
        {
            double contentWidth = maximumSize.X - (LeftPadding + RightPadding);
            double preferredContentWidth = PreferredSize.X - (LeftPadding + RightPadding);
            double contentHeight = maximumSize.Y - (TopPadding + BottomPadding);
            double fixedScale = 1.0;
            double expandingScale = 0.0;
            double left = -maximumSize.X / 2 + LeftPadding;
            double availableSpaceForObjects = contentWidth - (objects.Count - 1) * Spacing;

            if ((availableSpaceForObjects < _spaceRequestedByFixedSizeObjects) && (_spaceRequestedByFixedSizeObjects > 0.0))
            {
                // Not enough space for all the fixed-size objects, they must be shrinked.
                fixedScale = availableSpaceForObjects / _spaceRequestedByFixedSizeObjects;

                // No space is left for expanding-size objects.
                expandingScale = 0;
            }
            else if ((maximumSize.X < PreferredSize.X) && (_spaceRequestedByExpandingObjects > 0.0))
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
                double scale = (o.HorizontalSizing == Sizing.FixedSize) ? fixedScale : expandingScale;
                double width = o.PreferredSize.X * scale;
                double height = o.PreferredSize.Y;

                if ((o.PreferredSize.Y > contentHeight) || (o.VerticalSizing == Sizing.Expanding))
                {
                    height = contentHeight;
                }

                o.Size = new Vector(width, height);
                o.X = left + width / 2 + Parent.X;
                o.Y = (maximumSize.Y / 2) - (TopPadding + contentHeight / 2) + Parent.Y;

                left += width + Spacing;
            }
        }
    }
}
