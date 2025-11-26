using OpenGET.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET
{

    /// <summary>
    /// Component used for automatically localising text.
    /// </summary>
    [RequireComponent(typeof(TextFormatter))]
    public class LocalisedText : AutoTextFormat
    {
        public override string OnTextAutoFormat(string text)
        {
            return !string.IsNullOrEmpty(text) ? Localise.Text(text) : "";
        }
    }

}
