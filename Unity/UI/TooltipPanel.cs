using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET.UI
{

    public class TooltipPanel : BoundedViewPanel
    {

        /// <summary>
        /// UI descriptive text.
        /// </summary>
        public TMPro.TextMeshProUGUI body;

        protected override void Awake()
        {
            // No base awake, as this is always an instantiated prefab
        }

        public void Init(UIController UI, RectTransform rect)
        {
            _UI = UI;
            this.rect = rect;
            Hide(0);
        }

        public void SetText(string text)
        {
            body.text = text;
        }

    }

}
