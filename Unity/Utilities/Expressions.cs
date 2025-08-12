using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditorInternal;
using UnityEngine;

namespace OpenGET.Expressions
{

    /// <summary>
    /// Wrapper that determines how to operate on objects of different types.
    /// </summary>
    [NoMod]
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public abstract class Variant : IVariant
    {
        private enum VarType
        {
            Integer = 0,
            Float,
            String
        }

        /// <summary>
        /// The underlying value of this variant.
        /// </summary>
        public abstract object value { get; protected set; }

        /// <summary>
        /// Does this have a valid value?
        /// </summary>
        public bool hasValue => value != null;

        public Variant(object value)
        {
            this.value = value;
        }

        protected abstract Variant Add(Variant b);
        protected abstract Variant Subtract(Variant b);
        protected abstract Variant Multiply(Variant b);
        protected abstract Variant Divide(Variant b);

        public static Variant operator+(Variant a, Variant b)
        {
            return a.Add(b);
        }

        public static Variant operator-(Variant a, Variant b)
        {
            return a.Subtract(b);
        }

        public static Variant operator*(Variant a, Variant b)
        {
            return a.Multiply(b);
        }

        public static Variant operator/(Variant a, Variant b)
        {
            return a.Divide(b);
        }
    }

    [NoMod]
    public abstract class VariantFactory
    {
        /// <summary>
        /// Context objects with which variable fields are reflected.
        /// These are only used by DynamicVariables in expressions.
        /// </summary>
        public abstract object[] contexts { get; }

        public abstract Variant Create(object value, Type type);

        public Variant Create<T>(object value)
        {
            return Create(value, typeof(T));
        }

        public Variant Create<T>(T value)
        {
            return Create<T>((object)value);
        }
    }

    public class StandardVariantFactory : VariantFactory
    {
        public override object[] contexts => _contexts;

        private object[] _contexts = new object[0];

        public StandardVariantFactory(object[] contexts)
        {
            if (contexts != null)
            {
                _contexts = contexts;
            }
        }

        /// <summary>
        /// A type factory for OpenGET supported variants. If you want to add custom variants,
        /// you should make your own type factory to instantiate those variants.
        /// </summary>
        public override Variant Create(object value, Type type)
        {
            if (type.Equals(typeof(string)))
            {
                return new VariantString(value);
            }
            else if (type.Equals(typeof(float)))
            {
                return new VariantFloat(value);
            }
            else if (type.Equals(typeof(int)))
            {
                return new VariantInteger(value);
            }

            throw new ArgumentException($"Variant of type {type.FullName} is unsupported by {GetType().FullName}. Please use a custom type factory instead.");
        }
    }

    /// <summary>
    /// String value.
    /// </summary>
    [Serializable]
    public class VariantString : Variant
    {
        public string v;

        public override object value {
            get => v;
            protected set => v = (string)value;
        }

        public VariantString(object value) : base(value) { }

        protected override Variant Add(Variant b)
        {
            return new VariantString(v + b.value);
        }

        protected override Variant Divide(Variant b)
        {
            throw new InvalidOperationException("Cannot divide string variants!");
        }

        protected override Variant Multiply(Variant b)
        {
            throw new InvalidOperationException("Cannot multiply string variants!");
        }

        protected override Variant Subtract(Variant b)
        {
            throw new InvalidOperationException("Cannot subtract string variants!");
        }
    }

    public interface IVariant
    {
        public abstract object value { get; }
    }

    public interface IVariantNumeric : IVariant {
        public abstract float valueFloat { get; }
    };

    [Serializable]
    public abstract class VariantNumeric<T> : Variant, IVariantNumeric where T : struct
    {
        public T v;

        public override object value {
            get => v;
            protected set => v = (T)value;
        }
        public abstract float valueFloat { get; }

        public VariantNumeric(object value) : base(value) { }
    }

    /// <summary>
    /// Float value.
    /// </summary>
    [Serializable]
    public class VariantFloat : VariantNumeric<float>
    {
        public override float valueFloat => v;

        public VariantFloat(object value) : base(value) { }

        protected override Variant Add(Variant b)
        {
            if (b is VariantInteger)
            {
                return new VariantFloat(v + (int)b.value);
            }
            else if (b is VariantFloat)
            {
                return new VariantFloat(v + (float)b.value);
            }
            else if (b is VariantString)
            {
                return new VariantString(v + (string)b.value);
            }

            throw new InvalidOperationException($"Cannot add a variant of type {GetType().Name} with variant of type {b.GetType().Name}.");
        }

