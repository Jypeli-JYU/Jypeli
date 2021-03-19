#if WINDOWS_PHONE || XBOX

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Physics2DDotNet
{
    /// <summary>
    /// In Windows Phone, the generic list has no RemoveAll method,
    /// which is widely used in the physics library. This
    /// class adds the missing method to the List class.
    /// </summary>
    internal static class ListExtension
    {
        public static int RemoveAll<T>(this List<T> list, Predicate<T> match)
        {
            int count = 0;
            int i = 0;

            while (i < list.Count)
            {
                if (match(list[i]))
                {
                    list.RemoveAt(i);
                    count++;
                    continue;
                }

                i++;
            }

            return count;
        }
    }
}

#endif
