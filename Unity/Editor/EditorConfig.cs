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
        /// Editor localisation settings.
        /// </summary>
        public Localisation localisation;

        /// <summary>
        /// Tooling data for rendering a prefab to a 2D sprite.
        /// </summary>
        public PrefabToSprite prefabToSprite;

    }

}
