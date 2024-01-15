using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI {

    /// <summary>
    /// A discrete version of FillGraphic, where there are multiple UI images or sprite renderers
    /// which are toggled between filled or unfilled depending on the specified fill level.
    /// </summary>
    public class DiscreteFillGraphic : FillGraphic
    {

        /// <summary>
        /// Discrete set of images to fill.
        /// </summary>
        public Image[] discreteImages = new Image[0];

        /// <summary>
        /// Discrete set of sprites to fill.
        /// </summary>
        public SpriteRenderer[] discreteSpriteRenderers = new SpriteRenderer[0];

        /// <summary>
        /// Unused for discrete fill graphics.
        /// </summary>
        private new bool isVertical => false;

        /// <summary>
        /// Number of discrete images/sprites.
        /// </summary>
        public int count => type == Type.Image ? discreteImages.Length : discreteSpriteRenderers.Length;

        /// <summary>
        /// Number of discrete images/sprites that are currently shown as filled.
        /// </summary>
        public int discreteFill {
            get { return Mathf.FloorToInt(fill * count); }
            set { fill = count > 0 ? (float)Mathf.Clamp(value, 0, count) / count : 0; }
        }

        /// <summary>
        /// Get the fill implementation object.
        /// </summary>
        public override IPercentValue implementation {
            get {
                if (impl == null)
                {
                    switch (type)
                    {
                        case Type.Image:
                            Debug.Assert(discreteImages != null);
                            impl = new DiscreteImagesFill(this);
                            break;
                        case Type.Sprite:
                            Debug.Assert(discreteSpriteRenderers != null);
                            impl = new DiscreteSpritesFill(this);
                            break;
                    }
                }
                return impl;
            }
            set {
                impl = value;
            }
        }

        [System.Serializable]
        public abstract class DiscreteFill : IPercentValue
        {

            public DiscreteFillGraphic parentFill;

            [SerializeField]
            [HideInInspector]
            protected float fill = 0;

            public DiscreteFill(DiscreteFillGraphic parentFill)
            {
                this.parentFill = parentFill;
            }

            public float GetValue()
            {
                return fill;
            }

            public abstract void SetValue(float v);
        }

        public class DiscreteImagesFill : DiscreteFill
        {

            public DiscreteImagesFill(DiscreteFillGraphic parentFill) : base(parentFill) { }

            public override void SetValue(float v)
            {
                fill = Mathf.Clamp01(v);
                int discreteFill = parentFill.discreteFill;
                bool flip = parentFill.isFlipped;
                int fillValue = 0;
                for (
                    int i = flip ? parentFill.count - 1 : 0, counti = flip ? 0 : parentFill.count;
                    flip ? i >= 0 : i < counti;
                    i = flip ? i - 1 : i + 1
                ) {
                    if (parentFill.discreteImages[i] != null)
                    {
                        parentFill.discreteImages[i].sprite = discreteFill > fillValue ? parentFill.fillSprite : parentFill.baseSprite;
                        parentFill.discreteImages[i].color = discreteFill > fillValue ? parentFill.fillColor : parentFill.baseColor;
                    }
                    fillValue++;
                }
            }
        }

        public class DiscreteSpritesFill : DiscreteFill
        {

            public DiscreteSpritesFill(DiscreteFillGraphic parentFill) : base(parentFill) { }

            public override void SetValue(float v)
            {
                fill = Mathf.Clamp01(v);
                int discreteFill = parentFill.discreteFill;
                bool flip = parentFill.isFlipped;
                for (
                    int i = flip ? parentFill.count : 0, counti = flip ? 0 : parentFill.count;
                    flip ? i >= 0 : i < counti;
                    i = flip ? i - 1 : i + 1
                ) {
                    if (parentFill.discreteImages[i] != null)
                    {
                        parentFill.discreteSpriteRenderers[i].sprite = discreteFill >= i ? parentFill.fillSprite : parentFill.baseSprite;
                        parentFill.discreteSpriteRenderers[i].color = discreteFill >= i ? parentFill.fillColor : parentFill.baseColor;
                    }
                }
            }
        }

    }

}
