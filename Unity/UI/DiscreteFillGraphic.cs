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
        [Auto.NullCheck]
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
            get { return Mathf.FloorToInt(_fill * count); }
            set { _fill = count > 0 ? (float)Mathf.Clamp(value, 0, count) / count : 0; }
        }

        /// <summary>
        /// Shorthand for setting discrete value.
        /// </summary>
        public void SetValueDiscrete(int v)
        {
            discreteFill = v;
            SetValue(_fill);
        }

        public override void SetValue(float v)
        {
            _fill = Mathf.Clamp01(v);
            bool flip = isFlipped;
            for (int i = flip ? count - 1 : 0, counti = flip ? 0 : count; flip ? i >= 0 : i < counti; i = flip ? i - 1 : i + 1)
            {
                if (discreteImages[i] != null)
                {
                    if (type == Type.Sprite)
                    {
                        bool showFill = flip ? count - discreteFill < i : discreteFill >= i;
                        discreteSpriteRenderers[i].sprite = showFill ? fillSprite : baseSprite;
                        discreteSpriteRenderers[i].color = showFill ? fillColor : baseColor;
                    }
                    else
                    {
                        bool showFill = flip ? count - discreteFill <= i : discreteFill > i;
                        discreteImages[i].sprite = showFill ? fillSprite : baseSprite;
                        discreteImages[i].color = showFill ? fillColor : baseColor;
                    }
                }
            }
        }

    }

}
