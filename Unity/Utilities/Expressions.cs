using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OpenGET.Expressions
{

    /// <summary>
    /// Wrapper that determines how to operate on objects of different types.
    /// </summary>
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

    public abstract class VariantFactory : IContext
    {
        /// <summary>
        /// Context objects with which variable fields are reflected.
        /// These are only used by DynamicVariables in expressions.
        /// </summary>
        public abstract object[] args { get; set; }

        public abstract IContext.Parameter[] parameters { get; }

        public VariantFactory(params object[] args) {
            if (args != null)
            {
                this.args = args;
            }
        }

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
        /// <summary>
        /// Arguments corresponding to the context parameters.
        /// </summary>
        public override object[] args { get { return _args; } set { _args = value; } }
        private object[] _args = new object[0];

        /// <summary>
        /// Supply context parameters.
        /// </summary>
        public override IContext.Parameter[] parameters => new IContext.Parameter[0];

        public StandardVariantFactory() { }
        public StandardVariantFactory(params object[] args) : base(args) { }

        /// <summary>
        /// A type factory for OpenGET supported variants. If you want to add custom variants,
        /// you should make your own type factory to instantiate those variants.
        /// </summary>
        public override Variant Create(object value, Type type)
        {
            if (type.Equals(typeof(string)) || (value != null && value is string))
            {
                return new VariantString(value);
            }
            else if (type.Equals(typeof(float)) || (value != null && (value is float || value is double)))
            {
                return new VariantFloat(value);
            }
            else if (type.Equals(typeof(int)) || (value != null && (value is int)))
            {
                return new VariantInteger(value);
            }

            if (value == null)
            {
                throw new NullReferenceException("Specified value is null! StandardVariantFactory only supports non-null values in Variants.");
            }
            throw new ArgumentException($"Variant of type {type.FullName} is unsupported by {GetType().FullName}. Please use a custom type factory instead.");
        }
    }

    public class StandardVariantFactory<T1> : StandardVariantFactory
    {
        public StandardVariantFactory() { }
        public StandardVariantFactory(params object[] args) : base(args) { }

        public override IContext.Parameter[] parameters => new IContext.Parameter[] {
            new IContext.Parameter(typeof(T1), typeof(T1).FullName.Replace('+', '.'))
        };
    }

    public class StandardVariantFactory<T1, T2> : StandardVariantFactory
    {
        public StandardVariantFactory() { }
        public StandardVariantFactory(params object[] args) : base(args) { }

        public override IContext.Parameter[] parameters => new IContext.Parameter[] {
            new IContext.Parameter(typeof(T1), typeof(T1).FullName.Replace('+', '.')),
            new IContext.Parameter(typeof(T2), typeof(T2).FullName.Replace('+', '.'))
        };
    }

    public class StandardVariantFactory<T1, T2, T3> : StandardVariantFactory
    {
        public StandardVariantFactory() { }
        public StandardVariantFactory(params object[] args) : base(args) { }

        public override IContext.Parameter[] parameters => new IContext.Parameter[] {
            new IContext.Parameter(typeof(T1), typeof(T1).FullName.Replace('+', '.')),
            new IContext.Parameter(typeof(T2), typeof(T2).FullName.Replace('+', '.')),
            new IContext.Parameter(typeof(T3), typeof(T3).FullName.Replace('+', '.'))
        };
    }

    public class StandardVariantFactory<T1, T2, T3, T4> : StandardVariantFactory
    {
        public StandardVariantFactory() { }
        public StandardVariantFactory(params object[] args) : base(args) { }

        public override IContext.Parameter[] parameters => new IContext.Parameter[] {
            new IContext.Parameter(typeof(T1), typeof(T1).FullName.Replace('+', '.')),
            new IContext.Parameter(typeof(T2), typeof(T2).FullName.Replace('+', '.')),
            new IContext.Parameter(typeof(T3), typeof(T3).FullName.Replace('+', '.')),
            new IContext.Parameter(typeof(T4), typeof(T4).FullName.Replace('+', '.'))
        };
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

        public override string ToString() => v;
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

        public override string ToString() => v.ToString();
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
    /// Provides contextual data for expressions.
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// Defines an expected type parameter.
        /// </summary>
        public struct Parameter
        {
            public Parameter(Type type, string name)
            {
                this.type = type;
                this.name = name;
            }

            public Type type;
            public string name;
        }

        /// <summary>
        /// Returns the list of parameters expected for this context.
        /// </summary>
        public Parameter[] parameters { get; }

    }

    /// <summary>
    /// Represents a logical expression.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public abstract class Expression
    {
        [Flags]
        public enum Mutability
        {
            Immutable = 0,
            Value = 1,
            Type = 2,
            Operator = 4,
            Delete = 8,
            FullyMutable = Value | Type | Operator | Delete
        }

        [Serializable]
        public abstract class BaseSerialisable : ISerializationCallbackReceiver
        {
            /// <summary>
            /// Your custom expression.
            /// </summary>
            [HideInInspector]
            public Expression expression {
                get {
                    if (!string.IsNullOrEmpty(data) && isDirty) {
                        _expression = FromJSON(data);
                        isDirty = false;
                    }
                    return _expression;
                }
                set {
                    _expression = value;
                }
            }
#if UNITY_EDITOR
            [System.NonSerialized]
            public
#else
            private
#endif
                Expression _expression = new Constant(new VariantInteger(0));

            /// <summary>
            /// Get the variant factory.
            /// </summary>
            public abstract VariantFactory CreateFactory(params object[] args);

            /// <summary>
            /// Is the expression mutable, and if so in what way?
            /// </summary>
            public Mutability mutability = Mutability.FullyMutable;

            /// <summary>
            /// Serialised expression data.
            /// </summary>
            [HideInInspector]
            [SerializeField]
            private string data = "";

            /// <summary>
            /// Used for checking whether deserialisation has occurred.
            /// </summary>
            [System.NonSerialized]
            public bool isDirty = false;

            public BaseSerialisable() { }

            public BaseSerialisable(Mutability mutability)
            {
                this.mutability = mutability;
            }

            public abstract Variant Evaluate();

            public abstract Variant Evaluate(params object[] args);

            public void OnBeforeSerialize()
            {
#if UNITY_EDITOR
                if (!EditorApplication.isUpdating && !EditorApplication.isCompiling && _expression != null)
                {
#endif
                    data = ToJSON(expression);
#if UNITY_EDITOR
                }
#endif
            }

            public void OnAfterDeserialize()
            {
                // Do nothing; lazy-load from JSON via getter to avoid Unity being unhappy with Object references
                isDirty = true;
            }
        }

        [Serializable]
        public class Serialisable<FactoryType> : BaseSerialisable where FactoryType : VariantFactory, new()
        {
            public Serialisable() : base() { }

            public Serialisable(Mutability mutability) : base(mutability) { }

            public override Variant Evaluate()
            {
                return expression.Evaluate(CreateFactory());
            }

            public override Variant Evaluate(params object[] args)
            {
                return expression.Evaluate(CreateFactory(args));
            }

            public override VariantFactory CreateFactory(params object[] args)
            {
                FactoryType factory = new FactoryType();
                if (args != null)
                {
                    factory.args = args;
                }
                return factory;
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
        /// Default to a standard factory with no context provided.
        /// </summary>
        public Variant Evaluate()
        {
            return Evaluate(new StandardVariantFactory());
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

    [Serializable]
    public class BinOpAssign : BinaryOperator
    {
        public BinOpAssign(DynamicVariable a, Expression b) : base(a, b) { }

        public override string opString => "=";

        public override Variant Evaluate<FactoryType>(FactoryType factory)
        {
            Variant value = b.Evaluate(factory);
            (a as DynamicVariable).SetValue(factory, value);
            return value;
        }
    }

    /// <summary>
    /// Represents a variable that holds a specific value.
    /// </summary>
    public abstract class Variable : Expression
    {
        public abstract void SetValue<T>(T factory, Variant value) where T : VariantFactory;

        public Variable() { }

        public Variable(Variant value)
        {
            SetValue<VariantFactory>(null, value);
        }
    }

    /// <summary>
    /// Constant value that should not change at runtime.
    /// </summary>
    [Serializable]
    public class Constant : Variable
    {
        public override void SetValue<T>(T factory, Variant value) {
            _val = value;
        }

        [JsonRequired]
        protected Variant _val;

        public Constant() { }

        public Constant(Variant value) : base(value) { }

        public override string ToString()
        {
            return _val.ToString();
        }

        public override Variant Evaluate<FactoryType>(FactoryType factory) => _val;
    }

    /// <summary>
    /// A variable with a name.
    /// </summary>
    [Serializable]
    public abstract class NamedVariable : Variable
    {
        public override void SetValue<T>(T factory, Variant value) {
            target?.GetType()?.GetField(name)?.SetValue(target, value.value);
        }

        public string name;

        public abstract object target { get; }

        public NamedVariable(string name)
        {
            this.name = name;
        }

        public override Variant Evaluate<T>(T factory)
        {
            return Evaluate(factory, target);
        }

        // Internal handler for fields/properties access
        protected Variant Evaluate<T>(T factory, object arg) where T : VariantFactory
        {
            FieldInfo field = arg.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                return factory.Create(field.GetValue(arg), field.FieldType);
            }
            else
            {
                PropertyInfo prop = arg.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null)
                {
                    return factory.Create(prop.GetValue(arg), prop.PropertyType);
                }
                else
                {
                    Log.Error("No such member {0} of type {1} available ({2} {3})", name, arg.GetType().FullName, GetType().Name, name);
                }
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
    public class AssetVariable : NamedVariable
    {
        /// <summary>
        /// Workaround for serialising Unity object references.
        /// </summary>
        private class Converter : JsonConverter<AssetWrapper>
        {
            public override AssetWrapper ReadJson(JsonReader reader, Type objectType, AssetWrapper existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                int id = -1;
                bool valid = reader.TokenType == JsonToken.Integer && (id = Convert.ToInt32(reader.Value)) >= 0;
                AssetWrapper obj = new AssetWrapper();
                if (valid)
                {
                    Referrable reference = AssetRegistry.GetObject(id
#if UNITY_EDITOR
                        , Application.isPlaying ? null : AssetDatabase.LoadAssetAtPath<AssetRegistry>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:AssetRegistry").FirstOrDefault()))
#endif
                    )
                    as Referrable;
                    if (reference == null)
                    {
                        Log.Warning("Failed to locate reference with id {0}!", id);
                    }
                    obj = new AssetWrapper() {
                        reference = reference
                    };
                }
                return obj;
            }

            public override void WriteJson(JsonWriter writer, AssetWrapper value, JsonSerializer serializer)
            {
                writer.WriteValue(value.reference != null ? value.reference.PersistentId : -1);
            }
        }

        /// <summary>
        /// Shim class for using Unity serialisation.
        /// </summary>
        public struct AssetWrapper
        {
            public Referrable reference;
        }

        /// <summary>
        /// Target Unity object to reflect.
        /// </summary>
        [JsonConverter(typeof(Converter))]
        public AssetWrapper _target = new AssetWrapper();

        public override object target => _target.reference;

        public AssetVariable(string name, Referrable target) : base(name)
        {
            _target.reference = target;
        }

        public override string ToString()
        {
            return (((target as Referrable) != null ? (target as Referrable)?.name : null) ?? target?.GetType()?.Name) + "." + name;
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
    public class DynamicVariable : NamedVariable
    {
        public override object target => null;

        /// <summary>
        /// Parameter index.
        /// </summary>
        public int index = 0;

        public DynamicVariable(int index, string name) : base(name) { this.index = index; }

        public override void SetValue<T>(T factory, Variant value)
        {
            object obj = index < factory.args.Length ? factory.args[index] : null;
            obj?.GetType()?.GetField(name)?.SetValue(obj, value.value);
        }

        /// <summary>
        /// Warning: Provides formatters with argument indexers corresponding to parameters.
        /// </summary>
        public override string ToString()
        {
            return $"{{{index}}}.{name}";
        }

        public override Variant Evaluate<T>(T factory)
        {
            if (index >= factory.args.Length || index < 0)
            {
                throw new IndexOutOfRangeException($"Argument {index} of DynamicVariable \"{name}\" is invalid (out of range).");
            }
            
            object arg = factory.args[index];
            if (arg == null)
            {
                throw new NullReferenceException($"Argument {index} of DynamicVariable \"{name}\" is null.");
            }

            return Evaluate(factory, arg);
        }
    }

    /// <summary>
    /// Access rights for expressions.
    /// </summary>
    [Flags]
    public enum Access
    {
        None = 0,
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write
    }

    /// <summary>
    /// Defines access rights to a field or property by expressions.
    /// By default, private & protected members have no access, while public members have ReadWrite access.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AccessAttribute : Attribute
    {
        public Access access { get; private set; }

        public AccessAttribute(Access access = Access.ReadWrite) {
            this.access = access;
        }
    }

}
