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
            public bool isLeaf = false;
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
                int extensionIndex = path.LastIndexOf('.');
                if (extensionIndex >= 0)
                {
                    path = path.Remove(extensionIndex);
                }
                Log.Debug("Found IReferrable at path \"{0}\"", path);

                // Generate a class node
                Node GenerateClassNode(List<Node> target, int index, string[] parts)
                {
                    // Generate class node
                    string baseTabs = (new string('\t', index + 1));
                    string innerTabs = baseTabs + "\t";

                    string className = Normalise(parts[index]);

                    string gen_start = "\n\n" + baseTabs + "public static class " + className + "\n" + baseTabs + "{\n"
                        + innerTabs + "private static readonly Dictionary<string, WrapperBase> mapped = new Dictionary<string, WrapperBase>();\n\n"
                        + innerTabs + "public static Wrapper<T> Find<T>(string id) where T : UnityEngine.Object, OpenGET.IReferrable {\n"
                        + innerTabs + '\t' + "return (mapped.TryGetValue(id, out WrapperBase wrapper) && wrapper is Wrapper<T> ? wrapper : null) as Wrapper<T>;\n"
                        + innerTabs + "}\n";
                    string gen_end = "\n\n" + baseTabs + "}\n";
                    return new Node { name = parts[index], isLeaf = false, depth = index, generated_start = gen_start, generated_end = gen_end };
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
                current.generated_start = newline + newline + "/// <summary>" + newline + $"/// {obj.GetType().FullName}." + newline + "/// </summary>" 
                    + newline + $"public static readonly Wrapper<{obj.GetType().FullName}> " + Normalise(parts[parts.Length - 1], true)
                    + $" = new Wrapper<{obj.GetType().FullName}>(@\"{path}\");";
                current.generated_end = "";
                current.isLeaf = true;

                int rmExt = current.name.LastIndexOf('.');
                current.name = (rmExt >= 0 ? current.name.Remove(rmExt) : current.name);

                refCount++;
            }

            Log.Debug("Found {0} valid IReferrable assets out of {1} searched, generating Ref.cs...", refCount, found.Length);

            // Now generate the class file
            string generated = "";
            void Generate(Node node) {
                generated += node.generated_start;
                string genMap = "";
                for (int i = 0, counti = node.children.Count; i < counti; i++)
                {
                    string norm = Normalise(node.children[i].name);
                    int foundIndex = node.children[i].name.LastIndexOf('.');
                    string id = foundIndex < 0 ? node.children[i].name : node.children[i].name.Remove(foundIndex);
                    genMap += node.children[i].isLeaf ? "{ \"" + id + "\", " + norm + " }" + (i >= counti - 1 ? "" : ", ") : "";
                    Generate(node.children[i]);
                }

                if (!string.IsNullOrEmpty(genMap))
                {
                    node.generated_end = "\n\n" + (new string('\t', node.depth + 2)) + "static " + Normalise(node.name) + "() { mapped = new() { " + genMap + " }; }\n" + node.generated_end;
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
