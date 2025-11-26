using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    /// Most elements implement pointer handlers. However, these block scroll inputs.
    /// This class automatically passes the scroll event back up to the parent scroll rect (if any).
    /// </summary>
    public abstract class Element : AutoBehaviour, IElement, IScrollHandler, IDragHandler, IBeginDragHandler, IEndDragHandler {
        public abstract void SetValue(object value);
        public abstract object GetValue();

        private ScrollRect _containerScrollRect;
        protected ScrollRect parentScrollRect => 
            _containerScrollRect != null ? _containerScrollRect : _containerScrollRect = GetComponentInParent<ScrollRect>();

        public virtual void OnScroll(PointerEventData eventData)
        {
            if (parentScrollRect != null)
            {
                parentScrollRect.OnScroll(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (parentScrollRect != null)
            {
                parentScrollRect.OnDrag(eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (parentScrollRect != null)
            {
                parentScrollRect.OnBeginDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (parentScrollRect != null)
            {
                parentScrollRect.OnEndDrag(eventData);
            }
        }
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

    /// <summary>
    /// Attribute for dropdown elements, to be used on integer fields (the integer value is used for indexing the dropdown).
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class DropdownAttribute : ElementAttribute
    {
        public DropdownAttribute(string name = "") : base(name)
        {
        }
    }

}
