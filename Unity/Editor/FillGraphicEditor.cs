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
            if (didChange) {
                fill.implementation = null;
            }

            didChange |= fill.isVertical;
            fill.isVertical = EditorGUILayout.Toggle("Vertical Fill:", fill.isVertical);
            didChange = didChange != fill.isVertical;

            didChange |= fill.isInverted;
            fill.isInverted = EditorGUILayout.Toggle("Invert Fill:", fill.isInverted);
            didChange |= didChange != fill.isInverted;

            if (fill.type == FillGraphic.Type.Image) {
                /// ImageFill editor
                ImageFill fillImage = fill.implementation as ImageFill;
                if (fillImage == null) {
                    fillImage = new ImageFill(fill);
                    fill.implementation = fillImage;
                }

                fill.image = (Image)EditorGUILayout.ObjectField("Target Image:", fill.image, typeof(Image), allowSceneObjects: true);
                if (fill.image != null) {
                    Sprite oldSprite = fill.fillSprite;
                    fill.fillSprite = (Sprite)EditorGUILayout.ObjectField("Fill Sprite:", fill.fillSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.fillSprite != oldSprite && fill.fillSprite != null && fillImage.material != null) {
                        fillImage.material.SetTexture("_FillTex", fill.fillSprite.texture);
                        EditorUtility.SetDirty(fill);
                    }

                    bool isDirty = false;
                    Color oldColor = fill.fillColor;
                    fill.fillColor = EditorGUILayout.ColorField("Fill Color:", fill.fillColor);
                    if (oldColor != fill.fillColor)
                    {
                        fillImage.material.SetColor("_FillColor", fill.fillColor);
                        isDirty = true;
                    }

                    if (fill.baseSprite != null && fill.image.sprite != null && fillImage.material != null) {
                        // Default to whatever the image is using.
                        fillImage.material.SetTexture("_MainTex", fill.baseSprite.texture); 
                    }

                    oldSprite = fill.baseSprite;
                    fill.baseSprite = (Sprite)EditorGUILayout.ObjectField("Base Sprite:", fill.baseSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.baseSprite != oldSprite && fillImage.material != null) {
                        fillImage.material.SetTexture("_MainTex", fill.baseSprite?.texture);
                    }

                    oldColor = fill.fillColor;
                    fill.baseColor = EditorGUILayout.ColorField("Base Color:", fill.baseColor);
                    if (oldColor != fill.fillColor)
                    {
                        fillImage.material.SetColor("_BaseColor", fill.baseColor);
                        isDirty = true;
                    }

                    if (fill.baseSprite != null && fill.fillSprite != null) {
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

                fillImage.UpdateMaterial();

            }
            else if (fill.type == FillGraphic.Type.Sprite)
            {
                /// ImageFill editor
                SpriteFill fillSprite = fill.implementation as SpriteFill;
                if (fillSprite == null)
                {
                    fillSprite = new SpriteFill(fill);
                    fill.implementation = fillSprite;
                }

                fill.target = (SpriteRenderer)EditorGUILayout.ObjectField("Target Sprite:", fill.target, typeof(SpriteRenderer), allowSceneObjects: true);
                if (fill.target != null)
                {
                    Sprite oldSprite = fill.fillSprite;
                    fill.fillSprite = (Sprite)EditorGUILayout.ObjectField("Fill Sprite:", fill.fillSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.fillSprite != oldSprite && fill.fillSprite != null && fillSprite.material != null)
                    {
                        fillSprite.material.SetTexture("_FillTex", fill.fillSprite.texture);
                        EditorUtility.SetDirty(fill);
                    }

                    bool isDirty = false;
                    Color oldColor = fill.fillColor;
                    fill.fillColor = EditorGUILayout.ColorField("Fill Color:", fill.fillColor);
                    if (oldColor != fill.fillColor)
                    {
                        fillSprite.material.SetColor("_FillColor", fill.fillColor);
                        isDirty = true;
                    }

                    if (fill.baseSprite != null && fill.target.sprite != null && fillSprite.material != null)
                    {
                        // Default to whatever the image is using.
                        fillSprite.material.SetTexture("_MainTex", fill.baseSprite.texture);
                    }

                    oldSprite = fill.baseSprite;
                    fill.baseSprite = (Sprite)EditorGUILayout.ObjectField("Base Sprite:", fill.baseSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.baseSprite != oldSprite && fillSprite.material != null)
                    {
                        fillSprite.material.SetTexture("_MainTex", fill.baseSprite?.texture);
                    }

                    oldColor = fill.fillColor;
                    fill.baseColor = EditorGUILayout.ColorField("Base Color:", fill.baseColor);
                    if (oldColor != fill.fillColor)
                    {
                        fillSprite.material.SetColor("_BaseColor", fill.baseColor);
                        isDirty = true;
                    }

                    if (fill.baseSprite != null && fill.fillSprite != null)
                    {
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
                        EditorGUILayout.HelpBox("You must specify a base sprite and filled sprite for the fill sprite to work.", MessageType.Warning);
                    }

                }
                else
                {
                    EditorGUILayout.HelpBox("You must specify a target sprite renderer for this fill type.", MessageType.Warning);
                }

                fillSprite.UpdateMaterial();
            }

            if (didChange)
            {
                EditorUtility.SetDirty(fill);
            }

        }

    }

}
