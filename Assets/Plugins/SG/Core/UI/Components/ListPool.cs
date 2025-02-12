using System.Collections.Generic;

namespace SG.UI
{
    // https://github.com/DanielKegel/movement_unity/blob/master/Library/PackageCache/com.unity.textmeshpro%401.4.1/Scripts/Runtime/TMP_ListPool.cs
    internal static class ListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, l => l.Clear());

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }
}