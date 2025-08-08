using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace OpenGET.Expressions
{

    /// <summary>
    /// Wrapper that determines how to operate on objects of different types.
    /// </summary>
    public abstract class Variant : IVariant
    {
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

    public abstract class VariantFactory
    {
        public abstract Variant Create(object value, Type type);

        public Variant Create<T>(object value)
        {
            return Create(value, typeof(T));
        }
    }

    public class StandardVariantFactory : VariantFactory
    {
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
    public abstract class Expression
    {
        public abstract override string ToString();

        public abstract Variant Evaluate<FactoryType>(FactoryType factory) where FactoryType : VariantFactory;

        /// <summary>
        /// Default to standard factory.
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
    public class Add : BinaryOperator
    {
        public Add(Expression a, Expression b) : base(a, b) { }

        public override string opString => "+";

        public override Variant Evaluate<T>(T factory) => a.Evaluate(factory) + b.Evaluate(factory);
    }

    [Serializable]
    public class Subtract : BinaryOperator
    {
        public Subtract(Expression a, Expression b) : base(a, b) { }

        public override string opString => "-";

        public override Variant Evaluate<T>(T factory) => a.Evaluate(factory) - b.Evaluate(factory);
    }

    [Serializable]
    public class Multiply : BinaryOperator
    {
        public Multiply(Expression a, Expression b) : base(a, b) { }

        public override string opString => "*";

        public override Variant Evaluate<T>(T factory) => a.Evaluate(factory) * b.Evaluate(factory);
    }

    [Serializable]
    public class Divide : BinaryOperator
    {
        public Divide(Expression a, Expression b) : base(a, b) { }

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
            return value.ToString();
        }

        public override Variant Evaluate<T>(T factory)
        {
            return value;
        }
    }

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
    /// Named variable value. This maps to a reflection field on a Unity object.
    /// </summary>
    [Serializable]
    public class UnityVariable : Variable
    {
        /// <summary>
        /// Target Unity object to reflect.
        /// </summary>
        public UnityEngine.Object _target;

        public override object target => _target;

        public UnityVariable(string name, UnityEngine.Object target) : base(name)
        {
            _target = target;
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
    /// Named variable value. This maps to a reflection field on any object.
    /// Please note however that this is unsuitable for use with Unity serialisation.
    /// In cases where you require Unity Object serialisation, instead use UnityVariable or DynamicVariable.
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
    /// Represents a variable that can change depending on the context at the time the expression is evaluated.
    /// </summary>
    [Serializable]
    public class DynamicVariable : Variable
    {
        public override object target => null;

        public DynamicVariable(string name) : base(name) {}

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

}
