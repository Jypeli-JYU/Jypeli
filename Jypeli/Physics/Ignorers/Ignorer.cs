#region MIT License
/*
 * Copyright (c) 2005-2008 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion


using System;
using Jypeli.Physics;

namespace Jypeli
{
    /// <summary>
    /// Base class for Collision Ignorers to impliment.
    /// </summary>
    public abstract class Ignorer
    {
        bool isInverted;
        internal static bool CanCollide( IPhysicsBody leftBody, IPhysicsBody rightBody, Ignorer left, Ignorer right )
        {
            return left.CanCollideInternal( leftBody, rightBody, right );
        }
        protected Ignorer() { }
        protected Ignorer( Ignorer copy )
        {
            this.isInverted = copy.isInverted;
        }
        /// <summary>
        /// Get and sets if the result of this ignorer is inverted.
        /// </summary>
        public bool IsInverted
        {
            get { return isInverted; }
            set { isInverted = value; }
        }
        public abstract bool BothNeeded { get; }
        private bool CanCollideInternal( IPhysicsBody thisBody, IPhysicsBody otherBody, Ignorer other )
        {
            return isInverted ^ CanCollide( thisBody, otherBody, other );
        }
        public abstract bool CanCollide( IPhysicsBody thisBody, IPhysicsBody otherBody, Ignorer other );
        //public virtual void UpdateTime( TimeStep step ) { }
    }
}
