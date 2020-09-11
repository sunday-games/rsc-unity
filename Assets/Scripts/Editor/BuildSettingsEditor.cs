using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildSettings))]
public class BuildSettingsFeatures : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        if (GUILayout.Button("Delete PlayerPrefs")) PlayerPrefs.DeleteAll();

        GUILayout.Space(20);
        if (GUILayout.Button("Debug")) Core.build.SetupDebug();
        GUILayout.Space(5);
        if (GUILayout.Button("Release")) Core.build.SetupRelease();

        GUILayout.Space(5);
        base.OnInspectorGUI();
    }
}
