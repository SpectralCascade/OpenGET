using OpenGET.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Abstract class for fading stuff in and out.
    /// </summary>
    public class Fader
    {

        public delegate void OnFaded(Fader f, int fadeDir);

        public event OnFaded OnFadeComplete;

        /// <summary>
        /// Reference to the fader implementation.
        /// </summary>
        public IPercentValue implementation { get; private set; }

        /// <summary>
        /// Current fade coroutine.
        /// </summary>
        private Coroutine fadeCoroutine = null;

        /// <summary>
        /// Whether the fader is fading in (1), out (-1), or not fading (0).
        /// </summary>
        private int fadeDirection = 0;

        /// <summary>
        /// Is the fader implementation visible?
        /// </summary>
        private bool isVisible { get { return implementation.GetValue() > 0; } }

        /// <summary>
        /// Whether unscaled time should be used for the fade or not. Defaults to true.
        /// </summary>
        private bool useUnscaledTime = true;

        /// <summary>
        /// Use CanvasGroup implementation.
        /// </summary>
        public Fader(CanvasGroup canvasGroup, bool useUnscaledTime = true) {
            implementation = new CanvasGroupFader(canvasGroup);
            this.useUnscaledTime = useUnscaledTime;
        }

        /// <summary>
        /// Use Renderer implementation (e.g. MeshRenderer, SpriteRenderer).
        /// </summary>
        public Fader(Renderer renderer, bool useUnscaledTime = true) {
            implementation = new RendererFader(renderer);
            this.useUnscaledTime = useUnscaledTime;
        }

        /// <summary>
        /// Custom color fading implementation.
        /// </summary>
        public Fader(IColorFadeable colorFader, bool useUnscaledTime = true) {
            implementation = new ColorFader(colorFader);
            this.useUnscaledTime = useUnscaledTime;
        }

        /// <summary>
        /// Use custom Animator implementation.
        /// </summary>
        public Fader(Animator animator, int startHash, int layer = 0, bool useUnscaledTime = true)
        {
            implementation = new AnimatorFader(animator, startHash, layer);
            this.useUnscaledTime = useUnscaledTime;
        }

        /// <summary>
        /// Starts fading in, if we aren't fading in already.
        /// </summary>
        public void FadeIn(float time = 1.0f) {
            if (time <= 0 && implementation != null)
            {
                if (implementation.GetValue() < 1f)
                {
                    fadeDirection = 1;
                    implementation.SetValue(1);
                    fadeDirection = 0;
                    OnFadeComplete?.Invoke(this, 1);
                    fadeCoroutine = null;
                }
            }
            else if (implementation != null) {
                fadeDirection = 1;
                DoFade(implementation.GetValue(), 1, time);
            }
        }

        /// <summary>
        /// Starts fading out, if we aren't fading out already.
        /// </summary>
        public void FadeOut(float time = 1.0f) {
            if (time <= 0 && implementation != null)
            {
                if (implementation.GetValue() > 0)
                {
                    fadeDirection = -1;
                    implementation.SetValue(0);
                    fadeDirection = 0;
                    OnFadeComplete?.Invoke(this, -1);
                    fadeCoroutine = null;
                }
            }
            else if (implementation != null) {
                fadeDirection = -1;
                DoFade(implementation.GetValue(), 0, time);
            }
        }

        /// <summary>
        /// Starts a fading coroutine.
        /// </summary>
        private void DoFade(float start, float end, float lerpTime = 0.5f) {
            fadeDirection = end == 0 ? -1 : 1;
            if (fadeCoroutine != null)
            {
                Coroutines.Stop(fadeCoroutine);
            }
            fadeCoroutine = Coroutines.Start(Fade(start, end, lerpTime));
        }

        /// <summary>
        /// Is the faded object fully visible (i.e. finished fading and visible)?
        /// </summary>
        public bool isFullyVisible => isVisible && !isFading;

        /// <summary>
        /// Is the faded object invisible?
        /// </summary>
        public bool isFullyHidden => !isVisible;

        /// <summary>
        /// Is this fader currently fading at all?
        /// </summary>
        public bool isFading => fadeDirection != 0;

        /// <summary>
        /// Is the fader currently fading in?
        /// </summary>
        public bool isFadingIn => fadeDirection > 0;

        /// <summary>
        /// Is the fader currently fading out?
        /// </summary>
        public bool isFadingOut => fadeDirection < 0;

        /// <summary>
        /// Get the current fader value.
        /// </summary>
        public float value => implementation.GetValue();

        /// <summary>
        /// Coroutine that performs the actual fading.
        /// </summary>
        public IEnumerator Fade(float start, float end, float time = 1.0f) {
            float startTime = useUnscaledTime ? Time.unscaledTime : Time.time;
            float elapsedTime;
            float lerpValue;
            time = Mathf.Max(float.Epsilon, time);

            while (true) {
                elapsedTime = (useUnscaledTime ? Time.unscaledTime : Time.time) - startTime;
                lerpValue = elapsedTime / time;

                if (implementation != null)
                {
                    implementation.SetValue(Mathf.Lerp(start, end, lerpValue));
                }

                if (lerpValue >= 1) {
                    /// Fade is finished
                    int fadeDir = fadeDirection;

                    fadeDirection = 0;
                    OnFadeComplete?.Invoke(this, fadeDir);
                    fadeCoroutine = null;

                    if (fadeCoroutine == null)
                    {
                        fadeDirection = 0;
                    }
                    break;
                }

                yield return new WaitForEndOfFrame();
            }

            yield return null;
        }

    }

    /// <summary>
    /// Fader implementation for a canvas group.
    /// </summary>
    public class CanvasGroupFader : IPercentValue
    {

        private readonly CanvasGroup canvasGroup;

        public CanvasGroupFader(CanvasGroup canvasGroup) {
            this.canvasGroup = canvasGroup;
        }

        public float GetValue() {
            return canvasGroup != null ? canvasGroup.alpha : 0;
        }

        public void SetValue(float v) {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = v;
            }
        }

    }

    /// <summary>
    /// Fader implementation for renderers (e.g. MeshRenderer, SpriteRenderer).
    /// </summary>
    public class RendererFader : IPercentValue
    {

        private readonly Renderer renderer;

        public RendererFader(Renderer renderer)
        {
            this.renderer = renderer;
        }

        public float GetValue()
        {
            return renderer != null && renderer.material != null ? renderer.material.color.a : 0;
        }

        public void SetValue(float v)
        {
            if (renderer != null)
            {
                for (int i = 0, counti = renderer.materials.Length; i < counti; i++)
                {
                    renderer.materials[i].color = Colors.Alpha(renderer.materials[i].color, v);
                }
            }
        }

    }

    /// <summary>
    /// Fade between two different colours.
    /// </summary>
    public interface IColorFadeable
    {
        /// <summary>
        /// Target fade colour.
        /// </summary>
        public Color fadeColorMax { get; }

        /// <summary>
        /// Original fade colour.
        /// </summary>
        public Color fadeColorMin { get; }
        
        /// <summary>
        /// Current fade colour.
        /// </summary>
        public Color fadeColorActive { get; set; }
    }

    /// <summary>
    /// Fade between 2 colours.
    /// </summary>
    public class ColorFader : IPercentValue
    {
        private readonly IColorFadeable fadeable;

        /// <summary>
        /// Current fade value.
        /// </summary>
        private float fadeValue;

        public ColorFader(IColorFadeable fadeable, float fadeValue = 0)
        {
            this.fadeable = fadeable;
            this.fadeValue = fadeValue;
        }

        public float GetValue()
        {
            return fadeValue;
        }

        public void SetValue(float v)
        {
            fadeValue = v;
            fadeable.fadeColorActive = Color.Lerp(fadeable.fadeColorMin, fadeable.fadeColorMax, v);
        }
    }

    /// <summary>
    /// Fader implementation for animators.
    /// </summary>
    public class AnimatorFader : IPercentValue
    {

        private readonly Animator animator;

        /// <summary>
        /// Hash of the animation state id.
        /// </summary>
        private int animHash;

        /// <summary>
        /// Custom layer, if any.
        /// </summary>
        private int layer = 0;

        public AnimatorFader(Animator animator, int startAnimHash, int layer = 0)
        {
            // Setup animator with a speed of zero, so that we can manually step through frames
            this.animator = animator;
            animator.speed = 0;

            animHash = startAnimHash;
            this.layer = layer;
        }

        public void SetAnim(int hash, int layer = 0)
        {
            this.animHash = hash;
            this.layer = layer;
        }

        public float GetValue()
        {
            return animator != null ? animator.GetCurrentAnimatorStateInfo(0).normalizedTime : 0;
        }

        public void SetValue(float v)
        {
            if (animator != null && animator.gameObject.activeInHierarchy)
            {
                animator.Play(animHash, layer, v);
            }
        }

    }
}
