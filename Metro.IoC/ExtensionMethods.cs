using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Metro.IoC
{
    public static class ExtensionMethods
    {
        public static void Apply<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }
    }
}
