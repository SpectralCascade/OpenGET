using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpenGET.Editor
{

    [CustomPropertyDrawer(typeof(FolderPathAttribute))]
    public class SystemPathEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool pickerPressed = false;
            FolderPathAttribute folderAttr = attribute as FolderPathAttribute;
            FilePathAttribute fileAttr = attribute as FilePathAttribute;
            if (folderAttr != null || fileAttr != null)
            {
                EditorGUILayout.BeginHorizontal();
                property.stringValue = EditorGUILayout.TextField(label.text, property.stringValue);
                if (GUILayout.Button("..."))
                {
                    pickerPressed = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUI.LabelField(position, label.text, $"Use {attribute.GetType().Name} with string.");
            }

            if (folderAttr != null && pickerPressed)
            {
                property.stringValue = EditorUtility.OpenFolderPanel(folderAttr.title, folderAttr.root, "");
            }
            else if (fileAttr != null && pickerPressed)
            {
                property.stringValue = EditorUtility.OpenFilePanel(fileAttr.title, fileAttr.root, fileAttr.extension);
            }
        }
    }

}
