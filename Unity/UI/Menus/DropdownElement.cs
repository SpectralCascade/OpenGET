using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

namespace OpenGET.UI
{

    /// <summary>
    /// Similar to a dropdown but rather than overlaying on the current screen, acts as it's own view.
    /// Note: May be set to use carousel mode where no actual dropdown is shown (instead options are in a carousel layout).
    /// </summary>
    public class DropdownElement : Element, IPointerEnterHandler, IPointerExitHandler
    {

        public UnityEngine.Events.UnityEvent<bool> onHoverChange = new UnityEngine.Events.UnityEvent<bool>();

        /// <summary>
        /// Text showing the name of this element.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup]
        public TMPro.TextMeshProUGUI text;

        /// <summary>
        /// Label showing the chosen option. Optional (not required if a dropdown is used).
        /// </summary>
        public TMPro.TextMeshProUGUI label;

        /// <summary>
        /// Associated dropdown, if any. Not used in carousel mode.
        /// </summary>
        [SerializeField]
        private TMPro.TMP_Dropdown dropdown;

        /// <summary>
        /// Optional index increment button.
        /// </summary>
        [SerializeField]
        private Button indexIncButton;

        /// <summary>
        /// Optional index decrement button.
        /// </summary>
        [SerializeField]
        private Button indexDecButton;

        /// <summary>
        /// Get/set dropdown options.
        /// </summary>
        [System.NonSerialized]
        public string[] options = new string[0];

        public delegate void OnOptionChanged(int value);

        /// <summary>
        /// Event to handle the current option being changed.
        /// </summary>
        public event OnOptionChanged onOptionChanged;

        protected void OnEnable()
        {
            Init();
        }

        /// <summary>
        /// Generate and show the options list (or setup carousel mode).
        /// </summary>
        public void Init(int value = -1)
        {
            if (dropdown != null)
            {
                if (value < 0)
                {
                    value = dropdown.value;
                }
                dropdown.ClearOptions();

                dropdown.options = new List<TMPro.TMP_Dropdown.OptionData>(
                    options.Select(x => new TMPro.TMP_Dropdown.OptionData(Localise.Text(x)))
                );

                if (value > -1)
                {
                    dropdown.value = value;
                }

                dropdown.onValueChanged.RemoveAllListeners();
                dropdown.onValueChanged.AddListener((x) =>
                {
                    value = x;
                    dropdown.Hide();
                    onOptionChanged?.Invoke(x);
                });
            }
            _value = value >= 0 && _value < options.Length ? value : _value;

            if (indexIncButton == null && indexDecButton != null)
            {
                // Hide if only one of the buttons available.
                indexDecButton.gameObject.SetActive(false);
            }
            else if (indexDecButton == null && indexIncButton != null)
            {
                // Hide if only one of the buttons available.
                indexIncButton.gameObject.SetActive(false);
            }
            else if (indexDecButton != null && indexIncButton != null)
            {
                // Both buttons available, setup input actions
                indexDecButton.onClick.RemoveAllListeners();
                indexDecButton.onClick.AddListener(OnDec);

                indexIncButton.onClick.RemoveAllListeners();
                indexIncButton.onClick.AddListener(OnInc);

                // Update the initial state
                UpdateState();
            }
        }

        /// <summary>
        /// Handle index decrement.
        /// </summary>
        private void OnDec()
        {
            if (value > 0)
            {
                value--;
            }
        }

        /// <summary>
        /// Handle index increment.
        /// </summary>
        private void OnInc()
        {
            if (value < options.Length - 1)
            {
                value++;
            }
        }

        /// <summary>
        /// The option index.
        /// </summary>
        public int value
        {
            get { return (int)GetValue(); }
            private set { SetValue(value); }
        }

        /// <summary>
        /// Local option index if we have no dropdown.
        /// </summary>
        private int _value;

        public override void SetValue(object value)
        {
            if ((int)value != _value)
            {
                if (dropdown != null)
                {
                    dropdown.value = (int)value;
                }
                _value = (int)value;
                onOptionChanged?.Invoke(_value);
                UpdateState();
            }
        }

        public override object GetValue()
        {
            return _value;
        }


        private void UpdateState()
        {
            if (label != null && _value >= 0 && _value < options.Length)
            {
                label.text = Localise.Text(options[_value]);
            }

            // Update buttons
            if (indexDecButton != null && indexIncButton != null)
            {
                indexDecButton.interactable = _value > 0;
                indexIncButton.interactable = _value < options.Length - 1;
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            onHoverChange?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onHoverChange?.Invoke(false);
        }
    }

}
