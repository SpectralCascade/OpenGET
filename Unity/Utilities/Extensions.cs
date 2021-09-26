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

    }

}
