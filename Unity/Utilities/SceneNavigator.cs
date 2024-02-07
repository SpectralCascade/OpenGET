using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenGET
{

    public static class SceneNavigator
    {
        public delegate void SceneProcess(Scene scene);

        /// <summary>
        /// Get the first root GameObject instance in a scene that has a component of matching type.
        /// </summary>
        public static T FindRootObject<T>(Scene scene) where T : Behaviour {
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

        /// <summary>
        /// Get the hierarchy path to a GameObject instance as a string.
        /// </summary>
        public static string GetGameObjectPath(GameObject gameObject) {
            string path = gameObject.name;
            gameObject = gameObject.transform.parent?.gameObject;
            while (gameObject != null) {
                path = gameObject.name + "/" + path;
                gameObject = gameObject.transform.parent?.gameObject;
            }
            return path;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Load each scene individually in sequence running a processor function for each, then restore the original scenes.
        /// </summary>
        public static void RunSceneProcess(string[] paths, SceneProcess processor)
        {
            if (Application.isPlaying)
            {
                Log.Error("Cannot run scene processing in play mode.");
                return;
            }

            // Remember current loaded scenes
            string[] originalScenes = new string[SceneManager.sceneCount];
            int activeScene = SceneManager.GetActiveScene().buildIndex;
            for (int i = 0, counti = originalScenes.Length; i < counti; i++)
            {
                originalScenes[i] = SceneManager.GetSceneAt(i).path;
            }

            // Load and process every scene
            for (int i = 0, counti = paths.Length; i < counti; i++)
            {
                try
                {
                    processor.Invoke(
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                            paths[i], UnityEditor.SceneManagement.OpenSceneMode.Single
                        )
                    );
                }
                catch (System.Exception e)
                {
                    Log.Exception(e);
                }
            }

            // Restore scenes
            for (int i = 0, counti = originalScenes.Length; i < counti; i++)
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                    originalScenes[i],
                    i == 0 ? UnityEditor.SceneManagement.OpenSceneMode.Single
                    : UnityEditor.SceneManagement.OpenSceneMode.Additive
                );
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(activeScene));
        }
#endif

    }

}
