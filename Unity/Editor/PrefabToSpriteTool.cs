using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

namespace OpenGET
{

    public class PrefabToSpriteTool : EditorWindow
    {
        /// <summary>
        /// OpenGET editor settings object reference.
        /// </summary>
        public EditorConfig config;

        /// <summary>
        /// Prefab to Sprite config settings property field.
        /// </summary>
        private UnityEditor.UIElements.PropertyField prefabToSpriteConfig;

        /// <summary>
        /// Used to serialise this.
        /// </summary>
        private SerializedObject serialiser;

        /// <summary>
        /// Setup the editor window.
        /// </summary>
        [MenuItem("OpenGET/Prefab to Sprite")]
        public static void Open()
        {
            PrefabToSpriteTool window = GetWindow<PrefabToSpriteTool>();
            window.titleContent = new GUIContent("Prefab to Sprite [OpenGET]");
        }

        /// <summary>
        /// Editor window content.
        /// </summary>
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // OpenGET configuration settings reference
            serialiser = new SerializedObject(this);
            SerializedProperty prop = serialiser.FindProperty("config");
            prefabToSpriteConfig = new UnityEditor.UIElements.PropertyField(prop);
            UnityEditor.UIElements.BindingExtensions.Bind(prefabToSpriteConfig, serialiser);
            root.Add(prefabToSpriteConfig);

            // Make sure we always have a valid configuration
            if (config == null || config.name == null || config.name.Length <= 0)
            {
                config = EditorConfig.Instance;
            }

            // Display Prefab to Sprite settings
            SerializedObject obj = new SerializedObject(config);
            prop = obj.FindProperty("prefabToSprite");
            UnityEditor.UIElements.PropertyField addProp = new UnityEditor.UIElements.PropertyField(prop);
            UnityEditor.UIElements.BindingExtensions.Bind(addProp, obj);
            root.Add(addProp);

            Button button = new Button(() => {
                if (config != null && config.prefabToSprite != null && config.prefabToSprite.assetLoader != null && config.prefabToSprite.renderScene != null)
                {
                    // Load the render scene
                    string scenePath = AssetDatabase.GetAssetPath(config.prefabToSprite.renderScene);
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                        scenePath,
                        UnityEditor.SceneManagement.OpenSceneMode.Single
                    );

                    // Setup rendering system
                    Camera cam = Camera.main;
                    int width = config.prefabToSprite.spriteWidth;
                    int height = config.prefabToSprite.spriteHeight;
                    Rect rect = new Rect(0, 0, width, height);
                    RenderTexture renderOut = new RenderTexture(width, height, 24);
                    cam.targetTexture = renderOut;
                    Texture2D outputTexture = null;

                    // Load assets in one by one and render each one to images
                    GameObject[] assets = config.prefabToSprite.assetLoader.ReadAssets(config) as GameObject[];
                    List<Sprite> generated = new List<Sprite>();
                    for (int i = 0, counti = assets.Length; i < counti; i++)
                    {
                        if (assets[i] != null)
                        {
                            Sprite created = null;
                            try
                            {
                                // Setup prefab
                                GameObject instance = GameObject.Instantiate(assets[i]);
                                instance.transform.position = Vector3.zero;
                                MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>();
                                Bounds modelBounds = new Bounds(renderers.Length > 0 ? renderers[0].bounds.center : Vector2.zero, Vector2.zero);

                                // Setup camera
                                for (int j = 0, countj = renderers.Length; j < countj; j++)
                                {
                                    modelBounds.Encapsulate(renderers[j].bounds);
                                }

                                Vector3 camUp = cam.transform.up;
                                Vector3 camRight = cam.transform.right;

                                float maxExtents = Mathf.Max(modelBounds.extents.x, modelBounds.extents.y, modelBounds.extents.z);
                                Vector3 extents = new Vector3(maxExtents, maxExtents, maxExtents);

                                float vertical = Mathf.Abs(Vector3.Dot(extents, camUp));
                                float horizontal = Mathf.Abs(Vector3.Dot(extents, camRight));
                                float depthExtent = Mathf.Abs(Vector3.Dot(extents, cam.transform.forward));
                                cam.orthographicSize = Mathf.Max(vertical, horizontal / cam.aspect);

                                float distance = depthExtent * 10f + cam.nearClipPlane;
                                cam.transform.position = modelBounds.center - cam.transform.forward * distance;

                                outputTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                                cam.Render();

                                RenderTexture originalRT = RenderTexture.active;
                                RenderTexture.active = renderOut;
                                outputTexture.ReadPixels(rect, 0, 0);
                                outputTexture.Apply();

                                RenderTexture.active = originalRT;

                                DestroyImmediate(instance);

                                // Save texture as PNG and sprite
                                string name = config.prefabToSprite.outputFilenamePrefix + assets[i].name;
                                string assetPath = config.prefabToSprite.outputFolder + "/" + name + ".png";
                                string pngPath =
                                    Application.dataPath + "/" + config.prefabToSprite.outputFolder.Split("Assets/").Last() + "/" + name + ".png";

                                Log.Debug("Saving texture as PNG at {0}", pngPath);
                                System.IO.File.WriteAllBytes(pngPath, outputTexture.EncodeToPNG());
                                AssetDatabase.Refresh();
                                TextureImporter importer = (AssetImporter.GetAtPath(assetPath) as TextureImporter);
                                importer.textureType = TextureImporterType.Sprite;
                                importer.spriteImportMode = SpriteImportMode.Single;
                                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                                created = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                            }
                            catch (System.Exception e)
                            {
                                Log.Error("Failed to render prefab at index {0} due to exception: {1}", i, e);
                            }
                            finally
                            {
                                generated.Add(created);
                            }
                        }
                        else
                        {
                            Log.Warning("Failed to load prefab at index {0}, is it definitely a GameObject?", i);
                        }
                    }

                    cam.targetTexture = null;
                    DestroyImmediate(renderOut);

                    // Save to assets
                    config.prefabToSprite.assetLoader.WriteAssets(config, generated.ToArray());
                }
            });
            button.name = "Generate Sprite(s)";
            button.text = "Render prefabs to 2D sprites.";
            root.Add(button);

        }

    }

}
