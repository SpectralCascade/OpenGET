using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// Implement this component class for simple text formatting processes.
    /// </summary>
    [RequireComponent(typeof(TextFormatter))]
    public abstract class AutoTextFormat : AutoBehaviour
    {
        public abstract string OnTextAutoFormat(string text);

    }

}