        protected override Variant Divide(Variant b)
        {
            if (b is VariantInteger)
            {
                return new VariantFloat(v / (int)b.value);
            }
            else if (b is VariantFloat)
            {
                return new VariantFloat(v / (float)b.value);
            }

            throw new InvalidOperationException($"Cannot divide a variant of type {GetType().Name} with variant of type {b.GetType().Name}.");
        }

        protected override Variant Multiply(Variant b)
        {
            if (b is VariantInteger)
            {
                return new VariantFloat(v * (int)b.value);
            }
            else if (b is VariantFloat)
            {
                return new VariantFloat(v * (float)b.value);
            }

            throw new InvalidOperationException($"Cannot multiply a variant of type {GetType().Name}  with variant of type  {b.GetType().Name}.");
        }

        protected override Variant Subtract(Variant b)
        {
            if (b is VariantInteger)
            {
                return new VariantFloat(v - (int)b.value);
            }
            else if (b is VariantFloat)
            {
                return new VariantFloat(v - (float)b.value);
            }

            throw new InvalidOperationException($"Cannot subtract a variant of type {GetType().Name}  with variant of type  {b.GetType().Name}.");
        }
    }

    [Serializable]
    public class VariantInteger : VariantNumeric<int>
    {
        public override float valueFloat => v;

        public VariantInteger(object value) : base(value) { }

        protected override Variant Add(Variant b)
        {
            if (b is VariantInteger)
            {
                return new VariantInteger(v + (int)b.value);
            }
            else if (b is VariantFloat)
            {
                return new VariantFloat(v + (float)b.value);
            }
            else if (b is VariantString)
            {
                return new VariantString(v + (string)b.value);
            }

            throw new InvalidOperationException($"Cannot add a variant of type {GetType().Name} with variant of type {b.GetType().Name}.");
        }

        protected override Variant Divide(Variant b)
        {
            if (b is VariantInteger)
            {
                return new VariantInteger(v / (int)b.value);
            }
            else if (b is VariantFloat)
            {
                return new VariantFloat(v / (float)b.value);
            }

            throw new InvalidOperationException($"Cannot divide a variant of type {GetType().Name} with variant of type {b.GetType().Name}.");
        }

        protected override Variant Multiply(Variant b)
        {
            if (b is VariantInteger)
            {
                return new VariantInteger(v * (int)b.value);
            }
            else if (b is VariantFloat)
            {
                return new VariantFloat(v * (float)b.value);
            }

            throw new InvalidOperationException($"Cannot multiply a variant of type {GetType().Name}  with variant of type  {b.GetType().Name}.");
        }

        protected override Variant Subtract(Variant b)
        {
            if (b is VariantInteger)
            {
                return new VariantInteger(v - (int)b.value);
            }
            else if (b is VariantFloat)
            {
                return new VariantFloat(v - (float)b.value);
            }

            throw new InvalidOperationException($"Cannot subtract a variant of type {GetType().Name}  with variant of type  {b.GetType().Name}.");
        }
    }

    /// <summary>
    /// Represents a logical expression.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public abstract class Expression
    {
        [Serializable]
        public class Serialisable : ISerializationCallbackReceiver
        {
            /// <summary>
            /// Your custom expression.
            /// </summary>
            [HideInInspector]
            [NonSerialized]
            public Expression expression = new Constant(new VariantFloat(0f));

            /// <summary>
            /// Serialised expression data.
            /// </summary>
            [HideInInspector]
            [SerializeField]
            private string data = "";

            public void OnBeforeSerialize()
            {
                data = ToJSON(expression);
            }

            public void OnAfterDeserialize()
            {
                expression = FromJSON(data);
                data = "";
            }
        }

        public static JsonSerializerSettings JsonSerialization => new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
        };

        public static Expression FromJSON(string json)
        {
            return JsonConvert.DeserializeObject(json, JsonSerialization) as Expression;
        }

        public static string ToJSON(Expression obj)
        {
            return JsonConvert.SerializeObject(obj, JsonSerialization);
        }

        public abstract override string ToString();

        /// <summary>
        /// Evaluate an expression to get a value back.
        /// </summary>
        public abstract Variant Evaluate<FactoryType>(FactoryType factory) where FactoryType : VariantFactory;

