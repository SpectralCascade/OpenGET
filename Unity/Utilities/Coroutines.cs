using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    public class Coroutines : Singleton<Coroutines>
    {

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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllCoroutines();
        }

    }

}
