using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// Represents a single tab button.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class Tab : AutoBehaviour
    {

        [System.Serializable]
        protected struct ButtonState
        {
            [Tooltip("For button colour transition.")]
            public Color colour;

            [Tooltip("For button sprite swap transition.")]
            public Sprite sprite;

            [Tooltip("For button animation transition.")]
            public string anim;
        }

        /// <summary>
        /// Button associated with this tab.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup]
        public Button button;

        /// <summary>
        /// Optional associated text.
        /// </summary>
        [Auto.Hookup]
        public TMPro.TextMeshProUGUI text;

        /// <summary>
        /// Grouping of associated tabs.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup(Auto.Mode.Parent)]
        public TabGroup group;

        /// <summary>
        /// Associated panel that is shown or hidden when this tab is activated or deactivated respectively.
        /// </summary>
        [Tooltip("Associate a panel with this tab, automatically showing/hiding it appropriately.")]
        public ViewPanel associatedPanel;

        /// <summary>
        /// Custom callback for switching to this tab.
        /// </summary>
        public UnityEngine.Events.UnityEvent onTabActivate;

        /// <summary>
        /// Custom callback for switching from this tab.
        /// </summary>
        public UnityEngine.Events.UnityEvent onTabDeactivate;

        /// <summary>
        /// Activated button state.
        /// </summary>
        [SerializeField]
        protected ButtonState activatedState;

        /// <summary>
        /// Deactivated button state.
        /// </summary>
        [SerializeField]
        protected ButtonState deactivatedState;

        /// <summary>
        /// Is this the active tab?
        /// </summary>
        public bool active => group.current == this;

        protected override void Awake()
        {
            if (group == null)
            {
                group = GetComponentInParent<TabGroup>();
            }

            base.Awake();

            button.onClick.AddListener(SwitchTo);
        }

        protected void OnDestroy()
        {
            button.onClick.RemoveListener(SwitchTo);
        }

        /// <summary>
        /// Switch to this tab.
        /// </summary>
        public void SwitchTo()
        {
            group.SwitchTo(this);
        }

        /// <summary>
        /// Handle a switch to or from this tab.
        /// </summary>
        public void OnSwitch(bool activated)
        {
            if (activated)
            {
                onTabActivate?.Invoke();
                if (associatedPanel != null)
                {
                    associatedPanel.Show(0);
                }
            }
            else
            {
                if (associatedPanel != null)
                {
                    associatedPanel.Hide(0);
                }
                onTabDeactivate?.Invoke();
            }

            switch (button.transition)
            {
                case Selectable.Transition.ColorTint:
                    button.colors = new ColorBlock {
                        normalColor = activated ? activatedState.colour : deactivatedState.colour,
                        selectedColor = activated ? activatedState.colour : deactivatedState.colour,
                        colorMultiplier = button.colors.colorMultiplier,
                        disabledColor = button.colors.disabledColor,
                        pressedColor = button.colors.pressedColor,
                        highlightedColor = button.colors.highlightedColor,
                        fadeDuration = button.colors.fadeDuration
                    };
                    break;
                case Selectable.Transition.SpriteSwap:
                    button.image.sprite = activated ? activatedState.sprite : deactivatedState.sprite;
                    button.spriteState = new SpriteState {
                        selectedSprite = activated ? activatedState.sprite : deactivatedState.sprite,
                        disabledSprite = button.spriteState.disabledSprite,
                        highlightedSprite = button.spriteState.highlightedSprite,
                        pressedSprite = button.spriteState.pressedSprite
                    };
                    break;
                case Selectable.Transition.Animation:
                    button.animationTriggers.normalTrigger = activated ? activatedState.anim : deactivatedState.anim;
                    button.animationTriggers.selectedTrigger = activated ? activatedState.anim : deactivatedState.anim;
                    break;
                default:
                    break;
            }
        }

    }

}
