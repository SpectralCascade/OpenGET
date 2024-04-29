using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenGET.Input;

namespace OpenGET.UI {

    /// <summary>
    /// Represents some group of UI elements overlaid on screen.
    /// Useful primarily for linear menu paths.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ViewPanel : AccessUI
    {
        public delegate void OnSetShown(bool shown);

        /// <summary>
        /// Event invoked when this panel has fully finished fading in or out.
        /// </summary>
        public event OnSetShown onSetShown;

        /// <summary>
        /// The panel above this in stack, if any.
        /// </summary>
        protected ViewPanel above = null;

        /// <summary>
        /// The panel below this on the stack, if any.
        /// </summary>
        protected ViewPanel below = null;

        /// <summary>
        /// Canvas group used for fading.
        /// </summary>
        [SerializeField]
        [Auto.Hookup(Auto.Mode.Self)]
        [Auto.NullCheck]
        protected CanvasGroup canvasGroup;

        /// <summary>
        /// The button used to go back to the previous screen, if relevant. May be null.
        /// </summary>
        public UnityEngine.UI.Button backButton = null;

        /// <summary>
        /// Should this panel be shown when first loaded? Should help to prevent issues where people forget to turn on/off screens in the scene.
        /// </summary>
        [SerializeField]
        private bool startShown = false;

        /// <summary>
        /// Used to fade the view panel in or out, as an alternative to an animator.
        /// </summary>
        public Fader fader {
            get {
                if (_fader == null)
                {
                    if (Application.isPlaying)
                    {
                        // Make sure panel is in correct state to begin
                        canvasGroup.alpha = startShown ? 1 : 0;
                    }
                    _fader = new Fader(canvasGroup);
                    _fader.OnFadeComplete += OnFaded;
                }
                return _fader;
            }
        }
        private Fader _fader;

        /// <summary>
        /// Handle fade completion.
        /// </summary>
        private void OnFaded(Fader f, int fadeDir)
        {
            if (fadeDir > 0)
            {
                OnDidShow();
                onSetShown?.Invoke(true);
            }
            else
            {
                gameObject.SetActive(false);
                OnDidHide();
                onSetShown?.Invoke(false);
            }
        }

        /// <summary>
        /// Called after being set active but before being faded in.
        /// </summary>
        protected virtual void OnWillShow() { }

        /// <summary>
        /// Called when about to be faded out.
        /// </summary>
        protected virtual void OnWillHide() { }

        /// <summary>
        /// Called once fully faded in.
        /// </summary>
        protected virtual void OnDidShow() { }

        /// <summary>
        /// Called once fully faded out and deactivated.
        /// </summary>
        protected virtual void OnDidHide() { }

        /// <summary>
        /// Called when the back button is clicked.
        /// </summary>
        protected virtual void OnBack() { }

        /// <summary>
        /// Should this panel start shown?
        /// </summary>
        public bool ShouldStartShown() {
            return startShown;
        }

        /// <summary>
        /// Is this panel visible at all? Includes states where the panel is fading in/out but not inactive.
        /// </summary>
        public bool IsVisible()
        {
            return !fader.isFullyHidden;
        }

        /// <summary>
        /// Is this panel fully visible?
        /// </summary>
        public bool IsFullyShown()
        {
            return fader.isFullyVisible;
        }

        /// <summary>
        /// Show or hide this panel.
        /// </summary>
        public virtual void SetShown(bool value, float fadeTime) {
            if (value)
            {
                if (!fader.isFadingIn)
                {
                    gameObject.SetActive(true);
                    OnWillShow();
                    fader.FadeIn(fadeTime);
                }
            }
            else
            {
                if (!fader.isFadingOut)
                {
                    OnWillHide();
                    fader.FadeOut(fadeTime);
                }
            }
        }

        /// <summary>
        /// Show or hide this panel, using the default fade time defined in UI settings.
        /// </summary>
        public void SetShown(bool value)
        {
            SetShown(value, UI.settings.ViewPanelFadeTime);
        }

        /// <summary>
        /// Show this panel using the UI settings fade time.
        /// </summary>
        public void Show()
        {
            SetShown(true);
        }

        /// <summary>
        /// Show this panel with a specific fade time.
        /// </summary>
        public void Show(float fadeTime)
        {
            SetShown(true, fadeTime);
        }

        /// <summary>
        /// Hide this panel using the UI settings fade time.
        /// </summary>
        public void Hide()
        {
            SetShown(false);
        }
        
        /// <summary>
        /// Hide this panel with a specific fade time.
        /// </summary>
        public void Hide(float fadeTime)
        {
            SetShown(false, fadeTime);
        }

#if UNITY_EDITOR
        // TODO: Move this to an editor save event handler
        protected override void OnValidate() {
            base.OnValidate();
            if (UnityEditor.EditorUtility.IsDirty(this))
            {
                Debug.Assert(
                    startShown == gameObject.activeSelf,
                    "Warning: ViewPanel instance is " + (gameObject.activeSelf ? "active" : "inactive") + " in scene but startShown is set to " +
                    startShown.ToString() + ", did you remember to set the gameobject " + (startShown ? "active" : "inactive") + " in the scene?",
                    gameObject
                );
            }
        }
#endif

        protected override void Awake() {
            base.Awake();

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBack);
            }

