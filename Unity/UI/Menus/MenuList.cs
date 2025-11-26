using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;


#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OpenGET.UI
{

    /// <summary>
    /// A custom generated list of interactable UI elements such as buttons, sliders, toggles etc.
    /// </summary>
    public class MenuList : ViewPanel
    {

        [SerializeField]
        [Auto.NullCheck]
        [Tooltip("The root GameObject instance that all generated interactables are placed under.")]
        private GameObject root;

        [SerializeField]
        [Tooltip("This gameobject will always be moved to the end of the generated menu list, if specified.")]
        private GameObject bottomRoot = null;

        [SerializeField]
        [Auto.NullCheck]
        [Tooltip("The prefab for a toggle button element.")]
        private ButtonToggle elementTogglePrefab;

        [SerializeField]
        [Auto.NullCheck]
        [Tooltip("The prefab for a slider element.")]
        private SliderElement elementSliderPrefab;

        [SerializeField]
        [Auto.NullCheck]
        [Tooltip("The prefab for a dropdown list element.")]
        private DropdownElement elementDropdownPrefab;

        [SerializeField]
        [Tooltip("Optional container prefab for elements.")]
        private ElementContainer elementContainerPrefab;

        [SerializeField]
        [Tooltip("The prefab for an input binding element.")]
        private InputBindElement elementInputBindingPrefab;

        /// <summary>
        /// The local menu builder instance.
        /// </summary>
        public readonly Builder builder = new Builder();

        /// <summary>
        /// Used to build the menu.
        /// </summary>
        public class Builder
        {

            /// <summary>
            /// Has this build completed?
            /// </summary>
            public bool complete { get; private set; }

            /// <summary>
            /// List of all elements with a particular value associated.
            /// </summary>
            public readonly List<IElement> elements = new List<IElement>();

            /// <summary>
            /// List of all added components, regardless of whether they're an element or not.
            /// </summary>
            public readonly List<MonoBehaviour> added = new List<MonoBehaviour>();

            /// <summary>
            /// Begin the list generation/building process.
            /// </summary>
            public Builder Begin(GameObject root)
            {
                Clean();
                this.root = root;
                return this;
            }

            /// <summary>
            /// Destroy all built objects.
            /// </summary>
            public Builder Clean()
            {
                for (int i = 0, counti = added.Count; i < counti; i++)
                {
                    if (added[i] != null)
                    {
                        Destroy(added[i].gameObject);
                    }
                }
                added.Clear();
                elements.Clear();
                complete = false;
                return this;
            }

            public delegate void OnAdded<T>(T added) where T : MonoBehaviour;

            /// <summary>
            /// Add a custom prefab that implements IElement.
            /// </summary>
            public ElementContainer Add<T, V>(T prefab, V value, OnAdded<T> onAdded = null, ElementContainer containerPrefab = null) where T : MonoBehaviour, IElement
            {
                ElementContainer container = containerPrefab != null ? Instantiate(containerPrefab, root.transform) : null;
                T loaded = GameObject.Instantiate(prefab, (container != null ? container.content.transform : null) ?? root.transform);
                if (container != null)
                {
                    container.element = loaded;
                }
                added.Add(container as MonoBehaviour ?? loaded);
                elements.Add(container as IElement ?? loaded);

                loaded.SetValue(value);
                if (onAdded != null)
                {
                    onAdded(loaded);
                }
                return container;
            }

            /// <summary>
            /// Add a custom prefab that doesn't implement IElement (such as a button).
            /// </summary>
            public Builder Add<T>(T prefab, OnAdded<T> onAdded = null) where T : MonoBehaviour
            {
                T loaded = GameObject.Instantiate(prefab, root.transform);
                added.Add(loaded);
                if (onAdded != null)
                {
                    onAdded(loaded);
                }
                return this;
            }

            /// <summary>
            /// Initialise everything and end the list generating/building process.
            /// </summary>
            public void End()
            {
                complete = true;
            }

            /// <summary>
            /// The root gameobject under which the UI elements are placed.
            /// </summary>
            private GameObject root;

        }

        /// <summary>
        /// Build the menu list, using reflection. Returns the group type name.
        /// Optionally specify tooltip text that should be set when elements are hovered.
        /// </summary>
        public string Build(object group, TMPro.TextMeshProUGUI tooltipText = null, string groupDescription = null)
        {
            // Start the generation process
            builder.Begin(root);

            // Get all fields in settings and automagically create appropriate elements for them.
            System.Type groupType = group.GetType();
            FieldInfo[] fields = groupType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0, counti = fields.Length; i < counti; i++)
            {
                FieldInfo field = fields[i];
                System.Type type = field.FieldType;
                object fieldValue = field.GetValue(group);
                object applyField = fieldValue;
                ElementContainer container = null;
                bool hasApplyInterface = fieldValue is IApplySetting;
                if (hasApplyInterface)
                {
                    fieldValue = (fieldValue as IApplySetting).GetValue();
                    type = fieldValue.GetType();
                }
                // TODO: Localise text on elements
                if (type == typeof(float))
                {
                    // Slider
                    container = builder.Add(elementSliderPrefab, fieldValue, (SliderElement slider) => {
                        slider.text.text = (applyField as IApplySetting).GetName() ?? field.Name;
                        slider.button.onClick.AddListener(() => {
                            // TODO map slider value according to any field attributes if available.
                            if (hasApplyInterface)
                            {
                                (applyField as IApplySetting).SetValue(slider.normValue);
                                field.SetValue(group, applyField);
                            }
                            else
                            {
                                field.SetValue(group, slider.normValue);
                            }
                        });
                        if (tooltipText != null)
                        {
                            slider.onHoverChange.AddListener((bool enter) => {
                                if (enter)
                                {
                                    tooltipText.text = Localise.Text((applyField as IApplySetting).GetDescription());
                                }
                                else
                                {
                                    tooltipText.text = Localise.Text(groupDescription);
                                }
                            });
                        }
                        //slider.group = groupType.Name;
                        slider.gameObject.name = field.Name;
                    }, elementContainerPrefab);
                }
                else if (type == typeof(bool))
                {
                    // Toggle
                    container = builder.Add(elementTogglePrefab, fieldValue, (ButtonToggle button) => {
                        button.text.text = (applyField as IApplySetting).GetName() ?? field.Name;
                        button.onToggle += (bool toggled) => {
                            // Update the value in the referenced object.
                            if (hasApplyInterface)
                            {
                                (applyField as IApplySetting).SetValue(toggled);
                                field.SetValue(group, applyField);
                            }
                            else
                            {
                                field.SetValue(group, toggled);
                            }
                        };
                        if (tooltipText != null)
                        {
                            button.onHoverChange.AddListener((bool enter) => {
                                if (enter)
                                {
                                    tooltipText.text = Localise.Text((applyField as IApplySetting).GetDescription());
                                }
                                else
                                {
                                    tooltipText.text = Localise.Text(groupDescription);
                                }
                            });
                        }
                        //button.group = groupType.Name;
                        button.gameObject.name = field.Name;
                    }, elementContainerPrefab);

                }
                else if (type.IsEnum || type == typeof(int))
                {
                    // Dropdown selection
                    container = builder.Add(elementDropdownPrefab, (int)fieldValue, (DropdownElement dropdown) => {
                        // TODO: Localise option names instead of just grabbing enum member names...
                        bool isEnum = type.IsEnum;
                        if (isEnum)
                        {
                            dropdown.options = type.GetEnumNames();
                        }
                        else
                        {
                            dropdown.options = ((Setting<int>)applyField).customData.Select(x => x.ToString()).ToArray();
                        }
                        dropdown.text.text = Localise.Text((applyField as IApplySetting).GetName() ?? field.Name);
                        dropdown.onOptionChanged += (int value) => {
                            // Update the value in the referenced object
                            object valueChange = isEnum ? Enum.ToObject(type, value) : value;
                            if (hasApplyInterface)
                            {
                                (applyField as IApplySetting).SetValue(valueChange);
                                field.SetValue(group, applyField);
                            }
                            else
                            {
                                field.SetValue(group, valueChange);
                            }
                        };
                        if (tooltipText != null)
                        {
                            dropdown.onHoverChange.AddListener((bool enter) => {
                                if (enter)
                                {
                                    tooltipText.text = Localise.Text((applyField as IApplySetting).GetDescription());
                                }
                                else
                                {
                                    tooltipText.text = Localise.Text(groupDescription);
                                }
                            });
                        }
                        dropdown.gameObject.name = field.Name;
                        dropdown.Init();
                    }, elementContainerPrefab);
                }
#if ENABLE_INPUT_SYSTEM
                else if (fieldValue is IInputActionCollection2)
                {
                    IInputActionCollection2 actions = fieldValue as IInputActionCollection2;
                    foreach (InputAction action in actions) {
                        ElementContainer created = builder.Add(
                            elementInputBindingPrefab,
                            action,
                            (bindElement) => {
                                // TODO: Handle multiple bindings and control schemes
                                bindElement.bindingId = bindElement.action.bindings[0].id.ToString();
                            },
                            elementContainerPrefab
                        );
                    }
                }
#endif
                else
                {
                    // Unknown type!
                    Debug.LogError("Cannot build settings element for field \"" + field.Name + "\" as it is of unhandled type " + type.ToString());
                }

                if (container != null)
                {
                    container.description.text = (applyField as IApplySetting).GetDescription();
                }
            }

            // Make sure the player can use the back button, if available.
            /*if (backButton != null)
            {
                backButton.group = groupType.Name;
            }*/

            // Move bottomRoot to the end, if available.
            if (bottomRoot != null)
            {
                bottomRoot.transform.SetAsLastSibling();
            }

            builder.End();

            return groupType.Name;
        }

    }

}
