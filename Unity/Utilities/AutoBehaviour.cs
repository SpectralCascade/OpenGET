using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Utility behaviour to use instead of MonoBehaviour for convenience.
    /// Automatically processes Auto-attributes.
    /// </summary>
    public class AutoBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            Auto.NullCheck(this, true, true);
        }

        protected virtual void OnValidate()
        {
            Auto.Hookup(gameObject);
        }

        [ContextMenu("OpenGET Auto-hookup")]
        private void AutoHookup()
        {
            Auto.Hookup(this, true, true);
        }

    }

}
