using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenGET.UI;
using UnityEditor;
using UnityEngine.UI;

namespace OpenGET.Editor.UI
{

    [CustomEditor(typeof(DiscreteFillGraphic))]
    public class DiscreteFillGraphicEditor : UnityEditor.Editor
    {

        SerializedObject obj = null;

        public void OnEnable()
        {
            DiscreteFillGraphic fill = (DiscreteFillGraphic)target;

            obj = new SerializedObject(fill);
        }

        public override void OnInspectorGUI() {
            DiscreteFillGraphic fill = (DiscreteFillGraphic)target;

            if (obj == null || fill == null)
            {
                return;
            }

            DiscreteFillGraphic.Type oldFillType = fill.type;
            fill.type = (DiscreteFillGraphic.Type)EditorGUILayout.EnumPopup("Fill Type:", fill.type);
            bool didChange = fill.type != oldFillType;

            didChange |= fill.isFlipped;
            fill.isFlipped = EditorGUILayout.Toggle("Flipped:", fill.isFlipped);
            didChange |= didChange != fill.isFlipped;

            if (fill.type == DiscreteFillGraphic.Type.Image) {
                // Unity doesn't provide the full inspector GUI API so work around with a serialised property instead
                // for arrays.
                SerializedProperty propArray = obj.FindProperty("discreteImages");
                if (propArray != null)
                {
                    EditorGUILayout.PropertyField(propArray, includeChildren: true);
                    obj.ApplyModifiedProperties();
                }
                else
                {
                    Log.Error("Failed to find and draw property \"discreteImages\".");
                }

                if (fill.discreteImages != null) {
                    Sprite oldSprite = fill.fillSprite;
                    fill.fillSprite = (Sprite)EditorGUILayout.ObjectField("Fill Sprite:", fill.fillSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.fillSprite != oldSprite && fill.fillSprite != null) {
                        EditorUtility.SetDirty(fill);
                    }

                    bool isDirty = false;
                    Color oldColor = fill.fillColor;
                    fill.fillColor = EditorGUILayout.ColorField("Fill Color:", fill.fillColor);
                    if (oldColor != fill.fillColor)
                    {
                        isDirty = true;
                    }

                    oldSprite = fill.baseSprite;
                    fill.baseSprite = (Sprite)EditorGUILayout.ObjectField("Base Sprite:", fill.baseSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.baseSprite != oldSprite) {
                        EditorUtility.SetDirty(fill);
                    }

                    oldColor = fill.fillColor;
                    fill.baseColor = EditorGUILayout.ColorField("Base Color:", fill.baseColor);
                    if (oldColor != fill.fillColor)
                    {
                        isDirty = true;
                    }

                    if (fill.baseSprite != null && fill.fillSprite != null) {
                        int discreteValue = fill.discreteFill;
                        fill.discreteFill = EditorGUILayout.IntField("Discrete Fill Value", fill.discreteFill);
                        isDirty |= fill.discreteFill != discreteValue;
                        
                        // Show fill slider
                        float oldValue = fill.GetValue();
                        fill.SetValue(EditorGUILayout.Slider("Fill Value:", fill.GetValue(), 0.0f, 1.0f));
                        if (fill.GetValue() != oldValue || isDirty) {
                            EditorUtility.SetDirty(fill);
                        }
                    } else {
                        if (isDirty)
                        {
                            EditorUtility.SetDirty(fill);
                        }
                        EditorGUILayout.HelpBox("You must specify a base sprite and filled sprite for the fill image to work.", MessageType.Warning);
                    }

                } else {
                    EditorGUILayout.HelpBox("You must specify a target image for this fill type.", MessageType.Warning);
                }

                // Force update
                fill.SetValue(fill.GetValue());

            }
            else if (fill.type == DiscreteFillGraphic.Type.Sprite)
            {
                // Unity doesn't provide the full inspector GUI API so work around with a serialised property instead
                // for arrays.
                SerializedObject obj = new SerializedObject(fill);
                SerializedProperty propArray = obj.FindProperty("discreteSpriteRenderers");
                if (propArray != null)
                {
                    EditorGUILayout.PropertyField(propArray, includeChildren: true);
                    obj.ApplyModifiedProperties();
                }
                else
                {
                    Log.Error("Failed to find and draw property \"discreteSpriteRenderers\".");
                }

                if (fill.discreteSpriteRenderers != null)
                {
                    Sprite oldSprite = fill.fillSprite;
                    fill.fillSprite = (Sprite)EditorGUILayout.ObjectField("Fill Sprite:", fill.fillSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.fillSprite != oldSprite && fill.fillSprite != null)
                    {
                        EditorUtility.SetDirty(fill);
                    }

                    bool isDirty = false;
                    Color oldColor = fill.fillColor;
                    fill.fillColor = EditorGUILayout.ColorField("Fill Color:", fill.fillColor);
                    if (oldColor != fill.fillColor)
                    {
                        isDirty = true;
                    }

                    oldSprite = fill.baseSprite;
                    fill.baseSprite = (Sprite)EditorGUILayout.ObjectField("Base Sprite:", fill.baseSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.baseSprite != oldSprite)
                    {
                        EditorUtility.SetDirty(fill);
                    }

                    oldColor = fill.fillColor;
                    fill.baseColor = EditorGUILayout.ColorField("Base Color:", fill.baseColor);
                    if (oldColor != fill.fillColor)
                    {
                        isDirty = true;
                    }

                    if (fill.baseSprite != null && fill.fillSprite != null)
                    {
                        int discreteValue = fill.discreteFill;
                        fill.discreteFill = EditorGUILayout.IntField("Discrete Fill Value", fill.discreteFill);
                        isDirty |= fill.discreteFill != discreteValue;

                        // Show fill slider
                        float oldValue = fill.GetValue();
                        fill.SetValue(EditorGUILayout.Slider("Fill Value:", fill.GetValue(), 0.0f, 1.0f));
                        if (fill.GetValue() != oldValue || isDirty)
                        {
                            EditorUtility.SetDirty(fill);
                        }
                    }
                    else
                    {
                        if (isDirty)
                        {
                            EditorUtility.SetDirty(fill);
                        }
                        EditorGUILayout.HelpBox("You must specify a base sprite and filled sprite for the fill image to work.", MessageType.Warning);
                    }

                }
                else
                {
                    EditorGUILayout.HelpBox("You must specify a target image for this fill type.", MessageType.Warning);
                }

                // Force update
                fill.SetValue(fill.GetValue());

            }

            if (didChange)
            {
                EditorUtility.SetDirty(fill);
            }

        }

    }

}
