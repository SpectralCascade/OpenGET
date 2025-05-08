using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// A continuous fill bar used to display a ranged value such as progress.
    /// Can be used with UI images or sprite renderers.
    /// </summary>
    public class FillGraphic : AutoBehaviour, IPercentValue
    {

        /// <summary>
        /// Type of graphic component to be used, e.g. a UI image.
        /// </summary>
        public enum Type
        {
            Image = 0,
            Sprite
        }

        protected override void Awake() {
            base.Awake();
            isVertical = isVertical;
            isFlipped = isFlipped;
        }

        protected void Start()
        {
            UpdateMaterial();
        }

        [SerializeField]
        [HideInInspector]
        public Type type;

        /// <summary>
        /// Target UI image.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public Image image;

        /// <summary>
        /// Target sprite renderer.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public SpriteRenderer target;

        /// <summary>
        /// The "full", filled sprite.
        /// </summary>
        [Auto.NullCheck]
        [SerializeField]
        [HideInInspector]
        public Sprite fillSprite;

        /// <summary>
        /// Colour tint applied to the filled sprite.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public Color fillColor = Color.white;

        /// <summary>
        /// The "empty", unfilled sprite.
        /// </summary>
        [Auto.NullCheck]
        [SerializeField]
        [HideInInspector]
        public Sprite baseSprite;

        /// <summary>
        /// Colour tint applied to the unfilled base sprite.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public Color baseColor = Color.white;

        /// <summary>
        /// TODO: grayscale fill
        /// </summary>
        [SerializeField]
        [HideInInspector]
        public bool grayscale;

        [SerializeField]
        [HideInInspector]
        private bool _flipFill;

        [SerializeField]
        [HideInInspector]
        private bool _verticalFill = true;

        /// <summary>
        /// Should the graphic fill vertically?
        /// </summary>
        public bool isVertical {
            get { return _verticalFill; }
            set {
                _verticalFill = value;
                if (material != null) {
                    if (value) {
                        material.DisableKeyword("VERTICAL_FILL_OFF");
                    } else {
                        material.EnableKeyword("VERTICAL_FILL_OFF");
                    }
                }
            }
        }

        /// <summary>
        /// Should the graphic fill from the opposite direction?
        /// </summary>
        public bool isFlipped {
            get { return _flipFill; }
            set {
                _flipFill = value;
                if (target != null && material != null) {
                    if (value) {
                        material.EnableKeyword("FLIP_FILL_ON");
                    } else {
                        material.DisableKeyword("FLIP_FILL_ON");
                    }
                }
            }
        }

        public static readonly int propFill = Shader.PropertyToID("_FillAmount");
        public static readonly int propTexMain = Shader.PropertyToID("_MainTex");
        public static readonly int propTexFill = Shader.PropertyToID("_FillTex");
        public static readonly int propColourMain = Shader.PropertyToID("_BaseColor");
        public static readonly int propColourFill = Shader.PropertyToID("_FillColor");

        /// <summary>
        /// Material to use for the fill.
        /// </summary>
        public virtual Material material {
            get {
                if (_material == null)
                {
                    Log.Debug("Setup new FillImage material on FillGraphic at \"{0}\"", SceneNavigator.GetPath(gameObject));
                    Shader shader = Shader.Find("OpenGET/FillImage");
                    _material = new Material(shader);
                    if (target != null)
                    {
                        target.sharedMaterial = _material;
                    }
                    if (image != null)
                    {
                        image.material = _material;
                    }
                    UpdateMaterial();
                }
                return _material;
            }
            set {
                if (_material != value)
                {
                    _material = value;
                    UpdateMaterial();
                }
            }
        }
        [SerializeField]
        [HideInInspector]
        protected Material _material;

        /// <summary>
        /// Used for material instancing.
        /// </summary>
        protected MaterialPropertyBlock propertyBlock;

        /// <summary>
        /// Get or set the fill value.
        /// </summary>
        public float fill { get { return GetValue(); } set { SetValue(value); } }

        /// <summary>
        /// Normalised fill value.
        /// </summary>
        [SerializeField]
        [HideInInspector]
        protected float _fill = 0;

        /// <summary>
        /// Get the normalised fill value.
        /// </summary>
        public virtual float GetValue()
        {
            return _fill;
        }

        /// <summary>
        /// Set the fill value (normalised 0 to 1).
        /// </summary>
        public virtual void SetValue(float v)
        {
            float old = _fill;
            _fill = v;
            if (old != v)
            {
                UpdateMaterial();
            }
        }

        public virtual void UpdateMaterial()
        {
            if (image != null)
            {
                image.sprite = fillSprite;
            }
            if (target != null)
            {
                target.sprite = fillSprite;
            }

            material = material;

            if (image != null)
            {
                // CanvasRenderer doesn't support material property blocks
                _material.SetFloat(propFill, _fill);
                if (baseSprite != null && baseSprite.texture != null)
                {
                    _material.SetTexture(propTexMain, baseSprite.texture);
                    _material.SetColor(propColourMain, baseColor);
                }
                if (fillSprite != null && fillSprite.texture != null)
                {
                    _material.SetTexture(propTexFill, fillSprite.texture);
                    _material.SetColor(propColourFill, fillColor);
                }
                Material instance = Material.Instantiate(_material);
                instance.CopyPropertiesFromMaterial(_material);
                image.material = instance;
            }
            else if (target != null)
            {
                if (propertyBlock == null)
                {
                    propertyBlock = new MaterialPropertyBlock();
                }

                if (baseSprite != null && baseSprite.texture != null)
                {
                    propertyBlock.SetTexture(propTexMain, baseSprite.texture);
                    propertyBlock.SetColor(propColourMain, baseColor);
                }
                if (fillSprite != null && fillSprite.texture != null)
                {
                    propertyBlock.SetTexture(propTexFill, fillSprite.texture);
                    propertyBlock.SetColor(propColourFill, fillColor);
                }
                propertyBlock.SetFloat(propFill, _fill);

                if (target.sharedMaterial == null || target.sharedMaterial != _material)
                {
                    target.sharedMaterial = _material;
                }

                target.SetPropertyBlock(propertyBlock);
            }
        }

    }

}
