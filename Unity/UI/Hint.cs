using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenGET;

namespace OpenGET.UI
{

    /// <summary>
    /// Hints such as a pointer or highlight intended to guide the player to look towards a position or direction.
    /// </summary>
    public abstract class Hint : AutoBehaviour
    {

        /// <summary>
        /// Custom parameters for the pointing/highlight.
        /// </summary>
        public abstract class Parameters
        {
            /// <summary>
            /// Camera used for world-to-screen space positioning.
            /// </summary>
            public Camera camera = null;
        }

        /// <summary>
        /// Target transform for the hint to follow.
        /// </summary>
        public Transform target { get; protected set; }

        /// <summary>
        /// Origin transform for the hint to follow, if any.
        /// </summary>
        public Transform origin { get; protected set; }

        /// <summary>
        /// Associated parameters.
        /// </summary>
        public Parameters hintData { get; private set; }

        /// <summary>
        /// Is the transform actually a rect transform (i.e. in screen space)?
        /// </summary>
        protected bool isTargetScreen => target is RectTransform;

        public abstract void SetHintAt(Vector2 screenPos, Parameters parameters);

        public abstract void SetHintAt(Vector3 worldPos, Parameters parameters);

        public virtual void SetHintTarget(RectTransform screenTarget, Parameters parameters, Transform origin = null)
        {
            target = screenTarget;
            hintData = parameters;
            this.origin = origin;
        }

        public virtual void SetHintTarget(Transform worldTarget, Parameters parameters, Transform origin = null)
        {
            target = worldTarget;
            hintData = parameters;
            this.origin = origin;
        }

        /// <summary>
        /// Set hint data, handling null data.
        /// </summary>
        protected virtual void SetHintData<T>(T parameters) where T : Parameters, new()
        {
            if (parameters == null)
            {
                parameters = new T();
            }
            hintData = parameters;
        }

        protected virtual void Update()
        {
            if (origin != null)
            {
                transform.position = origin.position;
            }

            if (target != null)
            {
                if (isTargetScreen)
                {
                    SetHintAt((Vector2)target.position, hintData);
                }
                else
                {
                    SetHintAt(target.position, hintData);
                }
            }
        }

    }

}
