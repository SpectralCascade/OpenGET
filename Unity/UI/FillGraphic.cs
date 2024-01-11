using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    public class FillGraphic : MonoBehaviour
    {

        public enum Type
        {
            Image = 0,
            Sprite
        }

        protected void Awake() {
            Debug.Assert(fillSprite != null);
            Debug.Assert(baseSprite != null);
            isVertical = _verticalFill;
            isInverted = _invertFill;
        }

        public Type type;

        /// <summary>
        /// Reference to the fader implementation.
        /// </summary>
        /// TODO: Upgrade to 2019.3, add [SerializeReference] attribute,
        /// then instead of storing specific image/sprite stuff in this class
        /// we can store it in the ImageFill itself.
        public IPercentValue implementation {
            get {
                if (impl == null)
                {
                    switch (type)
                    {
                        case Type.Image:
                            Debug.Assert(image != null);
                            impl = new ImageFill(this);
                            ((ImageFill)impl).UpdateMaterial();
                            break;
                        case Type.Sprite:
                            Debug.Assert(target != null);
                            impl = new SpriteFill(this);
                            break;
                    }
                }
                return impl;
            }
            set {
                impl = value;
            }
        }
        private IPercentValue impl = null;

        /// <summary>
        /// TODO: see IPercentValue implementation TODO.
        /// Target UI image.
        /// </summary>
        public Image image;

        /// <summary>
        /// Target sprite renderer.
        /// </summary>
        public SpriteRenderer target;

        /// <summary>
        /// The "full", filled sprite.
        /// </summary>
        public Sprite fillSprite;

        /// <summary>
        /// Colour tint applied to the filled sprite.
        /// </summary>
        public Color fillColor = Color.white;

        /// <summary>
        /// The "empty", unfilled sprite.
        /// </summary>
        public Sprite baseSprite;

        /// <summary>
        /// Colour tint applie to the unfilled base sprite.
        /// </summary>
        public Color baseColor = Color.white;

        /// <summary>
        /// TODO: grayscale fill
        /// </summary>
        public bool grayscale;

        [SerializeField]
        [HideInInspector]
        private bool _invertFill;

        [SerializeField]
        [HideInInspector]
        private bool _verticalFill = true;

        public bool isVertical {
            get { return _verticalFill; }
            set { 
                _verticalFill = value;
                if (image != null && image.material != null) {
                    if (value) {
                        image.material.DisableKeyword("VERTICAL_FILL_OFF");
                    } else {
                        image.material.EnableKeyword("VERTICAL_FILL_OFF");
                    }
                } else if (target != null && target.sharedMaterial != null) {
                    if (value) {
                        target.sharedMaterial.DisableKeyword("VERTICAL_FILL_OFF");
                    } else {
                        target.sharedMaterial.EnableKeyword("VERTICAL_FILL_OFF");
                    }
                }
            }
        }

        public bool isInverted {
            get { return _invertFill; }
            set {
                _invertFill = value;
                if (image != null && image.material != null) {
                    if (value) {
                        image.material.EnableKeyword("INVERT_FILL_ON");
                    } else {
                        image.material.DisableKeyword("INVERT_FILL_ON");
                    }
                } else if (target != null && target.sharedMaterial != null) {
                    if (value) {
                        target.sharedMaterial.EnableKeyword("INVERT_FILL_ON");
                    } else {
                        target.sharedMaterial.DisableKeyword("INVERT_FILL_ON");
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

    [Serializable]
    public class ImageFill : IPercentValue
    {

        public FillGraphic parentFill;

        [SerializeField]
        [HideInInspector]
        private float fill = 0;

        public Material material {
            get {
                if (parentFill.image != null && parentFill.image.material == null) {
                    parentFill.image.material = new Material(Shader.Find("OpenGET/FillImage"));
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
                    material.SetTexture("_MainTex", parentFill.baseSprite.texture);
                    material.SetColor("_BaseColor", parentFill.baseColor);
                }
                if (parentFill.fillSprite != null) {
                    material.SetTexture("_FillTex", parentFill.fillSprite.texture);
                    material.SetColor("_FillColor", parentFill.fillColor);
                }
            }
        }

        public float GetValue() {
            return fill;
        }

        public void SetValue(float v) {
            fill = Mathf.Clamp01(v);
            material.SetFloat(fillProperty, fill);
        }
    }

    [Serializable]
    public class SpriteFill : IPercentValue
    {
        public FillGraphic parentFill;

        [SerializeField]
        [HideInInspector]
        private float fill = 0;

        public Material material {
            get {
                if (parentFill.target != null && parentFill.target.sharedMaterial == null)
                {
                    parentFill.target.sharedMaterial = new Material(Shader.Find("OpenGET/FillImage"));
                }
                return parentFill.target?.sharedMaterial;
            }
            private set {
                if (parentFill.target != null)
                {
                    parentFill.target.sharedMaterial = value;
                }
            }
        }

        public SpriteFill(FillGraphic parentFill)
        {
            this.parentFill = parentFill;
        }

        public readonly static int fillProperty = Shader.PropertyToID("_FillAmount");

        public void UpdateMaterial()
        {
            if (parentFill.target != null)
            {
                if (parentFill.target.sprite == null)
                {
                    parentFill.target.sprite = parentFill.fillSprite;
                }
                material = material;
                if (parentFill.baseSprite != null)
                {
                    material.SetTexture("_MainTex", parentFill.baseSprite.texture);
                    material.SetColor("_BaseColor", parentFill.baseColor);
                }
                if (parentFill.fillSprite != null)
                {
                    material.SetTexture("_FillTex", parentFill.fillSprite.texture);
                    material.SetColor("_FillColor", parentFill.fillColor);
                }
            }
        }

        public float GetValue()
        {
            return fill;
        }

        public void SetValue(float v)
        {
            fill = Mathf.Clamp01(v);

            // TODO: Support linear fill with 9-slice sprites

            // Correct fill to account for non-linear 9-slice UVs
            float correctedFill = fill;
            if (false) {//parentFill.target != null && parentFill.target.drawMode == SpriteDrawMode.Sliced && parentFill.target.sprite != null) {
                // Get 9-slice border and size of the rendered sprite in pixels
                Vector4 border = parentFill.baseSprite.border;
                
                // Rendered sprite size in pixels
                float renderSize;

                // Texture size in pixels
                float textureSize;
                
                // Border size in pixels
                float borderSize;

                if (parentFill.isVertical)
                {
                    borderSize = Mathf.Min(border.y, border.w);
                    renderSize = parentFill.target.size.y;
                    textureSize = parentFill.target.sprite.texture.height;
                }
                else
                {
                    borderSize = Mathf.Min(border.x, border.z);
                    renderSize = parentFill.target.size.x;
                    textureSize = parentFill.target.sprite.texture.width;
                }
                renderSize *= parentFill.target.sprite.pixelsPerUnit;

                // Compute percentages for border and centre sizes
                float renderBorderPercentage = borderSize / renderSize;
                float textureBorderPercentage = borderSize / textureSize;
                float renderCentrePercentage = (renderSize - borderSize * 2f) / renderSize;
                float textureCentrePercentage = (textureSize - borderSize * 2f) / textureSize;
                float textureCentreBorderPercentage = textureCentrePercentage + textureBorderPercentage;

                float mappedSize = renderSize / textureSize;
                float texels = MathUtils.MapRange(fill, 0, 1, 0, textureSize);

                //float normWidth = 1.0f - (threshold * 2f);

                if (fill < textureBorderPercentage)
                {
                    correctedFill = MathUtils.MapRange(fill, 0, renderBorderPercentage, 0, textureBorderPercentage);

                    if (correctedFill >= textureBorderPercentage)
                    {
                        correctedFill += MathUtils.MapRange(
                            fill - textureBorderPercentage,
                            0, renderCentrePercentage,
                            0, textureCentrePercentage
                        );
                    }
                }
                else if (fill < textureCentreBorderPercentage)
                {
                }
                else
                {
                }
                
            }

            material.SetFloat(fillProperty, correctedFill);
        }
    }

}
