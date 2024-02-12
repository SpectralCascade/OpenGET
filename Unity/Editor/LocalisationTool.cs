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
    public class LocalisationTool : EditorWindow
    {

        /// <summary>
        /// Pattern matching data for extracting strings from function call arguments.
        /// </summary>
        [System.Serializable]
        public class Marker
        {
            public Marker(string functionName, int extractMax = 1)
            {
                this.functionName = functionName;
                this.extractMax = extractMax;
            }

            /// <summary>
            /// Name of the function to extract from.
            /// </summary>
            public string functionName;

            /// <summary>
            /// Maximum number of arguments to consider for string extraction.
            /// </summary>
            public int extractMax;

        }

        /// <summary>
        /// Contains extracted string data as well as additional information such as it's marker.
        /// Used exclusively for extracting from code.
        /// </summary>
        private class ExtractionData
        {
            public ExtractionData(string data, Marker marker)
            {
                this.data = data;
                this.marker = marker;
            }

            /// <summary>
            /// Extracted raw string data.
            /// </summary>
            public string data;

            /// <summary>
            /// Associated marker.
            /// </summary>
            public Marker marker;
        }

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
            public string ExportRow()
            {
                return 
                    "\"" + raw + "\"," +
                    "\"" + sourceFileNames + "\"," +
                    "\"" + string.Join(
                        ", ",
                        (System.Enum.GetValues(typeof(SourceType)) as IEnumerable<SourceType>).Where(x => (sources & x) > 0)
                    ) + "\"," +
                    "\"" + formattingContext + "\""
                ;
            }
        }

        /// <summary>
        /// Database table of ids mapped to export data.
        /// </summary>
        private Dictionary<string, ExportData> exportTable = new Dictionary<string, ExportData>();

        /// <summary>
        /// Database table of ids mapped to imported localised strings.
        /// </summary>
        private Dictionary<string, string[]> importTable = new Dictionary<string, string[]>();

        /// <summary>
        /// How many columns are in the import table?
        /// </summary>
        private int numImportColumns = 0;

        /// <summary>
        /// OpenGET editor settings object reference.
        /// </summary>
        public EditorConfig config;

        /// <summary>
        /// Localisation config settings property field.
        /// </summary>
        private UnityEditor.UIElements.PropertyField localisationConfig;

        /// <summary>
        /// This object's serialisation binding.
        /// </summary>
        private SerializedObject serialiser;

        /// <summary>
        /// Info displayed about import/export data.
        /// </summary>
        private Label infoLabel;

        /// <summary>
        /// Setup the editor window.
        /// </summary>
        [MenuItem("OpenGET/Localisation")]
        public static void Open()
        {
            LocalisationTool window = GetWindow<LocalisationTool>();
            window.titleContent = new GUIContent("Localisation [OpenGET]");
        }

        /// <summary>
        /// Editor window content.
        /// </summary>
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Make sure we always have a valid configuration
            if (config == null || config.name.Length <= 0)
            {
                string[] found = AssetDatabase.FindAssets("t:" + typeof(EditorConfig).Name);

                config = found.Length > 0 ?
                    AssetDatabase.LoadAssetAtPath<EditorConfig>(AssetDatabase.GUIDToAssetPath(found[0])) :
                    CreateInstance<EditorConfig>();
            }

            // OpenGET configuration settings reference
            serialiser = new SerializedObject(this);
            SerializedProperty prop = serialiser.FindProperty("config");
            localisationConfig = new UnityEditor.UIElements.PropertyField(prop);
            UnityEditor.UIElements.BindingExtensions.Bind(localisationConfig, serialiser);
            root.Add(localisationConfig);

            // Display localisation settings
            SerializedObject obj = new SerializedObject(config);
            prop = obj.FindProperty("localisation");
            UnityEditor.UIElements.PropertyField addProp = new UnityEditor.UIElements.PropertyField(prop);
            UnityEditor.UIElements.BindingExtensions.Bind(addProp, obj);
            root.Add(addProp);

            root.Add(new Label("\nImport pre-existing localisations"));

            Button button = new Button(() => {
                string path = "";
                if (string.IsNullOrEmpty(config.localisation.importPath))
                {
                    path = EditorUtility.OpenFilePanel("Import localisations", Application.dataPath, "csv");
                }
                else
                {
                    path = config.localisation.importPath;
                }

                if (!string.IsNullOrEmpty(path))
                {
                    Log.Debug("Importing localisations CSV at \"{0}\"", path);
                    CSVFile.CSVReader csv = new CSVFile.CSVReader(new System.IO.StreamReader(path, System.Text.Encoding.UTF8));
                    csv.Settings.HeaderRowIncluded = true;
                    csv.Settings.TextQualifier = '"';
                    csv.Settings.LineSeparator = "\n";

                    int line = 0;
                    foreach (string[] row in csv)
                    {
                        Log.Debug("Importing CSV row {0}", line);
                        line++;
                        //if (line > 1)
                        //{
                            List<string> data = new List<string>(row);
                            data.RemoveAt(0);
                            if (!importTable.ContainsKey(row[0]))
                            {
                                importTable.Add(row[0], data.ToArray());
                                Log.Debug("Imported CSV row with key \"{0}\": {1}", row[0], string.Join(", ", row));
                            }
                        //}
                    }
                }
                else
                {
                    Log.Warning("Could not obtain path to import CSV.");
                }

                UpdateInfoLabel();
            });
            button.name = "ImportCSV";
            button.text = "Import localisations CSV";
            root.Add(button);

            button = new Button(() => MigrateFromTypicalCSV(
                EditorUtility.OpenFilePanel("Migrate old localisations", Application.dataPath, "csv"))
            );
            button.name = "MigrateCSV";
            button.text = "Migrate pre-existing localisation file";
            root.Add(button);

            root.Add(new Label("\nExtract strings"));

            button = new Button();
            button.name = "";
            button.text = "Test Regex";
            button.clicked += () => {

                string code = @"
                    SomeFunction(param1, param2);
                    Localise.Text(""key1 {0}"", Localise.Text(""Nested function call, with a comma... and some \funky\ chars"", x));
                    Localise.Text(AnotherFunction(param3), param4);
                    [SettingsGroup(""Test"", ""Desc test"")]
                    Localise.Text(""key2"", param5);
                    var Text = Localise.Text(""some \""escaped\"" string"", param6, Localise.Text(""Another nested call"", Localise.Text(""Double nested"", y)));
                    Localise.Text(""key3"", ""default"");
                    FunctionWithArray(new int[] { 1, 2, 3 }, param7);
                ";
                List<ExportData> newData = ProcessArgs(
                    ExtractArguments(
                        config.localisation.extractionMatches,
                        GetPatternString(config.localisation.extractionMatches),
                        new Stack<ExtractionData>(new ExtractionData[] { new ExtractionData(code, null) })
                    ),
                    "__test__"
                );

                Log.Debug(
                    "Found {0} new localisation strings: [{1}]",
                    newData.Count,
                    string.Join(", ", newData.Select(x => "\"" + x.raw.Replace("{", "{{").Replace("}", "}}") + "\""))
                );
            };

            root.Add(button);

            if (false)
            {
                button = new Button(() => ScrapeCode());
                button.name = "ScrapeCode";
                button.text = "Extract strings from scripts";
                root.Add(button);

                button = new Button(() => ScrapeScenes());
                button.name = "ScrapeScenes";
                button.text = "Extract strings from scenes";
                root.Add(button);

                button = new Button(() => ScrapePrefabs());
                button.name = "ScrapePrefabs";
                button.text = "Extract strings from prefabs";
                root.Add(button);
            }

            button = new Button(() => {
                ScrapeCode();
                ScrapePrefabs();
                ScrapeScenes();
            });
            button.name = "ScrapeAll";
            button.text = "Extract strings from ALL sources";
            root.Add(button);

            button = new Button(() => System.IO.File.WriteAllText(
                EditorUtility.SaveFilePanel("Export data", Application.dataPath, "export_strings", "csv"), ExportToCSV())
            );
            button.name = "ExportCSV";
            button.text = "Export strings to CSV";
            root.Add(button);

            root.Add(new Label("\nAmend imported localisations"));

            button = new Button(() => MergeExportIntoImport());
            button.name = "MergeExportIntoImport";
            button.text = "Merge export strings into import data";
            root.Add(button);

            button = new Button(() => System.IO.File.WriteAllText(
                EditorUtility.SaveFilePanel("Save localisations table", Application.dataPath, "strings", "csv"), ImportToCSV())
            );
            button.name = "ExportTable";
            button.text = "Save localisations to CSV";
            root.Add(button);

            root.Add(new Label("\nClear data"));

            button = new Button(() => {
                importTable.Clear();
                UpdateInfoLabel();
            });
            button.name = "ClearImportData";
            button.text = "Clear import data";
            root.Add(button);

            button = new Button(() => {
                exportTable.Clear();
                UpdateInfoLabel();
            });
            button.name = "ClearExportData";
            button.text = "Clear export data";
            root.Add(button);

            infoLabel = new Label("");
            UpdateInfoLabel();
            root.Add(infoLabel);
        }

        private void OnInspectorUpdate()
        {
            if (config == null)
            {
                config = CreateInstance<EditorConfig>();
            }
        }

        /// <summary>
        /// Convert export table into a CSV string.
        /// </summary>
        private string ExportToCSV()
        {
            return string.Join("\n", exportTable.Select(x => x.Value.ExportRow()));
        }

        /// <summary>
        /// Convert import able into a CSV string.
        /// </summary>
        private string ImportToCSV()
        {
            /*foreach (var row in importTable)
            {
                Log.Debug("Writing {0} : {1}", "\"" + row.Key + "\"", string.Join(", ", row.Value.Select(x => "\"" + x + "\"")));
            }*/
            return string.Join("\n", importTable.Select(kv => string.Join(",", kv.Value.Select(str => "\"" + str + "\""))));
        }

        /// <summary>
        /// Gets localisation extraction marker function names in RegEx string format.
        /// </summary>
        public string GetPatternString(Marker[] markers)
        {
            return "(?:" + string.Join("|", markers.Select(x => Regex.Escape(x.functionName))) + ")";
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
        private Stack<ExtractionData> ExtractArguments(Marker[] markers, string functionNames, Stack<ExtractionData> code, int depth = 0) {
            string pattern = functionNames + @"\s*\(([^()""']|""(?:\\.|[^""\\])*""|'(?:\\.|[^'\\])*'|\((?<DEPTH>)|\)(?<-DEPTH>))*(?(DEPTH)(?!))\)";

            ExtractionData source = code.Pop();
            MatchCollection matches = Regex.Matches(source.data, pattern);
            foreach (Match match in matches)
            {
                // Strip function call wrapping to get the actual arguments string
                string found = match.Groups[0].Value.Trim();
                Marker marker = markers.FirstOrDefault(x => found.StartsWith(x.functionName));
                string parameters = GetParameters(found);

                if (!string.IsNullOrEmpty(parameters))
                {
                    // Now push args on the stack and consider those
                    if (depth > 0)
                    {
                        source.data = source.data.Replace(match.Groups[0].Value, "[localised text]");
                    }
                    code.Push(new ExtractionData(parameters, marker));
                    ExtractArguments(markers, functionNames, code, depth + 1);
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
        /// Strip string literal wrapper characters.
        /// </summary>
        private string StripLiteral(string literal)
        {
            return literal.Remove(literal.Length - 1).Remove(0, literal[0] == '"' ? 1 : 2);
        }

        /// <summary>
        /// Extracts string literal arguments from the given function calls and gets strings setup for export.
        /// </summary>
        private List<ExportData> ProcessArgs(Stack<ExtractionData> code, string fileName)
        {
            // Step through the stack and extract strings
            List<ExportData> freshExport = new List<ExportData>();
            while (code.Count > 0)
            {
                ExtractionData extracted = code.Pop();
                string args = extracted.data;

                //Log.Debug("Processing args: {0}", args.Replace("{", "{{").Replace("}", "}}"));
                MatchCollection matches = Regex.Matches(args, @"""(?:\\.|[^""\\])*""|[^,]+");
                int counti = Mathf.Min(matches.Count, extracted.marker != null ? extracted.marker.extractMax : 1);
                List<string> extractedArgs = new List<string>();
                for (int i = 0; i < counti; i++)
                {
                    for (int j = 0, countj = matches[i].Captures.Count; j < countj; j++)
                    {
                        string arg = matches[i].Captures[j].Value.Trim();
                        if (arg.Length > 2 && (arg[0] == '"' || arg[1] == '"'))
                        {
                            extractedArgs.Add(arg);
                            args = args.Replace(arg, "_");
                        }
                        //Log.Debug("Obtained arg: {0}", arg.Replace("{", "{{").Replace("}", "}}"));
                    }
                }

                counti = extractedArgs.Count;
                for (int i = 0; i < counti; i++)
                {
                    string arg = extractedArgs[i];
                    arg = StripLiteral(arg);

                    string func = (extracted.marker != null ? extracted.marker.functionName : "");
                    if (!exportTable.ContainsKey(arg))
                    {
                        ExportData data = new ExportData();
                        data.raw = arg;
                        data.sourceFileNames = fileName;
                        data.sources = ExportData.SourceType.Script;
                        data.formattingContext = args.Length > 0 ? 
                            (counti > 1 ? "Param " + (i + 1) + " of " : "") + (func + "(" + args + ")") : "";
                        exportTable.Add(arg, data);
                        freshExport.Add(data);
                    }
                    else
                    {
                        exportTable[arg].sourceFileNames += ", " + fileName;
                        exportTable[arg].sources |= ExportData.SourceType.Script;
                        exportTable[arg].formattingContext += ", " + func + "(" + args + ")";
                    }
                }
            }
            return freshExport;
        }

        /// <summary>
        /// Export extracted localisation strings to CSV.
        /// </summary>
        public void ExportCSV(string path, List<ExportData> export)
        {
            Log.Debug("Found {0} localisation strings, saving to {1}", export.Count, path);
            string data = string.Join("\n", export.Select(x => x.ExportRow()));
            System.IO.File.WriteAllText(path, data, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Info label text update.
        /// </summary>
        private void UpdateInfoLabel()
        {
            infoLabel.text = string.Format("Import data rows: {0}\nExport data rows: {1}", importTable.Count, exportTable.Count);
        }

        private List<string> GetScriptPaths()
        {
            List<string> paths = new List<string>();
            for (int i = 0, counti = config.localisation.scriptIncludePaths.Length; i < counti; i++)
            {
                paths.AddRange(System.IO.Directory.EnumerateFiles(
                    System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, config.localisation.scriptIncludePaths[i])),
                    "*.cs",
                    System.IO.SearchOption.AllDirectories
                ));
            }
            return paths;
        }

        /// <summary>
        /// Scrape code for localisation strings.
        /// TODO: Parallelise to speed up processing time.
        /// </summary>
        public List<ExportData> ScrapeCode()
        {
            // Get paths to all scripts in valid directories
            List<string> paths = GetScriptPaths();

            // Read all scripts and extract raw strings to be localised
            List<ExportData> export = new List<ExportData>();
            string match = GetPatternString(config.localisation.extractionMatches);
            Log.Debug("Matching functions: {0}", match);
            for (int i = 0, counti = paths.Count; i < counti; i++)
            {
                Log.Debug("Parsing file {0}/{1} at \"{2}\"", i + 1, counti, paths[i]);

                try
                {
                    Stack<ExtractionData> code = new Stack<ExtractionData>();
                    code.Push(new ExtractionData(System.IO.File.ReadAllText(paths[i]), null));
                    export.AddRange(
                        ProcessArgs(
                            ExtractArguments(config.localisation.extractionMatches, match, code),
                            System.IO.Path.GetFileNameWithoutExtension(paths[i])
                        )
                    );
                }
                catch (System.Exception e)
                {
                    Log.Exception(e);
                }
            }
            UpdateInfoLabel();

            return export;
        }

        /// <summary>
        /// Scrape GameObjects from the root of some hierarchy such as a scene root GameObject, or a prefab.
        /// </summary>
        public List<ExportData> ScrapeHierarchy(GameObject root, ExportData.SourceType type)
        {
            LocalisedText[] allText = root.GetComponentsInChildren<LocalisedText>(true);
            List<ExportData> exported = new List<ExportData>();
            for (int i = 0, counti = allText.Length; i < counti; i++)
            {
                exported.Add(AddString(
                    allText[i].id,
                    type == ExportData.SourceType.Scene ? root.scene.name : root.name,
                    allText[i].gameObject.name,
                    type
                ));
            }
            UpdateInfoLabel();

            return exported;
        }
        
        /// <summary>
        /// Scrape all prefabs in the specified include paths.
        /// </summary>
        public List<ExportData> ScrapePrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:prefab", config.localisation.prefabIncludePaths);
            List<ExportData> extracted = new List<ExportData>();
            for (int i = 0, counti = guids.Length; i < counti; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Log.Debug("Extracting from prefab at \"{0}\"", path);
                GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root != null)
                {
                    extracted.AddRange(ScrapeHierarchy(root, ExportData.SourceType.Prefab));
                }
                else
                {
                    Log.Debug("Root GameObject is null :(");
                }
            }
            Log.Debug("Searched {0} prefab(s) and extracted {1} localisation strings.", guids.Length, extracted.Count);
            UpdateInfoLabel();
            return extracted;
        }

        /// <summary>
        /// Extract strings from LocalisedText objects in scenes.
        /// </summary>
        public List<ExportData> ScrapeScenes()
        {
            // TODO: Load scene data from file YAML rather than loading in entirety
            List<ExportData> export = new List<ExportData>();
            string[] paths = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0, counti = paths.Length; i < counti; i++) {
                paths[i] = EditorBuildSettings.scenes[i].path;
            }
            SceneNavigator.RunSceneProcess(
                paths,
                (Scene scene) => {
                    Log.Debug("Extracting localisation strings from scene {0}", scene.name);
                    GameObject[] roots = scene.GetRootGameObjects();
                    for (int i = 0, counti = roots.Length; i < counti; i++)
                    {
                        export.AddRange(ScrapeHierarchy(roots[i], ExportData.SourceType.Scene));
                    }
                }
            );
            Log.Debug(
                "Searched {0} scene(s) and extracted {1} localisation strings.",                
                paths.Length,
                export.Count
            );
            UpdateInfoLabel();
            return export;
        }

        /// <summary>
        /// Add a string to the localisation export table.
        /// </summary>
        private ExportData AddString(string raw, string sourceFileName, string context, ExportData.SourceType source)
        {
            ExportData data;
            if (!exportTable.ContainsKey(raw))
            {
                data = new ExportData();
                data.raw = raw;
                data.sourceFileNames = sourceFileName;
                data.sources = source;
                data.formattingContext = context;
                exportTable.Add(raw, data);
            }
            else
            {
                data = exportTable[raw];
                data.sourceFileNames += ", " + sourceFileName;
                data.sources |= source;
                data.formattingContext += ", " + context;
            }
            return data;
        }

        /// <summary>
        /// Merge export data into import data, with empty strings inserted in lieu of localised data.
        /// Optionally provide the index of the main language and add the raw IDs to that column as well.
        /// </summary>
        private void MergeExportIntoImport(int mainLanguageColumn = 1)
        {
            foreach (KeyValuePair<string, ExportData> entry in exportTable)
            {
                if (!importTable.ContainsKey(entry.Key))
                {
                    string[] data = new string[numImportColumns];
                    data[0] = entry.Key;
                    if (mainLanguageColumn > 0 && mainLanguageColumn < numImportColumns)
                    {
                        data[mainLanguageColumn] = entry.Key;
                    }
                    importTable.Add(entry.Key, data);
                }
            }
            UpdateInfoLabel();
        }

        /// <summary>
        /// Migrate a typical localisation table that has custom IDs to this localisation table, which uses raw strings as IDs.
        /// This does assume that the layout of the headers row is the same (i.e. IDs column, then language names).
        /// </summary>
        private void MigrateFromTypicalCSV(string path, int nativeStringColumn = 1, int skipRows = 1, bool migrateScripts = true)
        {
            CSVFile.CSVReader csv = new CSVFile.CSVReader(new System.IO.StreamReader(path, System.Text.Encoding.UTF8));
            csv.Settings.HeaderRowIncluded = true;
            csv.Settings.TextQualifier = '"';
            if (nativeStringColumn < 0 || nativeStringColumn >= csv.Headers.Length)
            {
                Log.Error("Failed to migrate from typical CSV as specified native string column index {0} is out of range.", nativeStringColumn);
                return;
            }

            int rowsRead = 0;
            numImportColumns = csv.Headers.Length;
            // Track IDs to match up in code and overwrite
            Dictionary<string, string> idMap = new Dictionary<string, string>();
            foreach (string[] row in csv)
            {
                rowsRead++;
                if (rowsRead > skipRows)
                {
                    if (!importTable.ContainsKey(row[nativeStringColumn]))
                    {
                        importTable.Add(row[nativeStringColumn], new string[numImportColumns]);
                        Log.Debug("Adding new localisation entry \"{0}\"", row[nativeStringColumn]);
                    }
                    else
                    {
                        Log.Debug("Importing existing localisation entry \"{0}\"", row[nativeStringColumn]);
                    }
                    importTable[row[nativeStringColumn]][0] = row[nativeStringColumn];
                    idMap[row[0]] = row[nativeStringColumn];
                    for (int i = 1, counti = Mathf.Min(row.Length, numImportColumns); i < counti; i++)
                    {
                        importTable[row[nativeStringColumn]][i] = row[i];
                    }
                }
            }

            if (migrateScripts)
            {
                // Do a simple find and replace operation to migrate old IDs
                List<string> scripts = GetScriptPaths();
                for (int i = 0, counti = scripts.Count; i < counti; i++)
                {
                    try
                    {
                        string code = System.IO.File.ReadAllText(scripts[i]);
                        bool didReplace = false;
                        foreach (var kv in idMap)
                        {
                            string idOld = "\"" + kv.Key + "\"";
                            if (code.Contains(idOld))
                            {
                                didReplace = true;
                                code = code.Replace(idOld, "\"" + kv.Value + "\"");
                            }
                        }

                        if (didReplace)
                        {
                            Log.Debug("Overwriting script \"{0}\" following localisation ID migration.", scripts[i]);
                            System.IO.File.WriteAllText(scripts[i], code, System.Text.Encoding.UTF8);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Log.Exception(e);
                    }
                }
            }

            Log.Debug("Imported {0} localisation entries.", rowsRead - skipRows);
            UpdateInfoLabel();
        }

    }

}
