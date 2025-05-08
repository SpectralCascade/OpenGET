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
            get { return Mathf.FloorToInt(_fill * count); }
            set { _fill = count > 0 ? (float)Mathf.Clamp(value, 0, count) / count : 0; }
        }

        public override void SetValue(float v)
        {
            _fill = Mathf.Clamp01(v);
            bool flip = isFlipped;
            int fillValue = 0;
            for (
                int i = flip ? count - 1 : 0, counti = flip ? 0 : count;
                flip ? i >= 0 : i < counti;
                i = flip ? i - 1 : i + 1
            )
            
            {
                if (discreteImages[i] != null)
                {
                    if (type == Type.Sprite)
                    {
                        discreteSpriteRenderers[i].sprite = discreteFill >= i ? fillSprite : baseSprite;
                        discreteSpriteRenderers[i].color = discreteFill >= i ? fillColor : baseColor;
                    }
                    else
                    {
                        discreteImages[i].sprite = discreteFill > fillValue ? fillSprite : baseSprite;
                        discreteImages[i].color = discreteFill > fillValue ? fillColor : baseColor;
                    }
                }
            }
        }

    }

}
