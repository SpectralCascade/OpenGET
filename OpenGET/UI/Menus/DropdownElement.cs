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
    /// </summary>
    public class DropdownElement : AutoBehaviour, IElement, IPointerEnterHandler, IPointerExitHandler
    {

        public UnityEngine.Events.UnityEvent<bool> onHoverChange = new UnityEngine.Events.UnityEvent<bool>();

        /// <summary>
        /// Text showing the name of this element.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup]
        public TMPro.TextMeshProUGUI text;

        /// <summary>
        /// Associated dropdown.
        /// </summary>
        [SerializeField]
        [Auto.NullCheck]
        [Auto.Hookup]
        private TMPro.TMP_Dropdown dropdown;

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
        /// Generate and show the options list.
        /// </summary>
        public void Init()
        {
            dropdown.ClearOptions();

            dropdown.options = new List<TMPro.TMP_Dropdown.OptionData>(
                options.Select(x => new TMPro.TMP_Dropdown.OptionData(Localise.Text(x)))
            );

            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener((x) => {
                value = x;
                dropdown.Hide();
                onOptionChanged?.Invoke(x);
            });
        }

        /// <summary>
        /// The option index.
        /// </summary>
        public int value {
            get { return (int)GetValue(); }
            private set { SetValue(value); }
        }

        public void SetValue(object value)
        {
            if (dropdown.value != (int)value)
            {
                dropdown.value = (int)value;
                onOptionChanged?.Invoke(dropdown.value);
            }
        }

        public object GetValue()
        {
            return dropdown.value;
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
