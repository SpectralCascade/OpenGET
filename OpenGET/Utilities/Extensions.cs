using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET {

    public static class Extensions {

        public static T AddComponentOnce<T>(this GameObject gameObject) where T : Component {
            if (!gameObject.TryGetComponent(out T component)) {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Swap the item at the given index with the last item in the list, then remove the item.
        /// Use this when order does not matter; theoretically more efficient than other removal methods.
        /// Can safely used to remove only some items as you iterate from end to beginning of a list.
        /// </summary>
        public static void SwapRemoveAt<T>(this IList<T> list, int index)
        {
            int end = list.Count - 1;
            T intermediate = list[end];
            list[end] = list[index];
            list[index] = intermediate;
            list.RemoveAt(end);
        }

    }

}
