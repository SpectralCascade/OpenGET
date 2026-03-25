using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI {

    /// <summary>
    /// Follows a worldspace or screenspace transform.
    /// </summary>
    public class Follower : AutoBehaviour
    {

        /// <summary>
        /// Target transform to follow, if any.
        /// </summary>
        public Transform target;

        /// <summary>
        /// UI to use for worldspace conversion.
        /// </summary>
        public UIController UI;

        /// <summary>
        /// Follow a given target in worldspace.
        /// </summary>
        public void Follow(Transform target, UIController UI)
        {
            this.target = target;
            this.UI = UI;
        }

        /// <summary>
        /// Follow a given target in screenspace.
        /// </summary>
        public void Follow(RectTransform target)
        {
            this.target = target;
            UI = null;
        }

        protected virtual void Update()
        {
            if (target != null)
            {
                if (UI != null)
                {
                    transform.position = UI.WorldToCanvasPoint(target.position);
                }
                else
                {
                    transform.position = target.position;
                }
            }
        }

    }

}
