#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using SG.Core;

namespace SG
{
    public class ConfiguratorWindow : EditorWindow
    {
        [MenuItem("Sunday/Delete Player Prefs")]
        public static void DeletePlayerPrefs() => Configurator.DeletePlayerPrefs();

        [MenuItem("Sunday/Delete Zone Save")]
        public static void DeleteZoneSave() => Utils.DeleteFile(Constants.INIT_STATES_EXTERNAL_PATH);

        [MenuItem("Sunday/Configurator")]
        public static void ShowWindow()
        {
            if (Configurator.Instance)
                GetWindow<ConfiguratorWindow>("Configurator");
        }

        private long zoneId;
        void OnGUI()
        {
            if (!Configurator.Instance)
                return;

            GUILayout.Label("Environment");
            if (GUILayout.Button(Configurator.production ? "Production" : "Debug"))
            {
                CurrentEnvironment.Instance.Environment = Configurator.production ? Environment.Name.QA : Environment.Name.Prod;
                Configurator.Instance.Apply();
            }

            GUILayout.Space(10);
            GUILayout.Label("Settings");
            if (GUILayout.Button("Apply"))
                Configurator.Instance.Apply();
#if SG_LOCALIZATION
            if (Configurator.Instance.localization)
            {
                GUILayout.Space(10);
                GUILayout.Label("Localization");

                if (GUILayout.Button("Open"))
                    foreach (var source in Localization.Instance.GetComponentsInChildren<LocalizationSource>())
                    {
                        source.Open();
                        break;
                    }

                if (GUILayout.Button("Download"))
                    Localization.Download();
            }
#endif
            GUILayout.Space(10);
            GUILayout.Label("Player Prefs");
            if (GUILayout.Button("Delete"))
                Configurator.DeletePlayerPrefs();
        }
    }
}
#endif