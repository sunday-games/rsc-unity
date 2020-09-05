using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Localization))]
public class LocalizationEditor : Editor
{
    Localization localization { get { return target as Localization; } }
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Spreadsheet"))
            SG_Utils.OpenLink(localization.urlSpreadsheet);

        GUILayout.Space(10);
        if (GUILayout.Button("Update"))
            EditorCoroutine.StartCoroutine(localization.LoadCoroutine(), localization);

        GUILayout.Space(10);
        base.OnInspectorGUI();
    }
}