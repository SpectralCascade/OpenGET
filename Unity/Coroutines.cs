using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    [DisallowMultipleComponent]
    public class Coroutines : MonoBehaviour
    {

        /// <summary>
        /// Setup the singleton instance, and ensure it doesn't get destroyed when the scene is unloaded.
        /// </summary>
        void Awake() {
            if (_sharedInstance != null) {
                Log.Warning("More than one Coroutines instance exists.");
                Destroy(_sharedInstance.gameObject);
            }
            _sharedInstance = this;
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
                if (_sharedInstance == null && Application.isPlaying) {
                    try {
                        _sharedInstance = new GameObject("Coroutines").AddComponent<Coroutines>();
                    } catch {
                        Log.Error("Failed to instantiate coroutines singleton! :(");
                    }
                }
                return _sharedInstance;
            }
            set {
                _sharedInstance = value;
            }
        }

    }

}
