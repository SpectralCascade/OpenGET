using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET.UI
{

    /// <summary>
    /// Represents a UI element that has a settable value.
    /// </summary>
    public interface IElement
    {
        void SetValue(object value);
        object GetValue();
    }

    /// <summary>
    /// Attribute for all individual elements.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class ElementAttribute : System.Attribute
    {

        /// <summary>
        /// The raw name of this element.
        /// </summary>
        public string name;

        /// <summary>
        /// Specify an individual element.
        /// </summary>
        /// <param name="name">The raw name of this element. If none is provided, falls back to member name.</param>
        public ElementAttribute(string name)
        {
            this.name = name;
        }

    }

    /// <summary>
    /// Attribute for slider elements with a minimum and maximum range of values.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class SliderAttribute : ElementAttribute
    {

        /// <summary>
        /// Minimum value.
        /// </summary>
        public float min;

        /// <summary>
        /// Maximum value.
        /// </summary>
        public float max;

        /// <summary>
        /// Specify an individual slider element with a minimum and maximum value.
        /// </summary>
        /// <param name="min">Minimum value of the slider element.</param>
        /// <param name="max">Maximum value of the slider element.</param>
        public SliderAttribute(string name = "", float min = 0, float max = 1) : base(name)
        {
            this.min = min;
            this.max = max;
        }
    }

}
