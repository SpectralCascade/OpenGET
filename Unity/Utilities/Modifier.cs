using Codice.CM.SEIDInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using OpenGET.Expressions;
using UnityEngine;

namespace OpenGET {

    /// <summary>
    /// Dynamic field reflection, useful for the Unity editor and/or modding support.
    /// For example, this can be used to run simple logic like adding a value to an integer field, configured in the Unity inspector - no code required.
    /// You can exclude specific fields from being accessible to Modifiers by using the [OpenGET.NoMod] attribute.
    /// Use the [OpenGET.Mod] attribute to specify reflection options to apply to Modifiers.
    /// </summary>
    [Serializable]
    [NoMod]
    [CreateAssetMenu(fileName = "DefaultModifier", menuName = "OpenGET/Modifier")]
    public class Modifier : Referrable, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Your custom expression.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public Polymorph expression = new Constant(new VariantInteger(0));

        [HideInInspector]
        [SerializeField]
        private string data;

        [HideInInspector]
        [SerializeField]
        private string dataType;

        /// <summary>
        /// Builds the expression, given some 
        /// </summary>
        public void Build()
        {

        }

        public void Save()
        {
            data = expression.PolymorphSerialise(out Type type);
            dataType = type.AssemblyQualifiedName;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(data))
            {
                expression = Polymorph.PolymorphLoad(data, dataType);
                data = null;
                dataType = null;
            }
        }

        /// <summary>
        /// Apply expression to modify the target value using custom Variants.
        /// Returns true if the value was changed.
        /// Throws an ArgumentException if the target type or any type in the expression is not supported by the FactoryType.
        /// Throws an InvalidOperationException if the expression is invalid (i.e. an operation is unsupported).
        /// </summary>
        //public bool Apply<TargetType, FactoryType>(TargetType target) where FactoryType : VariantFactory, new()
        //{
        //    bool changed = true;
        //    FieldInfo field = typeof(TargetType).GetField(fieldName);
        //    if (field != null)
        //    {
        //        object currentValue = field.GetValue(target);

        //        FactoryType factory = new FactoryType();
        //        try
        //        {
        //            factory.Create<TargetType>(currentValue);

        //            object setValue = expression.Evaluate<FactoryType>().value;

        //            changed = !setValue.Equals(currentValue);
        //            if (changed)
        //            {
        //                field.SetValue(target, setValue);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Exception(e);
        //        }
        //    }
        //    else
        //    {
        //        Log.Warning("Failed to find field \"{0}\" to modify.", fieldName);
        //        changed = false;
        //    }

        //    return changed;
        //}

        /// <summary>
        /// Apply expression to modify the target value using standard OpenGET Variants.
        /// Returns true if the value was changed.
        /// Throws an ArgumentException if the target type or any type in the expression is not supported by the StandardVariantFactory.
        /// Throws an InvalidOperationException if the expression is invalid (i.e. an operation is unsupported).
        /// </summary>
        //public bool Apply<TargetType>(TargetType target)
        //{
        //    return Apply<TargetType, StandardVariantFactory>(target);
        //}

        public override string ToString()
        {
            return expression.ToString();
        }
    }

    /// <summary>
    /// Specify binding options for Modifier(s).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ModAttribute : Attribute
    {
        /// <summary>
        /// Reflection binding flags.
        /// </summary>
        public BindingFlags flags;

        /// <summary>
        /// Is this "modifier" actually allowed to modify the target field?
        /// </summary>
        public bool readOnly;

        /// <summary>
        /// Specify binding options for Modifier instance(s).
        /// </summary>
        /// <param name="flags">Determine what kinds of fields can be accessed by this modifier.</param>
        /// <param name="readOnly">Only allow this modifier to read field values, rather than actually modify fields.</param>
        public ModAttribute(BindingFlags flags, bool readOnly = false) {
            this.flags = flags;
            this.readOnly = readOnly;
        }
    }

    /// <summary>
    /// Prevent modifiers from accessing a field or all fields within an entire class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    public class NoModAttribute : Attribute {}

}