            if (startShown)
            {
                Show();
            }
            else
            {
                Hide(0);
            }
        }

        protected virtual void OnDestroy()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(OnBack);
            }
        }

        protected virtual void Update() {
            // Trigger the back button if available, when the cancel action occurs.
            if (backButton != null
                && UI.input.HasControl(transform.parent != null ? transform.parent.gameObject : UI.gameObject)
                && UI.actionCancel.action.WasPressedThisFrame() 
            ) {
                backButton.onClick.Invoke();
            }
        }

        /// <summary>
        /// Push a ViewPanel instance onto the panel stack. This panel is automatically hidden until it is popped.
        /// If the overlay has a back button, optionally setup such that pressing it automatically pops this panel.
        /// If an overlay is already pushed on the stack, this method inserts the overlay hidden in-between.
        /// </summary>
        public void Push(ViewPanel overlay, bool autoPopOnBack = true) {
            // Hide this panel
            if (IsVisible()) {
                SetShown(false);
            }

            // Handle insertion
            bool inserting = above != null;
            if (inserting)
            {
                if (above.backButton != null && above.backButton.onClick.GetPersistentTarget(0) != null)
                {
                    above.backButton?.onClick.RemoveListener(Pop);
                    above.backButton?.onClick.AddListener(overlay.Pop);
                }
                overlay.above = above;
                above = overlay;
                Log.Debug(
                    "Inserting ViewPanel into stack between \"{0}\" and \"{1}\"",
                    SceneNavigator.GetGameObjectPath(gameObject),
                    SceneNavigator.GetGameObjectPath(overlay.above.gameObject)
                );
            }
            else
            {
                Log.Debug(
                    "Pushing ViewPanel \"{0}\" onto \"{1}\"",
                    SceneNavigator.GetGameObjectPath(overlay.gameObject),
                    SceneNavigator.GetGameObjectPath(gameObject)
                );
            }

            overlay.SetShown(!inserting);
            if (autoPopOnBack && overlay.backButton != null) {
                overlay.backButton.onClick.AddListener(Pop);
            }
            above = overlay;
            above.below = this;
        }

        /// <summary>
        /// Pop the pushed panel off the stack, hiding it and reshowing this one.
        /// </summary>
        public void Pop() {
            if (above != null) {
                if (above.backButton != null) {
                    above.backButton.onClick.RemoveListener(Pop);
                }
                // Hide the top of the stack
                above.SetShown(false);

                above.below = null;
                if (above.above != null)
                {
                    // Special case - popped but there are still higher panels on the stack.
                    // As this is a non-recursive pop, seamlessly remove the above panel as if it was never pushed
                    ViewPanel old = above;
                    above.above.below = this;
                    above = above.above;
                    if (above.backButton != null)
                    {
                        above.backButton.onClick.RemoveListener(old.Pop);
                        above.backButton.onClick.AddListener(Pop);
                    }
                }
                else
                {
                    above = null;
                }
            }
            // Show this panel now this is the top of the stack.
            if (above == null && !IsFullyShown()) {
                SetShown(true);
            }
        }

        /// <summary>
        /// Recursively pop all panels higher in the stack that have a valid back button reference.
        /// Stops when a panel is reached that has no back button.
        /// Optionally you can force all panels higher in the stack to be popped regardless of back button availability.
        /// </summary>
        public bool PopRecursive(bool force = false)
        {
            if (above == null || (above.PopRecursive(force) && (force || above.backButton != null)))
            {
                Pop();
                return true;
            }
            return false;
        }

    }

}
