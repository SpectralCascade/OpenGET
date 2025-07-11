using OpenGET;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenGET.UI
{

    /// <summary>
    /// Represents a contextual menu. This could be a radial menu, dropdown list, or anything else you can think of as a popup context/"right click" style menu.
    /// </summary>
    public class ContextList : BoundedViewPanel, TooltipArea.ITooltip, IPointerExitHandler
    {
        public delegate void ContextAction(ContextItem contextItem);
        public delegate string GetActionDisableReason();
        public delegate string GetTooltip();
        public delegate void OnTooltipChanged(string text);

        public event OnTooltipChanged onTooltipChanged;

        /// <summary>
        /// Context menu option.
        /// </summary>
        public class Option
        {
            public Option(string text, Sprite icon, ContextAction action = null, GetActionDisableReason disableReason = null, GetTooltip tooltip = null, ContextAction onSelect = null, bool closeOnAction = true)
            {
                this.text = text;
                this.icon = icon;
                this.action = action;
                this.disableReason = disableReason;
                this.tooltip = tooltip;
                this.onSelect = onSelect;
                this.closeOnAction = closeOnAction;
            }

            /// <summary>
            /// Displayed text for the option.
            /// </summary>
            public string text = "";

            /// <summary>
            /// Displayed icon for the option.
            /// </summary>
            public Sprite icon = null;

            /// <summary>
            /// Callback triggered when clicked.
            /// </summary>
            public ContextAction action = null;

            /// <summary>
            /// Condition to check whether the action is enabled.
            /// </summary>
            public GetActionDisableReason disableReason = null;

            /// <summary>
            /// Callback for getting dynamic tooltip text. Useful for indicating option state.
            /// </summary>
            public GetTooltip tooltip = null;

            /// <summary>
            /// Callback for handling hover selection.
            /// </summary>
            public ContextAction onSelect = null;

            /// <summary>
            /// Whether to close after the action callback or not.
            /// </summary>
            public bool closeOnAction = true;

        }

        [Tooltip("Title of the context menu; or the name of the context option selected.")]
        public TMPro.TextMeshProUGUI title;

        [Tooltip("Optional subtitle, typically used for option expansion of submenus.")]
        public TMPro.TextMeshProUGUI subtitle;

        [Tooltip("Context item prefab for instantiation.")]
        [Auto.NullCheck]
        public ContextItem prefab;

        /// <summary>
        /// To handle overrides.
        /// </summary>
        private ContextItem originalPrefab = null;

        [Tooltip("Where the context items are instantiated.")]
        [SerializeField]
        [Auto.NullCheck]
        private Transform root;

        /// <summary>
        /// All active context items.
        /// </summary>
        private List<ContextItem> _items = new List<ContextItem>();

        public ContextItem[] items => _items.ToArray();

        /// <summary>
        /// Retrieve the array of context options.
        /// </summary>
        public ContextItem[] options => _items.ToArray();

        /// <summary>
        /// Current highlighted option.
        /// </summary>
        public ContextItem highlighted { get; set; }

        /// <summary>
        /// Current selected option (i.e. clicked on).
        /// </summary>
        public ContextItem selected { get; set; }

        /// <summary>
        /// Initial description for tooltip(s), if any.
        /// </summary>
        [System.NonSerialized]
        public string normalDescription = "";

        /// <summary>
        /// Initial title for tooltip(s), if any.
        /// </summary>
        [System.NonSerialized]
        public string normalName = "";

        /// <summary>
        /// Actual tooltip text.
        /// </summary>
        private string tooltip = "";

        /// <summary>
        /// Set context tooltip text.
        /// </summary>
        public void SetTooltip(string body)
        {
            tooltip = body;
            onTooltipChanged?.Invoke(body);
        }

        /// <summary>
        /// Get context tooltip text.
        /// </summary>
        string TooltipArea.ITooltip.GetTooltip()
        {
            return tooltip;
        }

        /// <summary>
        /// Setup context list from scratch. Optionally specify a ContextItem prefab to override the one set on this object.
        /// </summary>
        public void Init(List<Option> options, string title = "", string description = "", string subtitle = "", ContextItem prefabOverride = null)
        {
            this.title.text = title;
            this.normalName = title;
            this.normalDescription = description;
            this.subtitle.text = subtitle;

            if (originalPrefab != null)
            {
                prefab = originalPrefab;
                originalPrefab = null;
            }
            if (prefabOverride != null)
            {
                originalPrefab = prefab;
                prefab = prefabOverride;
            }

            Clear();
            for (int i = 0, counti = options.Count; i < counti; i++)
            {
                AddOption(options[i]);
            }
        }

        protected override void OnDidHide()
        {
            base.OnDidHide();

            highlighted = null;
            selected = null;
        }

        /// <summary>
        /// Add an option to this context menu.
        /// </summary
        public void AddOption(ContextList.Option option)
        {
            ContextItem loaded = Instantiate(prefab, root);
            loaded.Init(option, this);
            loaded.gameObject.name = option.text;
            _items.Add(loaded);
        }

        /// <summary>
        /// Clear all associated items.
        /// </summary>
        public void Clear()
        {
            for (int i = 0, counti = _items.Count; i < counti; i++) {
                Destroy(_items[i].gameObject);
            }
            _items.Clear();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            title.text = normalName;
            subtitle.text = "";
            SetTooltip(normalDescription);
        }

    }

}
