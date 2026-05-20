using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.UI;
#endif

namespace OpenGET.UI
{

    /// <summary>
    /// Lists one or more editable input bindings associated with an InputAction.
    /// </summary>
    public class InputActionElement : Element, NavigationBlock.IElement
    {
        /// <summary>
        /// Prefab for editing a single binding associated with this action.
        /// </summary>
        [Auto.NullCheck]
        public InputBinding prefabRebind;

        /// <summary>
        /// Where all the binding elements live.
        /// </summary>
        [Auto.NullCheck]
        public Transform bindsRoot;

        /// <summary>
        /// WARNING: Some entries may be null e.g. parts of composite bindings, or hidden bindings.
        /// </summary>
        private List<InputBinding> binds = new List<InputBinding>();

        /// <summary>
        /// A navigation block is expected.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup]
        public NavigationBlock navigationBlock;

        [Tooltip("Optional text label that will receive the name of the action.")]
        [SerializeField]
        [Auto.NullCheck]
        private TMPro.TextMeshProUGUI _actionLabel;

        /// <summary>
        /// Which control schemes should be shown by this element. If null, shows all available bindings.
        /// </summary>
        public string[] enabledControls = null;

        /// <summary>
        /// Called when a binding is modified for this action.
        /// </summary>
        public UnityEngine.Events.UnityEvent<InputAction> onBindChanged = new UnityEngine.Events.UnityEvent<InputAction>();

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Reference to the action that is to be rebound.
        /// </summary>
        public InputAction action
        {
            get => _action;
            set
            {
                _action = value;
                UpdateBindings();
            }
        }

        private InputBinding selectedBinding => binds.FirstOrDefault(x => x != null);

        public Navigation navigation { get => selectedBinding != null ? selectedBinding.navigation : new Navigation(); set => SetupNavigation(value); }

        public Selectable selectable => selectedBinding != null ? selectedBinding.selectable : null;

        /// <summary>
        /// Action instance. Note - this is NOT necessarily the only instance to be modified.
        /// </summary>
        private InputAction _action;

        /// <summary>
        /// Assumes horizontal layout for bindings.
        /// </summary>
        private void SetupNavigation(Navigation nav)
        {
            int prev = -1;
            for (int i = 0, counti = binds.Count; i < counti; i++)
            {
                if (binds[i] != null)
                {
                    binds[i].navigation = new Navigation()
                    {
                        selectOnDown = nav.selectOnDown,
                        selectOnUp = nav.selectOnUp,
                        selectOnLeft = prev >= 0 && binds[prev] != null ? binds[prev].selectable : null,
                        selectOnRight = null, // This is set by the next bind
                        mode = Navigation.Mode.Explicit,
                        wrapAround = false
                    };

                    // Set previous nav right select
                    if (prev >= 0)
                    {
                        Navigation prevNav = binds[prev].navigation;
                        prevNav.selectOnRight = binds[i].selectable;
                        binds[prev].navigation = prevNav;
                    }

                    prev = i;
                }
            }
        }

#endif

        public void UpdateActionLabel()
        {
#if ENABLE_INPUT_SYSTEM
            if (_actionLabel != null)
            {
                _actionLabel.text = action != null ? action.name : string.Empty;
            }
#endif
        }

        public void UpdateBindings()
        {
#if ENABLE_INPUT_SYSTEM
            if (action != null)
            {
                // Setup bindings
                for (int i = 0, counti = action.bindings.Count; i < counti; i++)
                {
                    InputBinding bind = null;
                    if (i >= binds.Count)
                    {
                        // Only show bindings for enabled control schemes (AKA bind groups)
                        UnityEngine.InputSystem.InputBinding binding = action.bindings[i];
                        bool bindingEnabled = enabledControls == null || enabledControls.FirstOrDefault(
                            x =>
                            {
                                string groups = binding.groups;
                                if (!string.IsNullOrEmpty(groups) && groups.Contains(x))
                                {
                                    return true;
                                }
                                if (binding.isComposite)
                                {
                                    // Step through subsequent composite parts and check for a valid one
                                    for (int index = i + 1; index < counti; index++)
                                    {
                                        UnityEngine.InputSystem.InputBinding candidate = action.bindings[index];
                                        if (!candidate.isPartOfComposite)
                                        {
                                            break;
                                        }
                                        if (candidate.groups.Contains(x))
                                        {
                                            return true;
                                        }
                                    }
                                }
                                return false;
                            }
                        ) != null;

                        // Don't show individual parts of composite bindings
                        bind = binding.isPartOfComposite || !bindingEnabled ? null : Instantiate(prefabRebind, bindsRoot);
                        binds.Add(bind);

                        // Handle rebind event
                        if (bind != null)
                        {
                            bind.stopRebindEvent.AddListener((bindData, op) =>
                            {
                                if (!op.canceled && op.completed)
                                {
                                    onBindChanged?.Invoke(action);
                                }
                            });
                        }
                    }
                    bind = binds[i];
                    if (bind != null)
                    {
                        bind.Init(this, i);
                    }
                }

                // Cleanup old bindings if any
                for (int i = action.bindings.Count; i < binds.Count;)
                {
                    if (binds[i] != null)
                    {
                        Destroy(binds[i]);
                    }
                    binds.SwapRemoveAt(i);
                }
            }
            navigationBlock.SetDirty(NavigationBlock.Refresh.Navigation);
            UpdateActionLabel();
#endif
        }

        public override object GetValue()
        {
#if ENABLE_INPUT_SYSTEM
            return _action;
#else
            return null;
#endif
        }

        public override void SetValue(object value)
        {
#if ENABLE_INPUT_SYSTEM
            _action = value as InputAction;
#endif
        }
    }

}
