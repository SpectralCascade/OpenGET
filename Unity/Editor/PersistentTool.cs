using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpenGET
{

    public abstract class PersistentTool<Derived> : EditorWindow where Derived : PersistentTool<Derived>
    {
        /// <summary>
        /// OpenGET editor settings object reference.
        /// </summary>
        public EditorConfig config;

        /// <summary>
        /// This object's serialisation binding.
        /// </summary>
        private SerializedObject serialiser;

        /// <summary>
        /// Config settings property field.
        /// </summary>
        private UnityEditor.UIElements.PropertyField convertTextConfig;

        /// <summary>
        /// Get the name of the persistent settings.
        /// </summary>
        protected abstract string persistentSettings { get; }

        // Each editor window contains a root VisualElement object
        protected VisualElement root => rootVisualElement;

        protected static Derived CreateWindow(string name = null)
        {
            Derived window = GetWindow<Derived>();
            window.titleContent = new GUIContent($"{name ?? typeof(Derived).Name} [OpenGET]");
            return window;
        }

        public virtual void CreateGUI()
        {
            // Make sure we always have a valid configuration
            if (config == null || config.name.Length <= 0)
            {
                config = EditorConfig.Instance;
            }

            // OpenGET configuration settings reference
            serialiser = new SerializedObject(this);
            SerializedProperty prop = serialiser.FindProperty("config");
            convertTextConfig = new UnityEditor.UIElements.PropertyField(prop);
            UnityEditor.UIElements.BindingExtensions.Bind(convertTextConfig, serialiser);
            root.Add(convertTextConfig);


            // Display persistent settings
            SerializedObject obj = new SerializedObject(config);
            prop = obj.FindProperty(persistentSettings);
            UnityEditor.UIElements.PropertyField addProp = new UnityEditor.UIElements.PropertyField(prop);
            UnityEditor.UIElements.BindingExtensions.Bind(addProp, obj);
            root.Add(addProp);
        }

    }

}
