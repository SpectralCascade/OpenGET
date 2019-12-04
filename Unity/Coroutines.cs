using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    public class Coroutines : MonoBehaviour
    {

        /// <summary>
        /// Setup the singleton instance, and ensure it doesn't get destroyed when the scene is unloaded.
        /// </summary>
        void Awake() {
            if (sharedInstance != null) {
                Log.Warning("More than one Coroutines instance exists.");
            }
            sharedInstance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        public static Coroutine Start(IEnumerator routine) {
            Coroutine started = sharedInstance.StartCoroutine(routine);
            return started;
        }

        public static void Stop(Coroutine routine) {
            sharedInstance.StopCoroutine(routine);
        }

        public static void Stop(IEnumerator routine) {
            sharedInstance.StopCoroutine(routine);
        }

        public static void StopAll() {
            sharedInstance.StopAllCoroutines();
        }

        private static Coroutines _sharedInstance;

        /// <summary>
        /// Singleton instance.
        /// </summary>
        private static Coroutines sharedInstance { 
            get { 
                if (_sharedInstance == null) {
                    _sharedInstance = new GameObject("Coroutines").AddComponent<Coroutines>();
                }
                return _sharedInstance;
            }
            set {
                _sharedInstance = value;
            }
        }

    }

}
