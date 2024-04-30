using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using OpenGET.Input;

namespace OpenGET.UI
{

    /// <summary>
    /// A modal popup; static methods provide an API to show one anywhere with a custom prefab.
    /// </summary>
    [DisallowMultipleComponent]
    public class Modal : AutoBehaviour
    {

        /// <summary>
        /// Parameters to construct a modal.
        /// </summary>
        public class Parameters
        {

            public Parameters(
                string title, string description,
                string textPrimary, UnityAction actionPrimary,
                string textSecondary, UnityAction actionSecondary,
                UnityAction onClose,
                bool showCloseInput
            )
            {
                this.title = title;
                this.description = description;
                this.textPrimary = textPrimary;
                this.actionPrimary = actionPrimary;
                this.textSecondary = textSecondary;
                this.actionSecondary = actionSecondary;
                this.onClose = onClose;
                this.showCloseInput = showCloseInput;
            }

            /// <summary>
            /// Title text.
            /// </summary>
            public string title;

            /// <summary>
            /// Descriptive text.
            /// </summary>
            public string description;

            /// <summary>
            /// Text on the primary action button.
            /// </summary>
            public string textPrimary;

            /// <summary>
            /// Text on the secondary action button.
            /// </summary>
            public string textSecondary;

            /// <summary>
            /// The callback for handling primary action button press.
            /// </summary>
            public UnityAction actionPrimary;

            /// <summary>
            /// The callback for handling secondary action button press.
            /// </summary>
            public UnityAction actionSecondary;

            /// <summary>
            /// The callback for handling closure of the popup.
            /// This is called after any of the buttons are pressed (including action buttons).
            /// </summary>
            public UnityAction onClose;

            /// <summary>
            /// Should a dedicated close button be shown?
            /// </summary>
            public bool showCloseInput;

        }

        /// <summary>
        /// Title text. Not guaranteed to exist.
        /// </summary>
        public Text title;

        /// <summary>
        /// Description text. All modal popups must have this.
        /// </summary>
        [Auto.NullCheck]
        public Text description;

        /// <summary>
        /// Image shown on the popup. Not used currently but here in case we need it in future.
        /// Not guaranteed to exist.
        /// </summary>
        public Image image;

        /// <summary>
        /// Modal close button. This should be an X button in the upper right corner without any text.
        /// Not guaranteed to exist.
        /// </summary>
        public ButtonController buttonClose;

        /// <summary>
        /// Primary input prompt or button image. Not guaranteed to exist.
        /// Image reference used as it could just be an input prompt or a navigable button.
        /// </summary>
        public Image inputPrimary;

        /// <summary>
        /// Primary input text. Not guaranteed to exist.
        /// </summary>
        public Text textPrimary;

        /// <summary>
        /// Input action for the primary prompt, if available.
        /// </summary>
        public UnityEngine.InputSystem.InputActionReference promptActionPrimary;

        /// <summary>
        /// Secondary input prompt or button image. Not guaranteed to exist.
        /// Image reference used as it could just be an input prompt or a navigable button.
        /// </summary>
        public Image inputSecondary;

        /// <summary>
        /// Secondary input text. Not guaranteed to exist.
        /// </summary>
        public Text textSecondary;

        /// <summary>
        /// Input action for the secondary prompt, if available.
        /// </summary>
        public UnityEngine.InputSystem.InputActionReference promptActionSecondary;

        /// <summary>
        /// Optional fill for the secondary prompt; player has to hold the secondary prompt action button for some time.
        /// </summary>
        public Image promptFillSecondary;

        /// <summary>
        /// The canvas group associated with this modal.
        /// </summary>
        [Auto.NullCheck]
        public CanvasGroup canvasGroup;

        /// <summary>
        /// The canvas group associated with the background tint.
        /// </summary>
        [Auto.NullCheck]
        public CanvasGroup tintCanvasGroup;

        // Background tints, behind the popup itself. These can be used to highlight a specific part of the screen.
        [Auto.NullCheck]
        public RectTransform tintLeft;
        [Auto.NullCheck]
        public RectTransform tintRight;
        [Auto.NullCheck]
        public RectTransform tintTop;
        [Auto.NullCheck]
        public RectTransform tintBottom;

        /// <summary>
        /// Reference to the root UI controller.
        /// </summary>
        private UIController ui;

        /// <summary>
        /// Modal popup parameters.
        /// </summary>
        [System.NonSerialized]
        public Parameters data;

        /// <summary>
        /// Time in seconds the player must hold the secondary input to execute the action.
        /// </summary>
        private float holdSecondaryTime = 0.5f;//GameData.Input.ButtonHoldTime;