        /// <summary>
        /// Default to a standard factory with no contexts provided.
        /// </summary>
        public Variant Evaluate()
        {
            return Evaluate(new StandardVariantFactory(null));
        }
    }

    /// <summary>
    /// Binary operation, i.e. 2 expressions being operated on.
    /// </summary>
    [Serializable]
    public abstract class BinaryOperator : Expression
    {
        public Expression a;
        public Expression b;

        public BinaryOperator(Expression a, Expression b)
        {
            this.a = a;
            this.b = b;
        }

        public abstract string opString { get; }

        public override string ToString()
        {
            return "(" + a.ToString() + " " + opString + " " + b.ToString() + ")";
        }
    }

    /// <summary>
    /// Ternary operation, i.e. 3 expressions being operated on. The base class operates the same way as the C# ternary operator.
    /// </summary>
    [Serializable]
    public class TernaryOperator : Expression
    {
        public Expression a;
        public Expression b;
        public Expression c;

        public TernaryOperator() { }

        public TernaryOperator(Expression a, Expression b, Expression c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public override Variant Evaluate<T>(T factory)
        {
            Variant res = a.Evaluate(factory);
            return res.value != null && res.value.GetType().Equals(typeof(bool)) ? ((bool)res.value ? b.Evaluate(factory) : c.Evaluate(factory)) : factory.Create(null, typeof(object));
        }

        public override string ToString()
        {
            return "(" + a.ToString() + " ? " + b.ToString() + " : " + c.ToString() + ")";
        }
    }

    /// <summary>
    /// Applies Mathf.Clamp().
    /// </summary>
    [Serializable]
    public class ClampOperator : TernaryOperator
    {
        public ClampOperator(Expression a, Expression b, Expression c) : base(a, b, c) {}

        public override Variant Evaluate<T>(T factory)
        {
            IVariantNumeric a_val = a.Evaluate(factory) as IVariantNumeric;
            IVariantNumeric b_val = b.Evaluate(factory) as IVariantNumeric;
            IVariantNumeric c_val = c.Evaluate(factory) as IVariantNumeric;

            bool valid = a_val != null && b_val != null && c_val != null;
            bool allIntegers = a_val is VariantInteger && b_val is VariantInteger && c_val is VariantInteger;

            return valid ? (
                allIntegers ? 
                    new VariantInteger(Mathf.Clamp((int)a_val.value, (int)b_val.value, (int)c_val.value)) :
                    new VariantFloat(Mathf.Clamp(a_val.valueFloat, b_val.valueFloat, c_val.valueFloat))
            ) : factory.Create(null, typeof(object));
        }

        public override string ToString()
        {
            return "clamp(" + a.ToString() + ", " + b.ToString() + ", " + c.ToString() + ")";
        }
    }

    [Serializable]
    public class BinOpAdd : BinaryOperator
    {
        public BinOpAdd(Expression a, Expression b) : base(a, b) { }

        public override string opString => "+";

        public override Variant Evaluate<T>(T factory) => a.Evaluate(factory) + b.Evaluate(factory);
    }

    [Serializable]
    public class BinOpSubtract : BinaryOperator
    {
        public BinOpSubtract(Expression a, Expression b) : base(a, b) { }

        public override string opString => "-";

        public override Variant Evaluate<T>(T factory) => a.Evaluate(factory) - b.Evaluate(factory);
    }

    [Serializable]
    public class BinOpMultiply : BinaryOperator
    {
        public BinOpMultiply(Expression a, Expression b) : base(a, b) { }

        public override string opString => "*";

        public override Variant Evaluate<T>(T factory) => a.Evaluate(factory) * b.Evaluate(factory);
    }

    [Serializable]
    public class BinOpDivide : BinaryOperator
    {
        public BinOpDivide(Expression a, Expression b) : base(a, b) { }

        public override string opString => "/";

        public override Variant Evaluate<T>(T factory) => a.Evaluate(factory) / b.Evaluate(factory);
    }

    /// <summary>
    /// Constant value.
    /// </summary>
    [Serializable]
    public class Constant : Expression
    {
        public Variant value;

        public Constant(Variant value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.value?.ToString();
        }

        public override Variant Evaluate<T>(T factory)
        {
            return value;
        }
    }

    [Serializable]
    public abstract class Variable : Expression
    {
        public string name;

        public abstract object target { get; }

        public Variable(string name)
        {
            this.name = name;
        }

        public override Variant Evaluate<T>(T factory)
        {
            FieldInfo field = target.GetType().GetField(name);
            if (field != null)
            {
                return factory.Create(field.GetValue(target), field.FieldType);
            }
            return factory.Create(null, typeof(object));
        }

        public override string ToString()
        {
            string targetName = target?.GetType()?.Name;
            return (targetName != null ? targetName + "." : "") + name;
        }

    }

    /// <summary>
    /// Named variable value. This maps to a field on a Unity object.
    /// </summary>
    [Serializable]
    public class UnityVariable : Variable
    {
        private class ObjectConverter : JsonConverter<ObjectWrapper>
        {
            public override ObjectWrapper ReadJson(JsonReader reader, Type objectType, ObjectWrapper existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                bool valid = reader.Value != null && (string)reader.Value != "";
                ObjectWrapper obj = new ObjectWrapper();
                if (valid)
                {
                    object restored = JsonUtility.FromJson(reader.Value as string, objectType);
                    if (restored != null)
                    {
                        obj = (ObjectWrapper)restored;
                    }
                }
                return obj;
            }

            public override void WriteJson(JsonWriter writer, ObjectWrapper value, JsonSerializer serializer)
            {
                writer.WriteValue(JsonUtility.ToJson(value, false));
            }
        }

        /// <summary>
        /// Shim class for using Unity serialisation.
        /// </summary>
        public struct ObjectWrapper
        {
            public UnityEngine.Object reference;
        }

        /// <summary>
        /// Target Unity object to reflect.
        /// </summary>
        [JsonConverter(typeof(ObjectConverter))]
        public ObjectWrapper _target = new ObjectWrapper();

        public override object target => _target.reference;

        public UnityVariable(string name, UnityEngine.Object target) : base(name)
        {
            _target.reference = target;
        }

        public override string ToString()
        {
            return target?.GetType()?.Name + "." + name;
        }

        public override Variant Evaluate<T>(T factory)
        {
            FieldInfo field = target.GetType().GetField(name);
            if (field != null)
            {
                return factory.Create(field.GetValue(target), field.FieldType);
            }
            return factory.Create(null, typeof(object));
        }
    }

    /// <summary>
    /// Named variable value. This maps to a field on any object.
    /// Please note however that this is unsuitable for use with Unity serialisation.
    /// In cases where you require Unity serialisation, use UnityVariable or DynamicVariable instead.
    /// </summary>
    [Serializable]
    public class ReflectionVariable : Variable
    {
        /// <summary>
        /// Target object to reflect.
        /// </summary>
        public object _target;

        public override object target => _target;

        public ReflectionVariable(string name, object target) : base(name)
        {
            _target = target;
        }

        public override string ToString()
        {
            return target?.GetType()?.Name + "." + name;
        }
    }

    /// <summary>
    /// Represents a variable that changes depending on context provided at the time at which the expression is evaluated.
    /// The naming scheme for dynamic variables is based on the contexts used to create the variable initially,
    /// in the format [context index].[field name] where [context index] is the index to a target object that the target field is associated with.
    /// This means you MUST provide the same number of contexts, and types of contexts, whenever you evaluate this DynamicVariable.
    /// You should also take care when renaming fields, as doing so without updating DynamicVariables will break upon expression evaluation.
    /// </summary>
    [Serializable]
    public class DynamicVariable : Variable
    {
        public override object target => null;

        public DynamicVariable(string name) : base(name) {}

        public override Variant Evaluate<T>(T factory)
        {
            string[] split = name.Split(".");
            string id = split[0];
            string fieldName = split[1];

            if (!int.TryParse(id, out int index))
            {
                throw new Exception($"Invalid DynamicVariable \"{name}\", failed to parse context index.");
            }
            else if (index >= factory.contexts.Length)
            {
                throw new IndexOutOfRangeException($"Context at index {index} for DynamicVariable \"{name}\" is invalid (out of range).");
            }
            
            object context = factory.contexts[index];
            if (context == null)
            {
                throw new NullReferenceException($"Context {index} of DynamicVariable \"{name}\" is null!");
            }

            FieldInfo field = context.GetType().GetField(fieldName);
            if (field != null)
            {
                return factory.Create(field.GetValue(context), field.FieldType);
            }
            else
            {
                Log.Error("No such field {0} available on context {1} of type {2} (DynamicVariable {3})", fieldName, index, context.GetType().FullName, name);
            }
            return factory.Create(null, typeof(object));
        }
    }

}
