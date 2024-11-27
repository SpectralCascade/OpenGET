using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET.UI
{

    /// <summary>
    /// Represents a group of tabs.
    /// </summary>
    public class TabGroup : AutoBehaviour
    {

        /// <summary>
        /// All associated tabs.
        /// </summary>
        [SerializeField]
        [Auto.Hookup(Auto.Mode.Children)]
        protected Tab[] tabs = new Tab[0];

        /// <summary>
        /// Current active tab index.
        /// </summary>
        [SerializeField]
        [Min(0)]
        protected int index = 0;

        /// <summary>
        /// Getter for current index.
        /// </summary>
        public int currentIndex => index;

        /// <summary>
        /// Get the current active tab, if any.
        /// </summary>
        public Tab current => tabs.Length > 0 ? tabs[index] : null;

        protected void OnEnable()
        {
            if (current != null)
            {
                SwitchTo(current);
            }
        }

        /// <summary>
        /// Dynamically create a tab from a prefab.
        /// </summary>
        public Tab Add(Tab prefab)
        {
            int len = tabs.Length;
            System.Array.Resize(ref tabs, len + 1);
            Tab tab = Instantiate(prefab, transform);
            tabs[len] = tab;
            return tab;
        }

        /// <summary>
        /// Dynamically disassociate a tab.
        /// </summary>
        public bool Remove(Tab tab)
        {
            int found = System.Array.IndexOf(tabs, tab);
            if (found >= 0)
            {
                int count = tabs.Length - 1;
                tabs[found] = tabs[count];
                System.Array.Resize(ref tabs, count);
                Destroy(tab.gameObject);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Destroy all tabs.
        /// </summary>
        public void Clear()
        {
            for (int i = 0, counti = tabs.Length; i < counti; i++)
            {
                Destroy(tabs[i].gameObject);
            }
            tabs = new Tab[0];
        }

        /// <summary>
        /// Switch to a different tab.
        /// </summary>
        public void SwitchTo(Tab tab)
        {
            int found = System.Array.FindIndex(tabs, x => x == tab);
            if (found >= 0)
            {
                current.OnSwitch(false);
                index = found;
                current.OnSwitch(true);
            }
        }

    }

}
