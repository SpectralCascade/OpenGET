using OpenGET.Input;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OpenGET.UI {

    /// <summary>
    /// Attach to TMPro text to set a specified input prompt.
    /// Only use this for empty text components or icon images. You can call InputHelper.Player.GetActionPrompt() for dynamic text.
    /// In order for this to work with text, you MUST also add a TextFormatter component.
    /// </summary>
    public class InputPrompt : AutoTextFormat
    {

        /// <summary>
        /// Associated input action (rebindable reference).
        /// </summary>
        [Auto.NullCheck]
        public InputActionReference action;

        /// <summary>
        /// Associated text formatter, if any.
        /// </summary>
        [Auto.Hookup(Auto.Mode.Self)]
        [SerializeField]
        protected TextFormatter formatter;

        /// <summary>
        /// Optional separate image to use for the glyph, instead of inline text.
        /// </summary>
        public Image icon;

        /// <summary>
        /// Optional background image to use in certain circumstances (e.g. background image for keyboard keys).
        /// </summary>
        public Image background;

        [Tooltip("Append the prompt glyph before the text instead of after.")]
        public bool appendBefore = false;

        [Tooltip("Tint the prompt to match the text colour.")]
        public bool tint = false;

        /// <summary>
        /// Delimiter character(s) between the prompt and format text.
        /// </summary>
        public string delimiter = " ";

        /// <summary>
        /// Temporary Sprite created from glyph info.
        /// </summary>
        private Sprite iconSprite = null;

        /// <summary>
        /// Creates an icon sprite for a given glyph.
        /// </summary>
        private Sprite CreateIconSprite(TMP_SpriteGlyph glyph, TMP_SpriteAsset asset)
        {
            // Opt: Consider caching the glyph so we don't have to always destroy the sprite if it is reused.
            if (iconSprite != null)
            {
                Destroy(iconSprite);
                iconSprite = null;
            }

            if (asset.spriteSheet is Texture2D)
            {
                // Manual sprite creation
                Rect rect = new Rect(
                    glyph.glyphRect.x, glyph.glyphRect.y, glyph.glyphRect.width, glyph.glyphRect.height
                );
                iconSprite = Sprite.Create(asset.spriteSheet as Texture2D, rect, new Vector2(0.5f, 0.5f));
            }
            return iconSprite;
        }

        protected void OnEnable()
        {
            // Text formatter normally does this for us; but for icon-only InputPrompts, this is necessary.
            if (formatter == null && icon != null)
            {
                InputHelper.Get(0).GetActionPrompt(
                    action.action,
                    out TMP_SpriteAsset asset,
                    out TMP_SpriteGlyph glyph,
                    out string deviceLayoutName,
                    out string controlPath,
                    tint: tint
                );

                if (glyph != null)
                {
                    icon.sprite = CreateIconSprite(glyph, asset);
                }
                icon.enabled = glyph != null;
            }
        }

        /// <summary>
        /// Automatically append or prepend the input prompt.
        /// </summary>
        public override string OnTextAutoFormat(string text)
        {
            string prompt = InputHelper.Get(0).GetActionPrompt(
                action.action,
                out TMP_SpriteAsset asset,
                out TMP_SpriteGlyph glyph,
                out string deviceLayoutName,
                out string controlPath,
                tint: tint
            );
            
            if (glyph != null && icon != null)
            {
                icon.sprite = CreateIconSprite(glyph, asset);
                icon.enabled = true;
            }
            else if (icon != null)
            {
                icon.enabled = false;
            }
            if (background != null)
            {
                //background.enabled = 
            }
            return (appendBefore ? prompt + delimiter : "") + text + (appendBefore ? "" : delimiter + prompt);
        }

        protected void OnDestroy()
        {
            if (iconSprite != null)
            {
                Destroy(iconSprite);
            }
        }
    }

}
