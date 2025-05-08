using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI {

    /// <summary>
    /// Container for an individual element.
    /// </summary>
    public class ElementContainer : MonoBehaviour, IElement
    {

        /// <summary>
        /// Where the element should be placed.
        /// </summary>
        [Auto.NullCheck]
        public Transform content;

        /// <summary>
        /// Optional descriptive text for the element.
        /// </summary>
        public Text description;

        /// <summary>
        /// Reference to the contained element.
        /// </summary>
        public IElement element;

        public object GetValue()
        {
            return element.GetValue();
        }

        public void SetValue(object value)
        {
            element.SetValue(value);
        }

    }

}
