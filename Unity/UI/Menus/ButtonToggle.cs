using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// A button that can be toggled.
    /// </summary>
    public class ButtonToggle : TextButton, IElement
    {
        [Header("ButtonToggle")]

        /// <summary>
        /// The toggle checkbox mark, indicating whether the setting is on or off.
        /// </summary>
        public GameObject checkmark;

        /// <summary>
        /// Store state in a boolean, NOT the checkmark!
        /// </summary>
        private bool _value;

        /// <summary>
        /// Optional dropdown/carousel to use instead of the checkmark.
        /// </summary>
        public DropdownElement dropdown;

        /// <summary>
        /// Reference to container if available.
        /// </summary>
        [System.NonSerialized]
        public ElementContainer container;

        /// <summary>
        /// Is this button toggled on or off? Defaults to false.
        /// </summary>
        public bool value {
            get { return (bool)GetValue(); }
            private set { SetValue(value); }
        }

        public delegate void OnToggledEvent(bool value);

        /// <summary>
        /// Event to handle the button being toggled.
        /// </summary>
        public event OnToggledEvent onToggle;

        protected override void Awake()
        {
            base.Awake();

            if (dropdown != null)
            {
                dropdown.options = new string[] {
                    Localise.Runtime("Disabled"),
                    Localise.Runtime("Enabled")
                };
                dropdown.onOptionChanged += OnToggledDropdown;
                if (checkmark != null)
                {
                    checkmark.SetActive(false);
                }
            }
            else
            {
                button.onClick.AddListener(OnToggled);
            }
        }

        private void OnDestroy()
        {
            if (dropdown != null)
            {
                dropdown.onOptionChanged -= OnToggledDropdown;
            }
            else
            {
                button.onClick.RemoveListener(OnToggled);
            }
        }

        private void OnToggledDropdown(int index)
        {
            SetValue(index != 0);
            // Always toggle on dropdown regardless of current state, as it is guaranteed only on change
            onToggle?.Invoke(_value);
        }

        /// <summary>
        /// Callback to handle the button being clicked.
        /// </summary>
        private void OnToggled()
        {
            SetValue(!_value);
        }

        /// <summary>
        /// Update state and trigger the onToggle callback if value is changed.
        /// </summary>
        public void SetValue(object value)
        {
            if (checkmark != null)
            {
                if (dropdown != null)
                {
                    checkmark.SetActive(false);
                }
                checkmark.SetActive((bool)value);
            }

            if ((bool)value != this.value)
            {
                _value = (bool)value;
                if (dropdown != null)
                {
                    dropdown.SetValue((bool)value ? 1 : 0);
                }
                onToggle?.Invoke(_value);
            }
        }

        /// <summary>
        /// Get the cached value.
        /// </summary>
        public object GetValue()
        {
            return _value;
        }

        public ElementContainer TryGetContainer()
        {
            return container;
        }
    }

}
