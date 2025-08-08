using OpenGET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection;

namespace OpenGET.UI
{

    /// <summary>
    /// Individual item in a context popup.
    /// </summary>
    public class ContextItem : AutoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {

        /// <summary>
        /// Option associated with this item.
        /// </summary>
        public ContextList.Option option { get; private set; }

        /// <summary>
        /// Parent context menu.
        /// </summary>
        protected ContextList menu;

        /// <summary>
        /// Display icon.
        /// </summary>
        [SerializeField]
        [Auto.NullCheck]
        protected Image icon;

        [SerializeField]
        [Auto.Hookup]
        [Auto.NullCheck]
        private Animator animator;

        [Header("Animation triggers & states")]

        [SerializeField]
        private string animHighlight = "Highlight";

        [SerializeField]
        private string animUnhighlight = "Unhighlight";

        [SerializeField]
        private string animSelect = "Select";

        [SerializeField]
        private string animStateLocked = "Locked";

        private void ResetContextOptions()
        {
            for (int i = 0, counti = menu.items.Length; i < counti; i++)
            {
                if (menu.items[i] != this && menu.selected != menu.items[i])
                {
                    menu.items[i].animator.SetTrigger(animUnhighlight);
                }
            }
        }

        /// <summary>
        /// Initialise the item with option data.
        /// </summary>
        public virtual void Init(ContextList.Option option, ContextList menu)
        {
            this.option = option;
            this.menu = menu;

            bool show = option.icon != null;
            icon.enabled = show;
            if (show)
            {
                icon.sprite = option.icon;
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (menu.highlighted != this)
            {
                option.onSelect?.Invoke(this);
            }
            menu.highlighted = this;
            if (menu.selected != this)
            {
                animator.SetTrigger(animHighlight);
            }
            UpdateTooltip();
            ResetContextOptions();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (menu.highlighted == this)
            {
                menu.highlighted = null;
                menu.SetTooltip("");
                UpdateTooltip();
            }

            if (menu.selected != this)
            {
                animator.SetTrigger(animUnhighlight);
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (option.action == null)
            {
                return;
            }

            string disableReason = option.disableReason?.Invoke();
            if (option.closeOnAction || disableReason != null)
            {
                menu.Hide(0);
            }
            if (disableReason == null)
            {
                option.action?.Invoke(this);
            }
            menu.selected = this;
            animator.SetTrigger(animSelect);
            ResetContextOptions();
        }

        /// <summary>
        /// Update the tooltip text.
        /// </summary>
        private void UpdateTooltip()
        {
            string disableReason = null;
            if (menu.highlighted == this)
            {
                if (menu.title != null)
                {
                    menu.title.text = option.text;
                }
                if (menu.subtitle != null)
                {
                    menu.subtitle.text = option.disableReason?.Invoke() ?? "";
                }

                if (option.tooltip != null)
                {
                    string tooltip = option.tooltip.Invoke();
                    disableReason = option.disableReason?.Invoke();
                    if (!string.IsNullOrEmpty(tooltip))
                    {
                        menu.SetTooltip(tooltip + (disableReason != null ? "\n\n* " + disableReason : ""));
                    }
                    return;
                }
            }

            disableReason = menu.highlighted == this ? option.disableReason?.Invoke() : null;
            if (!string.IsNullOrEmpty(disableReason))
            {
                menu.SetTooltip(disableReason);
            }
            else if (option.tooltip == null || (option.tooltip.Invoke() != ""))
            {
                menu.SetTooltip(menu.normalDescription);
            }
        }

        /// <summary>
        /// Update display depending on context data.
        /// </summary>
        private void Update()
        {
            string disableReason = option?.disableReason?.Invoke();
            if (menu != null && menu.highlighted == this)
            {
                UpdateTooltip();

                if (menu.title != null && menu.title.text != option.text)
                {
                    menu.title.text = option.text;
                }
            }
            animator?.SetBool(Animator.StringToHash(animStateLocked), disableReason != null);
        }

    }

}
