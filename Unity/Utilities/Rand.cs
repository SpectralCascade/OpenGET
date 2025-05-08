using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        /// Pick a random element from an array of objects with associated bias weightings.
        /// Note that negative weightings are treated as absolute values.
        /// </summary>
        public static T Pick<T>(IList<System.Tuple<T, float>> array)
        {
            if (array.Count <= 0)
            {
                return default(T);
            }

            // Sum up weights cumulatively
            float sum = 0;
            int length = array.Count;// weightGroups.Length;
            float[] cumulativeWeights = array.Select(
                x => { float v = sum + x.Item2; sum += x.Item2; return v; }
            ).ToArray();

            float pick = Random.Range(0, sum);
            int index = MathUtils.BinarySearch(cumulativeWeights, pick, out int nearest);
            
            if (index < 0 && nearest >= 0)
            {
                // Use the closest weight
                index = Mathf.Clamp(nearest + Mathf.RoundToInt(MathUtils.MapRange(
                    cumulativeWeights[nearest],
                    nearest > 0 ? cumulativeWeights[nearest - 1] : cumulativeWeights[nearest],
                    nearest < length - 1 ? cumulativeWeights[nearest + 1] : cumulativeWeights[nearest],
                    -1,
                    1
                )), 0, length - 1);
            }

            return array[index].Item1;
        }

        /// <summary>
        /// Randomly shuffle an array. Note: Modifies in place.
        /// </summary>
        public static IList<T> Shuffle<T>(this IList<T> array)
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
            return array;
        }

    }

}
