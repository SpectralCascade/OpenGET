using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace OpenGET.UI
{

    /// <summary>
    /// A button with text.
    /// </summary>
    public class TextButton : AutoBehaviour
    {

        /// <summary>
        /// The button itself.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup]
        public Button button;

        /// <summary>
        /// Text associated with the button.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup]
        public TMPro.TextMeshProUGUI text;

        /// <summary>
        /// Convenience accessor.
        /// </summary>
        public Button.ButtonClickedEvent onClick { get { return button.onClick; } set { button.onClick = value; } }

    }

}
