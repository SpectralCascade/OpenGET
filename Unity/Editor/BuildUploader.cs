using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpenGET.Editor
{

    /// <summary>
    /// Handles upload of builds to Steamworks.
    /// </summary>
    public class BuildUploader : EditorWindow
    {
        /// <summary>
        /// OpenGET editor settings object reference.
        /// </summary>
        public EditorConfig config;

        /// <summary>
        /// Config field.
        /// </summary>
        private UnityEditor.UIElements.PropertyField fieldConfig;

        /// <summary>
        /// This object's serialisation binding.
        /// </summary>
        private SerializedObject serialiser;

        /// <summary>
        /// Prefix added to all Steamworks related editor prefs. DO NOT CHANGE.
        /// </summary>
        private const string KeyPrefixAppSteamworks = "OpenGET.Build.Steamworks.App.";

        private const string AppFilePrefix = "app_build_";
        private const string DepotFilePrefix = "depot_build_";

        /// <summary>
        /// Steam developer account name.
        /// </summary>
        private string account = "";

        /// <summary>
        /// Steam developer account password.
        /// </summary>
        private string password = "";

        /// <summary>
        /// Steamworks app config string field.
        /// </summary>
        private TextField SteamAppField(string key, string desc, string valDefault)
        {
            return EditorKeyField(KeyPrefixAppSteamworks, key, desc, valDefault) as TextField;
        }

        /// <summary>
        /// Steamworks app config int field.
        /// </summary>
        private IntegerField SteamAppField(string key, string desc, int valDefault)
        {
            return EditorKeyField(KeyPrefixAppSteamworks, key, desc, valDefault) as IntegerField;
        }

        /// <summary>
        /// Steamworks app config float field.
        /// </summary>
        private FloatField SteamAppField(string key, string desc, float valDefault)
        {
            return EditorKeyField(KeyPrefixAppSteamworks, key, desc, valDefault) as FloatField;
        }

        /// <summary>
        /// Editor prefs field.
        /// </summary>
        private BaseField<T> EditorKeyField<T>(string prefix, string key, string desc, T valDefault)
        {
            BaseField<T> field = null;
            string fullKey = prefix + key;
            if (valDefault is string)
            {
                string v = EditorPrefs.HasKey(fullKey) ? EditorPrefs.GetString(fullKey) : valDefault as string;
                field = new TextField()
                {
                    label = key,
                    tooltip = desc,
                    value = v
                } as BaseField<T>;
                field.RegisterCallback<ChangeEvent<string>>(
                    (change) => EditorPrefs.SetString(fullKey, change.newValue)
                );
            }
            else if (valDefault is int)
            {
                int v = (int)(object)valDefault;
                if (EditorPrefs.HasKey(fullKey))
                {
                    int.TryParse(EditorPrefs.GetString(fullKey), out v);
                }
                field = new IntegerField()
                {
                    label = key,
                    tooltip = desc,
                    value = v
                } as BaseField<T>;
                field.RegisterCallback<ChangeEvent<int>>(
                    (change) => EditorPrefs.SetString(fullKey, change.newValue.ToString())
                );
            }
            else if (valDefault is float)
            {
                float v = (float)(object)valDefault;
                if (EditorPrefs.HasKey(fullKey))
                {
                    float.TryParse(EditorPrefs.GetString(fullKey), out v);
                }
                field = new FloatField()
                {
                    label = key,
                    tooltip = desc,
                    value = v
                } as BaseField<T>;
                field.RegisterCallback<ChangeEvent<float>>(
                    (change) => EditorPrefs.SetString(fullKey, change.newValue.ToString())
                );
            }

            if (field == null)
            {
                Log.Error("Failed to create field {0}!", key);
            }
            return field;
        }

        /// <summary>
        /// Setup the editor window.
        /// </summary>
        [MenuItem("OpenGET/Build Uploader")]
        public static void Open()
        {
            BuildUploader window = GetWindow<BuildUploader>();
            window.titleContent = new GUIContent("Build Uploader [OpenGET]");
            window.account = null;
            window.password = null;
        }

        private void OnInspectorUpdate()
        {
            if (config == null)
            {
                config = EditorConfig.Instance;
            }
        }

        private delegate string VDF_Recurse(int indent);

        /// <summary>
        /// Recursive VDF function.
        /// </summary>
        private string VDF_KeyObject(int indent, string key, VDF_Recurse recurse)
        {
            string output = new string('\t', indent);
            indent++;
            output += "\"" + key + "\"\n" + output + "{\n" + recurse(indent);
            indent--;
            output += new string('\t', indent) + "}\n";
            return output;
        }

        /// <summary>
        /// Valve key-value format string.
        /// </summary>
        private string VDF_KeyValue(int indent, string key, string value)
        {
            return new string('\t', indent) + "\"" + key + "\" \"" + value + "\"\n";
        }

        private string TryGetPref(string key)
        {
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetString(key) : null;
        }

        private string VDF_TryGetKV(int indent, string key)
        {
            string v = TryGetPref(KeyPrefixAppSteamworks + key);
            return string.IsNullOrEmpty(v) ? "" : VDF_KeyValue(indent, key, v);
        }

        public static async Task<int> RunProcessAsync(Process process)
        {
            return await MakeTask(process).ConfigureAwait(false);
        }

        private static Task<int> MakeTask(Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            process.Exited += (s, info) => tcs.SetResult(process.ExitCode);
            process.OutputDataReceived += (s, info) =>
            {
                if (!string.IsNullOrEmpty(info.Data))
                {
                    Log.Debug(info.Data);
                }
            };
            process.ErrorDataReceived += (s, info) =>
            {
                if (!string.IsNullOrEmpty(info.Data))
                {
                    Log.Error(info.Data);
                }
            };

            bool started = process.Start();
            if (!started)
            {
                throw new System.InvalidOperationException("Could not start process: " + process);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }

        /// <summary>
        /// Uploads a build to Steamworks.
        /// </summary>
        private async Task RunSteampipeUpload()
        {
            string contentRoot = EditorUtility.OpenFolderPanel("Select Build Content Directory", Application.dataPath, "");
            if (string.IsNullOrEmpty(contentRoot))
            {
                // Cancelled
                return;
            }

            // Determine if chosen configuration is valid
            string configKey = KeyPrefixAppSteamworks + "config";
            string configId = TryGetPref(configKey);
            string[] options = config.buildUploader.configs.Select(x => x.id).ToArray();
            if (string.IsNullOrEmpty(configId) && options.Contains(configId))
            {
                Log.Error("Invalid configuration id \"{0}\". Options are: [{1}]", configId, string.Join(", ", options));
                return;
            }
            EditorConfig.BuildUploader.Config chosen = config.buildUploader.configs.First(x => x.id == configId);

            // Get script file output paths
            string appBuildPath = TryGetPref(KeyPrefixAppSteamworks + "CWD");
            if (string.IsNullOrEmpty(appBuildPath))
            {
                Log.Debug("CWD not specified, using Application.temporaryCachePath");
                appBuildPath = Application.temporaryCachePath;
            }
            if (!System.IO.Directory.Exists(appBuildPath))
            {
                Log.Warning("CWD path \"{0}\" is invalid! Please specify an existing directory with read+write permissions.", appBuildPath);
                return;
            }
            string appFilePath = System.IO.Path.Join(appBuildPath, AppFilePrefix + chosen.appId + ".vdf");

            // TODO: Generate .vdf files for chosen configuration
            string appBuild = VDF_KeyObject(0, "AppBuild", indent =>
                VDF_KeyValue(indent, "AppID", chosen.appId) +
                VDF_TryGetKV(indent, "Desc") +
                VDF_TryGetKV(indent, "Preview") +
                VDF_TryGetKV(indent, "SetLive") +
                VDF_KeyValue(indent, "ContentRoot", contentRoot) +
                VDF_TryGetKV(indent, "BuildOutput") +
                VDF_TryGetKV(indent, "verbose") +
                VDF_KeyObject(indent, "Depots", indent =>
                {
                    string output = "";
                    for (int i = 0, counti = chosen.depots.Length; i < counti; i++)
                    {
                        string depotId = chosen.depots[i];
                        output += VDF_KeyValue(indent, depotId, DepotFilePrefix + depotId + ".vdf");
                    }
                    return output;
                })
            );

            // Write app build script file
            File.WriteAllText(appFilePath, appBuild);

            // Depot files creation
            // TODO: Path exclusions etc.
            for (int i = 0, counti = chosen.depots.Length; i < counti; i++)
            {
                string depotId = chosen.depots[i];
                string depotPath = DepotFilePrefix + depotId + ".vdf";

                string depot = VDF_KeyObject(0, "DepotBuild", indent =>
                    VDF_KeyValue(indent, "DepotID", depotId) +
                    VDF_KeyObject(indent, "FileMapping", indent =>
                        VDF_KeyValue(indent, "LocalPath", "./*") +
                        VDF_KeyValue(indent, "DepotPath", ".") +
                        VDF_KeyValue(indent, "Recursive", "1")
                    )
                );

                string depotFilePath = System.IO.Path.Join(appBuildPath, DepotFilePrefix + depotId + ".vdf");

                // Write depot build script file
                File.WriteAllText(depotFilePath, depot);
            }
            Log.Debug("Output VDF scripts at {0}", appBuildPath);

            // Now run Steampipe upload
            string steamCMD = TryGetPref(KeyPrefixAppSteamworks + "SDK");
            if (!string.IsNullOrEmpty(steamCMD))
            {
                // Get Steam account login details from the user
                string args = $"+login {account} {password} +run_app_build {appFilePath} +quit";

                string exec = "";
                steamCMD = Path.GetFullPath(steamCMD);
                steamCMD = Path.Join(steamCMD, "tools/ContentBuilder/");
#if UNITY_STANDALONE_LINUX
                exec = "/bin/bash";
                steamCMD = Path.Join(steamCMD, "builder_linux/steamcmd.sh");
#elif UNITY_STANDALONE_WINDOWS
                steamCMD = Path.Join(steamCMD, "builder/steamcmd.exe");
#else
                exec = "/bin/bash";
                steamCMD = Path.Join(steamCMD, "builder_osx/steamcmd.sh");
#endif
                if (File.Exists(steamCMD))
                {
                    Log.Debug("Starting SteamCMD at \"{0}\"", steamCMD);
                    if (!string.IsNullOrEmpty(exec))
                    {
                        // Run shell process
                        Process process = new Process()
                        {
                            StartInfo =
                            {
                                FileName = "/bin/bash",
                                Arguments = "-c \"" + steamCMD + " " + args + "\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true, RedirectStandardError = true
                            },
                            EnableRaisingEvents = true
                        };

                        EditorUtility.DisplayProgressBar("Steamworks", "Initialising Steam CMD", 0f);
                        Task<int> task = RunProcessAsync(process);
                        while (task.Status == TaskStatus.Running || task.Status == TaskStatus.WaitingToRun || task.Status == TaskStatus.WaitingForChildrenToComplete || task.Status == TaskStatus.WaitingForActivation)
                        {
                            if (EditorUtility.DisplayCancelableProgressBar("Steam CMD", "Steampipe (Remember to check Steam Guard)", 0.5f))
                            {
                                process.Kill();
                                break;
                            }
                            else
                            {
                                await Task.WhenAny(task, Task.Delay(TimeSpan.FromMilliseconds(200)));
                            }
                        }

                        if (task.Result != 0)
                        {
                            Log.Error("Task ended with non-zero exit code {0}", task.Result);
                        }
                        else if (task.Exception != null)
                        {
                            Log.Exception(task.Exception);
                        }

                        EditorUtility.ClearProgressBar();
                    }
                    else
                    {
                        Process.Start(steamCMD, args);
                    }
                }
                else
                {
                    Log.Error("Failed to locate Steam CMD at \"{0}\"", steamCMD);
                }
            }
            else
            {
                Log.Error("You must specify the path to your Steamworks SDK tools/ContentBuilder directory, as well as your Steam developer account login and password.");
            }

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
                config = EditorConfig.Instance;
            }

            // OpenGET configuration settings reference
            serialiser = new SerializedObject(this);
            SerializedProperty prop = serialiser.FindProperty("config");
            fieldConfig = new UnityEditor.UIElements.PropertyField(prop);
            UnityEditor.UIElements.BindingExtensions.Bind(fieldConfig, serialiser);
            root.Add(fieldConfig);

            // Display settings
            SerializedObject obj = new SerializedObject(config);
            prop = obj.FindProperty("buildUploader");
            UnityEditor.UIElements.PropertyField addProp = new UnityEditor.UIElements.PropertyField(prop);
            UnityEditor.UIElements.BindingExtensions.Bind(addProp, obj);
            root.Add(addProp);

            // Steamworks uploads
            root.Add(new Label("\n<b>Steam Login</b>"));

            TextField enterAccount = new TextField("Steam Dev Username");
            enterAccount.RegisterValueChangedCallback(x => account = x.newValue);
            root.Add(enterAccount);

            TextField enterPass = new TextField("Steam Dev Password", 256, false, true, '*');
            enterPass.RegisterValueChangedCallback(x => password = x.newValue);
            root.Add(enterPass);

            root.Add(new Label("\n<b>Steamworks Build</b>"));

            // App builder script details
            TextField desc = SteamAppField("Desc", "Internal build/upload description", "");
            desc.multiline = true;
            root.Add(desc);

            root.Add(SteamAppField("Preview", "0 = Standard upload, 1 = Preview build only, nothing is uploaded", 0));
            root.Add(SteamAppField("SetLive", "Branch to set this build live on", ""));
            // ContentRoot is set by user on build/upload
            root.Add(SteamAppField("SDK", "Path to your local Steamworks SDK root folder.", ""));
            root.Add(SteamAppField("CWD", "SteamCMD working directory where the Steampipe build scripts will be stored.", ""));
            root.Add(SteamAppField("BuildOutput", "Where the Steamworks build/upload cache and log files will go.", ""));
            root.Add(SteamAppField("verbose", "How much logging detail you want in the Steamworks build/upload process.", 0));
            root.Add(SteamAppField("config", "Which configuration to use, matching on id.", ""));

            Button button = new Button(() => RunSteampipeUpload())
            {
                name = "Upload to Steam",
                text = "Upload build to Steam"
            };
            root.Add(button);
        }
    }

}
