using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenGET
{

    public static class SceneNavigator
    {
        public static T FindRootObject<T>(Scene scene) where T : MonoBehaviour {
            GameObject[] roots = scene.GetRootGameObjects();
            T rootObj = null;
            for (int i = 0, counti = roots.Length; i < counti; i++) {
                rootObj = roots[i].GetComponent<T>();
                if (rootObj != null) {
                    break;
                }
            }
            return rootObj;
        }

    }

}
