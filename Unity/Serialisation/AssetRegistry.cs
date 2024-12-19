using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Contains a list of RegisterPrefab & RegistryData assets in the project, the order of which never changes.
    /// The index number of assets in this registry can be used to load prefabs identified in serialised data,
    /// independent of Unity's GUIDs (which can change between Unity versions).
    /// </summary>
    [CreateAssetMenu(fileName = "AssetRegistry", menuName = "OpenGET/AssetRegistry")]
    public class AssetRegistry : ScriptableObject
    {
        private static AssetRegistry Instance => Application.isPlaying ? _instance : _instance = null;
        private static AssetRegistry _instance;

        /// <summary>
        /// Fast-access map of objects to ids.
        /// </summary>
        private Dictionary<Object, int> map => _map ??= new Dictionary<Object, int>(assets.Select((asset, i) => new KeyValuePair<Object, int>(asset, i)));
        private Dictionary<Object, int> _map = null;

        /// <summary>
        /// List of assets, with ids generated from their index. The order never changes, and if an asset is removed, the entry is made null instead of being removed.
        /// </summary>
        [OpenGET.ReadonlyField]
        public Object[] assets = new Object[0];

        /// <summary>
        /// Get the id of a given registered asset.
        /// </summary>
        public static int GetId(Object asset)
        {
            return Instance.map.TryGetValue(asset, out int id) ? id : -1;
        }

        /// <summary>
        /// Get registered asset by id.
        /// </summary>
        public static Object GetObject(int id)
        {
            return id >= 0 && id < Instance.assets.Length ? Instance.assets[id] : null;
        }

        /// <summary>
        /// Get registered asset by id, casted to the specified type.
        /// </summary>
        public static T GetObject<T>(int id) where T : Object
        {
            return GetObject(id) as T;
        }

        /// <summary>
        /// Initialise the singleton instance.
        /// </summary>
        public static void Init(AssetRegistry registry)
        {
            _instance = registry;
        }

#if UNITY_EDITOR
        [MenuItem("OpenGET/Build Asset Registry")]
        public static void BuildAssetList()
        {
            AssetRegistry registry = null;
            AssetDatabase.FindAssets("t:AssetRegistry").Select(
                x => AssetDatabase.GUIDToAssetPath(x)
            ).FirstOrDefault(
                x => (registry = AssetDatabase.LoadAssetAtPath<AssetRegistry>(x)) != null
            );

            if (registry != null)
            {
                Log.Debug("Found AssetRegistry, building assets...");
            }
            else
            {
                Log.Error("No AssetRegistry instance found! You must create an AssetRegistry asset before you can build the asset list.");
                return;
            }

            int registered = registry.assets.Length;
            RegistryData[] dataAssets = AssetDatabase.FindAssets("t:RegistryData").Select(
                x => AssetDatabase.LoadAssetAtPath<RegistryData>(AssetDatabase.GUIDToAssetPath(x))
            ).ToArray();
            
            PersistentIdentity[] prefabs = AssetDatabase.FindAssets("t:Prefab").Select(
                x => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(x)).GetComponent<RegisterPrefab>()
            ).Where(x => x != null).ToArray();

            List<IRegistrate> found = new List<IRegistrate>(dataAssets);
            found.AddRange(prefabs);

            Log.Debug("Checking {0} assets for registration...", found.Count);
            List<Object> updated = new List<Object>(registry.assets);
            for (int i = 0, counti = found.Count; i < counti; i++)
            {
                if (found[i].PersistentId < 0 || found[i].PersistentId >= registry.assets.Length)
                {
                    int index = System.Array.IndexOf(registry.assets, found[i]);
                    if (index < 0)
                    {
                        if (!updated.Contains(found[i] as Object))
                        {
                            // Add to registry
                            index = updated.Count;
                            updated.Add(found[i] as Object);
                            Log.Debug("Adding new asset \"{0}\" to registry.", updated[i].name);
                        }
                        else
                        {
                            Log.Warning("Duplicate object found in assets, somehow. Skipping so it is not added to the registry.");
                        }
                    }
                    else
                    {
                        // Already exists! For some reason not setup with an id
                        Log.Warning("Found asset \"{0}\" in registry but with an invalid id. Updating id to match registry index {1}.", (found[i] as Object).name, index);
                    }
                    found[i].PersistentId = index;
                    EditorUtility.SetDirty(found[i] as Object);
                }
                else if (registry.assets[found[i].PersistentId] == null)
                {
                    // In case an asset gets unhooked accidentally, the id should be backed up in the asset itself so re-registration is simple
                    updated[found[i].PersistentId] = found[i] as Object;
                }

                if (found[i] is RegisterPrefab)
                {
                    (found[i] as RegisterPrefab).RegisterIds();
                }
            }

            registry.assets = updated.ToArray();
            EditorUtility.SetDirty(registry);

            AssetDatabase.SaveAssets();
        }
#endif

    }

    internal interface IRegistrate
    {
        public int PersistentId {
            get;
#if UNITY_EDITOR
            set;
#endif
        }
    }

    /// <summary>
    /// Inherit from this instead of ScriptableObject for registry access.
    /// </summary>
    public abstract class RegistryData : ScriptableObject, IRegistrate
    {
        /// <summary>
        /// Get the registry identifier.
        /// </summary>
        public int PersistentId {
            get { return _registry_id; }
#if UNITY_EDITOR
            set { _registry_id = value; }
#endif
        }

        [HideInInspector]
        [SerializeField]
        private int _registry_id = -1;
    }

}
