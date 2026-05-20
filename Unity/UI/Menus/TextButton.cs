using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// A button with text.
    /// </summary>
    public class TextButton : AutoBehaviour, IPointerEnterHandler, IPointerExitHandler, IScrollHandler, NavigationBlock.IElement, ISelectHandler
    {

        /// <summary>
        /// The button itself.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup]
        public Button button;

        /// <summary>
        /// Text associated with the button.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup]
        public TMPro.TextMeshProUGUI text;

        /// <summary>
        /// Convenience accessor.
        /// </summary>
        public Button.ButtonClickedEvent onClick { get { return button.onClick; } set { button.onClick = value; } }

        public Navigation navigation { get => button.navigation; set { button.navigation = value; } }
        public Selectable selectable => button;

        /// <summary>
        /// Refresh parent navigation block on selection (if available).
        /// </summary>
        public bool refreshNavOnSelect = false;

        /// <summary>
        /// Event for handling a hover enter or exit event.
        /// </summary>
        public UnityEngine.Events.UnityEvent<bool> onHoverChange = new UnityEngine.Events.UnityEvent<bool>();

        public void OnPointerEnter(PointerEventData eventData)
        {
            onHoverChange?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onHoverChange?.Invoke(false);
        }

        private ScrollRect _containerScrollRect;

        public void OnScroll(PointerEventData eventData)
        {
            _containerScrollRect = _containerScrollRect != null ? _containerScrollRect : GetComponentInParent<ScrollRect>();
            if (_containerScrollRect != null)
            {
                _containerScrollRect.OnScroll(eventData);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (refreshNavOnSelect)
            {
                NavigationBlock parentNav = transform.parent.GetComponent<NavigationBlock>();
                if (parentNav.HasChild(gameObject))
                {
                    // Refresh navigation on selection if enabled
                    parentNav.SetDirty(NavigationBlock.Refresh.Navigation);
                }
            }
        }
    }

}
