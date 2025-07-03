using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// Layout radial UI elements.
    /// </summary>
    public class RadialLayoutGroup : LayoutGroup
    {
        [Tooltip("Minimum distance from the centre of this radial layout.")]
        public float radiusMin = 0;

        [Tooltip("Maximum distance from the centre of this radial layout.")]
        public float radiusMax = 0;

        [Tooltip("The maximum angle between each element in degrees.")]
        [Range(0f, 360f)]
        public float spreadAngleMax = 360f;

        [Tooltip("What rotation angle the first element should start at.")]
        [Range(0f, 360f)]
        public float angleStart = 0f;

        [Tooltip("The minimum limit of rotation.")]
        [Range(0f, 360f)]
        public float angleMin = 360f;

        [Tooltip("The maximum limit of rotation.")]
        [Range(0f, 360f)]
        public float angleMax = 0f;

        [Tooltip("Centre the layout on the start angle.")]
        public bool centerOnStartAngle = false;

        protected override void OnEnable() {
            base.OnEnable();
            CalculateRadial();
        }

        public override void SetLayoutHorizontal()
        {
        }

        public override void SetLayoutVertical()
        {
        }

        public override void CalculateLayoutInputVertical()
        {
            CalculateRadial();
        }

        public override void CalculateLayoutInputHorizontal()
        {
            CalculateRadial();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            CalculateRadial();
        }
#endif

        private void CalculateRadial()
        {
            m_Tracker.Clear();
            if (transform.childCount == 0)
            {
                // Early out
                return;
            }

            float angleOffset = ((angleMax - angleMin)) / (transform.childCount - 1);
            float angleChange = angleOffset < 0 ? Mathf.Max(angleOffset, -spreadAngleMax) : Mathf.Min(angleOffset, spreadAngleMax);
            float angleEnd = angleChange * (transform.childCount - 1);

            float angle = centerOnStartAngle ? angleStart - (angleEnd * 0.5f) : angleStart;
            for (int i = 0, counti = transform.childCount; i < counti; i++)
            {
                RectTransform child = (RectTransform)transform.GetChild(i);
                if (child != null)
                {
                    // Prevent modification from inspector
                    // TODO: Also drive width/height; add flags for controlling width/height
                    m_Tracker.Add(
                        this,
                        child,
                        DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Pivot
                    );

                    Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);

                    float t = counti <= 1 ? 0 : i / (counti - 1f);
                    float radius = Mathf.Lerp(radiusMin, radiusMax, t);
                    // TODO: Pad should affect width/height of rect transform
                    child.localPosition = (direction * radius) + new Vector3(m_Padding.left - m_Padding.right, m_Padding.bottom - m_Padding.top, 0);

                    // Force objects to be center aligned, this can be changed however I'd suggest you keep all of the objects with the same anchor points.
                    child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
                    angle += angleChange;
                }
            }

        }
    }

}
