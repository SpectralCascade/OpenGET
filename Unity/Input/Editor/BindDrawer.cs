using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OpenGET.Input {

    [CustomPropertyDrawer(typeof(Bind))]
    public class BindDrawer : PropertyDrawer
    {
        private int lineCount = 0;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * lineCount 
                + EditorGUIUtility.standardVerticalSpacing * lineCount;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

            lineCount = 0;
            Rect NextRect()
            {
                return new Rect(
                    position.min.x,
                    position.min.y + ((++lineCount) * EditorGUIUtility.singleLineHeight),
                    position.size.x,
                    EditorGUIUtility.singleLineHeight
                );
            }

            if (property.isExpanded) {

                SerializedProperty id = property.FindPropertyRelative("id");
                SerializedProperty source = property.FindPropertyRelative("source");
                SerializedProperty type = property.FindPropertyRelative("type");

                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(NextRect(), source);
                EditorGUI.PropertyField(NextRect(), type);

                Rect dropdown = NextRect();


                bool isButton = ((Bind.Type)typeof(Bind.Type).GetEnumValues().GetValue(type.enumValueIndex)) == Bind.Type.Button;
                switch ((Bind.Controller)typeof(Bind.Controller).GetEnumValues().GetValue(source.enumValueIndex))
                {
                    case Bind.Controller.Gamepad:
                        if (isButton)
                        {
                            id.intValue = Mathf.Min((int)(Bind.GamepadButton)EditorGUI.EnumPopup(
                                dropdown, "Gamepad Button", (Bind.GamepadButton)id.intValue
                            ), typeof(Bind.GamepadButton).GetEnumValues().Length - 1);
                        }
                        else
                        {
                            id.intValue = Mathf.Min((int)(Bind.GamepadAxis)EditorGUI.EnumPopup(
                                dropdown, "Gamepad Axis", (Bind.GamepadAxis)id.intValue
                            ), typeof(Bind.GamepadAxis).GetEnumValues().Length - 1);
                        }
                        break;
                    case Bind.Controller.Keyboard:
                        if (isButton)
                        {
                            id.intValue = Mathf.Min((int)(KeyCode)EditorGUI.EnumPopup(
                                dropdown, "KeyCode", (KeyCode)id.intValue
                            ), typeof(KeyCode).GetEnumValues().Length - 1);
                        }
                        else
                        {
                            id.intValue = 0;
                            EditorGUI.HelpBox(dropdown, "Keyboards have no axis bindings.", MessageType.Warning);
                        }
                        break;
                    case Bind.Controller.Mouse:
                        if (isButton)
                        {
                            id.intValue = Mathf.Min((int)(Bind.MouseButton)EditorGUI.EnumPopup(
                                dropdown, "Mouse Button", (Bind.MouseButton)id.intValue
                            ), typeof(Bind.MouseButton).GetEnumValues().Length - 1);
                        }
                        else
                        {
                            id.intValue = Mathf.Min((int)(Bind.MouseAxis)EditorGUI.EnumPopup(
                                dropdown, "Mouse Axis", (Bind.MouseAxis)id.intValue
                            ), typeof(Bind.MouseAxis).GetEnumValues().Length - 1);
                        }
                        break;
                    default:
                    case Bind.Controller.Custom:
                        id.intValue = EditorGUI.IntField(dropdown, "Custom id", id.intValue);
                        break;
                }
            }
            EditorGUI.indentLevel--;

            // Always one line for the property expander
            lineCount++;

            EditorGUI.EndProperty();
        }

    }

}
