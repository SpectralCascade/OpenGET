using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace OpenGET.UI
{

    /// <summary>
    /// UI tooltip trigger; setup to trigger on hover by default.
    /// </summary>
    [RequireComponent(typeof(EventTrigger))]
    public class TooltipArea : AccessUI, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        /// <summary>
        /// Implement this interface to set dynamic text on a tooltip.
        /// </summary>
        public interface ITooltip
        {
            public string GetTooltip();

            /// <summary>
            /// Optional offset to use instead of fixed offset.
            /// </summary>
            public Vector2? getTooltipOffset => null;

            /// <summary>
            /// Optional position to use instead of mouse pos.
            /// </summary>
            public Vector2? getTooltipPosition => null;
        }

        [Tooltip("Optional custom tooltip prefab to use. If null, a default tooltip is used from UI settings.")]
        public TooltipPanel prefab = null;

        /// <summary>
        /// The actual instantiated tooltip UI.
        /// </summary>
        private TooltipPanel tooltip;

        /// <summary>
        /// Used to access pointer events.
        /// </summary>
        [SerializeField]
        [Auto.NullCheck]
        [Auto.Hookup(Auto.Mode.Self)]
        private EventTrigger inputTrigger;

        [Tooltip("Show the tooltip when this object is hovered over")]
        public bool showOnHover = true;

        [Tooltip("Fixed offset to use when no dynamic tooltip text is in use.")]
        public Vector2 offset = new Vector2(32, -32);

        [Tooltip("Unlocalised (raw english) text string to show in the tooltip")]
        [TextArea]
        public string text = "";

        /// <summary>
        /// Hook up a dynamic tooltip.
        /// </summary>
        [System.NonSerialized]
        public ITooltip custom = null;

        protected override void Awake()
        {
            // Hookup UI
            if (_UI == null)
            {
                _UI = GetComponentInParent<UIController>();
            }

            base.Awake();

            if (prefab == null)
            {
                tooltip = UI.tooltipShared;
            }
            else {
                tooltip = Instantiate(prefab, UI.tooltipsRoot);
                tooltip.Init(UI, (RectTransform)UI.tooltipsRoot);
            }

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            EventTrigger.Entry pointerMove = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.eventID = EventTriggerType.Move;
            pointerEnter.callback.AddListener((e) => OnPointerEnter(e as PointerEventData));
            pointerExit.callback.AddListener((e) => OnPointerExit(e as PointerEventData));
            pointerMove.callback.AddListener((e) => OnPointerMove(e as PointerEventData));
            inputTrigger.triggers.Add(pointerEnter);
            inputTrigger.triggers.Add(pointerExit);
            inputTrigger.triggers.Add(pointerMove);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (showOnHover)
            {
                ShowTooltip();
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            UpdateTooltip();
        }

        public void UpdateTooltip()
        {
            Vector2 customOffset = custom?.getTooltipOffset ?? offset;
            Vector2 customPosition = custom?.getTooltipPosition ?? Pointer.current.position.ReadValue();

            tooltip.SetPosition(customPosition + customOffset);

            if (custom != null)
            {
                string customText = custom.GetTooltip();
                if (!string.IsNullOrEmpty(customText))
                {
                    SetTooltipText(customText);
                }
                else
                {
                    // Hide
                    HideTooltip();
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (showOnHover)
            {
                HideTooltip();
            }
        }

        public void ShowTooltip()
        {
            if (!string.IsNullOrEmpty(text))
            {
                tooltip.SetText(Localise.Text(text));
            }
            UpdateTooltip();
            tooltip.Show(0);
        }

        public void HideTooltip()
        {
            tooltip.Hide(0);
        }

        public void SetTooltipText(string text)
        {
            tooltip.SetText(text);
        }

        protected void OnDestroy()
        {
            if (prefab != null)
            {
                Destroy(tooltip.gameObject);
                tooltip = null;
            }
        }

    }

}
