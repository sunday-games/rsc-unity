//using UnityEditor;

//namespace SG.UI
//{
//    [CustomEditor(typeof(Button), true)]
//    [CanEditMultipleObjects]
//    /// <summary>
//    ///   Custom Editor for the Button Component.
//    ///   Extend this class to write a custom editor for an Button-derived component.
//    /// </summary>
//    public class ButtonEditor : SelectableEditor
//    {
//        SerializedProperty m_OnClickProperty;

//        protected override void OnEnable()
//        {
//            base.OnEnable();
//            m_OnClickProperty = serializedObject.FindProperty("OnClick");
//        }

//        public override void OnInspectorGUI()
//        {
//            base.OnInspectorGUI();
//            EditorGUILayout.Space();

//            serializedObject.Update();
//            EditorGUILayout.PropertyField(m_OnClickProperty);
//            serializedObject.ApplyModifiedProperties();
//        }
//    }
//}