        /// <summary>
        /// Show a modal popup.
        /// TODO: Document parameters.
        /// </summary>
        public static Modal Show(
            UIController ui,
            string prefabResource,
            string title,
            string description,
            string textPrimary = "OK", UnityAction actionPrimary = null,
            string textSecondary = null, UnityAction actionSecondary = null,
            UnityAction onClose = null,
            bool showCloseInput = true
        )
        {
            return Show(
                ui,
                Resources.Load<Modal>(prefabResource),
                title,
                description,
                textPrimary, actionPrimary,
                textSecondary, actionSecondary,
                onClose,
                showCloseInput
            );
        }

        /// <summary>
        /// Show a modal popup.
        /// TODO: Document parameters.
        /// </summary>
        public static Modal Show(
            UIController ui,
            Modal prefab,
            string title,
            string description,
            string textPrimary = "OK", UnityAction actionPrimary = null,
            string textSecondary = null, UnityAction actionSecondary = null,
            UnityAction onClose = null,
            bool showCloseInput = true
        )
        {
            if (prefab == null)
            {
                Log.Error("Cannot show modal popup using null prefab reference.");
                return null;
            }
            Modal modal = null;
            if (ui != null)
            {
                // Create the modal and initialise it
                modal = Instantiate(prefab, ui.modalsRoot ?? ui.transform);
                modal.Init(new Parameters(
                    title, description, textPrimary, actionPrimary, textSecondary, actionSecondary, onClose, showCloseInput
                ), ui);
                // TODO: Fade in modal
                modal.canvasGroup.alpha = 1;
                ui.input.RequestInputControl(modal.gameObject);
            }
            else
            {
                Log.Error("Failed to open modal popup as HUD instance could not be retrieved!");
            }
            return modal;
        }

        protected override void Awake()
        {
            base.Awake();

            if (promptFillSecondary != null)
            {
                promptFillSecondary.fillAmount = 1;
            }
        }

        /// <summary>
        /// Handle input events.
        /// </summary>
        private void Update()
        {
            InputHelper.Player input = ui.input;
            if (input.HasControl(gameObject))
            {
                if (!string.IsNullOrEmpty(data.textPrimary) && input.HasControl(gameObject) && promptActionPrimary != null && promptActionPrimary.action.WasPressedThisFrame())
                {
                    OnPrimary();
                }
                else if (!string.IsNullOrEmpty(data.textSecondary) && input.HasControl(gameObject) && promptActionSecondary != null && promptActionSecondary.action.WasPressedThisFrame())
                {
                    if (input.usingGamepad && promptFillSecondary != null)
                    {
                        holdSecondaryTime -= Time.unscaledDeltaTime;
                        promptFillSecondary.fillAmount = holdSecondaryTime / ui.settings.ButtonHoldTime;
                    }
                    else
                    {
                        // For keyboard and mouse, don't bother to hold
                        holdSecondaryTime = 0;
                    }
                    if (holdSecondaryTime <= 0)
                    {
                        OnSecondary();
                        holdSecondaryTime = ui.settings.ButtonHoldTime;
                    }
                }
                else
                {
                    // Reset time to hold the secondary input
                    holdSecondaryTime = ui.settings.ButtonHoldTime;
                    if (promptFillSecondary != null)
                    {
                        promptFillSecondary.fillAmount = 1;
                    }
                }
            }
        }

        /// <summary>
        /// Initialise the popup based on data parameters.
        /// </summary>
        public void Init(Parameters data, UIController ui)
        {
            this.data = data;
            this.ui = ui;

            // Setup buttons
            if (buttonClose != null)
            {
                buttonClose.gameObject.SetActive(data.showCloseInput);
            }
            bool show = !string.IsNullOrEmpty(data.textPrimary);
            inputPrimary?.gameObject.SetActive(show);
            if (textPrimary != null)
            {
                textPrimary.text = data.textPrimary;
                textPrimary.gameObject.SetActive(show);
            }
            show = !string.IsNullOrEmpty(data.textSecondary);
            inputSecondary?.gameObject.SetActive(show);
            if (textSecondary != null)
            {
                textSecondary.text = data.textSecondary;
                textSecondary.gameObject.SetActive(show);
            }

            // Setup all text
            // TODO: Localise text with IDs
            if (title != null && !string.IsNullOrEmpty(data.title))
            {
                title.text = data.title;
            }
            if (!string.IsNullOrEmpty(data.description))
            {
                description.text = data.description;
            }

            // TODO: Support images loaded from resources
        }

        /// <summary>
        /// Called when the modal primary button is clicked.
        /// </summary>
        public void OnPrimary()
        {
            data.actionPrimary?.Invoke();
            OnClose();
        }

        /// <summary>
        /// Called when the modal secondary button is clicked.
        /// </summary>
        public void OnSecondary()
        {
            data.actionSecondary?.Invoke();
            OnClose();
        }

        /// <summary>
        /// Called when the modal is closed.
        /// </summary>
        [SerializeField]
        public void OnClose()
        {
            ui.input.FreeInputControl(gameObject);

            data.onClose?.Invoke();
            // TODO: Fade out before destroying
            Destroy(gameObject);
        }

    }

}
