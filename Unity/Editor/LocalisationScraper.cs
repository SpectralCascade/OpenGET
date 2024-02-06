using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenGET
{

    /// <summary>
    /// Editor tool for generating a CSV of localisation strings from a Unity project.
    /// </summary>
    public class LocalisationScraper : EditorWindow
    {

        /// <summary>
        /// Contains localisation string export data, not just the raw string but also contextual information.
        /// </summary>
        [System.Serializable]
        public class ExportData
        {
            /// <summary>
            /// The source type of this data. In some cases there may be multiple sources, so these are bitflags.
            /// </summary>
            [System.Flags]
            public enum SourceType
            {
                Unknown = 0,
                Script = 1,
                Prefab = 2,
                Scene = 4
            }

            /// <summary>
            /// Raw string data, which doubles as the localisation ID.
            /// </summary>
            public string raw;

            /// <summary>
            /// Name of the file(s) this string was extracted from.
            /// </summary>
            public string sourceFileNames;

            /// <summary>
            /// What type of source file(s) this comes from.
            /// </summary>
            public SourceType sources;

            /// <summary>
            /// If the string is formatted with items such as {0}, those parameters are listed here.
            /// </summary>
            public string formattingContext;

            /// <summary>
            /// Export the data as a CSV row.
            /// </summary>
            public string[] ExportRow()
            {
                return new string[] {
                    "\"" + raw + "\"",
                    "\"" + sourceFileNames + "\"",
                    "\"" + string.Join(
                        ", ",
                        (System.Enum.GetValues(typeof(SourceType)) as IEnumerable<SourceType>).Where(x => (sources & x) > 0)
                    ) + "\"",
                    "\"" + formattingContext + "\""
                };
            }
        }

        /// <summary>
        /// All function names to match for when extracting strings for localisation.
        /// </summary>
        public string matchFunctionNames => functionNames.value;

        /// <summary>
        /// Path to the CSV file used as the persistent database table.
        /// </summary>
        private TextAsset csv = null;

        /// <summary>
        /// Database table of ids mapped to export data.
        /// </summary>
        private Dictionary<string, ExportData> exportTable = new Dictionary<string, ExportData>();

        /// <summary>
        /// Freshly found strings that don't already exist in the database.
        /// </summary>
        private List<string> fresh = new List<string>();

        private TextField functionNames;

        /// <summary>
        /// Setup the editor window.
        /// </summary>
        [MenuItem("OpenGET/Localisation")]
        public static void Open()
        {
            LocalisationScraper window = GetWindow<LocalisationScraper>();
            window.titleContent = new GUIContent("Localisation [OpenGET]");
        }

        /// <summary>
        /// Editor window content.
        /// </summary>
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Create button
            Button button = new Button();
            button.name = "";
            button.text = "Test Regex";
            button.clicked += () => {

                string code = @"
                    SomeFunction(param1, param2);
                    Localise.Text(""key1 {0}"", Localise.Text(""Nested function call, with a comma... and some \funky\ chars"", x));
                    Localise.Text(AnotherFunction(param3), param4);
                    Localise.Text(""key2"", param5);
                    var Text = Localise.Text(""some \ escaped \ string"", param6, Localise.Text(""Another nested call"", Localise.Text(""Double nested"", y)));
                    Localise.Text(""key3"", ""default"");
                    FunctionWithArray(new int[] { 1, 2, 3 }, param7);
                ";
                exportTable.Clear();
                List<ExportData> newData = ProcessArgs(ExtractArguments(matchFunctionNames, new Stack<string>(new string[] { code })), "__test__");

                Log.Debug(
                    "Found {0} new localisation strings: [{1}]",
                    newData.Count, 
                    string.Join(", ", newData.Select(x => "\"" + x.raw.Replace("{", "{{").Replace("}", "}}") + "\""))
                );
            };

            root.Add(button);

            functionNames = new TextField();
            // TODO: Serialise settings and load them in
            functionNames.value = "Localise\\.Text";
            functionNames.name = "FunctionNames";
            functionNames.label = "Function names to match";
            root.Add(functionNames);

            button = new Button(ScrapeCode);
            button.name = "ScrapeCode";
            button.text = "Extract strings from scripts";
            root.Add(button);
        }

        /// <summary>
        /// Trim a function call to it's parameters only. Expects the call to be trimmed already.
        /// </summary>
        private string GetParameters(string functionCall)
        {
            for (int i = 0, counti = functionCall.Length; i < counti; i++)
            {
                if (functionCall[i] == '(')
                {
                    return functionCall.Remove(counti - 1).Remove(0, i + 1);
                }
            }
            return functionCall;
        }

        /// <summary>
        /// Searches for relevant function calls and extracts the argument strings into a stack.
        /// Treats nested calls as separate stack items and replaces them with a dummy parameter in parent stack items.
        /// e.g. arguments string "\"Test {0}\", failed ? LocaliseFunc("Failed") : LocaliseFunc("Succeeded")"
        /// becomes string "\"Test {0}\", failed ? [localised text] : [localised text]"
        /// </summary>
        private Stack<string> ExtractArguments(string functionNames, Stack<string> code, int depth = 0) {
            string pattern = functionNames + @"\s*\(([^()""']|""(?:\\.|[^""\\])*""|'(?:\\.|[^'\\])*'|\((?<DEPTH>)|\)(?<-DEPTH>))*(?(DEPTH)(?!))\)"; ;

            string source = code.Pop();
            MatchCollection matches = Regex.Matches(source, pattern);
            foreach (Match match in matches)
            {
                // Strip function call wrapping to get the actual arguments string
                string parameters = GetParameters(match.Groups[0].Value.Trim());

                if (!string.IsNullOrEmpty(parameters))
                {
                    // Now push args on the stack and consider those
                    if (depth > 0)
                    {
                        source = source.Replace(match.Groups[0].Value, "[localised text]");
                    }
                    code.Push(parameters);
                    ExtractArguments(functionNames, code, depth + 1);
                }
            }

            // Ensure arguments of every Localise.Text call are stored
            if (depth > 0)
            {
                //Log.Debug("Localise.Text call at depth {0}, extracted args: \"{1}\"", depth, source.Replace("{", "{{").Replace("}", "}}"));
                code.Push(source);
            }

            return code;
        }

        /// <summary>
        /// Extracts string literal arguments from the given function calls and gets strings setup for export.
        /// </summary>
        private List<ExportData> ProcessArgs(Stack<string> code, string fileName)
        {
            // Step through the stack and extract strings
            List<ExportData> freshExport = new List<ExportData>();
            while (code.Count > 0)
            {
                string args = code.Pop();

                //Log.Debug("Processing args: {0}", args.Replace("{", "{{").Replace("}", "}}"));
                MatchCollection matches = Regex.Matches(args, @"(""[^""]*"")|[^,]+");
                if (matches.Count > 0)
                {
                    string arg = matches[0].Value.Trim();
                    args = args.Remove(0, matches[0].Value.Length).Trim();

                    if (arg.Length > 2 && (arg[0] == '"' || arg[1] == '"'))
                    {
                        arg = arg.Remove(arg.Length - 1).Remove(0, arg[0] == '"' ? 1 : 2);
                        if (!exportTable.ContainsKey(arg))
                        {
                            ExportData data = new ExportData();
                            data.raw = arg;
                            data.sourceFileNames = fileName;
                            data.sources = ExportData.SourceType.Script;
                            data.formattingContext = args.Length > 0 ? "(" + args + ")" : "";
                            exportTable.Add(arg, data);
                            freshExport.Add(data);
                        }
                        else
                        {
                            exportTable[arg].sourceFileNames += ", " + fileName;
                            exportTable[arg].sources |= ExportData.SourceType.Script;
                            exportTable[arg].formattingContext += ", (" + args + ")";
                        }
                    }
                    //Log.Debug("Obtained arg: {0}", arg.Replace("{", "{{").Replace("}", "}}"));
                }
            }
            return freshExport;
        }

        /// <summary>
        /// Scrape code for localisation strings.
        /// TODO: Parallelise to speed up processing time.
        /// </summary>
        public void ScrapeCode()
        {
            exportTable.Clear();
            string[] ids = AssetDatabase.FindAssets("t:script");
            string[] paths = ids.Select(x => AssetDatabase.GUIDToAssetPath(x)).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            List<ExportData> newStrings = new List<ExportData>();
            for (int i = 0, counti = paths.Length; i < counti; i++)
            {
                Log.Debug("Parsing file {0}/{1} at \"{2}\"", i + 1, counti, paths[i]);

                try
                {
                    TextAsset script = AssetDatabase.LoadAssetAtPath<TextAsset>(paths[i]);
                    Stack<string> code = new Stack<string>();
                    code.Push(script.text);
                    newStrings.AddRange(
                        ProcessArgs(
                            ExtractArguments(matchFunctionNames, code),
                            System.IO.Path.GetFileNameWithoutExtension(paths[i])
                        )
                    );
                }
                catch (System.Exception e)
                {
                    Log.Exception(e);
                }
            }

            string savePath = Application.dataPath + "/TestExport.csv";
            Log.Debug("Found {0} new localisation strings, saving to {1}", newStrings.Count, savePath);
            string data = string.Join("\n", newStrings.Select(x => string.Join(",", x.ExportRow())));
            System.IO.File.WriteAllText(savePath, data, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Scrape GameObjects from the root of some hierarchy such as a scene root GameObject, or a prefab.
        /// </summary>
        public void ScrapeHierarchy(GameObject root)
        {
            LocalisedText[] allText = root.GetComponentsInChildren<LocalisedText>();
            for (int i = 0, counti = allText.Length; i < counti; i++)
            {
                AddString(allText[i].text);
            }
        }

        /// <summary>
        /// Add a string to the localisation 
        /// </summary>
        private void AddString(string text)
        {
            //if ()
        }

    }

}
