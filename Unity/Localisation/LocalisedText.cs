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
        [Auto.NullCheck]
        [Auto.Hookup(Auto.Mode.Self)]
        private MaskableGraphic textGraphic;

        /// <summary>
        /// Is the associated text component built-in Unity text or from TextMeshPro?
        /// </summary>
        private bool isTMP => textGraphic is TMPro.TMP_Text;

        /// <summary>
        /// Set text on the graphic.
        /// </summary>
        private string text {
            get {
                return isTMP ? ((TMPro.TMP_Text)textGraphic).text : ((Text)textGraphic).text;
            }
            set {
                if (isTMP)
                {
                    ((TMPro.TMP_Text)textGraphic).text = value;
                }
                else
                {
                    ((Text)textGraphic).text = value;
                }
            }
        }

        /// <summary>
        /// The raw string. In addition to being the original/source text to be translated, this is also the localisation id.
        /// </summary>
        public string id {
            get {
                if (_id == null || !Application.isPlaying)
                {
                    _id = text;
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
        /// Returns true when this is a valid LocalisedText instance.
        /// </summary>
        public bool isValid => textGraphic != null && (isTMP || textGraphic is Text);

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
                text = localised;
#if UNITY_EDITOR
            }
#endif
        }

    }

}
