using System.Collections.Generic;
using UnityEngine;
using OpenGET.Expressions;
using UnityEditor;
using System.Reflection;
using System.Linq;

namespace OpenGET.Editor
{

    [CustomPropertyDrawer(typeof(Expression.BaseSerialisable), true)]
    public class ExpressionDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, bool> propertyStates = new Dictionary<string, bool>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            object target = property.serializedObject.targetObject;
            Expression.BaseSerialisable serialised = fieldInfo.GetValue(target) as Expression.BaseSerialisable;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.LabelField(fieldInfo.Name + ": " + serialised._expression?.ToString());
                EditorGUILayout.HelpBox("Expression modification is unavailable in Play Mode.", MessageType.Info);
                // Early out
                return;
            }

            Expression expression = serialised.expression;

            if (expression == null)
            {
                expression = new Constant(new VariantInteger(0));
                serialised.expression = expression;
                fieldInfo.SetValue(target, serialised);
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            label.text = fieldInfo.Name + ": " + expression?.ToString();

            // TODO: DynamicVariable contexts
            VariantFactory factory = serialised.CreateFactory();

            // Begin GUI
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (!propertyStates.TryGetValue(property.propertyPath, out bool expanded))
            {
                expanded = false;
            }
            expanded = EditorGUI.Foldout(foldoutRect, expanded, label, true);
            propertyStates[property.propertyPath] = expanded;

            if (expanded)
            {
                EditorGUI.BeginProperty(position, label, property);

                bool changed = false;
                Expression updated = ExpressionField(expression, factory, ref changed);
                if (changed && updated != null)
                {
                    serialised.expression = updated;
                    fieldInfo.SetValue(target, serialised);
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }

                EditorGUI.EndProperty();
            }
        }

        public Expression ExpressionField(Expression expression, VariantFactory factory, ref bool changed, Expression.Mutability mutability = Expression.Mutability.FullyMutable, string label = null)
        {
            changed |= false;

            // Only show evaluated value if readonly
            if (mutability == Expression.Mutability.Immutable)
            {
                EditorGUILayout.LabelField(label + expression?.ToString());
                return expression;
            }

            // Handle variable
            Variable data = expression as Variable;
            if (data != null) {
                data = VarField(data, factory, mutability, out Expression inner, ref changed);
                expression = inner ?? data;
            }

            // Handle binary operator
            BinaryOperator binOp = expression as BinaryOperator;
            if (binOp != null)
            {
                binOp = BinaryOperatorField(binOp, factory, mutability, ref changed);

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
        /// Takes a Variable. Displays relevant options depending on the type.
        /// Depending on the mutability, the field may be converted between derivative types of Variable (e.g. between Constant and AssetVariable).
        /// </summary>
        public Variable VarField(Variable data, VariantFactory factory, Expression.Mutability mutability, out Expression inner, ref bool changed)
        {
            changed |= false;
            inner = null;

            EditorGUILayout.BeginHorizontal();

            // Initialise options and option count
            List<string> options = new List<string>();
            int countOp = 0;

            // Indices for options
            int op_ConstInt, op_ConstFloat, op_ConstString,
                op_AssetInt, op_AssetFloat, op_AssetString,
                op_DynInt, op_DynFloat, op_DynString,
                op_BinOpAdd, op_BinOpSubtract, op_BinOpMultiply, op_BinOpDivide, op_Clamp,
                op_Delete;

            // Deletion
            if ((mutability & Expression.Mutability.Delete) != 0)
            {
                op_Delete = countOp++;
                options.Add("[Delete]");
            }
            else
            {
                op_Delete = -1;
            }

            // Type modification
            if ((mutability & Expression.Mutability.Type) == 0)
            {
                op_ConstInt = -1;
                op_ConstFloat = -1;
                op_ConstString = -1;
                op_AssetInt = -1;
                op_AssetFloat = -1;
                op_AssetString = -1;
                op_DynInt = -1;
                op_DynFloat = -1;
                op_DynString = -1;
            }
            else
            {
                op_ConstInt = countOp++;
                op_DynInt = countOp++;
                op_AssetInt = countOp++;
                op_ConstFloat = countOp++;
                op_DynFloat = countOp++;
                op_AssetFloat = countOp++;
                op_ConstString = countOp++;
                op_DynString = countOp++;
                op_AssetString = countOp++;
                options.AddRange(new string[] {
                    "Integer/Constant",
                    "Integer/Dynamic",
                    "Integer/Asset",
                    "Float/Constant",
                    "Float/Dynamic",
                    "Float/Asset",
                    "String/Constant",
                    "String/Dynamic",
                    "String/Asset"
                });
            }

            // Default when immutable
            if (mutability == Expression.Mutability.Immutable)
            {
                options.Add("READ-ONLY");
            }
            int index = 0;

            // Prevent editing of immutable values
            bool mutValue = (mutability & Expression.Mutability.Value) != 0;
            if (!mutValue)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            // Handle constant values
            if (data is Constant)
            {
                Variant constant = (data as Constant).value;
                if (constant is VariantInteger)
                {
                    int oldVal = (int)constant.value;
                    int newVal = mutValue ? EditorGUILayout.IntField(oldVal) : oldVal;
                    changed |= (newVal != oldVal);
                    data = new Constant(factory.Create(newVal));
                    if (op_ConstInt < 0)
                    {
                        op_ConstInt = countOp++;
                        options.Add("Integer/Constant");
                    }
                    index = op_ConstInt;
                }
                else if (constant is VariantFloat)
                {
                    float oldVal = (float)constant.value;
                    float newVal = mutValue ? EditorGUILayout.FloatField(oldVal) : oldVal;
                    changed |= (newVal != oldVal);
                    data = new Constant(factory.Create(newVal));
                    if (op_ConstFloat < 0)
                    {
                        op_ConstFloat = countOp++;
                        options.Add("Float/Constant");
                    }
                    index = op_ConstFloat;
                }
                else if (constant is VariantString)
                {
                    string oldVal = (string)constant.value;
                    string newVal = EditorGUILayout.TextField(oldVal);
                    changed |= (newVal != oldVal);
                    data = new Constant(factory.Create(newVal));
                    if (op_ConstString < 0)
                    {
                        op_ConstString = countOp++;
                        options.Add("String/Constant");
                    }
                    index = op_ConstString;
                }
                else
                {
                    // TODO: Support custom Variant factories
                }
            }
            else if (data is AssetVariable)
            {
                if (op_AssetInt < 0)
                {
                    op_AssetInt = countOp++;
                    op_AssetFloat = countOp++;
                    op_AssetString = countOp++;
                    options.AddRange(new string[] { "Integer/Asset", "Float/Asset", "String/Asset" });
                }

                AssetVariable uvar = data as AssetVariable;
                Referrable old = uvar._target.reference;
                uvar._target.reference = EditorGUILayout.ObjectField(uvar._target.reference, typeof(Referrable), allowSceneObjects: false) as Referrable;
                if (old != uvar._target.reference)
                {
                    changed = true;
                }
                int fieldIndex = -1;

                List<string> fieldNames = new List<string> { };
                System.Type targetType = uvar._target.reference != null ? uvar._target.reference.GetType() : null;
                FieldInfo[] fields = new FieldInfo[0];
                if (targetType != null)
                {
                    fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(
                        x => x.FieldType.Equals(typeof(int)) || x.FieldType.Equals(typeof(float)) || x.FieldType.Equals(typeof(string))
                    ).OrderBy(x => x.Name).ToArray();
                    for (int i = 0, counti = fields.Length; i < counti; i++)
                    {
                        fieldNames.Add(fields[i].Name + $" ({fields[i].FieldType.Name})");

                        if (uvar.name == fields[i].Name)
                        {
                            fieldIndex = i;
                        }

                        if (index == 0 || uvar.name == fields[i].Name)
                        {
                            fieldIndex = i;
                            uvar.name = string.IsNullOrEmpty(uvar.name) ? fields[i].Name : uvar.name;
                            index = fields[i].FieldType.Equals(typeof(int)) ? op_AssetInt
                                : (fields[i].FieldType.Equals(typeof(float)) ? op_AssetFloat : op_AssetString);
                        }
                    }
                }
                else
                {
                    index = op_AssetFloat;
                }

                if (fieldIndex < 0)
                {
                    fieldIndex = 0;
                    uvar.name = fields.Length > 0 ? fieldNames[0] : "";
                }

                if (fields.Length == 0)
                {
                    if (changed)
                    {
                        Log.Debug("Changed = {0}, former = {1}", changed, old?.name);
                    }
                    fieldNames.Add(targetType != null ? "(none)" : "(missing)");
                }

                int oldField = fieldIndex;
                fieldIndex = EditorGUILayout.Popup(fieldIndex, fieldNames.ToArray());
                if (fieldIndex != oldField)
                {
                    changed = true;
                    if (fieldIndex < fields.Length)
                    {
                        // Set to field
                        uvar.name = fields[fieldIndex].Name;
                    }
                    else
                    {
                        // Clear
                        uvar = new AssetVariable("", null);
                    }
                }
            }
            else if (data is DynamicVariable)
            {
                DynamicVariable dynVar = data as DynamicVariable;
                if (op_DynInt < 0)
                {
                    op_DynInt = countOp++;
                    op_DynFloat = countOp++;
                    op_DynString = countOp++;
                    options.AddRange(new string[] { "Integer/Dynamic", "Float/Dynamic", "String/Dynamic" });
                }

                List<string> fieldNames = new List<string> { };
                Expression.IContext context = factory;
                FieldInfo[] fields = new FieldInfo[0];
                //if (context != null)
                //{
                //    fields = context.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(
                //        x => x.FieldType.Equals(typeof(int)) || x.FieldType.Equals(typeof(float)) || x.FieldType.Equals(typeof(string))
                //    ).OrderBy(x => x.Name).ToArray();
                //    for (int i = 0, counti = fields.Length; i < counti; i++)
                //    {
                //        fieldNames.Add(fields[i].Name + $" ({fields[i].FieldType.Name})");

                //        if (dynVar.name == fields[i].Name)
                //        {
                //            fieldIndex = i;
                //        }

                //        if (index == 0 || dynVar.name == fields[i].Name)
                //        {
                //            fieldIndex = i;
                //            dynVar.name = string.IsNullOrEmpty(dynVar.name) ? fields[i].Name : dynVar.name;
                //            index = fields[i].FieldType.Equals(typeof(int)) ? op_AssetInt
                //                : (fields[i].FieldType.Equals(typeof(float)) ? op_AssetFloat : op_AssetString);
                //        }
                //    }
                //}
                //DynamicVariable dynVar = data as DynamicVariable;
                //int oldIndex = dynVar.index;
                //string oldName = dynVar.name;
                //EditorGUILayout.Popup(oldArgIndex, )
                //dynVar._target.reference = EditorGUILayout.ObjectField(dynVar._target.reference, typeof(Referrable), allowSceneObjects: false) as Referrable;
                //if (oldArgIndex != dynVar._target.reference)
                //{
                //    changed = true;
                //}
                //int fieldIndex = -1;

                
                //else
                //{
                //    index = op_AssetFloat;
                //}

                //if (fieldIndex < 0)
                //{
                //    fieldIndex = 0;
                //    dynVar.name = fields.Length > 0 ? fieldNames[0] : "";
                //}

                //if (fields.Length == 0)
                //{
                //    if (changed)
                //    {
                //        Log.Debug("Changed = {0}, former = {1}", changed, oldArgIndex?.name);
                //    }
                //    fieldNames.Add(context != null ? "(none)" : "(missing)");
                //}

                //int oldField = fieldIndex;
                //fieldIndex = EditorGUILayout.Popup(fieldIndex, fieldNames.ToArray());
                //if (fieldIndex != oldField)
                //{
                //    changed = true;
                //    if (fieldIndex < fields.Length)
                //    {
                //        // Set to field
                //        dynVar.name = fields[fieldIndex].Name;
                //    }
                //    else
                //    {
                //        // Clear
                //        dynVar = new AssetVariable("", null);
                //    }
                //}
            }

            if (!mutValue)
            {
                EditorGUI.EndDisabledGroup();
            }

            // Operator modification
            if ((mutability & Expression.Mutability.Operator) == 0)
            {
                op_BinOpAdd = -1;
                op_BinOpSubtract = -1;
                op_BinOpMultiply = -1;
                op_BinOpDivide = -1;
                op_Clamp = -1;
            }
            else
            {
                op_BinOpAdd = countOp++;
                op_BinOpSubtract = countOp++;
                op_BinOpMultiply = countOp++;
                op_BinOpDivide = countOp++;
                op_Clamp = countOp++;
                options.AddRange(new string[] {
                    "Operator/[+] Add",
                    "Operator/[-] Subtract",
                    "Operator/[*] Multiply",
                    "Operator/[\\] Divide",
                    "Operator/[Clamp]",
                });
            }

            bool mutType = (mutability & Expression.Mutability.Type) != 0;
            if (!mutType)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            // TODO: Cast old value into new type rather than discarding it entirely
            int oldIndex = index;
            index = EditorGUILayout.Popup(index, options.ToArray());
            if (index != oldIndex)
            {
                changed = true;
                if (index == op_ConstInt)
                {
                    data = new Constant(factory.Create(
                        Mathf.FloorToInt(oldIndex == 2 ? (float)data.value?.value : (oldIndex == 3 && float.TryParse(data.value?.value as string, out float oldFloat) ? oldFloat : 0f))
                    ));
                }
                else if (index == op_ConstFloat)
                {
                    data = new Constant(factory.Create(
                        oldIndex == 1 ? (int)data.value?.value : (oldIndex == 3 && float.TryParse(data.value?.value as string, out float oldValue) ? oldValue : 0f)
                    ));
                }
                else if (index == op_ConstString)
                {
                    data = new Constant(factory.Create(data.value?.value?.ToString()));
                }
                else if (index == op_AssetInt)
                {
                    data = new AssetVariable("", null);
                }
                else if (index == op_AssetFloat)
                {
                    data = new AssetVariable("", null);
                }
                else if (index == op_AssetString)
                {
                    data = new AssetVariable("", null);
                }
                else if (index == op_DynInt)
                {
                    data = new DynamicVariable(-1, "");
                }
                else if (index == op_DynFloat)
                {
                    data = new DynamicVariable(-1, "");
                }
                else if (index == op_DynString)
                {
                    data = new DynamicVariable(-1, "");
                }
                else if (index == op_BinOpAdd)
                {
                    inner = new BinOpAdd(data, data.value is IVariantNumeric ?
                        new Constant(new VariantFloat(0f)) : new Constant(new VariantString(""))
                    );
                }
                else if (index == op_BinOpSubtract)
                {
                    inner = new BinOpSubtract(data, new Constant(new VariantFloat(0f)));
                }
                else if (index == op_BinOpMultiply)
                {
                    inner = new BinOpMultiply(data, new Constant(new VariantFloat(0f)));
                }
                else if (index == op_BinOpDivide)
                {
                    inner = new BinOpDivide(data, new Constant(new VariantFloat(0f)));
                }
                else
                {
                    // Delete
                    data = null;
                }
            }

            if (!mutType)
            {
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndHorizontal();

            return data;
        }

        public BinaryOperator BinaryOperatorField(BinaryOperator op, VariantFactory factory, Expression.Mutability mutability, ref bool changed) {
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

    }

}
