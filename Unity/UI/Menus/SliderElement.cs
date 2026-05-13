using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// Slider UI element.
    /// </summary>
    public class SliderElement : TextButton, IElement
    {

        /// <summary>
        /// Associated slider.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup]
        public Slider slider;

        /// <summary>
        /// Get the normalised slider value.
        /// </summary>
        public float normValue => slider.normalizedValue;

        /// <summary>
        /// Get the sider value.
        /// </summary>
        public float value => slider.value;

        /// <summary>
        /// Associated UI, if any.
        /// </summary>
        private UIController UI;

        protected override void Awake()
        {
            base.Awake();
            Debug.Assert(slider != null);

            slider.onValueChanged.AddListener(x => onClick?.Invoke());
        }

        public void Init(UIController UI)
        {
            this.UI = UI;
        }

        protected void OnDestroy()
        {
            slider.onValueChanged.RemoveAllListeners();
        }

        public object GetValue()
        {
            return slider.value;
        }

        public void SetValue(object value)
        {
            slider.value = (float)value;
        }

        protected void Update()
        {
            if (UI != null && UI.ActionMoveSelection != null &&
                (UI.events.currentSelectedGameObject == slider.gameObject || (button != null && UI.events.currentSelectedGameObject == button.gameObject)) &&
                UI.ActionMoveSelection.IsPressed())
            {
                float amount = 0.75f * Time.deltaTime;
                Vector2 nav = UI.ActionMoveSelection.ReadValue<Vector2>();
                if (slider.direction == Slider.Direction.LeftToRight)
                {
                    slider.value += nav.x * amount;
                }
                else if (slider.direction == Slider.Direction.RightToLeft)
                {
                    slider.value -= nav.x * amount;
                }
                else if (slider.direction == Slider.Direction.BottomToTop)
                {
                    slider.value += nav.y * amount;
                }
                else
                {
                    slider.value -= nav.y * amount;
                }
            }
        }

    }

}
