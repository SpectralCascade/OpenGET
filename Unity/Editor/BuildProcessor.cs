using System.Collections;
using System.Collections.Generic;
using OpenGET.Build;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OpenGET.Editor
{

    /// <summary>
    /// Implement this class to generate useful OpenGET data such as Ref and BuildInfo.
    /// </summary>
    public abstract class BuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public virtual void OnPreprocessBuild(BuildReport report)
        {
            if (GenerateBuildInfo)
            {
                // Create build info
                BuildInfo info = CreateBuildInfo();
                Log.Debug("Creating info for build: {0}", info.ToString());
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                if (!AssetDatabase.IsValidFolder("Assets/Resources/Build"))
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "Build");
                }
                AssetDatabase.CreateAsset(info, "Assets/Resources/Build/Info.asset");
                AssetDatabase.SaveAssets();
            }
            if (GenerateRef)
            {
                AssetReferenceGenerator.Generate();
            }
        }

        public abstract BuildInfo CreateBuildInfo();

        public virtual bool GenerateRef => true;

        public virtual bool GenerateBuildInfo => true;

    }

}
