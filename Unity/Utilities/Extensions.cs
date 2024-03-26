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

        public static void SwapRemoveAt<T>(this IList<T> list, int index)
        {
            int end = list.Count - 1;
            T intermediate = list[list.Count - 1];
            list[end] = list[index];
            list[index] = intermediate;
            list.RemoveAt(end);
        }

    }

}
