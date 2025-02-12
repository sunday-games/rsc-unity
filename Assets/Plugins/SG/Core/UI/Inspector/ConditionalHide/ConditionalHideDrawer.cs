#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace SG.UI
{
    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHideDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var condAttribute = (ConditionalHideAttribute)attribute;

            var enabled = GetConditionalHideAttributeResult(condAttribute, property);

            var wasEnabled = GUI.enabled;
            GUI.enabled = enabled;

            if (!condAttribute.hideInInspector || enabled)
                EditorGUI.PropertyField(position, property, label, true);

            GUI.enabled = wasEnabled;
        }

        bool GetConditionalHideAttributeResult(ConditionalHideAttribute condAttribute, SerializedProperty property)
        {
            var enabled = true;

            var propertyPath = property.propertyPath;
            var conditionPath = propertyPath.Replace(property.name, condAttribute.conditionalSourceField);
            var sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

            if (sourcePropertyValue != null)
                enabled = sourcePropertyValue.boolValue;
            else
                Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condAttribute.conditionalSourceField);

            return enabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var condAttribute = (ConditionalHideAttribute)attribute;
            var enabled = GetConditionalHideAttributeResult(condAttribute, property);

            if (!condAttribute.hideInInspector || enabled)
                return EditorGUI.GetPropertyHeight(property, label);
            else
                return -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
#endif