using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenGET;

namespace OpenGET.UI
{

    public class PointerHint : Hint
    {
        public new class Parameters : Hint.Parameters
        {
            /// <summary>
            /// Colour of the pointer.
            /// </summary>
            public Color colour = Color.white;
        }

        /// <summary>
        /// The actual pointer rect.
        /// </summary>
        public RectTransform rectTransform => transform as RectTransform;

        public override void SetHintAt(Vector2 screenPos, Hint.Parameters parameters = null)
        {
            SetHintData(parameters as Parameters);
            Vector2 dir = (screenPos - (Vector2)transform.position);
            rectTransform.localScale = new Vector3(
                dir.magnitude / (rectTransform.rect.width * 0.5f),
                1,
                1
            );
            rectTransform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dir.normalized));
        }

        public override void SetHintAt(Vector3 worldPos, Hint.Parameters parameters)
        {
            SetHintData(parameters as Parameters);
            Parameters args = parameters as Parameters;
            if (args != null && args.camera != null)
            {
                SetHintAt((Vector2)args.camera.WorldToScreenPoint(worldPos), parameters);
            }
            else
            {
                Log.Error("Invalid parameters specified! Cannot set hint target in world-space without a camera.");
            }
        }

        public override void SetHintTarget(RectTransform screenTarget, Hint.Parameters parameters = null, Transform origin = null)
        {
            SetHintData(parameters as Parameters);
            base.SetHintTarget(screenTarget, parameters, origin);
        }

        public override void SetHintTarget(Transform worldTarget, Hint.Parameters parameters, Transform origin = null)
        {
            SetHintData(parameters as Parameters);
            base.SetHintTarget(worldTarget, parameters, origin);
        }

    }

}
