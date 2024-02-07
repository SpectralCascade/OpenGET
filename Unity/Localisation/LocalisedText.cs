using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET
{

    /// <summary>
    /// Component used for automatically localising constant text.
    /// </summary>
    public class LocalisedText : AutoBehaviour
    {
        /// <summary>
        /// Associated text component.
        /// </summary>
        [SerializeField]
        [Auto.Hookup(Auto.Mode.Self)]
#if OPENGET_LOCALISE_TMPRO
        private TMPro.TextMeshProUGUI textGraphic;
#else
        private Text textGraphic;
#endif

        /// <summary>
        /// The raw string. In addition to being the original/source text to be translated, this is also the localisation id.
        /// </summary>
        public string id {
            get {
                if (_id == null || !Application.isPlaying)
                {
                    _id = textGraphic.text;
                }
                return _id;
            }
            set {
                _id = value;
                AutoLocalise();
            }
        }
        private string _id = null;

        /// <summary>
        /// Whenever enabled, re-localise in case the language has changed.
        /// </summary>
        private void OnEnable()
        {
            AutoLocalise();
        }

        /// <summary>
        /// Translate the raw text string to the current active language.
        /// </summary>
        public void AutoLocalise()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
                string localised = !string.IsNullOrEmpty(id) ? Localise.Text(id) : "[NULL TEXT]";
                textGraphic.text = localised;
#if UNITY_EDITOR
            }
#endif
        }

    }

}
