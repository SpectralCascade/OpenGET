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
        /// Custom action called every update.
        /// </summary>
        private UnityAction<Component> onUpdate = null;

        /// <summary>
        /// Inner UI, if any.
        /// </summary>
        private Component inner = null;

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
                prefab = UI.tooltipShared;
            }
            tooltip = Instantiate(prefab, UI.tooltipsRoot);
            tooltip.Init(UI, (RectTransform)UI.tooltipsRoot);

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

            bool rawCustom = custom?.getTooltipPosition != null;

            Vector2 pos = customPosition + (rawCustom ? Vector2.zero : customOffset);
            pos = !rawCustom && UI.canvas.renderMode != RenderMode.ScreenSpaceOverlay ? UI.cam.ScreenToWorldPoint(pos) : pos;
            tooltip.SetPosition(pos, false);

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

            onUpdate?.Invoke(inner);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (showOnHover)
            {
                HideTooltip();
            }
        }

        /// <summary>
        /// Shows the tooltip.
        /// </summary>
        public void ShowTooltip()
        {
            ShowTooltip<Component>(null);
        }

        /// <summary>
        /// Shows the tooltip with an inner-UI prefab for the tooltip and an optional update callback.
        /// Returns the instantiated inner-UI object if a prefab is provided, otherwise returns null.
        /// Update callback provides the inner instance, if any.
        /// </summary>
        public T ShowTooltip<T>(T innerPrefab, UnityAction<Component> onUpdate = null) where T : Component
        {
            this.onUpdate = onUpdate;
            if (inner != null)
            {
                Destroy(inner.gameObject);
                inner = null;
            }
            if (innerPrefab != null)
            {
                // Add a child object to the tooltip. Useful for custom UI. Must be already instantiated and ready to go.
                inner = Instantiate(innerPrefab, tooltip.body.transform.parent);
                inner.gameObject.transform.SetAsFirstSibling();
            }
            if (!string.IsNullOrEmpty(text))
            {
                tooltip.SetText(Localise.Text(text));
            }
            UpdateTooltip();
            tooltip.Show(0);

            return inner as T;
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
