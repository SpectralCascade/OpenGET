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

            FillGraphic.FillType oldFillType = fill.fillType;
            fill.fillType = (FillGraphic.FillType)EditorGUILayout.EnumPopup("Fill Type:", fill.fillType);
            bool changedType = fill.fillType != oldFillType;
            if (changedType) {
                fill.implementation = null;
            }

            fill.verticalFill = EditorGUILayout.Toggle("Vertical Fill:", fill.verticalFill);
            fill.invertFill = EditorGUILayout.Toggle("Invert Fill:", fill.invertFill);

            if (fill.fillType == FillGraphic.FillType.UI_Image) {
                /// ImageFill editor
                ImageFill fillImage = (ImageFill)fill.implementation;
                if (fillImage == null) {
                    fillImage = new ImageFill((FillGraphic)target);
                    fill.implementation = fillImage;
                }

                fill.image = (Image)EditorGUILayout.ObjectField("Target Image:", fill.image, typeof(Image), allowSceneObjects: true);
                if (fill.image != null) {
                    Sprite oldSprite = fill.fillSprite;
                    fill.fillSprite = (Sprite)EditorGUILayout.ObjectField("Fill Sprite:", fill.fillSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.fillSprite != oldSprite && fill.fillSprite != null) {
                        fillImage.material.SetTexture("_FillTex", fill.fillSprite.texture);
                    }
                    // TODO: grayscale fill support

                    if (fill.baseSprite == null && fill.image.sprite != null) {
                        // Default to whatever the image is using.
                        fillImage.material.SetTexture("_BaseTex", fill.baseSprite.texture);
                    }

                    oldSprite = fill.baseSprite;
                    fill.baseSprite = (Sprite)EditorGUILayout.ObjectField("Base Sprite:", fill.baseSprite, typeof(Sprite), allowSceneObjects: false);
                    if (fill.baseSprite != oldSprite) {
                        fillImage.material.SetTexture("_BaseTex", fill.baseSprite?.texture);
                    }

                    if (fill.baseSprite != null && fill.fillSprite != null) {
                        // Show fill slider
                        fill.implementation.SetValue(EditorGUILayout.Slider("Fill Value:", fill.implementation.GetValue(), 0.0f, 1.0f));
                    } else {
                        EditorGUILayout.HelpBox("You must specify a base sprite and filled sprite for the fill image to work.", MessageType.Warning);
                    }

                } else {
                    EditorGUILayout.HelpBox("You must specify a target image for this fill type.", MessageType.Warning);
                }

                fillImage.UpdateMaterial();

            }

        }

    }

}
