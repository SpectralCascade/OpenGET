using OpenGET.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OpenGET.UI {

    /// <summary>
    /// Attach to TMPro text to set a specified input prompt.
    /// Only use this for empty text components. You can call InputHelper.Player.GetActionPrompt() for dynamic text.
    /// </summary>
    public class InputPrompt : AutoTextFormat
    {

        /// <summary>
        /// Associated input action (rebindable reference).
        /// </summary>
        [Auto.NullCheck]
        public InputActionReference action;

        /// <summary>
        /// Optional separate image to use for the glyph, instead of inline text.
        /// </summary>
        public Image icon;

        /// <summary>
        /// Optional background image to use in certain circumstances (e.g. background image for keyboard keys).
        /// </summary>
        public Image background;

        /// <summary>
        /// Whether to append the input prompt before or after any pre-existing text.
        /// </summary>
        public bool appendBefore = false;

        /// <summary>
        /// Display options for the prompt.
        /// </summary>
        public InputHelper.Player.InputPromptMode display = InputHelper.Player.InputPromptMode.Text | InputHelper.Player.InputPromptMode.Sprite;

        /// <summary>
        /// Delimiter character(s) between the prompt and format text.
        /// </summary>
        public string delimiter = " ";

        /// <summary>
        /// Automatically append or prepend the input prompt.
        /// </summary>
        public override string OnTextAutoFormat(string text)
        {
            string prompt = InputHelper.Get(0).GetActionPrompt(
                action.action,
                out Sprite glyph,
                out string deviceLayoutName,
                out string controlPath
            );
            
            if (glyph != null && icon != null)
            {
                icon.sprite = glyph;
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
    }

}
