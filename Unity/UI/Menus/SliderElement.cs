using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        public float sliderValue => slider.normalizedValue;


        protected override void Awake()
        {
            base.Awake();
            Debug.Assert(slider != null);

            slider.onValueChanged.AddListener(x => onClick?.Invoke());
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

    }

}
