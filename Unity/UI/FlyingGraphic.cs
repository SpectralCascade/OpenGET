using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// Temporary graphic used to give quick feedback to players without sacrificing a dedicated area on screen.
    /// </summary>
    public class FlyingGraphic : Follower
    {
        /// <summary>
        /// Offset position tracked independently of target movement.
        /// </summary>
        public Vector2 offset = Vector2.zero;

        /// <summary>
        /// The direction to "fly" in.
        /// </summary>
        public Vector2 direction = Vector2.up;

        /// <summary>
        /// Movement/"flying" speed in pixels per second.
        /// </summary>
        public float speed = 200;

        /// <summary>
        /// How long this text will fly for before being faded out.
        /// </summary>
        public float fadeDelay = 0.5f;

        /// <summary>
        /// How long this text takes to fade.
        /// </summary>
        public float fadeTime = 1f;

        /// <summary>
        /// Track time passed.
        /// </summary>
        private float timer = 0;

        /// <summary>
        /// For fading the text out before destroying it.
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

        protected override void Awake()
        {
            base.Awake();

            fader = new Fader(canvasGroup);
        }

        /// <summary>
        /// Initialise to follow a target. Provide a camera if it's a worldspace target.
        /// </summary>
        public void Init(string text = "", Color? colour = null, Transform target = null, Camera cam = null)
        {
            Log.Debug("Setting target = {0}, cam = {1}", target?.gameObject.name, cam?.gameObject.name);
            this.target = target;
            this.cam = cam;

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
        }

        protected override void Update()
        {
            timer += Time.deltaTime;

            Vector2 change = direction.normalized * speed * Time.deltaTime;
            offset += change;
            Vector3 movement = new Vector3(offset.x, offset.y, 0);

            if (target == null)
            {
                transform.position += new Vector3(change.x, change.y, 0);
            }
            else if (cam == null)
            {
                transform.position = target.position + movement;
            }
            else
            {
                transform.position = movement + cam.WorldToScreenPoint(target.position);
            }

            if (timer >= fadeTime + fadeDelay)
            {
                Destroy(gameObject);
            }
            else if (timer >= fadeDelay && !fader.isFadingOut)
            {
                fader.FadeOut(fadeTime - (timer - fadeDelay));
            }
        }
    }

}
