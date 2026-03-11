using System.Collections;
using System.Collections.Generic;
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
                int v = EditorPrefs.HasKey(fullKey) ? EditorPrefs.GetInt(fullKey) : (int)(object)valDefault;
                field = new IntegerField()
                {
                    label = key,
                    tooltip = desc,
                    value = v
                } as BaseField<T>;
                field.RegisterCallback<ChangeEvent<int>>(
                    (change) => EditorPrefs.SetInt(fullKey, change.newValue)
                );
            }
            else if (valDefault is float)
            {
                float v = EditorPrefs.HasKey(fullKey) ? EditorPrefs.GetFloat(fullKey) : (float)(object)valDefault;
                field = new FloatField()
                {
                    label = key,
                    tooltip = desc,
                    value = v
                } as BaseField<T>;
                field.RegisterCallback<ChangeEvent<float>>(
                    (change) => EditorPrefs.SetFloat(fullKey, change.newValue)
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
        }

        private void OnInspectorUpdate()
        {
            if (config == null)
            {
                config = EditorConfig.Instance;
            }
        }

        /// <summary>
        /// Uploads a build to Steamworks.
        /// </summary>
        private void RunSteampipeUpload()
        {
            string contentRoot = EditorUtility.OpenFolderPanel("Select Build Content Directory", Application.dataPath, "");
            if (string.IsNullOrEmpty(contentRoot))
            {
                // Cancelled
                return;
            }

            // TODO: Generate .vdf files for chosen configuration
            string appBuild = "";
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
            root.Add(new Label("\n<b>Platform: Steamworks</b>"));

            TextField desc = SteamAppField("Desc", "Internal build/upload description", "");
            desc.multiline = true;
            root.Add(desc);

            root.Add(SteamAppField("Preview", "0 = Standard upload, 1 = Preview build only, nothing is uploaded", 0));
            root.Add(SteamAppField("SetLive", "Branch to set this build live on", ""));
            // ContentRoot is set by user on build/upload
            root.Add(SteamAppField("BuildOutput", "Where the Steamworks build/upload cache and log files will go.", ""));
            root.Add(SteamAppField("verbose", "How much logging detail you want in the Steamworks build/upload process.", 0));
            root.Add(SteamAppField("Config id", "Which configuration to use, matching on id.", ""));

            Button button = new Button(() => RunSteampipeUpload())
            {
                name = "Upload to Steam",
                text = "Upload build to Steam"
            };
            root.Add(button);
        }
    }

}
