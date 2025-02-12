using UnityEngine;

namespace SG
{
    [CreateAssetMenu(menuName = "Sunday/CurrentEnvironment")]
    public class CurrentEnvironment : ScriptableObject
    {
        private static CurrentEnvironment _instance;
        public static CurrentEnvironment Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = Resources.Load<CurrentEnvironment>("CurrentEnvironment");

                if (_instance != null)
                    return _instance;

#if UNITY_EDITOR
                _instance = CreateInstance<CurrentEnvironment>();

                UnityEditor.AssetDatabase.CreateAsset(_instance, "Assets/Resources/CurrentEnvironment.asset");
                UnityEditor.AssetDatabase.SaveAssets();
#else
                _instance = Resources.Load<CurrentEnvironment>("DefaultEnvironment");
#endif

                return _instance;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Sunday/Environment/Set Prod")]
        public static void SetEnvironmentProd() => SetEnvironment(SG.Environment.Name.Prod);

        [UnityEditor.MenuItem("Sunday/Environment/Set Dev")]
        public static void SetEnvironmentDev() => SetEnvironment(SG.Environment.Name.Dev);

        [UnityEditor.MenuItem("Sunday/Environment/Set QA")]
        public static void SetEnvironmentQA() => SetEnvironment(SG.Environment.Name.QA);

        [UnityEditor.MenuItem("Sunday/Environment/Set Local")]
        public static void SetEnvironmentLocal() => SetEnvironment(SG.Environment.Name.Local);

        private static void SetEnvironment(Environment.Name env)
        {
            Instance.Environment = env;

            Log.Info("Environment set as " + env);

            UnityEditor.EditorUtility.SetDirty(Instance);
        }
#endif

        public Environment.Name Environment = SG.Environment.Name.QA;
        public bool TestProd;
        public ulong PrefferedStarSystem = 2;
    }
}