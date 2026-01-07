using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OpenGET.UI
{

    /// <summary>
    /// Subheading text (non-editable, set up dynamically from code).
    /// </summary>
    public class HeadingElement : Element
    {

        [Auto.NullCheck]
        [Auto.Hookup]
        public TextMeshProUGUI text;

        /// <summary>
        /// Raw (unlocalised) string.
        /// </summary>
        private string raw = "";

        public override object GetValue()
        {
            return raw;
        }

        public override void SetValue(object value)
        {
            if (value != null)
            {
                raw = value.ToString();
                text.text = Localise.Text(raw);
            }
            else
            {
                raw = "";
                text.text = "";
            }
        }
    }

}
