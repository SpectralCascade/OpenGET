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
        public static void Hookup<T>(T obj, bool includeInherited = false, bool useDerivedType = false) where T : MonoBehaviour
        {
            if (obj == null)
            {
                throw new System.NullReferenceException("Cannot autohookup a null instance.");
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
                if (fields[i].GetValue(obj) == null &&
                    fields[i].FieldType.IsSubclassOf(typeof(MonoBehaviour)) &&
                    !fields[i].IsNotSerialized
                )
                {
                    System.Reflection.CustomAttributeData attribute = fields[i].CustomAttributes.FirstOrDefault(
                        x => x.AttributeType == typeof(HookupAttribute)
                    );
                    if (attribute != null)
                    {
                        Mode autoHook = (Mode)attribute.ConstructorArguments[0].Value;
                        switch (autoHook)
                        {
                            case Mode.Self:
                                fields[i].SetValue(obj, obj.GetComponent(fields[i].FieldType));
                                break;
                            case Mode.Children:
                                fields[i].SetValue(obj, obj.GetComponentInChildren(fields[i].FieldType));
                                break;
                            case Mode.Parent:
                                fields[i].SetValue(obj, obj.GetComponentInParent(fields[i].FieldType));
                                break;
                            default:
                                break;
                        }

                        if (fields[i].GetValue(obj) != null)
                        {
                            Log.Debug(
                                "Successfully auto-assigned component to field \"{0}\" of type \"{1}\" on GameObject \"{2}\".",
                                fields[i].Name,
                                fields[i].FieldType,
                                SceneNavigator.GetGameObjectPath(obj.gameObject)
                            );
                            UnityEditor.EditorUtility.SetDirty(obj);
                        }
                    }
                }
            }
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
                if (fields[i].FieldType.IsSubclassOf(typeof(Object)) &&
                    !fields[i].IsNotSerialized &&
                    fields[i].CustomAttributes.Where(x => x.AttributeType == typeof(NullCheckAttribute)).Count() > 0
                )
                {
                    bool isGameObject = objType == typeof(GameObject);
                    bool isComponent = !isGameObject && objType.IsSubclassOf(typeof(MonoBehaviour));
                    string message = "Missing reference to " + fields[i].FieldType.ToString();
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

                    Debug.Assert(fields[i].GetValue(obj) != null, Log.PrefixStackInfo(Log.Format("red", message)), isComponent ? (Object)((obj as MonoBehaviour).gameObject) : obj);
                }
            }
        }

    }

}
