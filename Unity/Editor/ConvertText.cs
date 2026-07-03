using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpenGET
{

    /// <summary>
    /// Converts text strings in scripts by find-and-replace. Most useful for replacing non-standard characters.
    /// </summary>
    public class ConvertText : PersistentTool<ConvertText>
    {
        protected override string persistentSettings => "textConversion";

        [MenuItem("OpenGET/Convert Text...")]
        public static void Open()
        {
            CreateWindow("Text Converter");
        }

        private List<string> GetScriptPaths()
        {
            List<string> paths = new List<string>();
            for (int i = 0, counti = config.textConversion.scriptIncludePaths.Length; i < counti; i++)
            {
                paths.AddRange(System.IO.Directory.EnumerateFiles(
                    System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, config.textConversion.scriptIncludePaths[i])),
                    "*.cs",
                    System.IO.SearchOption.AllDirectories
                ));
            }
            return paths;
        }

        /// <summary>
        /// Check if a character is escaped at the given string index.
        /// </summary>
        private bool IsEscaped(string source, int index)
        {
            if (index > 0)
            {
                return source[index - 1] == '\\' && !IsEscaped(source, index - 1);
            }
            return false;
        }

        /// <summary>
        /// Find and replace all text strings.
        /// </summary>
        private string FindAndReplace(string source)
        {
            string result = "";
            string candidate = "";
            EditorConfig.TextConversion.FindReplacePair[] findPairs = config.textConversion.findAndReplace;
            for (int i = 0, counti = source.Length; i < counti; i++)
            {
                candidate += source[i];
                bool matching = false;
                for (int j = 0, countj = findPairs.Length; j < countj; j++)
                {
                    if (findPairs[j].find.StartsWith(candidate))
                    {
                        if (findPairs[j].find == candidate)
                        {
                            candidate = findPairs[j].replace;
                        }
                        else
                        {
                            matching = true;
                        }
                        break;
                    }
                }
                // Add to result if no matches, or candidate has been replaced
                if (!matching)
                {
                    result += candidate;
                    candidate = "";
                }
            }
            return result;
        }

        private void ConvertAll()
        {
            if (EditorUtility.DisplayDialog(
                "Convert All Text",
                "Are you sure you wish to convert all text strings? This will alter project scripts.",
                "Continue",
                "Cancel"
            )) {
                Log.Info("Starting text string conversion...");

                int total = 0;

                // Get paths to all scripts in valid directories
                List<string> paths = GetScriptPaths();
                int counti = paths.Count;
                for (int i = 0; i < counti; i++)
                {
                    int replacements = 0;
                    string script = File.ReadAllText(paths[i], Encoding.GetEncoding(config.textConversion.fileReadEncoding));
                    bool openString = false;
                    string found = "";
                    string replacementScript = "";
                    for (int j = 0, countj = script.Length; j < countj; j++)
                    {
                        if (!openString)
                        {
                            replacementScript += script[j];
                        }
                        if (script[j] == '"' && !IsEscaped(script, j))
                        {
                            if (openString)
                            {
                                // Find & replace on close string
                                replacementScript += FindAndReplace(found);
                                replacementScript += '"';
                                replacements++;
                            }
                            found = "";
                            openString = !openString;
                        }
                        else
                        {
                            found = string.Concat(found, script[j]);
                        }
                    }
                    total += replacements;
                    Log.Debug("Replaced {0} match{1} in file \"{2}\"", replacements, replacements != 1 ? "es" : "", paths[i]);

                    File.WriteAllText(paths[i], replacementScript, Encoding.GetEncoding(config.textConversion.fileWriteEncoding));
                }

                Log.Info("Replaced {0} match{1} in {2} file(s)", total, total != 1 ? "es" : "", counti);
            }
        }

        public override void CreateGUI()
        {
            base.CreateGUI();

            Button button = new Button(ConvertAll)
            {
                text = "Convert All Text"
            };
            root.Add(button);
        }
    }
}
