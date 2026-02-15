using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// Text and/or icon based discrete status indicator.
    /// </summary>
    public class DiscreteStatus : AutoBehaviour, TooltipArea.ITooltip
    {
        [Tooltip("Text to change with states.")]
        public TMPro.TextMeshProUGUI text;

        [Tooltip("Icon to change with states.")]
        public Image icon;

        [Tooltip("Whether to use state colour to set text colour.")]
        public bool setTextColour = true;

        [Tooltip("Whether to use state colour to set icon colour.")]
        public bool setIconColour = true;

        [Tooltip("Optional tooltip area to dynamically set with state text.")]
        public TooltipArea tooltipArea = null;

        [System.Serializable]
        public struct State
        {
            [Tooltip("Custom icon.")]
            public Sprite icon;

            [Tooltip("Icon/text colour. Make sure you set the alpha channel while you are setting colour.")]
            public Color color;

            [Tooltip("Optional raw (unlocalised) text.")]
            public string rawText;

            [Tooltip("Optional collection of gameobjects to enable.")]
            public GameObject[] enable;
        }

        [Tooltip("Your custom statuses.")]
        public State[] states = new State[0];

        /// <summary>
        /// Current state index.
        /// </summary>
        public int current { get; set; }

        /// <summary>
        /// Previous state index.
        /// </summary>
        private int previous { get; set; }

        /// <summary>
        /// The total number of states.
        /// </summary>
        public int limit => states.Length;

        /// <summary>
        /// Get current status. Returns default if no states are setup.
        /// </summary>
        public State status => limit > 0 ? states[current] : default;

        protected override void Awake()
        {
            base.Awake();
            if (tooltipArea != null)
            {
                tooltipArea.custom = this;
            }
            for (int i = 0, counti = states != null ? states.Length : 0; i < counti; i++)
            {
                for (int j = 0, countj = states[i].enable != null ? states[i].enable.Length : 0; j < countj; j++)
                {
                    states[i].enable[j].SetActive(false);
                }
            }
            UpdateStatus();
        }

        /// <summary>
        /// Set the current status.
        /// </summary>
        public void SetStatus(int index)
        {
            previous = current;
            current = index;
            UpdateStatus();
        }

        /// <summary>
        /// Update the text and/or icon.
        /// </summary>
        public void UpdateStatus()
        {
            if (current >= 0 && current < states.Length)
            {
                if (icon != null)
                {
                    icon.sprite = status.icon;
                    if (setIconColour)
                    {
                        icon.color = status.color;
                    }
                }
                if (text != null)
                {
                    text.text = Localise.Text(status.rawText);
                    if (setTextColour)
                    {
                        text.color = status.color;
                    }
                }

                // Disable previous state objs
                GameObject[] previousEnable = states[previous].enable;
                if (previousEnable != null)
                {
                    for (int i = 0, counti = previousEnable.Length; i < counti; i++)
                    {
                        if (previousEnable[i] == null)
                        {
                            Log.Warning("Unexpected null GameObject enable entry in previous state {0} of {1}", previous, SceneNavigator.GetPath(this));
                            continue;
                        }
                        previousEnable[i].SetActive(false);
                    }
                }

                // Enable new state objs
                GameObject[] enable = status.enable;
                if (enable != null)
                {
                    for (int i = 0, counti = enable.Length; i < counti; i++)
                    {
                        if (enable[i] == null)
                        {
                            Log.Warning("Unexpected null GameObject enable entry in current state {0} of {1}", current, SceneNavigator.GetPath(this));
                            continue;
                        }
                        enable[i].SetActive(true);
                    }
                }
            }
        }

        /// <summary>
        /// ITooltip implementation.
        /// </summary>
        public string GetTooltip()
        {
            return string.IsNullOrEmpty(status.rawText) ? "" :
                (setTextColour ? $"<color={status.color}>" : "") + Localise.Text(status.rawText) + (setTextColour ? $"</color>" : "");
        }
    }

}
