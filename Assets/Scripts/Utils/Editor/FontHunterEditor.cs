using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Collections;

[CustomEditor(typeof(FontHunter))]
public class FontHunterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Space(20);

        if (GUILayout.Button("Change Font")) ChangeFont();

        GUILayout.Space(20);

        base.OnInspectorGUI();
    }

    void ChangeFont()
    {
        var fontHunter = FindObjectOfType(typeof(FontHunter)) as FontHunter;

        int count = 0;
        foreach (var target in fontHunter.targets)
            foreach (var transform in target.GetComponentsInChildren<Transform>(true))
            {
                var text = transform.GetComponent<Text>();
                if (text != null && text.font != fontHunter.font)
                {
                    text.font = fontHunter.font;
                    ++count;
                }
            }

        Core.Log("FontHunter - Find and changed " + count);
    }
}
