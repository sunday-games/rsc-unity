#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;
#endif

namespace SG
{
#if UNITY_EDITOR
    public static class InspectorExtensions
    {
        public static IEnumerable<T> FindScriptableObjectsOfType<T>(string path = null) where T : ScriptableObject
        {
            var guids = path == null
                ? AssetDatabase.FindAssets($"t:{typeof(T).FullName}")
                : AssetDatabase.FindAssets($"t:{typeof(T).FullName}", new string[] { path });
            
            return guids.Select(guid =>
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            });
        }

        public static T FindOrCreateNewScriptableObject<T>(string creationPath) where T : ScriptableObject
        {
            T instance = null;
            if (AssetDatabase.FindAssets($"t:{typeof(T).FullName}").Any(guid =>
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    instance = AssetDatabase.LoadAssetAtPath<T>(path);
                    return true;
                })) return instance;

            instance = ScriptableObject.CreateInstance<T>();

            if (!System.IO.Directory.Exists(creationPath))
                System.IO.Directory.CreateDirectory(creationPath);

            AssetDatabase.CreateAsset(instance, $"{creationPath}/{typeof(T).Name}.asset");
            AssetDatabase.SaveAssets();

            return instance;
        }

        public static T CreateNewScriptableObject<T>(string creationPath, string name) where T : ScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();

            if (!System.IO.Directory.Exists(creationPath))
                System.IO.Directory.CreateDirectory(creationPath);

            AssetDatabase.CreateAsset(instance, $"{creationPath}/{name}.asset");
            AssetDatabase.SaveAssets();

            return instance;
        }

        public static void CreateOrReplaceScriptableObject<T>(string creationPath, string name, T instance) where T : ScriptableObject
        {
            AssetDatabase.DeleteAsset(creationPath + name + ".asset");

            if (!System.IO.Directory.Exists(creationPath))
                System.IO.Directory.CreateDirectory(creationPath);

            AssetDatabase.CreateAsset(instance, $"{creationPath}/{name}.asset");
            // AssetDatabase.SaveAssets();
        }
    }
#endif
}