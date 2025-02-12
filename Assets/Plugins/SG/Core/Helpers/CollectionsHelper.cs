using System.Collections;
using System.Collections.Generic;

namespace SG
{
    public static class CollectionsHelper
    {
        public static bool IsNullOrEmpty<TItem>(this IEnumerable<TItem> obj)
        {
            if (obj.IsNull())
                return true;

            if (obj is ICollection<TItem> collection)
                return collection.Count == 0;

            using var enumerator = obj.GetEnumerator();
            return !enumerator.MoveNext();
        }

        public static bool IsNullOrEmpty(this IEnumerable obj)
        {
            if (obj.IsNull())
                return true;

            if (obj is ICollection collection)
                return collection.Count == 0;

            return !obj.GetEnumerator().MoveNext();
        }
    }
}
