using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenGET;

namespace OpenGET.UI
{

    /// <summary>
    /// View panel constrained to some bounds.
    /// </summary>
    public class BoundedViewPanel : ViewPanel
    {
        /// <summary>
        /// Bounding rect transform.
        /// </summary>
        [Auto.NullCheck]
        public RectTransform rect;

        /// <summary>
        /// RectTransform containing the panel contents.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup(Auto.Mode.Self)]
        public RectTransform content;

        /// <summary>
        /// Track target world position, in case the camera moves.
        /// </summary>
        private Vector3 worldPosition = Vector3.zero;

        /// <summary>
        /// Update the current target world position.
        /// </summary>
        public void UpdatePosition(Camera worldCamera)
        {
            SetPosition(worldCamera, worldPosition);
        }

        /// <summary>
        /// Sets the position of the panel by world position, while staying within bounds.
        /// </summary>
        public void SetPosition(Camera worldCamera, Vector3 worldPos)
        {
            worldPosition = worldPos;
            SetPosition(worldCamera.WorldToScreenPoint(worldPos));
        }

        /// <summary>
        /// Sets the position of the panel by screen position, while staying within bounds.
        /// </summary>
        public void SetPosition(Vector2 screenPos)
        {
            Vector2 scale = ((RectTransform)UI.transform).localScale;
            Vector2 size = new Vector2(content.rect.width, content.rect.height);

            screenPos = new Vector2(
                Mathf.Clamp(
                    screenPos.x,
                    content.pivot.x * size.x * scale.x,
                    Screen.width - ((1 - content.pivot.x) * size.x * scale.x)
                ),
                Mathf.Clamp(
                    screenPos.y,
                    content.pivot.y * size.y * scale.y,
                    Screen.height - ((1 - content.pivot.y) * size.y * scale.y)
                )
            );

            transform.position = screenPos;
        }

    }

}
