using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using OpenGET.Input;
using System.Linq;

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
                bool showCloseInput,
                bool takeInputControl
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
                this.takeInputControl = takeInputControl;
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
            
            /// <summary>
            /// Has this modal taken input control?
            /// </summary>
            public bool takeInputControl;

        }

        public delegate Hint MakeHint(Modal modal);

        /// <summary>
        /// The main part of the modal popup, i.e. the visual popup itself not including the background or hints.
        /// </summary>
        [Auto.NullCheck]
        public RectTransform main;

        /// <summary>
        /// Title text. Not guaranteed to exist.
        /// </summary>
        public string title { get => GetText(textTitleGraphic); set => SetText(textTitleGraphic, value); }

        /// <summary>
        /// Text graphic for title.
        /// </summary>
        [Auto.NullCheck]
        [SerializeField]
        private MaskableGraphic textTitleGraphic;

        /// <summary>
        /// Description text. All modal popups must have this.
        /// </summary>
        public string description { get => GetText(textDescriptionGraphic); set => SetText(textDescriptionGraphic, value); }

        /// <summary>
        /// Text graphic for description.
        /// </summary>
        [Auto.NullCheck]
        [SerializeField]
        private MaskableGraphic textDescriptionGraphic;

        /// <summary>
        /// Image shown on the popup. Not used currently but here in case we need it in future.
        /// Not guaranteed to exist.
        /// </summary>
        public Image image;

        /// <summary>
        /// Modal close button. This should be an X button in the upper right corner without any text.
        /// Not guaranteed to exist.
        /// </summary>
        public Button buttonClose;

        /// <summary>
        /// Primary input prompt or button image. Not guaranteed to exist.
        /// Image reference used as it could just be an input prompt or a navigable button.
        /// </summary>
        public Image inputPrimary;

        /// <summary>
        /// Primary input text. Not guaranteed to exist.
        /// </summary>
        public string textPrimary { get => GetText(textPrimaryGraphic); set => SetText(textPrimaryGraphic, value); }

        /// <summary>
        /// Text graphic.
        /// </summary>
        [Auto.NullCheck]
        [SerializeField]
        private MaskableGraphic textPrimaryGraphic;

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
        /// Primary input text. Not guaranteed to exist.
        /// </summary>
        public string textSecondary { get => GetText(textSecondaryGraphic); set => SetText(textSecondaryGraphic, value); }

        /// <summary>
        /// Text graphic.
        /// </summary>
        [Auto.NullCheck]
        [SerializeField]
        private MaskableGraphic textSecondaryGraphic;

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
        /// Optional background root.
        /// </summary>
        public RectTransform hintsRoot;
        
        /// <summary>
        /// Optional background image.
        /// </summary>
        public Image background;

        /// <summary>
        /// Hint(s) used to indicate something to the player at a given position.
        /// </summary>
        private Hint[] hints = new Hint[0];

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
        private float holdSecondaryTime = 0.5f;

        /// <summary>
        /// Function to obtain bounds that this modal should avoid overlapping.
        /// </summary>
        public delegate Bounds GetAvoidanceArea();

        /// <summary>
        /// Callback to get the bounds this modal should avoid overlap with.
        /// </summary>
        private GetAvoidanceArea avoid;

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
            bool showCloseInput = true,
            RectTransform root = null,
            bool takeInputControl = true,
            bool showBackground = true,
            MakeHint[] hints = null
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
                showCloseInput,
                root,
                takeInputControl,
                showBackground,
                hints
            );
        }
        
        /// <summary>
        /// Convenience method for showing a simple popup that points/hints towards some transform(s) in the world.
        /// </summary>
        public static Modal Show(
            UIController ui,
            Modal prefab,
            string title,
            string description,
            Transform[] pointAtTargets,
            Sprite pointerSprite = null,
            string textPrimary = null, UnityAction actionPrimary = null,
            UnityAction onClose = null,
            bool showCloseInput = true,
            RectTransform root = null,
            bool takeInputControl = true,
            bool showBackground = true
        )
        {
            MakeHint[] hints = new MakeHint[pointAtTargets.Length];
            if (pointerSprite == null)
            {
                pointerSprite = Resources.Load<Sprite>("Sprites/Pixel");
            }
            for (int i = 0, counti = pointAtTargets.Length; i < counti; i++)
            {
                int index = i;
                hints[i] = (Modal modal) => {
                    GameObject hintObj = new GameObject("ModalHint_" + index);
                    hintObj.transform.parent = ui.modalsRoot;
                    hintObj.AddComponent<Image>().sprite = pointerSprite;
                    (hintObj.transform as RectTransform).sizeDelta = new Vector2(4, 4);
                    (hintObj.transform as RectTransform).pivot = new Vector2(0, 0.5f);
                    Hint hint = hintObj.AddComponent<PointerHint>();
                    PointerHint.Parameters args = new PointerHint.Parameters();
                    args.camera = Camera.main;
                    hint.SetHintTarget(pointAtTargets[index], args);
                    return hint;
                };
            }
            Modal modal = Show(
                ui,
                prefab,
                title,
                description,
                textPrimary, actionPrimary,
                null, null,
                onClose,
                showCloseInput,
                root,
                takeInputControl,
                showBackground,
                hints
            );
            return modal;
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
            bool showCloseInput = true,
            RectTransform root = null,
            bool takeInputControl = true,
            bool showBackground = true,
            MakeHint[] hints = null
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
                modal = Instantiate(prefab, root ?? ui.modalsRoot ?? ui.transform);
                modal.Init(new Parameters(
                    title,
                    description,
                    textPrimary,
                    actionPrimary,
                    textSecondary,
                    actionSecondary,
                    onClose,
                    showCloseInput,
                    takeInputControl
                ), ui);
                modal.background?.gameObject?.SetActive(showBackground);
                // TODO: Fade in modal
                modal.canvasGroup.alpha = 1;
                if (takeInputControl)
                {
                    ui.input.RequestInputControl(modal.gameObject);
                }
                if (hints != null)
                {
                    modal.hints = hints.Where(x => x != null).Select(x => x(modal)).ToArray();
                    for (int i = 0, counti = modal.hints.Length; i < counti; i++)
                    {
                        // For screen-space targets, make sure the pointer is attached & displayed beneath the popup
                        // or to the specified origin
                        modal.hints[i].SetHintTarget(modal.hints[i].target, modal.hints[i].hintData, modal.transform);
                    }
                }
                else
                {
                    modal.hints = new Hint[0];
                }
            }
            else
            {
                Log.Error("Failed to open modal popup as HUD instance could not be retrieved!");
            }
            return modal;
        }

        /// <summary>
        /// Get text string.
        /// </summary>
        private static string GetText(MaskableGraphic textGraphic)
        {
            return (textGraphic is TMPro.TextMeshProUGUI) ?
                (textGraphic as TMPro.TextMeshProUGUI).text : (textGraphic as Text).text;
        }

        /// <summary>
        /// Set text string.
        /// </summary>
        private static void SetText(MaskableGraphic textGraphic, string text)
        {
            if (textGraphic is TMPro.TextMeshProUGUI)
            {
                (textGraphic as TMPro.TextMeshProUGUI).text = text;
            }
            else
            {
                (textGraphic as Text).text = text;
            }
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
        /// Automatically reposition to avoid overlap with the given bounds.
        /// </summary>
        public void SetAvoidance(GetAvoidanceArea callback)
        {
            avoid = callback;
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

            if (avoid != null)
            {
                main.localPosition = Vector3.zero;
                Bounds bounds = main.GetTrueBounds();
                Vector2 delta = bounds.GetOverlapDelta(avoid()) * main.lossyScale * 0.5f;
                if (delta != Vector2.zero)
                {
                    //Log.Debug("Moving bounds from {0} to {1} (delta = {2})", bounds.center, bounds.center + (Vector3)delta, delta);
                }
                main.position = bounds.center + (Vector3)delta;
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

            // Primary button
            bool show = !string.IsNullOrEmpty(data.textPrimary);
            inputPrimary?.gameObject.SetActive(show);
            if (textPrimaryGraphic != null)
            {
                textPrimary = data.textPrimary;
                textPrimaryGraphic.gameObject.SetActive(show);
            }
            show |= data.actionPrimary != null;
            if (textPrimaryGraphic != null)
            {
                textPrimaryGraphic.transform.parent.gameObject.SetActive(show);
            }

            // Secondary button
            show = !string.IsNullOrEmpty(data.textSecondary);
            inputSecondary?.gameObject.SetActive(show);
            if (textSecondaryGraphic != null)
            {
                textSecondary = data.textSecondary;
                textSecondaryGraphic.gameObject.SetActive(show);
            }
            show |= data.actionSecondary != null;
            if (textSecondaryGraphic != null)
            {
                textSecondaryGraphic.transform.parent.gameObject.SetActive(show);
            }

            // Setup all text
            // TODO: Localise text with IDs
            if (title != null && !string.IsNullOrEmpty(data.title))
            {
                title = data.title;
            }
            if (!string.IsNullOrEmpty(data.description))
            {
                description = data.description;
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
            if (data.takeInputControl)
            {
                ui.input.FreeInputControl(gameObject);
            }

            for (int i = 0, counti = hints.Length; i < counti; i++)
            {
                Destroy(hints[i].gameObject);
            }

            data.onClose?.Invoke();
            // TODO: Fade out before destroying
            Destroy(gameObject);
        }

    }

}
