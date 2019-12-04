﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Interface for fader implementations.
    /// </summary>
    public interface IFader {
        /// <summary>
        /// Returns the fade value of the target fade object.
        /// </summary>
        float GetFadeValue();

        /// <summary>
        /// Sets the fade value of the target fade object.
        /// </summary>
        /// <param name="v"></param>
        void SetFadeValue(float v);

    }

    /// <summary>
    /// Abstract class for fading stuff in and out.
    /// TODO: Implement OnFullyVisible and OnHidden events.
    /// </summary>
    public class Fader
    {

        public delegate void OnFaded(Fader f, int fadeDir);

        public event OnFaded OnFadeComplete;

        /// <summary>
        /// Reference to the fader implementation.
        /// </summary>
        private IFader implementation;

        /// <summary>
        /// Whether the fader is fading in (1), out (-1), or not fading (0).
        /// </summary>
        private int fadeDirection;

        /// <summary>
        /// Is the fader implementation visible?
        /// </summary>
        private bool isVisible { get { return implementation.GetFadeValue() > 0; } }

        public Fader(CanvasGroup canvasGroup) {
            implementation = new CanvasGroupFader(canvasGroup);
        }

        /// <summary>
        /// Starts fading in, if we aren't fading in already.
        /// </summary>
        /// <param name="time"></param>
        public void FadeIn(float time = 1.0f) {
            if (fadeDirection <= 0) {
                fadeDirection = 1;
                DoFade(implementation.GetFadeValue(), 1, time);
            }
        }

        /// <summary>
        /// Starts fading out, if we aren't fading out already.
        /// </summary>
        /// <param name="time"></param>
        public void FadeOut(float time = 1.0f) {
            if (fadeDirection >= 0) {
                fadeDirection = -1;
                DoFade(implementation.GetFadeValue(), 0, time);
            }
        }

        /// <summary>
        /// Starts a fading coroutine.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lerpTime"></param>
        private void DoFade(float start, float end, float lerpTime = 0.5f) {
            fadeDirection = end > start ? 1 : -1;
            Coroutines.Start(Fade(start, end, lerpTime));
        }

        /// <summary>
        /// Is the faded object fully visible (i.e. finished fading and visible)?
        /// </summary>
        public bool isFullyVisible { get { return isVisible && !isFading; } }

        /// <summary>
        /// Is the faded object invisible?
        /// </summary>
        public bool isFullyHidden { get { return !isVisible; } }

        /// <summary>
        /// Is this fader currently fading at all?
        /// </summary>
        public bool isFading { get { return fadeDirection != 0; } }

        /// <summary>
        /// Is the fader currently fading in?
        /// </summary>
        public bool isFadingIn { get { return fadeDirection > 0; } }

        /// <summary>
        /// Is the fader currently fading out?
        /// </summary>
        public bool isFadingOut { get { return fadeDirection < 0; } }

        /// <summary>
        /// Coroutine that performs the actual fading.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        public IEnumerator Fade(float start, float end, float time = 1.0f) {
            float startTime = Time.time;
            float elapsedTime = 0;
            float lerpValue = 0;

            while (true) {
                elapsedTime = Time.time - startTime;
                lerpValue = elapsedTime / time;

                implementation.SetFadeValue(Mathf.Lerp(start, end, lerpValue));

                if (lerpValue >= 1) {
                    /// Fade is finished
                    int fadeDir = end < start ? -1 : 1;
                    if (fadeDir < 0) {
                        OnFadeComplete(this, -1);
                    } else {
                        OnFadeComplete(this, 1);
                    }

                    if (fadeDirection == fadeDir) {
                        fadeDirection = 0;
                    }
                    break;
                }
                yield return new WaitForEndOfFrame();
            }

        }

    }

    /// <summary>
    /// Fader implementation for a canvas group.
    /// </summary>
    public class CanvasGroupFader : IFader
    {

        private readonly CanvasGroup canvasGroup;

        public CanvasGroupFader(CanvasGroup canvasGroup) {
            this.canvasGroup = canvasGroup;
        }

        public float GetFadeValue() {
            return canvasGroup.alpha;
        }

        public void SetFadeValue(float v) {
            canvasGroup.alpha = v;
        }

    }

}
