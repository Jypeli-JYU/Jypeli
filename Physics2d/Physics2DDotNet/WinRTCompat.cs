using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics2DDotNet
{
    internal static class WinRTCompat
    {
        public static ReadOnlyCollection<T> AsReadOnly<T>( this IList<T> collection )
        {
            return new ReadOnlyCollection<T>( collection );
        }
    }
}
