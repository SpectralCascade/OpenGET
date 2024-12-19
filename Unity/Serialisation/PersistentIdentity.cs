using OpenGET;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Provides a way to store persistent integer ids on objects that never change between serialisation or Unity versions.
    /// </summary>
    public abstract class PersistentIdentity : AutoBehaviour, IRegistrate, BaseSerialiser.IReferenceSerialise
    {
        /// <summary>
        /// Get the persistent identifier.
        /// </summary>
        public int PersistentId {
            get { return _PersistentId; }
#if UNITY_EDITOR
            set { _PersistentId = value; }
#endif
        }

        /// <summary>
        /// Get the prefab id associated with this identity. Either this will be itself, or the parent prefab.
        /// </summary>
        public int PrefabId => !isPrefab ? parentPrefab.PersistentId : PersistentId;

        /// <summary>
        /// Get the unique id of this instance (combination of prefab, persistant and reference ids).
        /// </summary>
        public string InstanceId => PrefabId + "." + PersistentId + "." + reference.id;

        /// <summary>
        /// Persistent id.
        /// </summary>
        [OpenGET.ReadonlyField]
        [SerializeField]
        private int _PersistentId = -1;

        /// <summary>
        /// Is this id for a prefab, or a child object of a prefab?
        /// </summary>
        public virtual bool isPrefab => false;

        /// <summary>
        /// Reference for instance id generation, only persistent for serialisation.
        /// </summary>
        public BaseSerialiser.Reference reference => _reference ??= new BaseSerialiser.Reference(GenerateReference);
        private BaseSerialiser.Reference _reference = null;

        /// <summary>
        /// Associated prefab identity, if any. Not relevant if this is a prefab.
        /// </summary>
        [OpenGET.ReadonlyField]
        [SerializeField]
        private PersistentIdentity parentPrefab = null;

        /// <summary>
        /// Child identities, only relevant if this is a prefab. None of the child objects can be a prefab.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private List<PersistentIdentity> children = new List<PersistentIdentity>();

        /// <summary>
        /// Child prefab identities.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private List<PersistentIdentity> childPrefabs = new List<PersistentIdentity>();

        public string GenerateReference()
        {
            return GetInstanceID().ToString();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Generate relative persistent ids for child objects. You must ONLY do this for prefabs.
        /// </summary>
        [ContextMenu("Generate Persistent Ids")]
        public void RegisterIds()
        {
            // Safety checks. Make sure this actually is a prefab and not a random gameobject.
            if (!isPrefab)
            {
                Log.Warning("This object is not a prefab! Cannot generate persistent ids.");
                return;
            }

            Log.Debug("Setting up persistent ids on object \"{0}\"", name);
            Setup(transform);
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Careful recursive generation of persistent ids.
        /// </summary>
        private void Setup(Transform t)
        {
            PersistentIdentity[] idents = t.GetComponents<PersistentIdentity>();

            if (idents.Length == 0)
            {
                // No PersistentIdentity component, time to go deeper
                for (int i = 0, counti = t.childCount; i < counti; i++)
                {
                    Setup(t.GetChild(i));
                }
                return;
            }

            // If this transform is a child prefab, all the other idents should not be touched
            PersistentIdentity childPrefab = idents.FirstOrDefault(x => x.isPrefab);
            if (childPrefab != null && childPrefab != this)
            {
                // Child prefabs are tracked but not recursed
                childPrefabs.Add(childPrefab);
                return;
            }

            //Log.Debug("Checking {0} idents...", idents.Length);
            for (int identIndex = 0, numIdents = idents.Length; identIndex < numIdents; identIndex++)
            {
                PersistentIdentity ident = idents[identIndex];
                if (!ident.isPrefab)
                {
                    // Handle child identities
                    if (ident.PersistentId >= 0 && ident.PersistentId < children.Count && children[ident.PersistentId] == ident)
                    {
                        // Assigned and valid, update prefab
                        ident.parentPrefab = this;
                    }
                    else if (ident.PersistentId < 0)
                    {
                        // Never been assigned
                        ident.PersistentId = children.Count;
                        children.Add(ident);
                        ident.parentPrefab = this;
                    }
                    else
                    {
                        // Invalid id!
                        Log.Error("Mis-match detected on prefab \"{0}\" (total children: {1}), found child with invalid id [{2}].", name, children.Count, ident.PersistentId);
                        return;
                    }
                    EditorUtility.SetDirty(ident);
                }
                else if (ident == this)
                {
                    // Skip self
                    continue;
                }
                else
                {
                    Log.Warning("Detected additional prefab identity on object \"{0}\". You must not have more than one prefab identity per GameObject.", t.name);
                    return;
                }
            }

            // Now recurse children
            for (int i = 0, counti = t.childCount; i < counti; i++)
            {
                Setup(t.GetChild(i));
            }
        }
#endif

    }

}
