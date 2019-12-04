using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Interface for fader implementations.
    /// </summary>
    internal interface IFader {
        /// <summary>
        /// Starts fading in.
        /// </summary>
        /// <param name="time"></param>
        void FadeIn(float time = 1.0f);

        /// <summary>
        /// Starts fading out.
        /// </summary>
        /// <param name="time"></param>
        void FadeOut(float time = 1.0f);

        /// <summary>
        /// Coroutine that does the fading.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lerpTime"></param>
        IEnumerator Fade(float start, float end, float lerpTime = 0.5f);

    }

    /// <summary>
    /// Abstract class for fading stuff in and out.
    /// TODO: Implement OnFullyVisible and OnHidden events.
    /// </summary>
    public abstract class Fader : IFader
    {

        private bool _isFading;
        private bool _isVisible;

        public abstract void FadeIn(float time = 1.0f);
        public abstract void FadeOut(float time = 1.0f);
        public abstract IEnumerator Fade(float start, float end, float lerpTime = 0.5f);

        /// <summary>
        /// Is the faded object fully visible?
        /// </summary>
        public bool isFullyVisible { get { return _isVisible && !isFading; } }

        /// <summary>
        /// Is the faded object invisible?
        /// </summary>
        public bool isFullyHidden { get { return !_isVisible; } }

        /// <summary>
        /// Is this fader currently fading?
        /// </summary>
        public bool isFading { get { return _isFading; } }

    }

    /// <summary>
    /// Fader for a canvas group, see comments for how it works.
    /// </summary>
    public class CanvasGroupFader : Fader
    {

        private readonly CanvasGroup canvasGroup;

        public CanvasGroupFader(CanvasGroup canvasGroup) {
            this.canvasGroup = canvasGroup;
        }

        /// <summary>
        /// Fades the canvas group in.
        /// </summary>
        /// <param name="time"></param>
        public override void FadeIn(float time = 1.0f) {
            Coroutines.Start(Fade(canvasGroup.alpha, 1, time));
        }

        /// <summary>
        /// Fades the canvas group out.
        /// </summary>
        /// <param name="time"></param>
        public override void FadeOut(float time = 1.0f) {
            Coroutines.Start(Fade(canvasGroup.alpha, 0, time));
        }

        /// <summary>
        /// Coroutine that performs the actual fading specific to the CanvasGroup component.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        public override IEnumerator Fade(float start, float end, float time = 1.0f) {
            float startTime = Time.time;
            float elapsedTime = 0;
            float lerpValue = 0;

            if (end > start) {
                /// Make the gameobject active so it will definitely be visible.
                canvasGroup.gameObject.SetActive(true);
            }

            while (true) {
                elapsedTime = Time.time - startTime;
                lerpValue = elapsedTime / time;

                /// This is the component specific bit; we lerp the desired variable(s) here,
                /// in this case the canvas group's alpha.
                canvasGroup.alpha = Mathf.Lerp(start, end, lerpValue);

                if (lerpValue >= 1) {
                    /// Fade is finished
                    if (end <= 0) {
                        /// Make the gameobject inactive, we don't need to use it while it's invisible.
                        canvasGroup.gameObject.SetActive(false);
                    }
                    break;
                }
                yield return new WaitForEndOfFrame();
            }

        }

    }

}
