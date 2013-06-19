#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
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
 * Authors: Tero Jäntti, Tomi Karppinen
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Jypeli
{
    /// <summary>
    /// Kosketuspaneelin kosketus.
    /// </summary>
    public class Touch
    {
        protected Vector2 _previousPosition;
        protected Vector2 _position;

        internal long DurationInTicks = 0;

        /// <summary>
        /// Id-tunnus tälle kosketukselle.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Vapaasti asetettava muuttuja.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Kosketuksen paikka ruudulla.
        /// </summary>
        public Vector PositionOnScreen
        {
            get
            {
                return Game.Screen.FromXnaCoords( _position, Vector.Zero );
            }
        }

        /// <summary>
        /// Kosketuksen paikka pelimaailmassa.
        /// </summary>
        public Vector PositionOnWorld
        {
            get
            {
                return Game.Instance.Camera.ScreenToWorld( PositionOnScreen );
            }
        }

        /// <summary>
        /// Kosketuksen liike ruudulla.
        /// </summary>
        public Vector MovementOnScreen
        {
            get
            {
                return new Vector( _position.X - _previousPosition.X, _previousPosition.Y - _position.Y );
            }
        }

        /// <summary>
        /// Kosketuksen liike pelimaailmassa.
        /// </summary>
        public Vector MovementOnWorld
        {
            get
            {
                return MovementOnScreen / Game.Instance.Camera.ZoomFactor;
            }
        }

        public TouchLocationState State { get; internal set; }

        internal Touch( TouchLocation location )
        {
            this.Id = location.Id;
            this._position = this._previousPosition = location.Position;
        }

        internal void Update( TouchLocation location )
        {
            _previousPosition = _position;
            _position = location.Position;
            State = location.State;
            DurationInTicks++;
        }
    }
}
