using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpenGET
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
#if !VDF_PARSER
            Log.Error("You appear to be missing the VDF Parser package. Please install it from {0}", "https://github.com/SpectralCascade/UnityVDFParser.git");
            return;
#else
            string found = EditorUtility.OpenFilePanel("Select Steampipe Build Script", Application.dataPath, ".vdf");
            string buildScript = "";
            if (string.IsNullOrEmpty(found))
            {
                // Cancelled
                if (!System.IO.File.Exists(found))
                {
                    Log.Error("Invalid Steampipe build script.");
                }
                return;
            }
            buildScript = System.IO.File.ReadAllText(found);
#endif
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

            Button button = new Button(() => RunSteampipeUpload())
            {
                name = "Upload to Steam",
                text = "Upload build to Steam"
            };
            root.Add(button);

            // button = new Button(() =>
            // {
            //     importTable.Clear();
            //     UpdateInfoLabel();
            // });
            // button.name = "ClearImportData";
            // button.text = "Clear import data";
            // root.Add(button);

            // button = new Button(() =>
            // {
            //     exportTable.Clear();
            //     UpdateInfoLabel();
            // });
            // button.name = "ClearExportData";
            // button.text = "Clear export data";
            // root.Add(button);

            // infoLabel = new Label("");
            // UpdateInfoLabel();
            // root.Add(infoLabel);
        }
    }

}
