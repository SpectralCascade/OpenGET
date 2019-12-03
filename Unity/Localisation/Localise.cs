using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Contains information about a specific language.
    /// </summary>
    public class Language
    {
        public Language(string englishName, string localisedName, SystemLanguage systemIdent) {
            this.englishName = englishName;
            this.localisedName = localisedName;
            this.systemIdent = systemIdent;
        }

        /// <summary>
        /// The english name of the language, e.g. "Spanish"
        /// </summary>
        public readonly string englishName;

        /// <summary>
        /// The name of the language within the language, e.g. "Español".
        /// </summary>
        public readonly string localisedName;

        /// <summary>
        /// The corresponding language identifier of the operating system.
        /// </summary>
        public readonly SystemLanguage systemIdent;

        /// TODO: add support for language-specific formatting (e.g. left and right quotation marks).
    }

    /// <summary>
    /// Singleton that can translate text to a desired language.
    /// </summary>
    public class Localise : MonoBehaviour {

        /// <summary>
        /// All available languages.
        /// </summary>
        private static Language[] languages = {
            new Language("English", "English", SystemLanguage.English),
            new Language("Spanish", "Español", SystemLanguage.Spanish)
        };
        
        /// <summary>
        /// The index of the language we are translating to.
        /// </summary>
        private static uint targetLanguage = 0;

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static Localise shared;

        public void Start() {
            if (shared != null) {
                // TODO: Log warning
            }
            shared = this;
        }

        public static Localise sharedInstance {
            get {
                return shared;
            }
            private set { }
        }

        /// <summary>
        /// Switches the target language to a different language and updates all TranslatedText components.
        /// Very slow. Don't change language every frame... :P
        /// </summary>
        /// <param name="target"></param>
        public static void SwitchLanguage(uint target) {
            if (Application.isPlaying && targetLanguage != target && target < languages.Length) {
                /// Set the current language to the target language.
                targetLanguage = target;

                /// Find all TranslatedText components and update their text.
                TranslatedText[] allText = Resources.FindObjectsOfTypeAll<TranslatedText>();
                foreach (TranslatedText text in allText) {
                    text.Retranslate();
                }
            }
        }
        
        /// <summary>
        /// Returns a localised version of the string for the current language.
        /// </summary>
        /// <param name="original"></param>
        public static string Text(string original) {
            if (targetLanguage == 0) {
                return original;
            }
            /// TODO: map to desired language.
            return original;
        }

        /// <summary>
        /// Returns the currency formatting related to the desired language, e.g. "$5" -> "5$"
        /// </summary>
        /// <param name="original"></param>
        public static string Currency(float amount) {
            /// TODO: map to desired language.
            return string.Format("£{0}", amount);
        }

        public static string Date(int day, int month, int year) {
            /// TODO: map to desired language.
            return string.Format("{0}/{1}/{2}", day, month, year);
        }

        /// TODO: add more methods for formatting stuff like quotes etc.

    }

}
