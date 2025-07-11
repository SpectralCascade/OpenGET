using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// TMPro sprite glyph entry. Override this class for custom glyph entry assets (e.g. input prompts)
    /// </summary>
    [CreateAssetMenu(fileName = "NewGlyphEntry", menuName = "OpenGET/Glyph Entry")]
    public class GlyphEntry : ScriptableObject, IReferrable
    {
        /// <summary>
        /// Raw English text to show alongside the glyph, if any.
        /// </summary>
        public virtual string text => _text;

        /// <summary>
        /// Optionally specify text to show alongside the glyph.
        /// </summary>
        [SerializeField]
        private string _text = "";

        /// <summary>
        /// TextMeshPro sprite sheet asset to utilise.
        /// </summary>
        [SerializeField]
        protected TMPro.TMP_SpriteAsset spriteSheet;

        /// <summary>
        /// Identifier of the sprite to use in the sprite sheet.
        /// Override this in a derivative class for custom behaviour.
        /// </summary>
        public virtual string id => _id;

        /// <summary>
        /// Identifier for the sprite to use in the sprite sheet. If none, assumes the derivative class has overridden id getter.
        /// </summary>
        [SerializeField]
        private string _id = "";

        public override string ToString()
        {
            return $"{(string.IsNullOrEmpty(text) ? "" : text + " ")}<sprite=\"{spriteSheet.name}\" name=\"{id}\">";
        }

    }

}
