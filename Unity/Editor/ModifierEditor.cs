using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenGET.Expressions;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using System.Reflection;
using System.Linq;

namespace OpenGET.Editor
{

    [CustomEditor(typeof(Modifier))]
    public class ModifierEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            Modifier modifier = (Modifier)target;

            // TODO: DynamicVariable contexts
            VariantFactory factory = new StandardVariantFactory(null);

            bool modified = false;
            ExpressionField(modifier.expression, factory, ref modified, true, "Expression: ");
            modified = false;
            Expression changed = ExpressionField(modifier.expression, factory, ref modified);

            if (modified)
            {
                EditorUtility.SetDirty(modifier);
            }

            if (changed != null)
            {
                modifier.expression = changed;
            }
            else
            {
                modifier.expression = new Constant(new VariantInteger(0));
            }
        }

        public Expression ExpressionField(Expression expression, VariantFactory factory, ref bool changed, bool readOnly = false, string label = null)
        {
            changed |= false;

            if (readOnly)
            {
                EditorGUILayout.LabelField(label + expression?.ToString());
                return expression;
            }

            Constant constant = expression as Constant;
            if (constant != null)
            {
                Variant data = VariantField(constant.value as Variant, factory, out Expression inner, ref changed);
                if (data != null)
                {
                    if (inner != null)
                    {
                        expression = inner;
                    }
                    // Set constant value
                    constant.value = data;
                }
                else if (inner != null)
                {
                    // Expand inner expression
                    expression = inner;
                }
                else
                {
                    // Delete
                    expression = null;
                }
            }

            UnityVariable unityVar = expression as UnityVariable;
            if (unityVar != null)
            {
                unityVar = UnityVariableField(unityVar, out Expression inner, ref changed);
                if (unityVar != null)
                {
                    if (inner != null)
                    {
                        expression = inner;
                    }
                }
                else if (inner != null)
                {
                    // Expand inner expression
                    expression = inner;
                }
                else
                {
                    // Delete
                    expression = null;
                }
            }

            BinaryOperator binOp = expression as BinaryOperator;
            if (binOp != null)
            {
                binOp = BinaryOperatorField(binOp, factory, ref changed);

                if (binOp.a == null)
                {
                    expression = binOp.b;
                }
                else if (binOp.b == null)
                {
                    expression = binOp.a;
                }
                else
                {
                    expression = binOp;
                }
            }

            return expression;
        }

        /// <summary>
        /// Show a constant value field. Returns null if the variant is deleted or expanded into an inner expression.
        /// </summary>
        public Variant VariantField(Variant variant, VariantFactory factory, out Expression inner, ref bool changed)
        {
            changed |= false;
            Variant field = null;
            inner = null;

            EditorGUILayout.BeginHorizontal();

            List<string> options = new List<string> { "[Delete]", "Integer", "Float", "String", "[Variable]/Unity", "[Variable]/Dynamic", "[+]" };
            List<string> extensions = new List<string> { "[-]", "[*]", "[\\]", "[clamp()]" };
            int index = 0;
            if (variant is VariantInteger)
            {
                int oldVal = (int)variant.value;
                int newVal = EditorGUILayout.IntField(oldVal);
                changed |= (newVal != oldVal);
                field = factory.Create(newVal);
                index = 1;
                options.AddRange(extensions);
            }
            else if (variant is VariantFloat)
            {
                float oldVal = (float)variant.value;
                float newVal = EditorGUILayout.FloatField(oldVal);
                changed |= (newVal != oldVal);
                field = factory.Create(newVal);
                index = 2;
                options.AddRange(extensions);
            }
            else if (variant is VariantString)
            {
                string oldVal = (string)variant.value;
                string newVal = EditorGUILayout.TextField(oldVal);
                changed |= (newVal != oldVal);
                field = factory.Create(newVal);
                index = 3;
            }
            else
            {
                // Default to an integer of value 0
                field = factory.Create(0);
            }

            // TODO: Cast old value into new type rather than discarding it entirely
            int oldIndex = index;
            index = EditorGUILayout.Popup(index, options.ToArray());
            if (index != oldIndex)
            {
                changed = true;
                switch (index)
                {
                    // Delete the variant
                    default:
                    case 0:
                        field = null;
                        break;
                    // Set to specific type
                    case 1:
                        field = factory.Create(0);
                        break;
                    case 2:
                        field = factory.Create(0f);
                        break;
                    case 3:
                        field = factory.Create("");
                        break;
                    // Change to UnityVariable
                    case 4:
                        inner = new UnityVariable("", null);
                        field = null;
                        break;
                    case 5:
                        // TODO
                        field = null;
                        break;
                    // Expand into an inner expression
                    case 6:
                        inner = new BinOpAdd(new Constant(field), field is IVariantNumeric ?
                            new Constant(new VariantFloat(0f)) : new Constant(new VariantString(""))
                        );
                        field = null;
                        break;
                    // Expand into extension arithmetic expressions
                    case 7:
                        inner = new BinOpSubtract(new Constant(field), new Constant(new VariantFloat(0f)));
                        field = null;
                        break;
                    case 8:
                        inner = new BinOpMultiply(new Constant(field), new Constant(new VariantFloat(0f)));
                        field = null;
                        break;
                    case 9:
                        inner = new BinOpDivide(new Constant(field), new Constant(new VariantFloat(0f)));
                        field = null;
                        break;
                    // TODO: Expand into clamp expression
                    case 10:
                        field = null;
                        break;
                }
            }

            EditorGUILayout.EndHorizontal();

            return field;
        }

        public BinaryOperator BinaryOperatorField(BinaryOperator op, VariantFactory factory, ref bool changed) {
            Expression a = ExpressionField(op.a, factory, ref changed);

            string[] options = new string[] { "+", "-", "*", "\\" };
            int selected = 0;
            if (op is BinOpSubtract)
            {
                selected = 1;
            }
            else if (op is BinOpMultiply)
            {
                selected = 2;
            }
            else if (op is BinOpDivide)
            {
                selected = 3;
            }

            int old = selected;
            selected = EditorGUILayout.Popup(selected, options);

            if (old != selected)
            {
                changed = true;
                switch (selected)
                {
                    default:
                    case 0:
                        op = new BinOpAdd(a, op.b);
                        break;
                    case 1:
                        op = new BinOpSubtract(a, op.b);
                        break;
                    case 2:
                        op = new BinOpMultiply(a, op.b);
                        break;
                    case 3:
                        op = new BinOpDivide(a, op.b);
                        break;
                }
            }
            else
            {
                op.a = a;
            }

            op.b = ExpressionField(op.b, factory, ref changed);
            return op;
        }

        public UnityVariable UnityVariableField(UnityVariable field, out Expression inner, ref bool changed)
        {
            inner = null;

            EditorGUILayout.BeginHorizontal();

            field._target = EditorGUILayout.ObjectField(field._target, typeof(Object), allowSceneObjects: true);
            int index = 0;

            List<string> options = new List<string> { };
            System.Type targetType = field._target != null ? field._target.GetType() : null;
            FieldInfo[] fields = new FieldInfo[0];
            if (targetType != null) {
                fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(
                    x => x.FieldType.Equals(typeof(string)) || x.FieldType.Equals(typeof(int)) || x.FieldType.Equals(typeof(float))
                ).OrderBy(x => x.Name).ToArray();
                for (int i = 0, counti = fields.Length; i < counti; i++) {
                    options.Add(field._target.name + "/" + fields[i].Name);

                    if (field.name == fields[i].Name)
                    {
                        index = i;
                    }
                }

                options.Add("[Delete]");
            }
            else
            {
                options.Add("Missing");
            }

            List<string> extensions = new List<string> { "[-]", "[*]", "[\\]", "[clamp()]" };

            int oldIndex = index;
            index = EditorGUILayout.Popup(index, options.ToArray());
            if (index != oldIndex)
            {
                changed = true;
                if (index < options.Count - 1)
                {
                    // Set to field
                    field.name = fields[index].Name;
                }
                else if (index == options.Count - 1)
                {
                    // Delete
                    field = null;
                }
                else
                {
                    //switch (index - options.Count)
                    //{
                    //    case 0:
                    //        inner = new BinOpAdd(new Constant(field), field is IVariantNumeric ?
                    //            new Constant(new VariantFloat(0f)) : new Constant(new VariantString(""))
                    //        );
                    //        field = null;
                    //        break;
                    //    // Expand into extension arithmetic expressions
                    //    case 1:
                    //        inner = new BinOpSubtract(new Constant(field), new Constant(new VariantFloat(0f)));
                    //        field = null;
                    //        break;
                    //    case 2:
                    //        inner = new BinOpMultiply(new Constant(field), new Constant(new VariantFloat(0f)));
                    //        field = null;
                    //        break;
                    //    case 3:
                    //        inner = new BinOpDivide(new Constant(field), new Constant(new VariantFloat(0f)));
                    //        field = null;
                    //        break;
                    //    // TODO: Expand into clamp expression
                    //    case 4:
                    //        field = null;
                    //        break;
                    //}
                }
            }

            EditorGUILayout.EndHorizontal();

            return field;
        }

    }

}
