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
        /// Localised text to show alongside the glyph, if any.
        /// </summary>
        public virtual string text => Localise.Text(_text);

        /// <summary>
        /// Text to show alongside the glyph, if any.
        /// </summary>
        [SerializeField]
        protected string _text = "";

        /// <summary>
        /// TextMeshPro sprite sheet asset to utilise, if any. Default sprite sheet used if not specified.
        /// </summary>
        [SerializeField]
        protected TMPro.TMP_SpriteAsset spriteSheet;

        /// <summary>
        /// Identifier of the glyph to use in the sprite sheet.
        /// Override this in a derivative class for custom behaviour.
        /// </summary>
        public virtual string id => glyphName;

        [Tooltip("Identifier for the glyph to use in the sprite sheet. If empty, no glyph will be shown.")]
        [SerializeField]
        protected string glyphName = "";

        public override string ToString()
        {
            return $"{(string.IsNullOrEmpty(_text) ? "" : "<b>" + text + "</b> ")}" + (!string.IsNullOrEmpty(id) ? "<sprite=\"{spriteSheet.name}\" name=\"{id}\">" : "");
        }

    }

}
