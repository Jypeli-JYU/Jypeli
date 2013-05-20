#region MIT License
/*
 * Copyright (c) 2009 University of Jyv�skyl�, Department of Mathematical
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
 * Authors: Tero J�ntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    public partial class GameObject
    {
        private Vector _size;
        private Shape _shape;

        /// <summary>
        /// Olion koko pelimaailmassa.
        /// Kertoo olion ��rirajat, ei muotoa.
        /// </summary>
        public override Vector Size
        {
            get
            {
                if ( _layoutNeedsRefreshing )
                {
                    // This is needed if, for example, child objects have
                    // been added right before the size is read.
                    RefreshLayout();
                }
                return _size;
            }
            set
            {
                if ( value.X < 0.0 || value.Y < 0.0 )
                    throw new ArgumentException( "The size must be positive." );

                Vector oldSize = _size;
                _size = value;

                if (autoResizeChildObjects)
                {
                    UpdateChildSizes(oldSize, value);
                }
            }
        }

        /// <summary>
        /// Olion kulma tai rintamasuunta.
        /// Nolla = osoittaa oikealle.
        /// </summary>      
        public override Angle Angle { get; set; }

        /// <summary>
        /// Olion muoto.
        /// </summary>
        public virtual Shape Shape
        {
            get { return _shape; }
            set { _shape = value; }
        }

        /// <summary>
        /// Olion muoto merkkijonona (kentt�editorin k�ytt��n)
        /// </summary>
        internal string ShapeString
        {
            get { return Shape.GetType().Name; }
            set { Shape = Shape.FromString( value ); }
        }

        private void InitDimensions( double width, double height, Shape shape )
        {
            this._size = new Vector( width, height );
            this._shape = shape;
        }

        /// <summary>
        /// Onko piste <c>p</c> t�m�n olion sis�puolella.
        /// </summary>
        public bool IsInside( Vector point )
        {
            Vector p = this.AbsolutePosition;
            double pX, pY;
            double pointX, pointY;

            if ( AbsoluteAngle == Angle.Zero )
            {
                // A special (faster) case of the general case below
                pX = p.X;
                pY = p.Y;
                pointX = point.X;
                pointY = point.Y;
            }
            else
            {
                Vector unitX = Vector.FromLengthAndAngle( 1, this.AbsoluteAngle );
                Vector unitY = unitX.LeftNormal;
                pX = p.ScalarProjection( unitX );
                pY = p.ScalarProjection( unitY );
                pointX = point.ScalarProjection( unitX );
                pointY = point.ScalarProjection( unitY );
            }

            if ( Shape.IsUnitSize )
            {
                double x = 2 * ( pointX - pX ) / this.Width;
                double y = 2 * ( pointY - pY ) / this.Height;
                if ( this.Shape.IsInside( x, y ) )
                    return true;
            }
            else
            {
                double x = pointX - pX;
                double y = pointY - pY;
                if ( this.Shape.IsInside( x, y ) )
                    return true;
            }

            return IsInsideChildren( point );
        }

        /// <summary>
        /// Onko piste <c>p</c> t�m�n olion ymp�r�iv�n suorakulmion sis�puolella.
        /// </summary>
        public bool IsInsideRect( Vector point )
        {
            Vector p = this.AbsolutePosition;

            if ( AbsoluteAngle == Angle.Zero )
            {
                // A special (faster) case of the general case below
                if ( point.X >= ( p.X - Width / 2 )
                    && point.X <= ( p.X + Width / 2 )
                    && point.Y >= ( p.Y - Height / 2 )
                    && point.Y <= ( p.Y + Height / 2 ) ) return true;
            }
            else
            {
                Vector unitX = Vector.FromLengthAndAngle( 1, this.AbsoluteAngle );
                Vector unitY = unitX.LeftNormal;
                double pX = p.ScalarProjection( unitX );
                double pY = p.ScalarProjection( unitY );
                double pointX = point.ScalarProjection( unitX );
                double pointY = point.ScalarProjection( unitY );

                if ( pointX >= ( pX - Width / 2 )
                    && pointX <= ( pX + Width / 2 )
                    && pointY >= ( pY - Height / 2 )
                    && pointY <= ( pY + Height / 2 ) ) return true;
            }

            return IsInsideChildren( point );
        }
    }
}
