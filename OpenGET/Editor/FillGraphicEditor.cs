using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenGET.UI;
using UnityEditor;
using UnityEngine.UI;

namespace OpenGET.Editor.UI
{

    [CustomEditor(typeof(FillGraphic))]
    public class FillGraphicEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI() {
            FillGraphic fill = (FillGraphic)target;

            FillGraphic.Type oldFillType = fill.type;
            fill.type = (FillGraphic.Type)EditorGUILayout.EnumPopup("Fill Type:", fill.type);
            bool didChange = fill.type != oldFillType;

            didChange |= fill.isVertical;
            fill.isVertical = EditorGUILayout.Toggle("Vertical Fill:", fill.isVertical);
            didChange = didChange != fill.isVertical;

            didChange |= fill.isFlipped;
            fill.isFlipped = EditorGUILayout.Toggle("Flip Fill:", fill.isFlipped);
            didChange |= didChange != fill.isFlipped;

            Material oldMaterial = fill.material;
            Material loadedMaterial = (Material)EditorGUILayout.ObjectField("Custom Material", fill.material, typeof(Material), allowSceneObjects: false);

            if (oldMaterial != loadedMaterial)
            {
                didChange = true;
                fill.material = loadedMaterial;

                if (loadedMaterial == null)
                {
                    // Auto-generate material
                    fill.material = fill.material;
                }
                else
                {
                    Log.Debug("Set custom material to {0}", loadedMaterial.name);
                }
            }

            if (fill.type == FillGraphic.Type.Image) {
                fill.image = (Image)EditorGUILayout.ObjectField("Target Image:", fill.image, typeof(Image), allowSceneObjects: true);
                if (fill.image != null) {
                    Sprite oldSprite = fill.fillSprite;
                    fill.fillSprite = (Sprite)EditorGUILayout.ObjectField("Fill Sprite:", fill.fillSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.fillSprite != oldSprite && fill.fillSprite != null && fill.material != null) {
                        fill.material.SetTexture("_FillTex", fill.fillSprite.texture);
                        EditorUtility.SetDirty(fill);
                    }

                    bool isDirty = false;
                    Color oldColor = fill.fillColor;
                    fill.fillColor = EditorGUILayout.ColorField("Fill Color:", fill.fillColor);
                    if (oldColor != fill.fillColor)
                    {
                        fill.material.SetColor("_FillColor", fill.fillColor);
                        isDirty = true;
                    }

                    if (fill.baseSprite != null && fill.image.sprite != null && fill.material != null) {
                        // Default to whatever the image is using.
                        fill.material.SetTexture("_MainTex", fill.baseSprite.texture); 
                    }

                    oldSprite = fill.baseSprite;
                    fill.baseSprite = (Sprite)EditorGUILayout.ObjectField("Base Sprite:", fill.baseSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.baseSprite != oldSprite && fill.material != null) {
                        fill.material.SetTexture("_MainTex", fill.baseSprite != null ? fill.baseSprite.texture : null);
                    }

                    oldColor = fill.fillColor;
                    fill.baseColor = EditorGUILayout.ColorField("Base Color:", fill.baseColor);
                    if (oldColor != fill.fillColor)
                    {
                        fill.material.SetColor("_BaseColor", fill.baseColor);
                        isDirty = true;
                    }

                    if (fill.baseSprite != null && fill.fillSprite != null) {
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

                fill.UpdateMaterial();
            }
            else if (fill.type == FillGraphic.Type.Sprite)
            {
                fill.target = (SpriteRenderer)EditorGUILayout.ObjectField("Target Sprite:", fill.target, typeof(SpriteRenderer), allowSceneObjects: true);
                if (fill.target != null)
                {
                    bool isDirty = false;
                    Sprite oldSprite = fill.fillSprite;
                    fill.fillSprite = (Sprite)EditorGUILayout.ObjectField("Fill Sprite:", fill.fillSprite, typeof(Sprite), allowSceneObjects: false);
                    isDirty |= fill.fillSprite != oldSprite && fill.fillSprite != null;

                    Color oldColor = fill.fillColor;
                    fill.fillColor = EditorGUILayout.ColorField("Fill Color:", fill.fillColor);
                    isDirty |= oldColor != fill.fillColor;

                    oldSprite = fill.baseSprite;
                    fill.baseSprite = (Sprite)EditorGUILayout.ObjectField("Base Sprite:", fill.baseSprite, typeof(Sprite), allowSceneObjects: false);
                    isDirty |= fill.baseSprite != oldSprite;

                    oldColor = fill.fillColor;
                    fill.baseColor = EditorGUILayout.ColorField("Base Color:", fill.baseColor);
                    isDirty |= oldColor != fill.fillColor;

                    if (fill.baseSprite != null && fill.fillSprite != null)
                    {
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
                        EditorGUILayout.HelpBox("You must specify a base sprite and filled sprite for the fill sprite to work.", MessageType.Warning);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("You must specify a target sprite renderer for this fill type.", MessageType.Warning);
                }

                fill.UpdateMaterial();
            }

            if (didChange)
            {
                EditorUtility.SetDirty(fill);
            }

        }

    }

}
