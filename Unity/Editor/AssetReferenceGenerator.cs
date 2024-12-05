using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using Unity.VisualScripting;
using JetBrains.Annotations;

namespace OpenGET
{

    /// <summary>
    /// Generates references to Referrables.
    /// </summary>
    public class AssetReferenceGenerator
    {

        private class Node
        {
            public string name = "";
            public string generated_start = "";
            public string generated_end = "";
            public int depth = 0;
            public List<Node> children = new List<Node>();
        }

        /// <summary>
        /// Generate Ref class using Referrable assets.
        /// </summary>
        [MenuItem("OpenGET/Generate Asset Ref")]
        public static void Generate()
        {
            // Find the template asset.
            string[] found = AssetDatabase.FindAssets("_TemplateRef_ a:all t:TextAsset");
            string path = "";
            for (int i = 0, counti = found.Length; i < counti; i++)
            {
                path = Application.dataPath + AssetDatabase.GUIDToAssetPath(found[i]).Substring("Assets".Length);
                if (path.EndsWith("/_TemplateRef_.txt") || path.EndsWith("\\_TemplateRef_.txt"))
                {
                    break;
                }
                path = "";
            }

            if (string.IsNullOrEmpty(path))
            {
                Log.Error("Failed to locate template file \"_TemplateRef_.txt\"! Cannot generate code.");
                return;
            }

            // Read the .cs template file and get the insertion point
            string content = File.ReadAllText(path);
            content = content.Replace("public static class _TemplateRef_", "public static class Ref");
            string marker = "#region __GENERATED_CLASSES__";

            int insert = content.IndexOf(marker);
            if (insert < 0)
            {
                Log.Error("_TemplateRef_.txt found at {0} is invalid! Must have a marker matching \"{1}\".", path, marker);
                return;
            }
            insert += marker.Length;

            // Now find all Referrables and generate classes for them. Sorted alphanumerically.
            List<Node> map = new List<Node>();
            string alphaNumeric = "_" + (new string(Enumerable.Range(
                48, 10
            ).Concat(
                Enumerable.Range(65, 26)
            ).Concat(
                Enumerable.Range(97, 26)
            ).Select(x => (char)x).ToArray()));

            int refCount = 0;
            found = AssetDatabase.FindAssets("t:Object a:all glob:\"**/Resources/**\"");
            for (int i = 0, counti = found.Length; i < counti; i++)
            {
                path = AssetDatabase.GUIDToAssetPath(found[i]);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (obj is not IReferrable || (obj is GameObject && (obj = AssetDatabase.LoadAssetAtPath<Behaviour>(path)) is not IReferrable))
                {
                    continue;
                }
                path = path.Substring(path.ToLowerInvariant().IndexOf("resources/") + "resources/".Length);
                Log.Debug("Found IReferrable at path \"{0}\"", path);

                // Normalise a class or variable name
                string Normalise(string name, bool isAsset = false)
                {
                    string normalised = "";
                    name = isAsset && name.Contains('.') ? name.Remove(name.LastIndexOf('.')) : name;
                    for (int k = 0, countk = name.Length; k < countk; k++)
                    {
                        normalised += alphaNumeric.Contains(name[k]) ? name[k] : "_";
                    }
                    return normalised;
                }

                // Generate a class node
                Node GenerateClassNode(List<Node> target, int index, string[] parts)
                {
                    // Generate class node
                    string baseTabs = (new string('\t', index + 1));
                    string innerTabs = baseTabs + "\t";

                    string gen_start = "\n\n" + baseTabs + "public static class " + Normalise(parts[index]) + "\n" + baseTabs + "{\n";
                    string gen_end = "\n\n" + baseTabs + "}\n";
                    return new Node { name = parts[index], depth = index, generated_start = gen_start, generated_end = gen_end };
                }

                // Get or create the root node
                string[] parts = path.Split('/');
                Node current = map.FirstOrDefault(x => x.name == parts[0]);
                if (current == null)
                {
                    current = parts.Length > 1 ? new Node { name = parts[0] } : GenerateClassNode(map, 0, parts);
                    map.Add(current);
                }

                // Step through and create nodes apart from root and leaf
                for (int index = 1, totalParts = parts.Length; index < totalParts; index++)
                {
                    Node child = current.children.Find(x => x.name == parts[index]);
                    if (child == null)
                    {
                        child = GenerateClassNode(current.children, index, parts);
                        current.children.Add(child);
                    }
                    current = child;
                }

                // Generate leaf
                string newline = "\n" + new string('\t', current.depth + 1);
                current.generated_start = newline + newline + "/// <summary>" + newline + $"/// {obj.GetType()}" + newline + "/// </summary>" + newline + "public const string " + Normalise(parts[parts.Length - 1], true)
                    + " = @\"" + path + "\";";
                current.generated_end = "";

                refCount++;
            }

            Log.Debug("Found {0} valid IReferrable assets out of {1} searched, generating Ref.cs...", refCount, found.Length);

            // Now generate the class file
            string generated = "";
            void Generate(Node node) {
                generated += node.generated_start;
                for (int i = 0, counti = node.children.Count; i < counti; i++)
                {
                    Generate(node.children[i]);
                }
                generated += node.generated_end;
            }

            for (int i = 0, counti = map.Count; i < counti; i++)
            {
                Generate(map[i]);
            }
            content = content.Insert(insert, generated);

            // TODO: Save to file
            File.WriteAllText(Application.dataPath + "/Scripts/Ref.cs", content, System.Text.Encoding.UTF8);

            AssetDatabase.Refresh();

            Log.Debug("Successfully generated Ref.cs");

        }

    }

}
