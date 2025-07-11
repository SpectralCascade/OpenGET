using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// Convenience component that stores the original string of a text object
    /// and can be used to apply formatting.
    /// </summary>
    public class TextFormatter : AutoBehaviour
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
        /// Text formatting chain, applied in order on AutoFormat.
        /// </summary>
        [SerializeField]
        [Auto.NullCheck]
        [Auto.Hookup(Auto.Mode.Self)]
        private AutoTextFormat[] formatters = new AutoTextFormat[0];

        /// <summary>
        /// Get/set text on the graphic directly.
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
        /// The raw string - the original/source text to be formatted.
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
                AutoFormat();
            }
        }
        private string _id = null;

        /// <summary>
        /// Returns true when this is a valid TextFormatter instance.
        /// </summary>
        public bool isValid => textGraphic != null && (isTMP || textGraphic is Text);

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0, counti = formatters.Length; i < counti; i++)
            {
                Debug.Assert(formatters[i] != null, $"Null text formatter on \"{SceneNavigator.GetPath(this)}\" at index {i}.");
            }
        }

        /// <summary>
        /// Whenever enabled, re-format in case there are formatting changes.
        /// </summary>
        private void OnEnable()
        {
            AutoFormat();
        }

        /// <summary>
        /// Translate the raw text string to the current active language.
        /// </summary>
        public void AutoFormat()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
                string delta = id;
                for (int i = 0, counti = formatters.Length; i < counti; i++)
                {
                    delta = formatters[i].OnTextAutoFormat(delta);
                }
                text = delta;
#if UNITY_EDITOR
            }
#endif
        }

    }

}
