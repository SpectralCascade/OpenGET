using OpenGET.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
        /// Whether to wrap the prompt in [] brackets or not.
        /// </summary>
        public bool wrapBrackets = false;

        /// <summary>
        /// Automatically append or prepend the input prompt.
        /// </summary>
        public override string OnTextAutoFormat(string text)
        {
            string prompt = (wrapBrackets ? "[" : "") + InputHelper.Get(0).GetActionPrompt(action.action) + (wrapBrackets ? "]" : "");
            return (appendBefore ? prompt + delimiter : "") + text + (appendBefore ? "" : delimiter + prompt);
        }
    }

}
