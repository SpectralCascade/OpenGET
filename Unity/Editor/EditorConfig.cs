using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            public string extractionOutputPath = "OpenGET_Localisation_Export.csv";

            /// <summary>
            /// CSV file containing imported string localisations.
            /// </summary>
            public TextAsset importData;

        }

        /// <summary>
        /// Editor localisation settings.
        /// </summary>
        public Localisation localisation;

    }

}
