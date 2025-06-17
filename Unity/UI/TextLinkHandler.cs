using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace OpenGET.UI
{

    [RequireComponent(typeof(TMP_Text))]
    public class TextLinkHandler : AutoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IPointerClickHandler
    {
        public delegate void OnLinkInput(string id);
        public event OnLinkInput onClick;
        public event OnLinkInput onEnter;
        public event OnLinkInput onExit;
        public event OnLinkInput onMove;

        /// <summary>
        /// The id of the link that is currently hovered, if any.
        /// </summary>
        private string currentHovered = null;

        /// <summary>
        /// Text containing the link.
        /// </summary>
        [SerializeField]
        [Auto.NullCheck]
        [Auto.Hookup(Auto.Mode.Self)]
        private TMP_Text text;

        private int TryGetLink(PointerEventData data, out TMP_LinkInfo link)
        {
            Vector3 mousePos = new Vector3(data.position.x, data.position.y, 0);

            int index = TMP_TextUtilities.FindIntersectingLink(text, mousePos, null);
            link = index >= 0 ? text.textInfo.linkInfo[index] : new TMP_LinkInfo();
            return index;
        }

    
        public void OnPointerClick(PointerEventData data)
        {
            if (TryGetLink(data, out TMP_LinkInfo link) >= 0)
            {
                onClick?.Invoke(link.GetLinkID());
            }
        }

        public void OnPointerMove(PointerEventData data)
        {
            if (TryGetLink(data, out TMP_LinkInfo link) >= 0)
            {
                string linkID = link.GetLinkID();
                if (currentHovered != linkID)
                {
                    if (currentHovered != null)
                    {
                        onExit?.Invoke(currentHovered);
                    }
                    currentHovered = linkID;
                    onEnter?.Invoke(linkID);
                }
            }
            else if (currentHovered != null) {
                onExit?.Invoke(currentHovered);
                currentHovered = null;
            }

            if (currentHovered != null)
            {
                onMove?.Invoke(currentHovered);
            }
        }

        public void OnPointerEnter(PointerEventData data)
        {
            OnPointerMove(data);
        }

        public void OnPointerExit(PointerEventData data)
        {
            OnPointerMove(data);
        }
    }
}
