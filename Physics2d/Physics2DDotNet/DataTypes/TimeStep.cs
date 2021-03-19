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




#if UseDouble
using Scalar = System.Double;
#else
using Scalar = System.Single;
#endif
namespace Physics2DDotNet
{
    /// <summary>
    /// Class that holds information about a change in time;
    /// </summary>
    public sealed class TimeStep
    {
        private Scalar trueDt;
        private Scalar trueDtInv;
        private Scalar dt;
        private Scalar dtInv;
        private int updateCount;
        /// <summary>
        /// Creates a new Timestep instance.
        /// </summary>
        /// <param name="dt">The current change in time. (seconds)</param>
        /// <param name="updateCount">The number for the current update.</param>
        public TimeStep(Scalar dt, /*Scalar trueDt,*/ int updateCount)
        {
            //this.trueDt = trueDt;
            this.trueDtInv = (trueDt > 0) ? (1 / trueDt) : (0);
            this.dt = dt;
            this.dtInv = (dt > 0) ? (1 / dt) : (0);
            this.updateCount = updateCount;
        }
        /// <summary>
        /// The current change in time. (seconds)
        /// </summary>
        public Scalar Dt { get { return dt; } }
        /// <summary>
        /// The inverse of the change in time. (0 if dt is 0)
        /// </summary>
        public Scalar DtInv { get { return dtInv; } }
        /// <summary>
        /// The actaul change in time. (seconds)
        /// </summary>
        public Scalar TrueDt { get { return trueDt; } }
        /// <summary>
        /// The inverse of the actaul change in time. (0 if dt is 0)
        /// </summary>
        public Scalar TrueDtInv { get { return trueDtInv; } }
        /// <summary>
        /// The number for the current update.
        /// </summary>
        public int UpdateCount { get { return updateCount; } }
    }
}