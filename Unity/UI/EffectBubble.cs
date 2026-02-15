using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET.UI
{

    /// <summary>
    /// Bubble effect that can be used for comic book effects, with or without text.
    /// </summary>
    public class EffectBubble : AutoBehaviour, IReferrable
    {
        //[Tooltip("Optional effect bubble text component(s).")]
        private TMPro.TextMeshProUGUI[] text = new TMPro.TextMeshProUGUI[0];

        /// <summary>
        /// Target to follow every update, if any.
        /// </summary>
        private Transform target;

        /// <summary>
        /// Associated world camera, required for worldspace conversion.
        /// </summary>
        private Camera worldCamera;

        /// <summary>
        /// Initialise the text bubble with a target to follow. If a RectTransform is provided, no world space conversion is performed.
        /// </summary>
        public EffectBubble Init(Transform followTarget, Camera worldCamera, string text = "")
        {
            if (this.text != null)
            {
                for (int i = 0, counti = this.text.Length; i < counti; i++)
                {
                    this.text[i].text = text;
                }
            }
            target = followTarget;
            this.worldCamera = worldCamera;
            return this;
        }

        /// <summary>
        /// Follow a UI transform target.
        /// </summary>
        public EffectBubble Init(RectTransform followTarget, string text = "")
        {
            return Init(followTarget, null, text);
        }

        /// <summary>
        /// Initialise the text bubble at a specified location, not following a target.
        /// </summary>
        public EffectBubble Init(Vector3 position, string text = "")
        {
            if (this.text != null)
            {
                for (int i = 0, counti = this.text.Length; i < counti; i++)
                {
                    this.text[i].text = text;
                }
            }
            transform.position = position;
            target = null;
            worldCamera = null;
            return this;
        }

        void Update()
        {
            if (target != null)
            {
                if (worldCamera == null || target is RectTransform)
                {
                    transform.position = target.position;
                }
                else
                {
                    transform.position = worldCamera.WorldToScreenPoint(target.position);
                }
            }
        }

        public static EffectBubble Spawn(EffectBubble prefab, Camera worldCamera, Transform root, Transform target, string text = "")
        {
            return Spawn(prefab, root).Init(target, worldCamera, text);
        }

        public static EffectBubble Spawn(EffectBubble prefab, Transform root, RectTransform target, string text = "")
        {
            return Spawn(prefab, root).Init(target, text);
        }

        public static EffectBubble Spawn(EffectBubble prefab, Transform root, Vector3 position, string text = "")
        {
            return Spawn(prefab, root).Init(position, text);
        }

        private static EffectBubble Spawn(EffectBubble prefab, Transform root)
        {
            EffectBubble instance = Instantiate(prefab, root);
            instance.transform.localPosition = Vector3.zero;
            return instance;
        }

    }

}
