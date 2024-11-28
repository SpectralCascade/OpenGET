using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OpenGET
{

    /// <summary>
    /// Contains user settings for OpenGET editor tools.
    /// </summary>
    [CreateAssetMenu(fileName = "ConfigOpenGET", menuName = "OpenGET/Editor Tools Config")]
    public class EditorConfig : ScriptableObject
    {
        [System.Serializable]
        public class Localisation
        {
            /// <summary>
            /// Functions to pattern match when extracting strings from code.
            /// </summary>
            [Header("Functions to pattern match when extracting strings from code.")]
            public LocalisationTool.Marker[] extractionMatches = new LocalisationTool.Marker[] {
                new LocalisationTool.Marker("Localise.Text"),
                new LocalisationTool.Marker("SettingsGroup", 2)
            };

            /// <summary>
            /// Paths that should be considered during strings extraction, relative to project assets directory.
            /// </summary>
            public string[] scriptIncludePaths = new string[0];

            /// <summary>
            /// Paths that should be considered during scenes extraction, relative to project assets directory.
            /// </summary>
            public string[] prefabIncludePaths = new string[0];

            /// <summary>
            /// Output path for the extracted strings CSV file.
            /// </summary>
            public string extractionOutputPath = "strings_export.csv";

            /// <summary>
            /// CSV file containing imported string localisations.
            /// </summary>
            public string importPath = "strings.csv";

        }

        [System.Serializable]
        public class PrefabToSprite
        {
            /// <summary>
            /// Callback to read and write assets.
            /// </summary>
            public EditorAssetBatcher assetLoader;

            /// <summary>
            /// Scene used to render the prefab(s) to sprites.
            /// </summary>
            public SceneAsset renderScene;

            public int spriteWidth = 256;
            public int spriteHeight = 256;

            /// <summary>
            /// Sprites output directory.
            /// </summary>
            public string outputFolder = "Sprites";

            /// <summary>
            /// Optional filename prefix for output sprites.
            /// </summary>
            public string outputFilenamePrefix = "";

        }

        /// <summary>
        /// Configure OpenGET logging in editor.
        /// </summary>
        [System.Serializable]
        public class Logging
        {
            /// <summary>
            /// Which logging levels are enabled in editor. Does not affect release builds.
            /// </summary>
            public Log.Level level = Log.Level.All;
        }

        /// <summary>
        /// Find and load (or create) an EditorConfig instance.
        /// </summary>
        public static EditorConfig Instance {
            get {
                string[] found = AssetDatabase.FindAssets("t:" + typeof(EditorConfig).Name);
                EditorConfig config = found.Length > 0 ?
                    AssetDatabase.LoadAssetAtPath<EditorConfig>(AssetDatabase.GUIDToAssetPath(found[0])) :
                    CreateInstance<EditorConfig>();
                EditorPrefs.SetInt("OpenGET/LogLevel", (int)config.logging.level);
                return config;
            }
        }

        private const string LogPrefix = "OpenGET/Log Level/";

#if UNITY_EDITOR
        private static void ToggleLog(Log.Level level)
        {
            EditorConfig config = Instance;
            if (level == Log.Level.All)
            {
                config.logging.level = (config.logging.level ^ Log.Level.All) != 0 ? Log.Level.All : ~Log.Level.All;
            }
            else
            {
                config.logging.level ^= level;
            }
            EditorPrefs.SetInt("OpenGET/LogLevel", (int)config.logging.level);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);
        }

        private static bool ValidateLog(Log.Level level)
        {
            EditorConfig config = Instance;
            Menu.SetChecked(
                LogPrefix + level.ToString(),
                (level ^ (config.logging.level & level)) == 0
            );
            return true;
        }

        [MenuItem(LogPrefix + "All", true)]
        public static bool ValidateLogAll()
        {
            return ValidateLog(Log.Level.All);
        }

        [MenuItem(LogPrefix + "All", priority = 1)]
        public static void ToggleLogAll()
        {
            ToggleLog(Log.Level.All);
        }

        [MenuItem(LogPrefix + "Verbose", true)]
        public static bool ValidateLogVerbose()
        {
            return ValidateLog(Log.Level.Verbose);
        }

        [MenuItem(LogPrefix + "Verbose", priority = 2)]
        public static void ToggleLogVerbose()
        {
            ToggleLog(Log.Level.Verbose);
        }

        [MenuItem(LogPrefix + "Debug", true)]
        public static bool ValidateLogDebug()
        {
            return ValidateLog(Log.Level.Debug);
        }

        [MenuItem(LogPrefix + "Debug", priority = 3)]
        public static void ToggleLogDebug()
        {
            ToggleLog(Log.Level.Debug);
        }

        [MenuItem(LogPrefix + "Info", true)]
        public static bool ValidateLogInfo()
        {
            return ValidateLog(Log.Level.Info);
        }

        [MenuItem(LogPrefix + "Info", priority = 4)]
        public static void ToggleLogInfo()
        {
            ToggleLog(Log.Level.Info);
        }

        [MenuItem(LogPrefix + "Warning", true)]
        public static bool ValidateLogWarning()
        {
            return ValidateLog(Log.Level.Warning);
        }

        [MenuItem(LogPrefix + "Warning", priority = 5)]
        public static void ToggleLogWarning()
        {
            ToggleLog(Log.Level.Warning);
        }

        [MenuItem(LogPrefix + "Error", true)]
        public static bool ValidateLogError()
        {
            return ValidateLog(Log.Level.Error);
        }

        [MenuItem(LogPrefix + "Error", priority = 6)]
        public static void ToggleLogError()
        {
            ToggleLog(Log.Level.Error);
        }
#endif

        /// <summary>
        /// Log settings.
        /// </summary>
        public Logging logging;

        /// <summary>
        /// Editor localisation settings.
        /// </summary>
        public Localisation localisation;

        /// <summary>
        /// Tooling data for rendering a prefab to a 2D sprite.
        /// </summary>
        public PrefabToSprite prefabToSprite;

    }

}
