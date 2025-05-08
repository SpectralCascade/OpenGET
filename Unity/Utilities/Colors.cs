using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    public static class Colors
    {

        /// <summary>
        /// Returns a color with a modified alpha value.
        /// </summary>
        public static Color Alpha(Color c, float alpha)
        {
            return new Color(c.r, c.g, c.b, alpha);
        }

        /// <summary>
        /// Returns a colour that is brighter or darker according to the modifier.
        /// 1 = no change, 0 = black.
        /// </summary>
        public static Color Brightness(Color c, float mod) {
            return new Color(c.r * mod, c.g * mod, c.b * mod, c.a);
        }

    }

}
