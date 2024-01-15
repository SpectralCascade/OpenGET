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

        public override void OnInspectorGUI() {            
            DiscreteFillGraphic fill = (DiscreteFillGraphic)target;

            DiscreteFillGraphic.Type oldFillType = fill.type;
            fill.type = (DiscreteFillGraphic.Type)EditorGUILayout.EnumPopup("Fill Type:", fill.type);
            bool didChange = fill.type != oldFillType;
            if (didChange) {
                fill.implementation = null;
            }

            didChange |= fill.isFlipped;
            fill.isFlipped = EditorGUILayout.Toggle("Flipped:", fill.isFlipped);
            didChange |= didChange != fill.isFlipped;

            if (fill.type == DiscreteFillGraphic.Type.Image) {
                /// DiscreteImagesFill editor
                DiscreteFillGraphic.DiscreteImagesFill discreteFillImages = fill.implementation as DiscreteFillGraphic.DiscreteImagesFill;
                if (discreteFillImages == null) {
                    discreteFillImages = new DiscreteFillGraphic.DiscreteImagesFill(fill);
                    fill.implementation = discreteFillImages;
                }

                // Unity doesn't provide the full inspector GUI API so work around with a serialised property instead
                // for arrays.
                SerializedObject obj = new SerializedObject(fill);
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
                        float oldValue = fill.implementation.GetValue();
                        fill.implementation.SetValue(EditorGUILayout.Slider("Fill Value:", fill.implementation.GetValue(), 0.0f, 1.0f));
                        if (fill.implementation.GetValue() != oldValue || isDirty) {
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
                discreteFillImages.SetValue(discreteFillImages.GetValue());

            }
            else if (fill.type == DiscreteFillGraphic.Type.Sprite)
            {
                /// DiscreteImagesFill editor
                DiscreteFillGraphic.DiscreteSpritesFill discreteFillSprites = fill.implementation as DiscreteFillGraphic.DiscreteSpritesFill;
                if (discreteFillSprites == null)
                {
                    discreteFillSprites = new DiscreteFillGraphic.DiscreteSpritesFill(fill);
                    fill.implementation = discreteFillSprites;
                }

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

                if (fill.discreteImages != null)
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
                        float oldValue = fill.implementation.GetValue();
                        fill.implementation.SetValue(EditorGUILayout.Slider("Fill Value:", fill.implementation.GetValue(), 0.0f, 1.0f));
                        if (fill.implementation.GetValue() != oldValue || isDirty)
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
                discreteFillSprites.SetValue(discreteFillSprites.GetValue());

            }

            if (didChange)
            {
                EditorUtility.SetDirty(fill);
            }

        }

    }

}
