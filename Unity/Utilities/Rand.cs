using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// Utility functions using randomness.
    /// </summary>
    public static class Rand
    {

        /// <summary>
        /// Pick a random element from an array.
        /// </summary>
        public static T Pick<T>(IList<T> array)
        {
            if (array.Count <= 0)
            {
                return default(T);
            }
            int index = Random.Range(0, array.Count);
            return array[index];
        }

    }

}
