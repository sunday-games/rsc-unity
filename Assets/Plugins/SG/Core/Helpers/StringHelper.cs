using System.Runtime.CompilerServices;

namespace SG.Core.Helpers
{
    public static class StringHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);
    }
}
