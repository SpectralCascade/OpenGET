using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    [CreateAssetMenu(fileName = "NewGlossaryEntry", menuName = "OpenGET/Glossary Entry")]
    public class GlossaryEntry : GlyphEntry, IReferrable
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

        /// <summary>
        /// Only get the glyph. Do not tint.
        /// </summary>
        public string glyphOnly => Get(false, true, false);

        /// <summary>
        /// Get string without the glossary tooltip or styling; tinted to match the text colour.
        /// </summary>
        public string clean => Get(false, true, true, null, true);

        /// <summary>
        /// Get the localised text string in a specific style, with or without specific elements. By default, all elements are enabled.
        /// </summary>
        public string Get(bool showLink = true, bool showGlyph = true, bool showText = true, string style = "GlossaryEntry", bool tintGlyph = false)
        {
            showText = showText && !string.IsNullOrEmpty(_text);
            bool gotStyle = showText && !string.IsNullOrEmpty(style);
            showGlyph = showGlyph && !string.IsNullOrEmpty(id);
            showLink = showLink && !string.IsNullOrEmpty(description);
            return (showLink ? $"<link=\"{name}\">" : "") 
                + (gotStyle ? $"<style={style}>" : "")
                + text
                + (gotStyle ? "</style>" : "")
                + (showLink ? "</link>" : "")
                + (showGlyph ? (showText ? " " : "") 
                + "<sprite" + (spriteSheet != null ? $"=\"{spriteSheet.name}\"" : "") + $" name=\"{id}\"" + (tintGlyph ? " tint=1" : "") + ">" : "");
        }

        public override string ToString()
        {
            return Get();
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
