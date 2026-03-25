using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OpenGET.UI
{

    /// <summary>
    /// Alters camera viewports to be letterboxed to a specific aspect ratio.
    /// If you want this for UI, you will need your canvas render mode set to "Screenspace - Camera".
    /// </summary>
    public class Letterbox : AutoBehaviour
    {
        public enum AspectMode
        {
            Snap, // Snap to specific aspect ratios 
            Clamp // Clamp within range of min & max aspect ratios
        }

        /// <summary>
        /// Screen resolution. Used for aspect ratio fitting of the viewports.
        /// </summary>
        private Vector2Int resolution = Vector2Int.zero;

        /// <summary>
        /// Cameras that should be letterboxed.
        /// </summary>
        public Camera[] cameras = new Camera[0];

        /// <summary>
        /// Target aspect ratios for letterboxing.
        /// </summary>
        [Tooltip("Supported aspect ratios. You can type ratios in directly, e.g. 16:9 can be entered as 16/9 and Unity will calculate the value.")]
        public float[] targetAspectRatios = new float[] { 16f / 9f };

        [Tooltip("Snap: Closest aspect ratio in targets used.\nClamp: Aspect ratio is clamped within min-max range of all targets.")]
        public AspectMode mode;

        [Tooltip("Letterbox border color.")]
        public Color color = Color.black;

        /// <summary>
        /// Checks for changes in screen resolution and sets the camera viewport according to aspect ratio.
        /// </summary>
        protected void Update()
        {
            if ((Screen.width == resolution.x && Screen.height == resolution.y) || targetAspectRatios.Length < 1)
            {
                // Early out, we have already handled resolution change.
                return;
            }

            // Get current screen aspect
            float screenRatio = (float)Screen.width / Screen.height;
            List<float> targets = mode switch
            {
                AspectMode.Clamp => new List<float> { Mathf.Min(targetAspectRatios), Mathf.Max(targetAspectRatios) },
                _ => targetAspectRatios.ToList()
            };

            // Pick closest target from supported aspect ratios
            float target = targets[0];
            if (mode == AspectMode.Snap || screenRatio < targets[0] || screenRatio > targets[1])
            {
                for (int i = 1, counti = targets.Count; i < counti; i++)
                {
                    float candidate = targets[i];
                    if (Mathf.Abs(screenRatio - candidate) < Mathf.Abs(screenRatio - target))
                    {
                        target = candidate;
                    }
                }
            }

            // Update all camera viewports to target aspect
            for (int i = 0, counti = cameras.Length; i < counti; i++)
            {
                Camera camera = cameras[i];

                // Calculate new viewport rect based on target aspect
                float scale = screenRatio / target;
                Rect rect = camera.rect;
                if (scale < 1.0f)
                {
                    // Scale height
                    rect.width = 1.0f;
                    rect.height = scale;
                    rect.x = 0;
                    rect.y = (1.0f - scale) / 2.0f;
                }
                else
                {
                    // Scale width
                    scale = 1.0f / scale;
                    rect.width = scale;
                    rect.height = 1.0f;
                    rect.x = (1.0f - scale) / 2.0f;
                    rect.y = 0;
                }

                // Update the viewport to the new aspect ratio
                camera.rect = rect;
            }

            // Letterbox camera
            Camera clearCam = gameObject.AddComponentOnce<Camera>();
            clearCam.clearFlags = CameraClearFlags.SolidColor;
            clearCam.backgroundColor = color;
            clearCam.cullingMask = 0;
            clearCam.orthographic = true;
            clearCam.depth = float.MinValue;

            // Cache resolution
            resolution = new Vector2Int(Screen.width, Screen.height);
        }

    }

}
