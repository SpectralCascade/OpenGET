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

    }

}
