using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET
{

    /// <summary>
    /// Improved Text component that automatically attempts to translate the text to a desired language.
    /// </summary>
    public class LangText : Text
    {
        /// <summary>
        /// The text in the root language.
        /// </summary>
        public string rootText { get; private set; }

        /// <summary>
        /// Automatically translates the text already specified.
        /// </summary>
        protected override void Start() {
            Translate(text);
        }

        /// <summary>
        /// Re-translates the text. Called when the target language is changed.
        /// </summary>
        public void Retranslate() {
            Translate(rootText);
        }

        /// <summary>
        /// Translates the given text to the current target language.
        /// </summary>
        /// <param name="targetText"></param>
        private void Translate(string targetText) {
            if (Application.isPlaying) {
                rootText = targetText;
                text = Localiser.Text(targetText);
            }
        }

    }

}
