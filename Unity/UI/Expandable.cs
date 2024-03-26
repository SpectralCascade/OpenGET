using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenGET.UI
{

    /// <summary>
    /// UI element that can be expanded into a list of elements.
    /// </summary>
    public class Expandable : AutoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        /// <summary>
        /// Root under which expanded elements live.
        /// </summary>
        [SerializeField]
        [Auto.NullCheck]
        [Auto.Hookup(Auto.Mode.Self)]
        protected Transform root;

        /// <summary>
        /// Is this expandable expanded?
        /// </summary>
        public virtual bool isExpanded => root.gameObject.activeSelf;

        /// <summary>
        /// Is this expandable currently hovered?
        /// </summary>
        public bool isHovered { get; private set; }

        public void Expand()
        {
            SetExpanded(true);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
        }

        public void Retract()
        {
            SetExpanded(false);
        }

        public virtual void SetExpanded(bool expand)
        {
            root.gameObject.SetActive(expand);
        }

    }

}
