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
        /// Camera to use for worldspace conversion.
        /// </summary>
        public Camera cam;

        /// <summary>
        /// Only required if you use one on the canvas.
        /// </summary>
        public CanvasScaler scaler;

        /// <summary>
        /// Follow a given target in worldspace.
        /// </summary>
        public void Follow(Transform target, Camera cam, CanvasScaler scaler = null)
        {
            this.target = target;
            this.cam = cam;
            this.scaler = scaler;
        }

        /// <summary>
        /// Follow a given target in screenspace.
        /// </summary>
        public void Follow(RectTransform target)
        {
            this.target = target;
            cam = null;
            scaler = null;
        }

        protected virtual void Update()
        {
            if (target != null)
            {
                if (cam != null)
                {
                    transform.position = cam.WorldToScreenPoint(target.position) * (scaler != null ? scaler.scaleFactor : 1);
                }
                else
                {
                    transform.position = target.position;
                }
            }
        }

    }

}
