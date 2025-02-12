using System;
using System.Collections.Generic;

namespace SG
{
    using DictSO = Dictionary<string, object>;
    using ListO = List<object>;

    public interface IDictionarizable<T>
    {
        DictSO ToDictionary();
        T FromDictionary(DictSO data);
    }

    public static class IDictionarizableExtensions
    {
        public static ListO ToObjectList<T>(this IList<T> list) where T : IDictionarizable<T>
        {
            if (list == null) return null;
            var objectList = new ListO(list.Count);
            for (int i = 0; i < list.Count; ++i)
                objectList.Add(list[i].ToDictionary());
            return objectList;
        }

        public static T ToClass<T>(this object objectDict) where T : IDictionarizable<T>
        {
            if (objectDict == null)
            {
                Log.Error("IDictionarizable.ToClass - object is null");
                return default;
            }
            else if (!(objectDict is DictSO))
            {
                Log.Error("IDictionarizable.ToClass - object is not Dictionary<string, object>");
                return default;
            }
            return ((T)Activator.CreateInstance(typeof(T), true)).FromDictionary(objectDict as DictSO);
        }

        public static List<T> ToClassList<T>(this object data) where T : IDictionarizable<T>
        {
            if (data == null) return null;

            var objectList = data as ListO;
            var classList = new List<T>();
            objectList.ForEach(obj =>
            {
                var cls = obj.ToClass<T>();
                if (cls != null)
                    classList.Add(cls);
            });
            return classList;
        }
        public static T[] ToClassArray<T>(this object data) where T : IDictionarizable<T>
        {
            if (data == null) return null;

            var objectList = data as ListO;
            var classList = new List<T>();
            objectList.ForEach(obj =>
            {
                var cls = obj.ToClass<T>();
                if (cls != null)
                    classList.Add(cls);
            });
            return classList.ToArray();
        }
    }
}