using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    public class FillGraphic : MonoBehaviour
    {

        public enum FillType
        {
            UI_Image = 0
        }

        protected void Awake() {
            // TODO: replace with single assert (implementation != null) once we upgrade to 2019.3
            switch (fillType) {
                case FillType.UI_Image:
                    Debug.Assert(image != null);
                    Debug.Assert(fillSprite != null);
                    Debug.Assert(baseSprite != null);
                    implementation = new ImageFill(this);
                    ((ImageFill)implementation).UpdateMaterial();
                    verticalFill = _verticalFill;
                    invertFill = _invertFill;
                    break;
            }
        }

        public FillType fillType;

        /// <summary>
        /// Reference to the fader implementation.
        /// </summary>
        /// TODO: Upgrade to 2019.3, add [SerializeReference] attribute,
        /// then instead of storing specific image/sprite stuff in this class
        /// we can store it in the ImageFill itself.
        public IPercentValue implementation;

        /// <summary>
        /// TODO: see IPercentValue implementation TODO.
        /// </summary>
        public Image image;
        public Sprite fillSprite;
        public Sprite baseSprite;
        /// <summary>
        /// TODO: grayscale fill
        /// </summary>
        public bool grayscale;
        private bool _invertFill;
        private bool _verticalFill = true;

        public bool verticalFill {
            get { return _verticalFill; }
            set { 
                _verticalFill = value;
                if (image != null && image.material != null) {
                    if (value) {
                        image.material.DisableKeyword("VERTICAL_FILL_OFF");
                    } else {
                        image.material.EnableKeyword("VERTICAL_FILL_OFF");
                    }
                }
            }
        }

        public bool invertFill {
            get { return _invertFill; }
            set {
                _invertFill = value;
                if (image != null && image.material != null) {
                    if (value) {
                        image.material.EnableKeyword("INVERT_FILL_ON");
                    } else {
                        image.material.DisableKeyword("INVERT_FILL_ON");
                    }
                }
            }
        }

        /// <summary>
        /// Get or set the fill value of the ProgressFill implementation.
        /// </summary>
        public float fill { 
            get { return implementation.GetValue(); }
            set { implementation.SetValue(value); }
        }

    }

    // TODO: maybe replace image with Graphic so the code works for sprites automagically...
    [Serializable]
    public class ImageFill : IPercentValue
    {

        /// <summary>
        /// Workaround until we upgrade to 2019.3 for [SerializeReference]
        /// attribute so we can store image and sprite references within this class.
        /// </summary>
        public FillGraphic parentFill;

        public Material material {
            get {
                if (parentFill.image != null && parentFill.image.material == null) {
                    parentFill.image.material = new Material(Shader.Find("TSDoors/FillImage"));
                }
                return parentFill.image?.material;
            }
            private set {
                if (parentFill.image != null) {
                    parentFill.image.material = value;
                }
            } 
        }

        public readonly static int fillProperty = Shader.PropertyToID("_FillAmount");

        public ImageFill(FillGraphic parentFill) {
            this.parentFill = parentFill;
        }

        public void UpdateMaterial() {
            if (parentFill.image != null) {
                if (parentFill.image.sprite == null) {
                    parentFill.image.sprite = parentFill.fillSprite;
                }
                material = material;
                if (parentFill.baseSprite != null) {
                    material.SetTexture("_BaseTex", parentFill.baseSprite.texture);
                }
                if (parentFill.fillSprite != null) {
                    material.SetTexture("_FillTex", parentFill.fillSprite.texture);
                }
            }
        }

        private float fill = 0;

        public float GetValue() {
            return fill;
        }

        public void SetValue(float v) {
            fill = Mathf.Clamp01(v);
            material.SetFloat(fillProperty, fill);
        }
    }

}
