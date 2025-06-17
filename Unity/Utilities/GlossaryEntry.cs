using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    [CreateAssetMenu(fileName = "NewGlossaryEntry", menuName = "OpenGET/Glossary Entry")]
    public class GlossaryEntry : ScriptableObject, IReferrable
    {
        [System.Flags]
        public enum EntryType
        {
            Unknown = 0,
            Noun = 1,
            Verb = 2,
            Adjective = 4,
            Adverb = 8
        }

        /// <summary>
        /// The raw English word/phrase/acronym.
        /// </summary>
        public string text = "";

        /// <summary>
        /// The word class(es) of this entry.
        /// </summary>
        [Tooltip("Specify which class(es) this entry belongs to.")]
        public EntryType type = EntryType.Noun;

        /// <summary>
        /// A description explaining the meaning.
        /// </summary>
        [Tooltip("Describe the meaning of the text.")]
        [TextArea]
        public string description = "";

        [Tooltip("Allow imprecise lookups (i.e. case insensitive)")]
        public bool impreciseLookup = true;

        private string GetString(string text)
        {
            return $"<link=\"{name}\"><u>{text}</u><sup>[?]</sup></link>";
        }

        public override string ToString()
        {
            return GetString(text);
        }

        /// <summary>
        /// Get the class(es) of the entry.
        /// </summary>
        public string GetClassification()
        {
            string result = "";

            int[] values = typeof(EntryType).GetEnumValues() as int[];
            for (int i = 0, counti = values.Length; i < counti; i++)
            {
                string Join(string text)
                {
                    return string.IsNullOrEmpty(result) ? text : string.Join(", ", result, text);
                }

                switch ((EntryType)values[i] & type)
                {
                    case EntryType.Noun:
                        result = Join(Localise.Text("Noun"));
                        break;
                    case EntryType.Verb:
                        result = Join(Localise.Text("Verb"));
                        break;
                    case EntryType.Adjective:
                        result = Join(Localise.Text("Adjective"));
                        break;
                    case EntryType.Adverb:
                        result = Join(Localise.Text("Adverb"));
                        break;
                    default:
                    case 0:
                        break;
                }
            }

            return result;
        }

    }

}
