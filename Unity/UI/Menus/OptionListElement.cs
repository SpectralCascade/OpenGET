using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// Similar to a dropdown but rather than overlaying on the current screen, acts as it's own view.
    /// </summary>
    public class OptionListElement : TextButton, IElement
    {

        [Header("OptionsListElement")]

        /// <summary>
        /// The panel to use.
        /// </summary>
        public ViewPanel optionsMenu;

        /// <summary>
        /// The root gameobject to build options under.
        /// </summary>
        public GameObject optionsRoot;

        /// <summary>
        /// The prefab to use for generating the option buttons.
        /// </summary>
        [SerializeField]
        private TextButton optionButtonPrefab;

        /// <summary>
        /// The original ViewPanel instance so we can return to it. May be null.
        /// </summary>
        public ViewPanel parentPanel = null;

        /// <summary>
        /// The formattable text to display on the button.
        /// </summary>
        public string optionSelectionText = "{0}";

        /// <summary>
        /// Array of all possible option names.
        /// TODO: Use localisation IDs.
        /// </summary>
        public string[] options = new string[0];

        /// <summary>
        /// The index of the selected option.
        /// </summary>
        private int _value = 0;

        /// <summary>
        /// Local options list builder instance.
        /// </summary>
        private MenuList.Builder builder = new MenuList.Builder();

        public delegate void OnOptionChanged(int value);

        /// <summary>
        /// Event to handle the current option being changed.
        /// </summary>
        public event OnOptionChanged onOptionChanged;

        protected override void Awake()
        {
            base.Awake();

            Debug.Assert(optionButtonPrefab != null);

            button.onClick.AddListener(GoOptions);
        }

        private void OnDestroy()
        {
            builder.Clean();
        }

        /// <summary>
        /// Handle the options list being closed.
        /// </summary>
        private void Cleanup(bool shown)
        {
            if (!shown)
            {
                builder.Clean();
                optionsMenu.onSetShown -= Cleanup;
            }
        }

        /// <summary>
        /// Generate and show the options list.
        /// </summary>
        public void GoOptions()
        {
            if (parentPanel != null && optionsMenu != null && optionsRoot != null)
            {
                optionsMenu.onSetShown += Cleanup;

                builder.Begin(optionsRoot.gameObject);

                // Add all possible options to the list
                for (int i = 0, counti = options.Length; i < counti; i++)
                {
                    builder.Add(optionButtonPrefab, (TextButton button) => {
                        // Capture index by value for inner lambda
                        int index = i;
                        // TODO: Localise
                        button.text.text = options[i];
                        button.onClick.AddListener(() => {
                            Debug.Log("Setting option to " + index.ToString());
                            value = index;
                            optionsMenu.backButton.onClick.Invoke();
                        });
                        //button.group = optionsMenu.backButton.gameObject.name;
                        button.gameObject.name = "OptionButton" + i.ToString();
                    });

                }

                builder.End();

                // Make sure the player can use the back button, if available, and move it to the end if a child of the root.
                if (optionsMenu.backButton.transform.parent == optionsRoot.transform)
                {
                    optionsMenu.backButton.transform.SetAsLastSibling();
                }
                else if (optionsMenu.backButton.transform.parent.parent == optionsRoot.transform)
                {
                    optionsMenu.backButton.transform.parent.SetAsLastSibling();
                }

                // Show the options list
                parentPanel.Push(optionsMenu);

            }
            else
            {
                Debug.LogError(
                    "Cannot show options list as " + (parentPanel == null ? "parentPanel" : (optionsMenu == null ? "optionsMenu" : "optionsRoot")) + " has not been set.",
                    gameObject
                );
            }
        }

        /// <summary>
        /// The enum integer value.
        /// </summary>
        public int value {
            get { return (int)GetValue(); }
            private set { SetValue(value); }
        }

        public void SetValue(object value)
        {
            if (_value != (int)value)
            {
                _value = (int)value;
                if (options.Length > 0)
                {
                    text.text = string.Format(optionSelectionText, options[_value]);
                }
                onOptionChanged?.Invoke(_value);
            }
        }

        public object GetValue()
        {
            return _value;
        }

    }

}
