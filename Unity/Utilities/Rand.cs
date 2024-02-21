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

        /// <summary>
        /// Randomly shuffle an array.
        /// </summary>
        public static void Shuffle<T>(this IList<T> array)
        {
            int n = array.Count;
            while (n > 1)
            {
                n--;
                int k = Mathf.FloorToInt(Random.Range(0f, n + 1));
                T value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
        }

    }

}
