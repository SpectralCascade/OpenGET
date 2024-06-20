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
            Debug.Assert(checkmark != null);

            button.onClick.AddListener(OnToggled);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnToggled);
            }
        }

        /// <summary>
        /// Callback to handle the button being clicked.
        /// </summary>
        private void OnToggled()
        {
            SetValue(!value);
        }

        /// <summary>
        /// Update state and trigger the onToggle callback if value is changed.
        /// </summary>
        public void SetValue(object value)
        {
            if (checkmark.activeSelf != (bool)value)
            {
                checkmark.SetActive((bool)value);
                onToggle?.Invoke(checkmark.activeSelf);
            }
        }

        /// <summary>
        /// Get the cached value.
        /// </summary>
        public object GetValue()
        {
            return checkmark.activeSelf;
        }

        public ElementContainer TryGetContainer()
        {
            return container;
        }
    }

}
