using OpenGET;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Add this component to your prefabs to register it.
    /// </summary>
    public sealed class RegisterPrefab : MonoBehaviour, IRegistrate
    {
        /// <summary>
        /// Get the registry identifier.
        /// </summary>
        public int RegistryId {
            get { return _RegistryId; }
#if UNITY_EDITOR
            set { _RegistryId = value; }
#endif
        }

        [OpenGET.ReadonlyField]
        [SerializeField]
        private int _RegistryId = -1;
    }

}
