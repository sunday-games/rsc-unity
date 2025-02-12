#if UNITY_EDITOR
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace SG.UI
{
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonDrawer : PropertyDrawer
    {
        MethodInfo _eventMethodInfo;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            var shift = pos.width * 0.3805f;

            if (GUI.Button(new Rect(pos.x + shift, pos.y, pos.width - shift, pos.height), label.text))
            {
                var eventOwnerType = prop.serializedObject.targetObject.GetType();
                var eventName = (attribute as ButtonAttribute).MethodName;

                if (_eventMethodInfo == null)
                    _eventMethodInfo = eventOwnerType.GetMethod(eventName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (_eventMethodInfo != null)
                    _eventMethodInfo.Invoke(prop.serializedObject.targetObject, null);
                else
                    Log.UI.Warning($"ButtonAttribute: Unable to find method {eventName} in {eventOwnerType}");
            }
        }
    }
}
#endif