using System;
using System.Collections.Generic;
using System.Linq;

namespace Rbmk.Utils.Linq
{
    public static class EnumerableExtensions
    {
        public static int FirstIndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> filter)
        {
            return enumerable.Select((item, i) => new { Item = item, Index = i })
                .FirstOrDefault(tuple => filter(tuple.Item))?.Index ?? -1;
        }
        
        public static int LastIndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> filter)
        {
            return enumerable.Select((item, i) => new { Item = item, Index = i })
                       .LastOrDefault(tuple => filter(tuple.Item))?.Index ?? -1;
        }

        public static int[] AllIndexesOf<T>(this IEnumerable<T> enumerable, Func<T, bool> filter)
        {
            return enumerable.Select((item, i) => new {Item = item, Index = i})
                .Where(tuple => filter(tuple.Item))
                .Select(tuple => tuple.Index)
                .ToArray();
        }
    }
}