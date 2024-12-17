using UnityEngine;
using UnityEditor;

namespace OpenGET.Editor
{

    /// <summary>
    /// Read-only property field.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadonlyFieldAttribute))]
    public class InspectorReadonlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

}