using System;
using System.Collections.Generic;

namespace SG
{
    public static class DictionarizableObjectExtensions
    {
        public static List<object> ToObjectList<T>(this IList<T> list) where T : DictionarizableObject
        {
            if (list == null) return null;
            var objectList = new List<object>(list.Count);
            for (int i = 0; i < list.Count; ++i)
                objectList.Add(list[i].ToDictionary());
            return objectList;
        }

        public static T ToClass<T>(this object objectDict) where T : DictionarizableObject
        {
            if (objectDict == null)
            {
                UnityEngine.Debug.LogError("IDictionarizable.ToClass - object is null");
                return default;
            }

            if (objectDict is Dictionary<string, object> dict)
            {
                var obj = (T)Activator.CreateInstance(typeof(T), true);
                obj.FromDictionary(dict);
                return obj;
            }
            else
            {
                UnityEngine.Debug.LogError("IDictionarizable.ToClass - object is not DictSO");
                return default;
            }
        }

        public static bool TryGetClass<T>(this object objectDict, out T obj) where T : DictionarizableObject
        {
            obj = default;

            if (objectDict == null)
            {
                UnityEngine.Debug.LogError("IDictionarizable.TryGetClass - object is null");
                return false;
            }

            if (objectDict is Dictionary<string, object> dict)
            {
                obj = (T)Activator.CreateInstance(typeof(T), true);
                obj.FromDictionary(dict);
                return true;
            }
            else
            {
                UnityEngine.Debug.LogError("IDictionarizable.TryGetClass - object is not DictSO");
                return false;
            }
        }

        public static List<T> ToClassList<T>(this object data) where T : DictionarizableObject
        {
            if (data == null) return null;

            var objectList = data as List<object>;
            if (objectList == null) return null;

            var classList = new List<T>();
            objectList.ForEach(obj =>
            {
                if (obj.TryGetClass(out T cls)) classList.Add(cls);
            });
            return classList;
        }

        public static List<T> ToClassListTyped<T>(this object data) where T : DictionarizableObject
        {
            if (data == null) return null;

            var objectList = data as List<object>;
            if (objectList == null) return null;

            var classList = new List<T>();
            
            foreach (var objectData in objectList)
                if (objectData is Dictionary<string, object> objectDict)
                {
                    var type = Type.GetType(objectDict["type"].ToString()); // Because of it this method must be in SG.RolePlaying namespace
                    var obj = (T)Activator.CreateInstance(type, true);
                    obj.FromDictionary(objectDict);
                    classList.Add(obj);
                };
            return classList;
        }

        //public static List<T> ToClassListTyped<T>(this object data) where T : DictionarizableObject
        //{
        //    if (data == null) return null;

        //    var objectList = data as ListO;
        //    if (objectList == null) return null;

        //    var classList = new List<T>();
        //    foreach (var objectData in objectList)
        //        if (objectData is DictSO objectDict)
        //        {
        //            var type = Type.GetType(objectDict["type"].ToString());
        //            var obj = (T)Activator.CreateInstance(type, true);
        //            obj.FromDictionary(objectDict);
        //            classList.Add(obj);
        //        };
        //    return classList;
        //}

        public static T[] ToClassArray<T>(this object data) where T : DictionarizableObject
        {
            if (data == null) return null;

            var objectList = data as List<object>;
            var classList = new List<T>();
            objectList.ForEach(obj =>
            {
                if (obj.TryGetClass(out T cls)) 
                    classList.Add(cls);
            });
            return classList.ToArray();
        }
    }
}
