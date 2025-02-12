using System;
using System.Runtime.CompilerServices;

namespace SG
{
    public static class ObjectHelper
    {
        private static readonly Func<UnityEngine.Object, bool> IsNativeObjectAlive;

        static ObjectHelper()
        {
            IsNativeObjectAlive = MakeDelegate<Func<UnityEngine.Object, bool>>(typeof(UnityEngine.Object), nameof(IsNativeObjectAlive));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T MakeDelegate<T>(Type classType, string methodName) where T : Delegate =>
            Delegate.CreateDelegate(typeof(T), classType, methodName) as T;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this object obj) => !obj.IsAlive();

        private static bool IsAlive(this object obj)
        {
            if (obj == null)
                return false;
            
            if (obj is UnityEngine.Object uo)
                return IsNativeObjectAlive(uo);
            
            return true;
        }
    }
}
