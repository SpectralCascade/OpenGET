using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET.UI
{

    /// <summary>
    /// Global UI settings.
    /// </summary>
    [CreateAssetMenu(fileName = "UISettings.asset", menuName = "OpenGET/UI Settings")]
    public class UIConfig : ScriptableObject
    {

        /// <summary>
        /// How long a button input should be pressed before it actually causes an action to complete.
        /// </summary>
        public float ButtonHoldTime = 2f;

        /// <summary>
        /// How long ViewPanel instances should take to fade in or out by default.
        /// </summary>
        public float ViewPanelFadeTime = 0.3f;

        /// <summary>
        /// How long to wait until the player can move focus again after holding down a button.
        /// </summary>
        public float MoveFocusCooldown = 0.2f;

    }

}
