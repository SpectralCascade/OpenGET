using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenGET;
using System.Net.Mime;

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
        public void UpdatePosition()
        {
            SetPosition(worldPosition);
        }

        /// <summary>
        /// Sets the position of the panel by world position, while staying within bounds.
        /// </summary>
        public void SetPosition(Vector3 worldPos)
        {
            worldPosition = worldPos;
            SetPosition((Vector2)UI.WorldToCanvasPoint(worldPos));
        }

        /// <summary>
        /// Sets the position of the panel by screen position, while staying within bounds.
        /// Note: Screen position should be in worldspace when using ScreenSpaceCamera mode for UI.
        /// </summary>
        public void SetPosition(Vector2 screenPos, bool useCanvasOffset = true)
        {
            RectTransform canvasRect = UI.canvas.transform as RectTransform;
            Vector2 canvasPivot = canvasRect.pivot;
            float scale = UI.canvas.transform.localScale.x;

            // Get canvas dimensions in worldspace
            Rect canvasDim = new Rect(
                -canvasRect.rect.width * canvasPivot.x * scale,
                -canvasRect.rect.height * canvasPivot.y * scale,
                canvasRect.rect.width * scale,
                canvasRect.rect.height * scale
            );

            // Get size of the content in worldspace
            Vector2 contentSize = new Vector2(content.rect.width, content.rect.height) * scale;
            Vector2 contentPivot = content.pivot;

            Vector2 boundingSize = new Vector2(rect.rect.width, rect.rect.height) * scale;
            Vector2 boundingPivot = rect.pivot;

            // Canvas offset
            // Not sure why but in some setups accounting for the canvas offset is not necessary (e.g. toolips)
            Vector2 canvasOffset = useCanvasOffset ? canvasPivot * new Vector2(canvasDim.width, canvasDim.height) : Vector2.zero;

            // Get bounds in worldspace
            Rect bounds = new Rect(
                (rect.rect.x * scale) + (contentSize.x * contentPivot.x) - canvasOffset.x,
                (rect.rect.y * scale) + (contentSize.y * contentPivot.y) - canvasOffset.y,
                boundingSize.x - contentSize.x,
                boundingSize.y - contentSize.y
            );

            // Compute new position in the worldspace canvas
            screenPos = new Vector2(
                Mathf.Clamp(
                    screenPos.x,
                    bounds.x,
                    bounds.x + bounds.width
                ),
                Mathf.Clamp(
                    screenPos.y,
                    bounds.y,
                    bounds.y + bounds.height
                )
            );

            transform.position = screenPos;
        }

    }

}
