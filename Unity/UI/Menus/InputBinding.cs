using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OpenGET.UI
{

    /// <summary>
    /// Element used to modify a single binding of an InputAction.
    /// Partly based on the Unity InputSystem RebindActionUI sample script.
    /// </summary>
    public class InputBinding : ScrollPassback
    {
        // This is a necessary evil due to input actions being tied to specific runtime instances of InputActionAsset,
        // rather than referencing a single instance of an asset
        private UIController UI => _UI = (_UI != null ? _UI : GetComponentInParent<UIController>());
        private UIController _UI = null;

        private InputActionElement actionElement;

        /// <summary>
        /// Generate and show the options list.
        /// </summary>
        public void Init(InputActionElement actionElement, int index)
        {
            this.actionElement = actionElement;
            bindingId = actionElement.action.bindings[index].id.ToString();
            if (s_RebindActionUIs == null)
            {
                s_RebindActionUIs = new List<InputBinding>();
            }
            s_RebindActionUIs.Add(this);
            
            if (s_RebindActionUIs.Count == 1)
            {
                InputSystem.onActionChange += OnActionChange;
            }
        }

#if ENABLE_INPUT_SYSTEM

        /// <summary>
        /// Reference to the action that is to be rebound.
        /// </summary>
        public InputAction action {
            get => actionElement != null ? actionElement.action : null;
        }

        /// <summary>
        /// ID (in string form) of the binding that is to be rebound on the action.
        /// </summary>
        /// <seealso cref="UnityEngine.InputSystem.InputBinding.id"/>
        public string bindingId {
            get => _bindingId;
            set {
                _bindingId = value;
                UpdateBindingDisplay();
            }
        }

        public UnityEngine.InputSystem.InputBinding.DisplayStringOptions displayStringOptions {
            get => m_DisplayStringOptions;
            set {
                m_DisplayStringOptions = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Text component that receives the display string of the binding. Can be <c>null</c> in which
        /// case the component entirely relies on <see cref="updateBindingUIEvent"/>.
        /// </summary>
        public TMPro.TextMeshProUGUI bindingText {
            get => _bindingText;
            set {
                _bindingText = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Optional text component that receives a text prompt when waiting for a control to be actuated.
        /// </summary>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="rebindOverlay"/>
        public TMPro.TextMeshProUGUI rebindPrompt {
            get => _rebindPromptText;
            set => _rebindPromptText = value;
        }

        /// <summary>
        /// Optional UI that is activated when an interactive rebind is started and deactivated when the rebind
        /// is finished. This is normally used to display an overlay over the current UI while the system is
        /// waiting for a control to be actuated.
        /// </summary>
        /// <remarks>
        /// If neither <see cref="rebindPrompt"/> nor <c>rebindOverlay</c> is set, the component will temporarily
        /// replaced the <see cref="bindingText"/> (if not <c>null</c>) with <c>"Waiting..."</c>.
        /// </remarks>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="rebindPrompt"/>
        public GameObject rebindOverlay {
            get => rebindInProgressPopup;
            set => rebindInProgressPopup = value;
        }

        /// <summary>
        /// Event that is triggered every time the UI updates to reflect the current binding.
        /// This can be used to tie custom visualizations to bindings.
        /// </summary>
        public UpdateBindingUIEvent updateBindingUIEvent {
            get {
                if (m_UpdateBindingUIEvent == null)
                {
                    m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
                }
                return m_UpdateBindingUIEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind is started on the action.
        /// </summary>
        public InteractiveRebindEvent startRebindEvent {
            get {
                if (m_RebindStartEvent == null)
                {
                    m_RebindStartEvent = new InteractiveRebindEvent();
                }
                return m_RebindStartEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind has been completed or canceled.
        /// </summary>
        public InteractiveRebindEvent stopRebindEvent {
            get {
                if (m_RebindStopEvent == null)
                {
                    m_RebindStopEvent = new InteractiveRebindEvent();
                }
                return m_RebindStopEvent;
            }
        }

        /// <summary>
        /// When an interactive rebind is in progress, this is the rebind operation controller.
        /// Otherwise, it is <c>null</c>.
        /// </summary>
        public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

        /// <summary>
        /// Keep track of whether the action map being rebound should be enabled.
        /// </summary>
        private bool wasEnabled = false;

        /// <summary>
        /// Return the action and binding index for the binding that is targeted by the component.
        /// </summary>
        public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
        {
            bindingIndex = -1;

            action = this.action;
            if (action == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_bindingId))
            {
                return false;
            }

            // Look up binding index.
            Guid bindingId = new System.Guid(_bindingId);
            bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
            if (bindingIndex == -1)
            {
                Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Trigger a refresh of the currently displayed binding.
        /// </summary>
        public void UpdateBindingDisplay()
        {
            string displayString = string.Empty;
            string deviceLayoutName = default;
            string controlPath = default;

            // Get display string from action.
            InputAction action = this.action;
            if (action != null)
            {
                int bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == _bindingId);
                if (bindingIndex != -1)
                {
                    displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, displayStringOptions);
                }
            }

            // Set on label (if any).
            if (_bindingText != null)
            {
                _bindingText.text = displayString;
            }

            // Give listeners a chance to configure UI in response.
            m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        }

        /// <summary>
        /// Remove currently applied binding overrides.
        /// </summary>
        public void ResetToDefault()
        {
            if (!ResolveActionAndBinding(out InputAction action, out int bindingIndex))
            {
                return;
            }

            if (action.bindings[bindingIndex].isComposite)
            {
                // It's a composite. Remove overrides from part bindings.
                for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                {
                    action.RemoveBindingOverride(i);
                }
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }
            UpdateBindingDisplay();
        }

        /// <summary>
        /// Initiate an interactive rebind that lets the player actuate a control to choose a new binding
        /// for the action.
        /// </summary>
        public void StartInteractiveRebind()
        {
            if (!ResolveActionAndBinding(out InputAction action, out int bindingIndex))
            {
                return;
            }

            // If the binding is a composite, we need to rebind each part in turn.
            if (action.bindings[bindingIndex].isComposite)
            {
                int firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                {
                    PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
                }
            }
            else
            {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            Log.Debug("Performing rebind of action {0}", action.name);

            m_RebindOperation?.Cancel(); // Will null out m_RebindOperation.

            wasEnabled = action.actionMap.enabled;
            if (wasEnabled)
            {
                action.actionMap.Disable();
            }

            void CleanUp()
            {
                m_RebindOperation?.Dispose();
                m_RebindOperation = null;
                if (wasEnabled)
                {
                    action.actionMap.Enable();
                }
            }

            // Configure the rebind.
            m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .OnCancel(
                    operation => {
                        m_RebindStopEvent?.Invoke(this, operation);
                        if (rebindInProgressPopup != null)
                        {
                            rebindInProgressPopup.SetActive(false);
                        }
                        UpdateBindingDisplay();
                        CleanUp();
                    })
                .OnComplete(
                    operation => {
                        if (rebindInProgressPopup != null)
                        {
                            rebindInProgressPopup?.SetActive(false);
                        }

                        // Apply to ALL action map instances
                        for (int j = 0, countj = UI.inputActionAssets.Length; j < countj; j++)
                        {
                            InputActionAsset asset = UI.inputActionAssets[j];
                            InputAction found = asset.FindAction(action.id);
                            if (found != null) {
                                bool mapEnabled = found.actionMap.enabled;
                                if (mapEnabled)
                                {
                                    found.actionMap.Disable();
                                }
                                found.ApplyBindingOverride(bindingIndex, action.bindings[bindingIndex]);
                                if (mapEnabled)
                                {
                                    found.actionMap.Enable();
                                }
                            }
                        }

                        m_RebindStopEvent?.Invoke(this, operation);
                        UpdateBindingDisplay();
                        CleanUp();

                        // If there's more composite parts we should bind, initiate a rebind
                        // for the next part.
                        if (allCompositeParts)
                        {
                            int nextBindingIndex = bindingIndex + 1;
                            if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                            {
                                PerformInteractiveRebind(action, nextBindingIndex, true);
                            }
                        }
                    });

            // If it's a part binding, show the name of the part in the UI.
            string partName = default;
            if (action.bindings[bindingIndex].isPartOfComposite)
            {
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";
            }

            // Bring up rebind overlay, if we have one.
            if (rebindInProgressPopup != null) {
                rebindInProgressPopup.SetActive(true);
            }
            if (_rebindPromptText != null)
            {
                string text = !string.IsNullOrEmpty(m_RebindOperation.expectedControlType)
                    ? $"{partName}Waiting for {m_RebindOperation.expectedControlType} input..."
                    : $"{partName}Waiting for input...";
                _rebindPromptText.text = text;
            }

            // If we have no rebind overlay and no callback but we have a binding text label,
            // temporarily set the binding text label to "<Waiting>".
            if (rebindInProgressPopup == null && _rebindPromptText == null && m_RebindStartEvent == null && _bindingText != null)
            {
                _bindingText.text = "<Waiting...>";
            }

            // Give listeners a chance to act on the rebind starting.
            m_RebindStartEvent?.Invoke(this, m_RebindOperation);

            m_RebindOperation.Start();
        }

        protected void OnDisable()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;

            if (s_RebindActionUIs != null)
            {
                s_RebindActionUIs.Remove(this);
                if (s_RebindActionUIs.Count == 0)
                {
                    s_RebindActionUIs = null;
                    InputSystem.onActionChange -= OnActionChange;
                }
            }
        }

        // When the action system re-resolves bindings, we want to update our UI in response. While this will
        // also trigger from changes we made ourselves, it ensures that we react to changes made elsewhere. If
        // the user changes keyboard layout, for example, we will get a BoundControlsChanged notification and
        // will update our UI to reflect the current keyboard layout.
        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
            {
                return;
            }

            InputAction action = obj as InputAction;
            InputActionMap actionMap = action?.actionMap ?? obj as InputActionMap;
            InputActionAsset actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            for (int i = 0; i < s_RebindActionUIs.Count; ++i)
            {
                InputBinding component = s_RebindActionUIs[i];
                InputAction referencedAction = component.action;
                if (referencedAction == null)
                {
                    continue;
                }

                if (referencedAction == action || referencedAction.actionMap == actionMap || referencedAction.actionMap?.asset == actionAsset)
                {
                    component.UpdateBindingDisplay();
                }
            }
        }

        /// <summary>
        /// Id of the specific binding to be modified in the action.
        /// </summary>
        private string _bindingId;

        [SerializeField]
        private UnityEngine.InputSystem.InputBinding.DisplayStringOptions m_DisplayStringOptions;

        [Tooltip("Text label that will receive the current, formatted binding string.")]
        [SerializeField]
        [Auto.NullCheck]
        private TMPro.TextMeshProUGUI _bindingText;

        [Tooltip("Optional UI that will be shown while a rebind is in progress.")]
        [SerializeField]
        private GameObject rebindInProgressPopup;

        [Tooltip("Optional text label that will be updated with prompt for user input.")]
        [SerializeField]
        private TMPro.TextMeshProUGUI _rebindPromptText;

        [Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying "
            + "bindings in custom ways, e.g. using images instead of text.")]
        [SerializeField]
        private UpdateBindingUIEvent m_UpdateBindingUIEvent;

        [Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, "
            + "to implement custom UI behavior while a rebind is in progress. It can also be used to further "
            + "customize the rebind.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStartEvent;

        [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStopEvent;

        private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

        private static List<InputBinding> s_RebindActionUIs;

        // We want the label for the action name to update in edit mode, too, so
        // we kick that off from here.
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            UpdateBindingDisplay();
        }

#endif

        [Serializable]
        public class UpdateBindingUIEvent : UnityEvent<InputBinding, string, string, string>
        {
        }

        [Serializable]
        public class InteractiveRebindEvent : UnityEvent<InputBinding, InputActionRebindingExtensions.RebindingOperation>
        {
        }
#endif
    }

}
