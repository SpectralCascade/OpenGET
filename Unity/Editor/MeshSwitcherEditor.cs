using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OpenGET.Editor
{
    [CustomEditor(typeof(MeshSwitcher))]
    public class MeshSwitcherEditor : UnityEditor.Editor
    {


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //EditorGUILayout.DropdownButton();

        }

    }

}
