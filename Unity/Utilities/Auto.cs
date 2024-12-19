using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace OpenGET
{

    /// <summary>
    /// Semi-automated helper tools working with attributes to do maintenance tasks and validation checks.
    /// </summary>
    public static class Auto
    {

        /// <summary>
        /// Automatically assign component references.
        /// </summary>
        [System.AttributeUsage(System.AttributeTargets.Field)]
        public class HookupAttribute : System.Attribute
        {
            /// <summary>
            /// Indicates the mode to use when automatically hooking up component references.
            /// Only valid for use with MonoBehaviour classes.
            /// </summary>
            public readonly Mode mode;

            /// <summary>
            /// Defaults to first found child component.
            /// </summary>
            public HookupAttribute(Mode mode = Mode.Children)
            {
                this.mode = mode;
            }
        }

        public enum Mode
        {
            Self,       // GetComponent
            Children,   // GetComponentInChildren
            Parent      // GetComponentInParent
        }

        /// <summary>
        /// Perform auto-hookup on all components attached to a GameObject.
        /// </summary>
        public static void Hookup(GameObject gameObject)
        {
            MonoBehaviour[] all = gameObject.GetComponents<MonoBehaviour>();
            for (int i = 0, counti = all.Length; i < counti; i++)
            {
                Hookup(all[i], true, true);
            }
        }

        /// <summary>
        /// Automatically assign all component references to fields with AutoHook attributes.
        /// By default only checks non-inherited members but you can optionally include those.
        /// </summary>
        public static void Hookup<T>(T obj, bool includeInherited = false, bool useDerivedType = false) where T : Behaviour
        {
#if UNITY_EDITOR
            if (obj == null)
            {
                throw new System.NullReferenceException("Cannot Auto Hookup a null instance!");
            }

            System.Type objType = useDerivedType ? obj.GetType() : typeof(T);
            // Get all non-inherited fields
            System.Reflection.FieldInfo[] fields = objType.GetFields(
                (includeInherited ? 0 : System.Reflection.BindingFlags.DeclaredOnly) |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance
            );

            for (int i = 0, counti = fields.Length; i < counti; i++)
            {
                System.Reflection.FieldInfo field = fields[i];
                if (field.GetValue(obj) == null &&
                    field.FieldType.IsSubclassOf(typeof(Behaviour)) &&
                    !field.IsNotSerialized
                )
                {
                    System.Reflection.CustomAttributeData attribute = field.CustomAttributes.FirstOrDefault(
                        x => x.AttributeType == typeof(HookupAttribute)
                    );
                    if (attribute != null)
                    {
                        Mode autoHook = (Mode)attribute.ConstructorArguments[0].Value;
                        switch (autoHook)
                        {
                            case Mode.Self:
                                field.SetValue(obj, obj.GetComponent(field.FieldType));
                                break;
                            case Mode.Children:
                                field.SetValue(obj, obj.GetComponentInChildren(field.FieldType));
                                break;
                            case Mode.Parent:
                                field.SetValue(obj, obj.GetComponentInParent(field.FieldType));
                                break;
                            default:
                                break;
                        }

                        if (field.GetValue(obj) != null)
                        {
                            Log.Debug(
                                "Successfully auto-assigned component to field \"{0}\" of type \"{1}\" on GameObject \"{2}\".",
                                field.Name,
                                field.FieldType,
                                SceneNavigator.GetGameObjectPath(obj.gameObject)
                            );
                            UnityEditor.EditorUtility.SetDirty(obj);
                        }
                    }
                }
                else if (field.FieldType.IsArray && field.FieldType.GetElementType().IsSubclassOf(typeof(Component)) &&
                    !field.IsNotSerialized)
                {
                    System.Reflection.CustomAttributeData attribute = field.CustomAttributes.FirstOrDefault(
                        x => x.AttributeType == typeof(HookupAttribute)
                    );
                    if (attribute != null)
                    {
                        Mode autoHook = (Mode)attribute.ConstructorArguments[0].Value;

                        System.Type elementType = field.FieldType.GetElementType();
                        Component[] comps = null;

                        switch (autoHook)
                        {
                            case Mode.Self:
                                comps = obj.GetComponents(elementType);
                                break;
                            case Mode.Children:
                                comps = obj.GetComponentsInChildren(elementType);
                                break;
                            case Mode.Parent:
                                comps = obj.GetComponentsInParent(elementType);
                                break;
                            default:
                                break;
                        }
                        System.Array instance = System.Array.CreateInstance(elementType, comps.Length);
                        for (int j = 0, countj = instance.Length; j < countj; j++)
                        {
                            instance.SetValue(comps[j], j);
                        }
                        field.SetValue(obj, instance);

                        object[] array = field.GetValue(obj) as object[];
                        if (array != null && array.Length > 0)
                        {
                            Log.Verbose(
                                "Successfully auto-assigned {0} component(s) to field \"{1}\" of type \"{2}\" on GameObject \"{3}\".",
                                array.Length,
                                field.Name,
                                field.FieldType,
                                SceneNavigator.GetGameObjectPath(obj.gameObject)
                            );
                            UnityEditor.EditorUtility.SetDirty(obj);
                        }
                    }
                }
            }
#endif
        }


        [System.AttributeUsage(System.AttributeTargets.Field)]
        public class NullCheckAttribute : System.Attribute
        {
        }

        /// <summary>
        /// Assert that there are no null references on an object for all fields with the NullCheck attribute.
        /// By default only checks non-inherited members but you can optionally include those in the checks.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("OPENGET_DEBUG")]
        public static void NullCheck<T>(T obj, bool includeInherited = false, bool useDerivedType = false) where T : Object
        {
            if (obj == null)
            {
                throw new System.NullReferenceException("Cannot null check a null instance.");
            }

            System.Type objType = useDerivedType ? obj.GetType() : typeof(T);
            // Get all non-inherited fields
            System.Reflection.FieldInfo[] fields = objType.GetFields(
                (includeInherited ? 0 : System.Reflection.BindingFlags.DeclaredOnly) |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance
            );

            for (int i = 0, counti = fields.Length; i < counti; i++)
            {
                var field = fields[i];
                if (field.FieldType.IsSubclassOf(typeof(Object)) &&
                    !field.IsNotSerialized &&
                    field.CustomAttributes.Where(x => x.AttributeType == typeof(NullCheckAttribute)).Count() > 0
                )
                {
                    bool isGameObject = objType == typeof(GameObject);
                    bool isComponent = !isGameObject && objType.IsSubclassOf(typeof(MonoBehaviour));
                    string message = "Missing " + field.FieldType.ToString() + " reference '" + field.Name + "' on instance of type " + field.DeclaringType.Name;
                    GameObject target = isGameObject ? obj as GameObject : (isComponent ? (obj as MonoBehaviour).gameObject : null);
                    if (target != null)
                    {
                        message += string.Format(" at hierarchy path '{0}'", SceneNavigator.GetGameObjectPath(target));
                    }
                    else
                    {
                        System.Diagnostics.StackFrame info = new System.Diagnostics.StackTrace(true).GetFrame(1);
                        message += " in instance of " + info.GetMethod()?.DeclaringType.FullName;
                    }

                    Debug.Assert(field.GetValue(obj) != null, Log.PrefixStackInfo(Log.Format("red", message)), isComponent ? (Object)((obj as MonoBehaviour).gameObject) : obj);
                }
            }
        }

    }

}
