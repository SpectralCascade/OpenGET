using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// Temporary graphic used to give quick feedback to players without sacrificing a dedicated area on screen.
    /// Similar to FlyingGraphic, but instead of following an object and flying in a particular direction,
    /// this "flows" from a fixed source position to a destination position with tweening.
    /// </summary>
    public class FlowGraphic : AutoBehaviour, IReferrable
    {

        /// <summary>
        /// UI ref.
        /// </summary>
        private UIController UI;

        /// <summary>
        /// Source position.
        /// </summary>
        private Vector3 source;

        /// <summary>
        /// Destination position.
        /// </summary>
        private Vector3 dest;

        /// <summary>
        /// Minimum random range amount the flow path deviates left or right enroute to the destination.
        /// </summary>
        public float deviationMin = 0;

        /// <summary>
        /// Maximum random range amount the flow path deviates left or right enroute to the destination.
        /// </summary>
        public float deviationMax = 0;

        /// <summary>
        /// Random computed deviation.
        /// </summary>
        private float deviation = 0;

        /// <summary>
        /// Point at which the max deviation occurs.
        /// </summary>
        private float deviationPoint = 0;

        [Tooltip("The minimum normalised point between source & destination where max deviation of the flow path occurs.")]
        [Range(0f, 1f)]
        public float minDeviationPoint = 0.5f;

        [Tooltip("The maximum normalised point between source & destination where max deviation of the flow path occurs.")]
        [Range(0f, 1f)]
        public float maxDeviationPoint = 0.5f;

        /// <summary>
        /// Tweening curve for this graphic to use.
        /// </summary>
        public AnimationCurve travelTween = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        /// Tweening curve applied to deviation (and in reverse from the max deviation).
        /// </summary>
        public AnimationCurve deviationTween = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        /// Time in seconds that the graphic travels from start to end.
        /// </summary>
        public float travelTime = 3f;

        /// <summary>
        /// How long this graphic will flow for before being faded out.
        /// </summary>
        public float fadeDelay = 0.5f;

        /// <summary>
        /// How long this graphic takes to fade after reaching the destination.
        /// </summary>
        public float fadeTime = 1f;

        /// <summary>
        /// Track time passed.
        /// </summary>
        private float timer = 0;

        /// <summary>
        /// For fading the graphic out before destroying it.
        /// </summary>
        private Fader fader;

        /// <summary>
        /// Must have a canvas group for fading.
        /// </summary>
        [SerializeField]
        [Auto.NullCheck]
        [Auto.Hookup(Auto.Mode.Self)]
        private CanvasGroup canvasGroup;

        /// <summary>
        /// Associated text, if any.
        /// </summary>
        [SerializeField]
        private MaskableGraphic textGraphic;

        /// <summary>
        /// Associated image, if any.
        /// </summary>
        [SerializeField]
        private Image image;

        protected override void Awake()
        {
            base.Awake();
            fader = new Fader(canvasGroup);
        }

        /// <summary>
        /// Initialise and start moving from source position.
        /// If a UIController is provided, the source and dest positions are assumed to be world-space.
        /// Otherwise, they are assumed to be in UI space already.
        /// </summary>
        public void Init(Vector3 source, Vector3 dest, string text = null, Sprite icon = null, Color? colour = null, UIController UI = null, float fadeTime = -1)
        {
            this.source = source;
            this.dest = dest;
            this.UI = UI;
            timer = 0;

            if (UI != null)
            {
                transform.position = UI.WorldToCanvasPoint(source);
            }
            else
            {
                transform.position = source;
            }

            if (textGraphic != null && !string.IsNullOrEmpty(text))
            {
                if (textGraphic is TMPro.TMP_Text)
                {
                    ((TMPro.TMP_Text)textGraphic).text = text;
                }
                else if (textGraphic is Text)
                {
                    ((Text)textGraphic).text = text;
                }

                if (colour.HasValue)
                {
                    textGraphic.color = colour.Value;
                }
            }

            if (image != null && icon != null)
            {
                image.sprite = icon;
                if (colour.HasValue)
                {
                    image.color = colour.Value;
                }
            }

            if (fadeTime >= 0)
            {
                this.fadeTime = fadeTime;
            }

            deviation = deviationMin <= deviationMax ? Random.Range(deviationMin, deviationMax) : 0;
            deviationPoint = Mathf.Clamp01(Random.Range(minDeviationPoint, maxDeviationPoint));
        }

        protected void Update()
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / travelTime);

            Vector3 travel = UI != null ? UI.WorldToCanvasPoint(dest) - UI.WorldToCanvasPoint(source) : dest - source;

            // Get deviation direction
            Vector3 deviationDir = new Vector3(travel.normalized.y, -travel.normalized.x, 0);
            Vector3 deviationVec = deviationDir * deviation * deviationTween.Evaluate(
                t <= maxDeviationPoint ? t / maxDeviationPoint : 1f - ((t - maxDeviationPoint) / (1f - maxDeviationPoint))
            );
            if (UI != null)
            {
                transform.position = UI.WorldToCanvasPoint(source) + (travel * travelTween.Evaluate(t)) + deviationVec * UI.canvas.transform.localScale.x;
            }
            else
            {
                transform.position = source + travel * travelTween.Evaluate(t) + deviationVec * UI.canvas.transform.localScale.x;
            }

            if (timer >= travelTime + fadeTime + fadeDelay)
            {
                Destroy(gameObject);
            }
            else if (timer >= travelTime + fadeDelay && !fader.isFadingOut)
            {
                fader.FadeOut(fadeTime - (timer - fadeDelay));
            }
        }
    }

}
