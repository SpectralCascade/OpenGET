using System.Collections;
using System.Collections.Generic;
using OpenGET;
using UnityEditor;
using UnityEngine;

namespace OpenGET.Build
{

    /// <summary>
    /// Use ONLY for displaying build information.
    /// You may override this, but you should not mess with the internal data.
    /// </summary>
    public class BuildInfo : ScriptableObject, IReferrable
    {
        internal long timestamp => _timestamp;
        [SerializeField]
        [ReadonlyField]
        internal long _timestamp;

        [SerializeField]
        [ReadonlyField]
        internal string symbols;

        public override string ToString()
        {
            return new System.DateTime(timestamp).ToString("yyyy-MM-dd_HHmm").Replace("_", symbols);
        }

        public static BuildInfo Asset => cached != null ? cached : (cached = Resources.Load<BuildInfo>("Build/Info"));
        private static BuildInfo cached = null;

#if UNITY_EDITOR
        public virtual void Init(long timestamp, string symbols = "")
        {
            _timestamp = timestamp;
            this.symbols = symbols;
        }
#endif

    }

}
