using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;

namespace OpenGET
{

    public class SceneNavigator : Singleton<SceneNavigator>
    {
        public delegate void SceneProcess(Scene scene);

        /// <summary>
        /// Implement this interface for things you want to persist between scene loads and unloads.
        /// </summary>
        public interface IPersist
        {
        }

        /// <summary>
        /// Persistent object. You may only have one of these; if you need to persist lots of stuff,
        /// build a custom struct or class.
        /// </summary>
        public static IPersist persist { get => sharedInstance._persist; set => sharedInstance._persist = value; }
        private IPersist _persist;

        /// <summary>
        /// All currently loading scenes. Operations are automatically removed when complete.
        /// </summary>
        private Dictionary<string, AsyncOperation> loadingScenes = new Dictionary<string, AsyncOperation>();

        /// <summary>
        /// Get the loading operation for the first scene. Returns null if there are no scenes loading.
        /// </summary>
        public static AsyncOperation TryGetLoadOperation(string name)
        {
            return sharedInstance.loadingScenes.TryGetValue(name, out AsyncOperation op) ? op : null;
        }

        /// <summary>
        /// Get the first GameObject instance in a scene that has a component of matching type.
        /// Optionally specify that only the root objects should be checked.
        /// By default only checks active objects, for inactive objects as well specify includeActive = true.
        /// </summary>
        public static T FindObject<T>(Scene scene, bool rootsOnly = false, bool includeInactive = false) where T : Behaviour
        {
            GameObject[] roots = scene.GetRootGameObjects();
            T rootObj = null;
            for (int i = 0, counti = roots.Length; i < counti; i++) {
                rootObj = roots[i].GetComponentInChildren<T>(includeInactive);
                if (rootObj != null) {
                    break;
                }
            }
            return rootObj;
        }

        /// <summary>
        /// Find an object of a given type among all loaded scenes.
        /// </summary>
        public static T FindObject<T>(bool rootsOnly = false, bool includeInactive = false) where T : Behaviour
        {
            for (int i = 0, counti = SceneManager.loadedSceneCount; i < counti; i++)
            {
                T obj = FindObject<T>(SceneManager.GetSceneAt(i), rootsOnly, includeInactive);
                if (obj != null)
                {
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// Attempt to find and unload a scene. Returns false if no such scene was found.
        /// </summary>
        public static bool TryUnloadScene(string name, SceneProcess onSceneRemoved = null)
        {
            void onSceneUnloaded(Scene scene)
            {
                if (name == scene.name)
                {
                    Log.Debug("Scene \"{0}\" unloaded successfully.", scene.name);
                    SceneManager.sceneUnloaded -= onSceneUnloaded;
                }
            }

            for (int i = 0, counti = SceneManager.sceneCount; i < counti; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == name)
                {
                    Log.Info("Unloading scene \"{0}\"", scene.name);
                    SceneManager.sceneUnloaded += onSceneUnloaded;
                    SceneManager.UnloadSceneAsync(scene);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Find a scene by name, or additively load it if not, and do some processing.
        /// Optionally force reloading of a scene if it is found.
        /// </summary>
        public static void FindOrAddScene(string name, SceneProcess onSceneReady, bool forceReload = false)
        {
            void onSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (name == scene.name)
                {
                    SceneManager.sceneLoaded -= onSceneLoaded;
                    if (sharedInstance.loadingScenes.ContainsKey(name))
                    {
                        sharedInstance.loadingScenes.Remove(name);
                    }
                    onSceneReady(scene);
                }
            }

            void doLoadScene()
            {
                SceneManager.sceneLoaded += onSceneLoaded;
                sharedInstance.loadingScenes.Add(name, SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive));
            }

            void onSceneUnloaded(Scene scene)
            {
                if (name == scene.name)
                {
                    Log.Debug("Scene \"{0}\" unloaded successfully, reloading...", scene.name);
                    SceneManager.sceneUnloaded -= onSceneUnloaded;
                    doLoadScene();
                }
            }

            Log.Debug("Searching for scene \"{0}\" to add and/or process with custom callback.", name);
            for (int i = 0, counti = SceneManager.sceneCount; i < counti; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == name)
                {
                    if (forceReload)
                    {
                        Log.Info("Force reloading scene \"{0}\"", scene.name);
                        SceneManager.sceneUnloaded += onSceneUnloaded;
                        SceneManager.UnloadSceneAsync(scene);
                    }
                    else
                    {
                        Log.Debug("Found scene \"{0}\", processing with custom callback...");
                        onSceneReady(scene);
                    }
                    return;
                }
            }

            doLoadScene();

        }

        /// <summary>
        /// Get the hierarchy path to a GameObject instance as a string.
        /// </summary>
        public static string GetPath(GameObject gameObject) {
            if (gameObject == null)
            {
                return null;
            }
            string path = gameObject.name;
            gameObject = gameObject.transform.parent != null ? gameObject.transform.parent.gameObject : null;
            while (gameObject != null) {
                path = gameObject.name + "/" + path;
                gameObject = gameObject.transform.parent != null ? gameObject.transform.parent.gameObject : null;
            }
            return path;
        }

        /// <summary>
        /// Get the hierarchy path to a component instance as a string.
        /// Convenience overload.
        /// </summary>
        public static string GetPath(Component component)
        {
            return GetPath(component != null ? component.gameObject : null);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Load each scene individually in sequence running a processor function for each, then restore the original scenes.
        /// </summary>
        public static void EditorSceneProcess(string[] paths, SceneProcess processor)
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
