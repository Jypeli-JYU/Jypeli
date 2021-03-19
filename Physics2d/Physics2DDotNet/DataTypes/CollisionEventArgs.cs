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
using System;
using System.Collections.ObjectModel;

using AdvanceMath;
using System.Threading.Tasks;

namespace Physics2DDotNet
{
    public class CollisionEventArgs : EventArgs, IAsyncDisposable, IDisposable
    {
        TimeStep step;
        IContact contact;
        Body other;
        object customIntersectionInfo;
        private CollisionEventArgs(TimeStep step, Body other)
        {
            this.step = step;
            this.other = other;
        }
        public CollisionEventArgs(TimeStep step, Body other, IContact contact)
            :this(step,other)
        {
            this.contact = contact;
        }
        public CollisionEventArgs(TimeStep step,Body other, object customIntersectionInfo)
            : this(step, other)
        {
            this.customIntersectionInfo = customIntersectionInfo;
        }
        public TimeStep Step
        {
            get { return step; }
        }
        public Body Other
        {
            get { return other; }
        }
        public object CustomCollisionInfo
        {
            get { return customIntersectionInfo; }
        }
        public IContact Contact
        {
            get { return contact; }
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
            }

            disposed = true;
        }

        public virtual ValueTask DisposeAsync()
        {
            try
            {
                Dispose();
                return default;
            }
            catch (Exception exception)
            {
                return new ValueTask(Task.FromException(exception));
            }
        }

        ~CollisionEventArgs()
        {
            Dispose();
        }
    }
}