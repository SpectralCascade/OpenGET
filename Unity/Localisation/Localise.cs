using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Contains information about a specific language.
    /// </summary>
    public class Language
    {
        /// <summary>
        /// Index of this language in the CSV file.
        /// </summary>
        public readonly int index;

        /// <summary>
        /// The name of the language within the language, e.g. "Español".
        /// </summary>
        public readonly string localisedName;

        /// <summary>
        /// The corresponding language identifier of the operating system.
        /// </summary>
        public readonly string code;

        /// <summary>
        /// Map of localisation keys to translated strings.
        /// </summary>
        private Dictionary<string, string> map = new Dictionary<string, string>();

        public Language(int index, string localisedName, string code)
        {
            this.index = index;
            this.localisedName = localisedName;
            this.code = code;
        }

        /// <summary>
        /// Setup the data map of localisation keys to text strings.
        /// </summary>
        public void Init(Dictionary<string, string> map)
        {
            this.map = map;
        }

        /// <summary>
        /// Attempt to get a localised string given a key.
        /// </summary>
        public Result<string> Get(string key)
        {
            return map.ContainsKey(key) ? 
                new Result<string>(value: map[key]) : 
                new Result<string>(error: 
                    string.Format("Failed to find string with localisation key \"{0}\" in language \"{1}\"", key, localisedName)
                );
        }

        /// TODO: add support for language-specific formatting (e.g. left and right quotation marks).
    }

    /// <summary>
    /// Singleton that can localise text to a desired language.
    /// </summary>
    public static class Localise {

        /// <summary>
        /// Current language everything is being localised to.
        /// </summary>
        public static Language language { get; private set; }

        /// <summary>
        /// Load an array of localisation strings from a CSV file.
        /// </summary>
        public static Dictionary<string, string>[] LoadLanguages(TextAsset csv)
        {
            // TODO: Handle newline characters properly
            string[] lines = csv.text.Split('\n');
            string[] entries = lines[0].Trim().Split(',');
            Dictionary<string, string>[] data = new Dictionary<string, string>[entries.Length];
            for (int i = 0, counti = entries.Length - 1; i < counti; i++)
            {
                entries[i] = entries[i].Trim('\"');
                data[i] = new Dictionary<string, string>();
            }

            // Line-by-line loading from the CSV
            for (int i = 1, counti = lines.Length; i < counti; i++)
            {
                // Regex split to handle various characters
                entries = Regex.Split(lines[i].Trim(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                for (int entry = 1, numEntries = entries.Length; entry < numEntries; entry++) {
                    data[entry - 1][entries[0].Trim('\"')] = entries[entry].Trim('\"');
                }
            }
            Log.Debug("Loaded {0} languages with a total of {1} entries.", data.Length, lines.Length);

            return data;
        }

        /// <summary>
        /// Switches the target language to a different language and updates all active LocalisedText components.
        /// </summary>
        public static void SwitchLanguage(Language language) {
            if (Application.isPlaying && Localise.language != language) {
                Localise.language = language;

                /// Find all enabled LocalisedText components and update their text.
                LocalisedText[] allText = GameObject.FindObjectsOfType<LocalisedText>();
                for (int i = 0, counti = allText.Length; i < counti; i++)
                {
                    if (allText[i].enabled)
                    {
                        allText[i].AutoLocalise();
                    }
                }
            }
        }

        public static string Runtime(string raw)
        {
            return raw;
        }

        /// <summary>
        /// Attempts to return a localised version of a string for the current language.
        /// On failure to find an associated localised string, returns the [MISSING STRING "{raw}"]
        /// If no language is set, this just returns the raw string but formatted.
        /// </summary>
        /// <param name="raw">Text string to be localised. This is also doubles as the localisation id.</param>
        public static string Text(string raw, params object[] args) {
            if (language != null && Application.isPlaying)
            {
                Result<string> res = language.Get(raw);
#if UNITY_EDITOR
                if (res.hasError)
                {
                    Log.Error(
                        "Failed to get string localisation for language: {0}, raw string: \"{0}\"",
                        language.localisedName,
                        raw
                    );
                }
#endif
                return res.hasValue ? string.Format(res.value, args) : "[MISSING STRING: \"" + raw + "\"]";
            }
            return string.Format(raw, args);
        }

        /// <summary>
        /// Returns a number with formatting applied in relation to the desired language.
        /// </summary>
        public static string Number(string formatting, float n) {
            /// TODO: map to desired language.
            return string.Format(formatting, n);
        }

        /// <summary>
        /// Returns the currency formatting related to the desired language, e.g. "$5" -> "5$"
        /// </summary>
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
