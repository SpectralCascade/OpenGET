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
            float sum = 0;

            // Group & sort by weights, such that multiple items with the same weight are treated equally
            System.Tuple<float, T[]>[] weightGroups = array.GroupBy(
                x => x.Item2,
                x => x.Item1,
                (weight, group) => System.Tuple.Create(weight, group.ToArray())
            ).OrderBy(x => x.Item1).ToArray();

            int length = weightGroups.Length;

            float[] weights = weightGroups.Select(x => x.Item1).ToArray();
            float pick = Random.Range(0, sum);
            Log.Debug("Performing binary search with weights: {0}", string.Join(", ", weights.Select(x => x.ToString())));
            int index = MathUtils.BinarySearch(weights, pick, out int nearest);
            
            if (index < 0 && nearest >= 0)
            {
                // Use the closest weight
                Log.Debug("Found nearest = {0}", nearest);
                index = Mathf.Clamp(nearest + Mathf.RoundToInt(MathUtils.MapRange(
                    weights[nearest],
                    nearest > 0 ? weights[nearest - 1] : weights[nearest],
                    nearest < length - 1 ? weights[nearest + 1] : weights[nearest],
                    -1,
                    1
                )), 0, length - 1);
            }

            return Pick(weightGroups[index].Item2);
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
