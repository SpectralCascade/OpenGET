using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET.UI
{

    public class AccessUI : AutoBehaviour
    {

        [SerializeField]
        [Auto.NullCheck]
        protected UIController _UI = null;

        public UIController UI => _UI;

        /// <summary>
        /// Convenience getter that can do the cast implicitly for you.
        /// </summary>
        public T GetUI<T>() where T : UIController { return (T)_UI; }

#if UNITY_EDITOR
        [ContextMenu("Assign UI")]
        private void AutoAssignUI()
        {
            if (_UI == null)
            {
                Debug.Log("Auto assigning UI to " + gameObject.name + "...");
                _UI = FindObjectOfType<UIController>();
                Debug.Assert(_UI != null, "Failed to find and auto-assign UI to " + gameObject.name, gameObject);
                if (_UI != null)
                {
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
#endif

    }

}
