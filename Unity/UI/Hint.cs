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
        public class Parameters
        {
            /// <summary>
            /// Camera used for world-to-screen space positioning.
            /// </summary>
            public Camera camera = null;

            /// <summary>
            /// Target world position, if given.
            /// </summary>
            public Vector3? targetWorldPosition = null;
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
                    SetHintAt((Vector2)(target as RectTransform).GetTrueBounds().center, hintData);
                }
                else
                {
                    SetHintAt(target.position, hintData);
                }
            }
            else if (hintData != null && hintData.targetWorldPosition.HasValue) {
                SetHintAt(hintData.targetWorldPosition.Value, hintData);
                Log.Debug("Set target world pos to {0}", hintData.targetWorldPosition.Value);
            }
        }

        /// <summary>
        /// Convenience method to create a hint for a world target.
        /// </summary>
        public static T Create<T, ParamsType>(T prefab, Transform worldTarget = null, Camera cam = null, Transform root = null, Transform origin = null)
            where T : Hint where ParamsType : Parameters, new()
        {
            T created = root != null ? Instantiate(prefab, root) : Instantiate(prefab);
            if (worldTarget != null)
            {
                ParamsType args = new ParamsType();
                if (cam == null)
                {
                    cam = Camera.main;
                }
                args.camera = cam;
                created.SetHintTarget(worldTarget, args, origin);
            }
            return created;
        }

        /// <summary>
        /// Convenience method to create a hint for a world target.
        /// </summary>
        public static T Create<T>(T prefab, Transform worldTarget = null, Camera cam = null, Transform root = null, Transform origin = null) where T : Hint
        {
            return Create<T, Parameters>(prefab, worldTarget, cam, root, origin);
        }

        /// <summary>
        /// Convenience method to create a hint for a screen target.
        /// </summary>
        public static T Create<T, ParamsType>(T prefab, RectTransform screenTarget = null, Transform root = null, ParamsType args = null, Transform origin = null)
            where T : Hint where ParamsType : Parameters, new()
        {
            T created = root != null ? Instantiate(prefab, root) : Instantiate(prefab);
            if (screenTarget != null)
            {
                created.SetHintTarget(screenTarget, args, origin);
            }
            return created;
        }

        /// <summary>
        /// Create a new hint (optionally setup with a target using pre-initialised parameters).
        /// </summary>
        public static T Create<T, ParamsType>(T prefab, Transform target = null, Transform root = null, ParamsType args = null, Transform origin = null) 
            where T : Hint where ParamsType : Parameters, new()
        {
            T created = root != null ? Instantiate(prefab, root) : Instantiate(prefab);
            if (target != null && args != null)
            {
                if (target is RectTransform)
                {
                    created.SetHintTarget(target as RectTransform, args, origin);
                }
                else
                {
                    created.SetHintTarget(target, args, origin);
                }
            }
            else if (args != null)
            {
                created.SetHintData(args);
            }
            return created;
        }

    }

}
