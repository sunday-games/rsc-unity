using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using DG.Tweening;
using DictSO = System.Collections.Generic.Dictionary<string, object>;
using ListO = System.Collections.Generic.List<object>;
using Text = TMPro.TextMeshProUGUI;
using Dropdown = TMPro.TMP_Dropdown;
using InputField = TMPro.TMP_InputField;
using Object = UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace SG
{
    public enum Platform { Windows, Mac, Linux, iOS, Android, Web, tvOS, Editor }
    public enum Store { AppStore, GooglePlay, Amazon, Tizen, Facebook, Epic, Steam, Xsolla }
    public enum RenderPipeline { Buildin, Universal, HighDefinition }

    public static class Utils
    {
        public static DateTime EPOCH = new DateTime(1970, 1, 1);
        public static DateTime ToDateTime(this long timestamp) => EPOCH.AddMilliseconds(timestamp);
        public static long ToTimestamp(this DateTime dateTime) => (long)(dateTime - EPOCH).TotalMilliseconds;

        public static bool IsEnum<T>(this string data)
        {
            try
            {
                var value = (T)Enum.Parse(typeof(T), data, true);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static T ToEnum<T>(this object data)
        {
            if (data is int || data is long)
                return (T)Enum.ToObject(typeof(T), data.ToInt());
            if (data is string)
                return (T)Enum.Parse(typeof(T), data.ToString(), true);
            return default;
        }

        public static float ToFloat(this object data, float defaultValue = default)
        {
            try
            { return float.Parse(data.ToString(), CultureInfo.InvariantCulture); }
            catch { Log.Error($"Can't convert '{data}' to float"); return defaultValue; }
        }
        public static double ToDouble(this object data, double defaultValue = default)
        {
            try
            { return double.Parse(data.ToString(), CultureInfo.InvariantCulture); }
            catch { Log.Error($"Can't convert '{data}' to double"); return defaultValue; }
        }
        public static decimal ToDecimal(this object data, decimal defaultValue = default)
        {
            try
            { return decimal.Parse(data.ToString(), CultureInfo.InvariantCulture); }
            catch { Log.Error($"Can't convert '{data}' to decimal"); return defaultValue; }
        }
        public static byte ToByte(this object data, byte defaultValue = default)
        {
            try
            { return Convert.ToByte(data); }
            catch { Log.Error($"Can't convert '{data}' to byte"); return defaultValue; }
        }
        public static short ToShort(this object data, byte defaultValue = default)
        {
            try
            { return Convert.ToInt16(data); }
            catch { Log.Error($"Can't convert '{data}' to short"); return defaultValue; }
        }
        public static int ToInt(this object data, int defaultValue = default)
        {
            try
            { return Convert.ToInt32(data); }
            catch { Log.Error($"Can't convert '{data}' to int/int32"); return defaultValue; }
        }
        public static long ToLong(this object data, long defaultValue = default)
        {
            try
            { return Convert.ToInt64(data); }
            catch { Log.Error($"Can't convert '{data}' to long/int64"); return defaultValue; }
        }
        public static long ToLong(this BigInteger data, long defaultValue = default)
        {
            try
            { return (long)data; }
            catch { Log.Error($"Can't convert '{data}' to long/int64"); return defaultValue; }
        }
        public static ulong ToULong(this object data, ulong defaultValue = default)
        {
            try
            { return Convert.ToUInt64(data); }
            catch { Log.Error($"Can't convert '{data}' to ulong/uint64"); return defaultValue; }
        }
        public static uint ToUInt(this object data, uint defaultValue = default)
        {
            try
            { return Convert.ToUInt32(data); }
            catch { Log.Error($"Can't convert '{data}' to uint/uint32"); return defaultValue; }
        }
        public static ushort ToUShort(this object data, ushort defaultValue = default)
        {
            try
            { return Convert.ToUInt16(data); }
            catch { Log.Error($"Can't convert '{data}' to ushort/uint16"); return defaultValue; }
        }
        public static BigInteger ToBigInteger(this object data, BigInteger defaultValue = default)
        {
            try
            { return BigInteger.Parse(data.ToString()); }
            catch { Log.Error($"Can't convert '{data}' to BigInteger"); return defaultValue; }
        }
        public static bool ToBool(this object data, bool defaultValue = default)
        {
            try
            { return Convert.ToBoolean(data); }
            catch { Log.Error($"Can't convert '{data}' to bool"); return defaultValue; }
        }
        public static List<T> ToList<T>(this object data, List<T> defaultValue = null)
        {
            try
            { return data as List<T>; }
            catch { Log.Error($"Can't convert '{data}' to List {typeof(T)}"); return defaultValue; }
        }

        #region byte[]
        public static byte[] ToBytes(this string data) => Encoding.UTF8.GetBytes(data);
        public static string FromBytes(this byte[] data) => Encoding.UTF8.GetString(data);
        public static DictSO ToDict(this byte[] data) => (DictSO)Json.Deserialize(data.FromBytes());
        public static ListO ToList(this byte[] data) => (ListO)Json.Deserialize(data.FromBytes());

        public static byte[] ToBytes(this DictSO data) => Json.Serialize(data).ToBytes();
        public static byte[] ToBytes(this ListO data) => Json.Serialize(data).ToBytes();

        public static string FromBase64(this object data) => Convert.FromBase64String(data.ToString()).FromBytes();
        public static byte[] FromBase64ToBytes(this object data) => Convert.FromBase64String(data.ToString());
        #endregion

        #region DateTime
        public static bool InRange(this DateTime value, DateTime min, DateTime max, bool include = true) =>
            include
                ? value.Equals(min) || value.InRangeNotIncluded(min, max) || value.Equals(max)
                : value.InRangeNotIncluded(min, max);
        public static bool InRangeNotIncluded(this DateTime value, DateTime min, DateTime max) =>
            value > min && value < max;
        #endregion

        public static string ToDebugString(this DictSO dict) =>
            string.Join(", ", dict.Select(pair => pair.Key + "=" + pair.Value).ToArray());

        public static double Clamp(this double value, double min, double max) => value < min ? min : (value > max ? max : value);
        public static decimal Clamp(this decimal value, decimal min, decimal max) => value < min ? min : (value > max ? max : value);

        public static bool IsNotEmpty(this DictSO dict) => dict != null && dict.Count > 0;
        public static bool IsValue(this DictSO dict, string key) => dict != null && dict.ContainsKey(key) && dict[key] != null;
        public static string GetString(this DictSO dict, string key, string defaultValue = null) => dict.IsValue(key) ? dict[key].ToString() : defaultValue;
        public static bool GetBool(this DictSO dict, string key, bool defaultValue = false) => dict.IsValue(key) ? dict[key].ToBool() : defaultValue;

        public static float GetFloat(this DictSO dict, string key, float defaultValue = default) => dict.IsValue(key) ? dict[key].ToFloat() : defaultValue;
        public static double GetDouble(this DictSO dict, string key, double defaultValue = default) => dict.IsValue(key) ? dict[key].ToDouble() : defaultValue;
        public static decimal GetDecimal(this DictSO dict, string key, decimal defaultValue = default) => dict.IsValue(key) ? dict[key].ToDecimal() : defaultValue;
        public static int GetInt(this DictSO dict, string key, int defaultValue = default) => dict.IsValue(key) ? dict[key].ToInt() : defaultValue;
        public static BigInteger GetBigInteger(this DictSO dict, string key, BigInteger defaultValue = default) => dict.IsValue(key) ? dict[key].ToBigInteger() : defaultValue;
        public static long GetLong(this DictSO dict, string key, long defaultValue = default) => dict.IsValue(key) ? dict[key].ToLong() : defaultValue;
        public static ulong GetULong(this DictSO dict, string key, ulong defaultValue = default) => dict.IsValue(key) ? dict[key].ToString().ToULong() : defaultValue;
        public static ulong? GetULongNullable(this DictSO dict, string key)
        {
            if (dict.IsValue(key))
                return dict[key].ToString().ToULong();
            return null;
        }
        public static DateTime GetDateTime(this DictSO dict, string key) => dict.IsValue(key) ? dict[key].ToLong().ToDateTime() : default;
        public static T GetEnum<T>(this DictSO dict, string key)
        {
            if (!dict.IsValue(key))
                return default;
            if (dict[key] is int || dict[key] is long)
                return (T)Enum.ToObject(typeof(T), dict[key].ToInt());
            if (dict[key] is string)
                return (T)Enum.Parse(typeof(T), dict[key].ToString(), true);
            return default;
        }
        public static T GetClass<T>(this DictSO dictionary, string key) where T : IDictionarizable<T> => dictionary.IsValue(key) ? dictionary[key].ToClass<T>() : default;
        public static List<T> GetClassList<T>(this DictSO dictionary, string key) where T : IDictionarizable<T> => dictionary.IsValue(key) ? dictionary[key].ToClassList<T>() : default;

        public static bool TryGet(this DictSO dict, string key, object value)
        {
            if (dict != null && dict.TryGetValue(key, out object obj) && obj != null)
            {
                value = obj;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGet<T>(this DictSO dict, string key, out T value) where T : class
        {
            if (dict != null && dict.TryGetValue(key, out object obj) && obj != null)
            {
                value = obj as T;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetStruct<T>(this DictSO dict, string key, out T value) where T : struct
        {
            if (dict != null && dict.TryGetValue(key, out object obj) && obj != null)
            {
                value = (T)obj;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetString(this DictSO dict, string key, out string value)
        {
            if (dict != null && dict.IsValue(key) && dict[key] is string)
            {
                value = dict[key].ToString();
                return true;
            }

            value = default;
            return false;
        }
        public static bool TryGetInt(this DictSO dict, string key, out int value)
        {
            if (dict != null && dict.IsValue(key) && (dict[key] is int || dict[key] is long))
            {
                value = dict[key].ToInt();
                return true;
            }

            value = default;
            return false;
        }
        public static bool TryGetLong(this DictSO dict, string key, out long value)
        {
            if (dict != null && dict.IsValue(key) && (dict[key] is int || dict[key] is long))
            {
                value = dict[key].ToLong();
                return true;
            }

            value = default;
            return false;
        }
        public static bool TryGetULong(this DictSO dict, string key, out ulong value)
        {
            if (dict != null && dict.IsValue(key) && (dict[key] is int || dict[key] is long))
            {
                value = dict[key].ToULong();
                return true;
            }

            value = default;
            return false;
        }
        public static bool TryGetDouble(this DictSO dict, string key, out double value)
        {
            if (dict != null && dict.IsValue(key) && (dict[key] is float || dict[key] is double))
            {
                value = dict[key].ToDouble();
                return true;
            }

            value = default;
            return false;
        }
        public static bool TryGetDecimal(this DictSO dict, string key, out decimal value)
        {
            if (dict != null && dict.IsValue(key) && (dict[key] is int || dict[key] is long || dict[key] is float || dict[key] is double || dict[key] is decimal))
            {
                value = dict[key].ToDecimal();
                return true;
            }

            value = default;
            return false;
        }
        public static bool TryGetBigInteger(this DictSO dict, string key, out BigInteger value)
        {
            if (dict != null && dict.IsValue(key))
            {
                value = dict[key].ToBigInteger();
                return true;
            }

            value = default;
            return false;
        }
        public static bool TryGetBool(this DictSO dict, string key, out bool value)
        {
            if (dict != null && dict.IsValue(key) && dict[key] is bool)
            {
                value = dict[key].ToBool();
                return true;
            }

            value = default;
            return false;
        }
        public static bool TryGetDict(this DictSO dict, string key, out DictSO value)
        {
            if (dict != null && dict.IsValue(key) && dict[key] is DictSO)
            {
                value = (DictSO)dict[key];
                return true;
            }

            value = default;
            return false;
        }
        public static bool TryGetList(this DictSO dict, string key, out ListO value)
        {
            if (dict != null && dict.IsValue(key) && dict[key] is ListO)
            {
                value = (ListO)dict[key];
                return true;
            }

            value = default;
            return false;
        }

        #region Vector3
        public static ListO ToObject(this Vector3 v, int digits = default) =>
            digits == default ? new ListO { v.x, v.y, v.z } : new ListO { Math.Round(v.x, digits), Math.Round(v.y, digits), Math.Round(v.z, digits) };
        public static Vector3 ToVector3(this ListO list) => new Vector3(list[0].ToFloat(), list[1].ToFloat(), list[2].ToFloat());
        public static Vector3 GetVector3(this DictSO dict, string key) => dict[key] is ListO l ? new Vector3(l[0].ToFloat(), l[1].ToFloat(), l[2].ToFloat()) : default;
        public static bool TryGetVector3(this DictSO dict, string key, out Vector3 value)
        {
            if (dict != null && dict.IsValue(key) && dict[key] is ListO list)
            {
                value = list.ToVector3();
                return true;
            }

            value = default;
            return false;
        }
        public static Vector3 Round(this Vector3 v, int d) => new Vector3(v.x.Round(d), v.y.Round(d), v.z.Round(d));
        public static Vector3 RoundToNearestHalf(this Vector3 v) => new Vector3(v.x.RoundToNearestHalf(), v.y.RoundToNearestHalf(), v.z.RoundToNearestHalf());
        public static Vector3 ToVector3(this object v)
        {
            if (v is string stringValue)
            {
                if (stringValue.StartsWith("(") && stringValue.EndsWith(")"))
                    stringValue = stringValue.Substring(1, stringValue.Length - 2);

                var values = stringValue.Split(',');
                return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            }

            return (Vector3)v;
        }

        public static bool IsNear(this Vector3 v, Vector3 other, float distance = 0)
        {
            const float tolerance = 0.01f;
            return (v - other).sqrMagnitude <= (distance * distance) + tolerance;
        }
        #endregion

        #region Quaternion
        public static ListO ToObject(this Quaternion q, int digits = default) =>
            digits == default ? new ListO { q.x, q.y, q.z, q.w } : new ListO { Math.Round(q.x, digits), Math.Round(q.y, digits), Math.Round(q.z, digits), Math.Round(q.w, digits) };
        public static Quaternion ToQuaternion(this ListO list) => new Quaternion(list[0].ToFloat(), list[1].ToFloat(), list[2].ToFloat(), list[3].ToFloat());
        public static Quaternion GetQuaternion(this DictSO dict, string key) => dict[key] is ListO l ? new Quaternion(l[0].ToFloat(), l[1].ToFloat(), l[2].ToFloat(), l[3].ToFloat()) : default;
        public static bool TryGetQuaternion(this DictSO dict, string key, out Quaternion value)
        {
            if (dict != null && dict.IsValue(key) && dict[key] is ListO list)
            {
                value = list.ToQuaternion();
                return true;
            }

            value = default;
            return false;
        }
        public static Quaternion Round(this Quaternion q, int d) => new Quaternion(q.x.Round(d), q.y.Round(d), q.z.Round(d), q.w.Round(d));
        #endregion

        public static float Round(this float f, int digits) => (float)Math.Round(f, digits);
        public static double Round(this double f, int digits) => Math.Round(f, digits);
        public static float RoundToNearestHalf(this float f) => (float)Math.Round(f * 2f) * 0.5f;

        public static string ToStringRGB(this Color color) => ColorUtility.ToHtmlStringRGB(color);
        public static Color SetAlpha(this Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);

        public static bool Contains<T>(this T[] array, T item) => Array.IndexOf(array, item) >= 0;
        public static int IndexOf<T>(this T[] array, T item) => Array.IndexOf(array, item);

        public static T[] Concat<T>(params T[][] list)
        {
            var resultLength = 0;
            for (var i = 0; i < list.Length; i++)
            {
                resultLength += list[i].Length;
            }

            var result = new T[resultLength];
            var offset = 0;
            for (var x = 0; x < list.Length; x++)
            {
                list[x].CopyTo(result, offset);
                offset += list[x].Length;
            }
            return result;
        }

        public static T Copy<T>(this T prefab, Transform parent = null, bool active = true) where T : Component
        {
            var instance = Object.Instantiate(prefab, parent ?? prefab.transform.parent);
            instance.name = prefab.name;
            instance.gameObject.SetActive(active);
            return instance;
        }

        public static void CopyStruct<T>(in T[] from, ref T[] to) where T : struct
        {
            if (from == null && to == null)
                return;
            if (from == null && to != null)
            {
                to = null;
                return;
            }
            if (to == null || to.Length != from.Length)
            {
                Array.Resize(ref to, from.Length);
            }
            Array.Copy(from, to, from.Length);
        }

        public static T DeepClone<T>(this T obj) where T : struct
        {
            using var ms = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Context = new StreamingContext(StreamingContextStates.Clone);
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T)formatter.Deserialize(ms);
        }

        public static void DestroyAndClear<T>(this IList<T> list) where T : Component
        {
            if (list == null)
                return;

            if (Application.isPlaying)
            {
                for (int i = 0; i < list.Count; ++i)
                    if (list[i] != null)
                        Object.Destroy(list[i].gameObject);
            }
            else
            {
                for (int i = 0; i < list.Count; ++i)
                    if (list[i] != null)
                        Object.DestroyImmediate(list[i].gameObject);
            }
            list.Clear();
        }

        public static void DestroyAndClearByValue<K, T>(this Dictionary<K, T> dict) where T : Component
        {
            foreach (var item in dict)
                if (item.Value != null)
                    Object.Destroy(item.Value.gameObject);
            dict.Clear();
        }

        public static void DestroyAndClearByKey<K, V>(this Dictionary<K, V> dict) where K : Component
        {
            foreach (var item in dict)
                if (item.Key != null)
                    Object.Destroy(item.Key.gameObject);
            dict.Clear();
        }

        public static string ToText(this DictSO dict)
        {
            string text = "";
            foreach (var pair in dict)
                text += pair.Key + "=" + pair.Value + ", ";
            return text;
        }

        public static bool IsEmpty(this string text) => string.IsNullOrEmpty(text);
        public static bool IsNotEmpty(this string text) => string.IsNullOrEmpty(text) == false;
        public static bool IsEqualIgnoreCase(this string a, string b) => (a.IsEmpty() && b.IsEmpty()) || string.Equals(a, b, StringComparison.OrdinalIgnoreCase); // ?? StringComparison.InvariantCultureIgnoreCase
        public static bool IsContainsIgnoreCase(this string a, string b) => a.ToLower().Contains(b.ToLower());
        public static string EscapeURL(this string url) => UnityWebRequest.EscapeURL(url);
        public static string CutMiddle(this string text, int left, int right = -1)
        {
            if (right < 0)
                right = left;
            return text.Substring(0, left) + "..." + text.Substring(text.Length - right, right);
        }
        public static string MD5(this string text)
        {
            var hashBytes = new MD5CryptoServiceProvider()
                .ComputeHash(new UTF8Encoding().GetBytes(text));

            var hashString = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                hashString.Append(Convert.ToString(hashBytes[i], 16).PadLeft(2, '0'));

            return hashString.ToString().PadLeft(32, '0');
        }

        public static int GetHash<T1>(T1 value1) => GetHash(23, value1); // using magic number for base argument
        public static int GetHash<T1, T2>(T1 value1, T2 value2) => GetHash(GetHash(value1), value2);
        public static int GetHash<T1, T2, T3>(T1 value1, T2 value2, T3 value3) => GetHash(GetHash(value1, value2), value3);
        public static int GetHash<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4) => GetHash(GetHash(value1, value2), GetHash(value3, value4));
        public static int GetHash<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) => GetHash(GetHash(value1, value2), GetHash(value3, value4, value5));
        public static int GetHash<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6) => GetHash(GetHash(value1, value2, value3), GetHash(value4, value5, value6));
        public static int GetHash<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) => GetHash(GetHash(value1, value2, value3), GetHash(value4, value5, value6, value7));
        private static int GetHash<T1>(int hash, T1 value) => hash * 31 + value.GetHashCode(); // using magic number for secondary argument

        public static float RandomValue() => Random.value;
        public static bool RandomChance(float chance) => Random.value < chance;
        public static int RandomRange(int min, int max) => Random.Range(min, max);
        public static float RandomRange(float min, float max) => Random.Range(min, max);
        public static float RandomRange(double min, double max) => Random.Range((float)min, (float)max);
        public static string RandomString(int length, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        {
            var random = new System.Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static T First<T>(this IList<T> list) => list[0];
        public static T Last<T>(this IList<T> list) => list[list.Count - 1];
        public static T GetRandom<T>(this IList<T> list) => list[RandomRange(0, list.Count)];
        public static T GetNext<T>(this IList<T> list, T item)
        {
            if (list == null || list.Count == 0)
                return default;
            if (list.Count == 1)
                return list[0];
            var index = list.IndexOf(item);
            return list[index == list.Count - 1 ? 0 : index + 1];
        }
        public static T GetNext<T>(this IList<T> list, T item, int change)
        {
            if (list == null || list.Count == 0)
                return default;
            if (list.Count == 1)
                return list[0];
            return list[list.GetNextIndex(list.IndexOf(item), change)];
        }
        public static int GetNextIndex<T>(this IList<T> list, int current, int change)
        {
            var next = current + change;
            if (next < 0)
                return list.Count + next;
            if (next >= list.Count)
                return next - list.Count;
            return next;
        }
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            var rng = new System.Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
        public static bool HasElement<T>(this IList<T> list, Func<T, bool> match) => list.Any(match);

        public static bool TryFind<T>(this Queue<T> queue, Func<T, bool> match, out T item)
        {
            item = default;
            foreach (var q in queue)
                if (match(q))
                {
                    item = q;
                    return true;
                }
            return false;
        }
        public static T Find<T>(this IList<T> list, Func<T, bool> match)
        {
            if (list != null)
                for (int i = 0; i < list.Count; ++i)
                    if (match(list[i]))
                        return list[i];
            return default;
        }
        public static bool TryFind<T>(this IList<T> list, Func<T, bool> match, out T item)
        {
            item = default;
            for (int i = 0; i < list.Count; ++i)
                if (match(list[i]))
                {
                    item = list[i];
                    return true;
                }
            return false;
        }
        public static bool TryFind<T>(this IList<T> list, Func<T, bool> match, out int index)
        {
            index = default;
            for (int i = 0; i < list.Count; ++i)
                if (match(list[i]))
                {
                    index = i;
                    return true;
                }
            return false;
        }
        public static bool TryFindSubtype<T2, T1>(this IList<T1> list, Func<T1, bool> match, out T2 item) where T2 : T1
        {
            item = default;
            for (int i = 0; i < list.Count; ++i)
                if (match(list[i]))
                {
                    item = (T2)list[i];
                    return true;
                }
            return false;
        }
        public static bool TryFind<T>(this IList<T> list, Func<T, bool> match, out List<T> items)
        {
            items = new List<T>();
            for (int i = 0; i < list.Count; ++i)
                if (match(list[i]))
                    items.Add(list[i]);
            return items.Count > 0;
        }

        public static string GetStringFormater(int n, char c)
        {
            var formater = "";
            while (formater.Length < n)
                formater += c;
            return "0." + formater;
        }

        public static float MapRange(float fromMin, float fromMax, float toMin, float toMax, float val) =>
            toMin + (val - fromMin) * (toMax - toMin) / (fromMax - fromMin);

        // Clamps a value between a minimum float and maximum float value
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
        // Clamps a value between a minimum int and maximum int value
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static DateTime ToDateTime(this string text)
        {
            // data = 2022-12-31 13:00:00
            if (text.Split(' ', out string date, out string time) &&
                date.Split('-', out string year, out string mounth, out string day) &&
                time.Split(':', out string hour, out string minute, out string second))
                return new DateTime(year.ToInt(), mounth.ToInt(), day.ToInt(), hour.ToInt(), minute.ToInt(), second.ToInt());
            return default;
        }
        public static bool Split(this string data, char c, out string p0, out string p1)
        {
            var array = data.Split(c);
            if (array.Length == 2)
            {
                p0 = array[0];
                p1 = array[1];
                return true;
            }

            p0 = null;
            p1 = null;
            return false;
        }
        public static bool Split(this string data, char c, out string p0, out string p1, out string p2)
        {
            var array = data.Split(c);
            if (array.Length == 3)
            {
                p0 = array[0];
                p1 = array[1];
                p2 = array[2];
                return true;
            }

            p0 = null;
            p1 = null;
            p2 = null;
            return false;
        }

        // TODO Remove
        public static float CheckObstacle(Ray ray, float distace)
        {
            LayerMask obstacles = LayerMask.GetMask("AstarObstacles");
            if (Physics.Raycast(ray, out RaycastHit hit, distace, obstacles))
                return hit.distance;

            return -1;
        }
        public static bool HasObstacle(Ray ray, float distance)
        {
            LayerMask obstacles = LayerMask.GetMask("AstarObstacles");
            return Physics.Raycast(ray, out RaycastHit hit, distance, obstacles);
        }

        public static bool HasObstacle(Collider me, Vector3 startPoint, Vector3 endPoint, float radius, Collider other = null, bool showDebug = false)
        {
            var direction = endPoint - startPoint;
            var obstaclesLayer = 1 << LayerMask.NameToLayer("AstarObstacles");
            var interactiveLayer = 1 << LayerMask.NameToLayer("Interactible");
            var defaultLayer = 1 << LayerMask.NameToLayer("Default");
            var layerMask = obstaclesLayer | interactiveLayer | defaultLayer;

            var hits = Physics.SphereCastAll(startPoint, radius, direction.normalized, direction.magnitude, layerMask);
            var hitsCount = hits.Count(hit => !ReferenceEquals(hit.collider, me) && !ReferenceEquals(hit.collider, other) && hit.point != Vector3.zero);
            if (hitsCount == 0)
            {
                if (showDebug)
                {
                    Debug.DrawLine(startPoint, endPoint, Color.blue, 0.5f);
                }

                return false;
            }

            if (showDebug)
            {
                Debug.DrawLine(startPoint, endPoint, Color.red, 0.5f);
            }

            return true;
        }

        public static bool IsDictionaryEqualsByKeys<K, V>(this Dictionary<K, V> first, Dictionary<K, V> second)
        {
            if (first.Count != second.Count)
                return false;
            if (first.Keys.Except(second.Keys).Any())
                return false;
            if (second.Keys.Except(first.Keys).Any())
                return false;

            return true;
        }

        public static V GetValue<K, V>(this Dictionary<K, V> dict, K key) =>
            dict.ContainsKey(key) ? dict[key] : default;

        #region GameObject

        public static GameObject Activate(this GameObject go) { if (!go.activeSelf) go.SetActive(true); return go; }
        public static GameObject Deactivate(this GameObject go) { if (go.activeSelf) go.SetActive(false); return go; }

        public static bool ActivateIf(this GameObject go, bool value)
        {
            if (go.activeSelf != value)
                go.SetActive(value);

            return value;
        }

        public static void Destroy(this GameObject go)
        {
            if (go == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(go);
            else
                Object.DestroyImmediate(go);
        }

        public static string GetPath(this GameObject go)
        {
            var t = go.transform;

            var path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return "/" + path;
        }

        public static void SetMaterial(this GameObject go, Material material)
        {
            var renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer.materials.Length > 1)
                    for (int i = 0; i < renderer.materials.Length; i++)
                        renderer.materials[i] = material;
                else
                    renderer.material = material;
            }
        }

        #endregion

        #region Component

        public static Component Activate(this Component component) { component.gameObject.SetActive(true); return component; }
        public static Component Deactivate(this Component component) { component.gameObject.SetActive(false); return component; }
        public static bool ActivateIf(this Component component, bool value) { component.gameObject.SetActive(value); return value; }

        public static RectTransform RectTransform(this Component component) => component.transform as RectTransform;

        public static void Destroy(this Component component)
        {
            if (component == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(component.gameObject);
            else
                Object.DestroyImmediate(component.gameObject);

            component = null;
        }

        public static bool TryGetComponentInParent<T>(this Component component, out T componentInParent)
        {
            componentInParent = component.GetComponentInParent<T>();
            return componentInParent != null;
        }

        public static bool TryFind<T>(string name, out T component)
        {
            component = default;

            var go = GameObject.Find(name);

            if (go == null)
            {
                Log.Error("Fail to find GameObject with name " + name);
                return false;
            }

            component = go.GetComponent<T>();

            if (component == null)
            {
                Log.Error("Fail to get " + nameof(T) + " form " + name);
                return false;
            }

            return true;
        }

        public static void Shake(this Component component, float strength = 50f)
        {
            if (component.gameObject.GetComponent<SG.UI.Shaker>() != null)
                return;

            var shaker = component.gameObject.AddComponent<SG.UI.Shaker>();
            shaker.Strength = strength;
        }

        public static Tweener DoFloat(this Animator animator, string paramName, float endValue, float duration) =>
            DOTween.To(() => animator.GetFloat(paramName), x => animator.SetFloat(paramName, x), endValue, duration)
                .SetTarget<Tweener>(paramName);

        #endregion

        #region UI Components

        public static void SetText(this Text component, object text) => component.text = text.ToString();

        public static Text SetTextAndActivate(this Text component, string text)
        {
            component.gameObject.SetActive(true);
            component.text = text;
            return component;
        }

        public static void WrongInput(this InputField input)
        {
            input.textComponent.color = Color.red;
            input.textComponent.Shake(15);
        }

        public static void SetValue(this Dropdown dropdown, int index)
        {
            dropdown.value = index;
            dropdown.onValueChanged.Invoke(index);
        }

        public static void SetValue(this Dropdown dropdown, string text)
        {
            if (dropdown.options.TryFind(option => option.text == text, out int index))
                dropdown.SetValue(index);
        }

        public static void SetNextValue(this Dropdown dropdown) =>
            dropdown.SetValue(dropdown.value < dropdown.options.Count - 1 ? dropdown.value + 1 : 0);

        public static void SetNextValueWithoutNotify(this Dropdown dropdown) =>
            dropdown.SetValueWithoutNotify(dropdown.value < dropdown.options.Count - 1 ? dropdown.value + 1 : 0);

        public static string GetTextValue(this Dropdown dropdown) => dropdown.options[dropdown.value].text;

        #endregion

        #region File

        public static void SaveToFile(string path, string text, bool isLog = true)
        {
            if (File.Exists(path))
                File.Delete(path);
            else
                new FileInfo(path).Directory.Create();

            File.WriteAllText(path, text);
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
            if (isLog)
                Log.Info("Data saved to " + path);
        }
        public static void SaveToFile(string path, byte[] bytes, bool isLog = true)
        {
            if (File.Exists(path))
                File.Delete(path);
            else
                new FileInfo(path).Directory.Create();

            File.WriteAllBytes(path, bytes);
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
            if (isLog)
                Log.Info("Data saved to " + path);
        }

        public static bool LoadFromFile(string path, out string data)
        {
            if (!File.Exists(path))
            {
                Log.Debug("File at path: " + path + " not found");
                data = null;
                return false;
            }

            data = File.ReadAllText(path);
            Log.Info("Data loaded from " + path);
            return true;
        }
        public static bool LoadFromFile(string path, out byte[] bytes, bool isLog)
        {
            if (!File.Exists(path))
            {
                if (isLog)
                    Log.Info("File at path: " + path + " not found");
                bytes = null;
                return false;
            }

            bytes = File.ReadAllBytes(path);
            if (isLog)
                Log.Info("Data loaded from " + path);
            return true;
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }
        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static void DeleteFile(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir) && File.Exists(path))
                File.Delete(path);
        }

        #endregion

        #region Resources

        public static bool HaveTextResource(string name, out TextAsset textAsset, bool isLog)
        {
            textAsset = Resources.Load<TextAsset>(name);
            if (textAsset == null)
            {
                if (isLog)
                    Log.Info("Resource file with name: " + name + " not found");

                return false;
            }

            return true;
        }

        public static bool LoadFromResources(string name, out string data, bool isLog)
        {
            if (HaveTextResource(name, out TextAsset textAsset, isLog))
            {
                if (isLog)
                    Log.Info("Data loaded from " + name);

                data = textAsset.text;
                return true;
            }

            data = null;
            return false;
        }
        public static bool LoadFromResources(string name, out byte[] bytes, bool isLog)
        {
            if (HaveTextResource(name, out TextAsset textAsset, isLog))
            {
                if (isLog)
                    Log.Info("Data loaded from " + name);

                bytes = textAsset.bytes;
                return true;
            }

            bytes = null;
            return false;
        }

        #endregion

#if UNITY_EDITOR
        public static T GetSerializedValue<T>(this SerializedProperty property)
        {
            object @object = property.serializedObject.targetObject;
            string[] propertyNames = property.propertyPath.Split('.');

            // Clear the property path from "Array" and "data[i]".
            if (propertyNames.Length >= 3 && propertyNames[propertyNames.Length - 2] == "Array")
                propertyNames = propertyNames.Take(propertyNames.Length - 2).ToArray();

            // Get the last object of the property path.
            foreach (string path in propertyNames)
            {
                @object = @object.GetType()
                    .GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .GetValue(@object);
            }

            if (@object.GetType().GetInterfaces().Contains(typeof(IList<T>)))
            {
                int propertyIndex = int.Parse(property.propertyPath[property.propertyPath.Length - 2].ToString());

                return ((IList<T>)@object)[propertyIndex];
            }

            return (T)@object;
        }
#endif

        public static bool IsStore(Store store) => Configurator.Instance.Store == store;

        public static bool IsPlatform(Platform platform) => Application.platform == PlatformToRPlatform[platform];
        public static Dictionary<Platform, RuntimePlatform> PlatformToRPlatform = new Dictionary<Platform, RuntimePlatform>
        {
            [Platform.Windows] = RuntimePlatform.WindowsPlayer,
            [Platform.Mac] = RuntimePlatform.OSXPlayer,
            [Platform.Linux] = RuntimePlatform.LinuxPlayer,
            [Platform.Web] = RuntimePlatform.WebGLPlayer,
            [Platform.iOS] = RuntimePlatform.IPhonePlayer,
            [Platform.Android] = RuntimePlatform.Android,
            [Platform.tvOS] = RuntimePlatform.tvOS,
        };

        public static bool IsPlatform(params Platform[] platforms)
        {
            foreach (var platform in platforms)
                if (IsPlatform(platform))
                    return true;
            return false;
        }
        public static bool IsPlatform(params RuntimePlatform[] platforms)
        {
            foreach (var platform in platforms)
                if (platform == Application.platform)
                    return true;
            return false;
        }

        public static bool IsPlatformEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
        public static bool IsPlatformMobile() => IsPlatform(RuntimePlatform.IPhonePlayer, RuntimePlatform.Android);
        public static bool IsPlatformDesktop() => IsPlatform(RuntimePlatform.WindowsPlayer, RuntimePlatform.OSXPlayer, RuntimePlatform.LinuxPlayer);

        public static string ActiveSceneName => SceneManager.GetActiveScene().name;

        public static IEnumerator WaitAndExecute(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            action();
        }

        public static Type CreateComponent<Type>(this Transform transform, bool required = true, string name = null) where Type : Component
        {
            var instance = transform.GetComponentInChildren<Type>();

            if (required && !instance)
            {
                instance = new GameObject(name ?? typeof(Type).Name).AddComponent<Type>();
                instance.transform.SetParent(transform);
            }
            else if (!required && instance)
            {
                Object.DestroyImmediate(instance.gameObject);
                instance = null;
            }

            return instance;
        }

        public static Component CreateComponent(this Transform transform, Type type, bool required = true, string name = null)
        {
            var instance = transform.GetComponentInChildren(type);

            if (required && !instance)
            {
                instance = new GameObject(name ?? nameof(type)).AddComponent(type);
                instance.transform.SetParent(transform);
            }
            else if (!required && instance)
            {
                Object.DestroyImmediate(instance.gameObject);
                instance = null;
            }

            return instance;
        }

        public static string SpaceFormat(this int n) => SpaceFormat((long)n);
        public static string SpaceFormat(this long n)
        {
            if (n < 10000) return n.ToString();

            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            string result = n.ToString("#,#", nfi);

            return string.IsNullOrEmpty(result) ? "0" : result;
        }

        public static string ToStringFormated(this float d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.##################}", d);
        public static string ToStringFormated1(this float d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.#}", d);
        public static string ToStringFormated2(this float d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.##}", d);
        public static string ToStringFormated3(this float d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.###}", d);
        public static string ToStringFormated(this double d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.##################}", d);
        public static string ToStringFormated1(this double d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.#}", d);
        public static string ToStringFormated2(this double d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.##}", d);
        public static string ToStringFormated3(this double d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.###}", d);
        public static string ToStringFormated(this decimal d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.##################}", d);
        public static string ToStringFormated1(this decimal d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.#}", d);
        public static string ToStringFormated2(this decimal d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.##}", d);
        public static string ToStringFormated3(this decimal d) => string.Format(CultureInfo.InvariantCulture, "{0:#,##0.###}", d);

        public static string TagColor(this object value, Color color) => $"<color=#{color.ToStringRGB()}>{value}</color>";
        public static string TagColor(this object value, Color color, string format) => TagColor(string.Format(format, value), color);
        public static string TagSup(object text) => $"<sup>{text}</sup>";
        public static string TagItalic(this object text) => $"<i>{text}</i>";
        public static string TagSize(this object text, float size) => $"<size={size * 100}%>{text}</size>";

        public static string TagIndent(int value) => $"<indent={value}%>";
        public static string TagIndent50 = "<indent=50%>";
        public static string TagIndent60 = "<indent=60%>";
    }
}