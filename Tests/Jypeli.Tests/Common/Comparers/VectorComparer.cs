using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli.Tests.Common.Comparers
{
    public class VectorComparer : IEqualityComparer<Vector>
    {
        static public VectorComparer Epsilon = new VectorComparer(float.Epsilon);

        private readonly double epsilon;

        public VectorComparer(double eps)
        {
            this.epsilon = eps;
        }

        public bool Equals(Vector x, Vector y)
        {
            return Math.Abs(x.X - y.X) < epsilon &&
                   Math.Abs(x.Y - y.Y) < epsilon;
        }

        public int GetHashCode(Vector obj)
        {
            throw new NotImplementedException();
        }
    }
}
