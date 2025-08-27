using OpenGET.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OpenGET.Editor
{

    [CustomPropertyDrawer(typeof(Expression.BaseSerialisable), true)]
    public class ExpressionDrawer : PropertyDrawer
    {
        private class PropertyState
        {
            public bool expanded = false;
            public Rect rect;
        }

        private static readonly Dictionary<string, PropertyState> propertyStates = new Dictionary<string, PropertyState>();

        private readonly float LineHeight = EditorGUIUtility.singleLineHeight;

        private int propIndex = 0;

        private string key = "";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            key = property.propertyPath;
            state.rect = position;
            state.rect.height = 0;
            state.rect.width = position.width;

            object target = property.serializedObject.targetObject;
            Expression.BaseSerialisable serialised = fieldInfo.GetValue(target) as Expression.BaseSerialisable;
            if (serialised == null)
            {
                // FieldInfo is pointing at an array of some sort
                if (fieldInfo.FieldType.IsArray)
                {
                    Expression.BaseSerialisable[] data = fieldInfo.GetValue(target) as Expression.BaseSerialisable[];

                    bool changed = false;
                    if (property.propertyPath.Contains("[0]"))
                    {
                        propIndex = 0;
                    }
                    int i = Mathf.Clamp(propIndex, 0, data.Length);
                    ExpressionGUI(data[i], property, label, ref changed, i, true);

                    propIndex++;
                    if (changed)
                    {
                        fieldInfo.SetValue(target, data);
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }
                }
                else
                {
                    Log.Error("Serialised type is null or not supported! Arrays must NOT be lists.");
                }
            }
            else
            {
                bool changed = false;
                ExpressionGUI(serialised, property, label, ref changed);
                if (changed)
                {
                    fieldInfo.SetValue(target, serialised);
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }
        }

        private Expression.BaseSerialisable ExpressionGUI(Expression.BaseSerialisable serialised, SerializedProperty property, GUIContent label, ref bool changed, int index = 0, bool isElement = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUI.LabelField(state.rect, fieldInfo.Name + ": " + serialised._expression?.ToString());
                EditorGUI.HelpBox(state.rect, "Expression modification is unavailable in Play Mode.", MessageType.Info);
                // Early out
                return null;
            }

            Expression expression = serialised.expression;
            if (expression == null)
            {
                changed = true;
                expression = new Constant(new VariantInteger(0));
                serialised.expression = expression;
            }

            VariantFactory factory = serialised.CreateFactory();

            // Format expression with dynamic variable names
            string text = (isElement ? ($"Element {index}") : fieldInfo.Name) +
                string.Format(": " + expression?.ToString(), factory.parameters.Select(x => (object)x.name).ToArray());

            // Begin GUI
            state.expanded = EditorGUI.Foldout(GetSection(), state.expanded, text, true);
            StepLines();

            if (state.expanded)
            {
                Expression updated = ExpressionField(expression, factory, ref changed);
                if (changed && updated != null)
                {
                    serialised.expression = updated;
                }
            }

            return serialised;
        }

        /// <summary>
        /// Current state getter/setter.
        /// </summary>
        private PropertyState state {
            get => propertyStates.TryGetValue(key, out PropertyState state) && state != null ? state : propertyStates[key] = new PropertyState();
            set => propertyStates[key] = value;
        }

        /// <summary>
        /// Get the rect for a section consisting of a specified number of lines.
        /// </summary>
        private Rect GetSection(int parts = 1, int index = 0, int lines = 1) => 
            new Rect(state.rect.x + (index * (state.rect.width / parts)), state.rect.y, state.rect.width / parts, lines * LineHeight);

        private void StepLines(int lines = 1)
        {
            float change = (lines * LineHeight) + (lines * EditorGUIUtility.standardVerticalSpacing);
            state.rect.y += change;
            state.rect.height += change;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            key = property.propertyPath;
            if (!state.expanded)
            {
                return LineHeight;
            }

            return state.rect.height;
        }

        public Expression ExpressionField(Expression expression, VariantFactory factory, ref bool changed, Expression.Mutability mutability = Expression.Mutability.FullyMutable, string label = null, int depth = 0)
        {
            changed |= false;

            // Only show evaluated value if readonly
            if (mutability == Expression.Mutability.Immutable)
            {
                EditorGUI.LabelField(state.rect, label + expression?.ToString());
                StepLines();
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
                binOp = BinaryOperatorField(binOp, factory, mutability, ref changed, depth + 1);

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
        /// Gets all fields and props for access by expressions on a given type.
        /// Always returns pub
        /// </summary>
        private IEnumerable<MemberInfo> GetFieldsAndProps(Type targetType)
        {
            IEnumerable<MemberInfo> fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(
                        x => (x.FieldType.Equals(typeof(int)) || x.FieldType.Equals(typeof(float)) || x.FieldType.Equals(typeof(string))) &&
                            (
                                (x.IsPublic && x.GetCustomAttribute<AccessAttribute>() == null) || 
                                ((x.GetCustomAttribute<AccessAttribute>()?.access ?? Access.None) & Access.Read) != 0
                            )
                    );
            IEnumerable<MemberInfo> props = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(
                x => (x.PropertyType.Equals(typeof(int)) || x.PropertyType.Equals(typeof(float)) || x.PropertyType.Equals(typeof(string))) && (
                    x.GetGetMethod(nonPublic: true) != null && 
                    (
                        (x.GetGetMethod(nonPublic: true).IsPublic && x.GetCustomAttribute<AccessAttribute>() == null) ||
                        ((x.GetCustomAttribute<AccessAttribute>()?.access ?? Access.None) & Access.Read) != 0
                    )
                )
            );
            return fields.Concat(props).OrderBy(x => x.Name);
        }

        /// <summary>
        /// Takes a Variable. Displays relevant options depending on the type.
        /// Depending on the mutability, the field may be converted between derivative types of Variable (e.g. between Constant and AssetVariable).
        /// </summary>
        public Variable VarField(Variable data, VariantFactory factory, Expression.Mutability mutability, out Expression inner, ref bool changed)
        {
            changed |= false;
            inner = null;

            int parts = 2;

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
                Variant constant = (data as Constant).Evaluate(factory);
                if (constant is VariantInteger)
                {
                    int oldVal = (int)constant.value;
                    int newVal = mutValue ? EditorGUI.IntField(GetSection(parts), oldVal) : oldVal;
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
                    float newVal = mutValue ? EditorGUI.FloatField(GetSection(parts), oldVal) : oldVal;
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
                    string newVal = EditorGUI.TextField(GetSection(parts), oldVal);
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

                parts++;
                AssetVariable uvar = data as AssetVariable;
                Referrable old = uvar._target.reference;
                uvar._target.reference = EditorGUI.ObjectField(GetSection(parts), uvar._target.reference, typeof(Referrable), allowSceneObjects: false) as Referrable;
                if (old != uvar._target.reference)
                {
                    changed = true;
                }
                int memberIndex = -1;

                List<string> memberNames = new List<string> { };
                System.Type targetType = uvar._target.reference != null ? uvar._target.reference.GetType() : null;
                MemberInfo[] fieldsAndProps = new MemberInfo[0];
                if (targetType != null)
                {
                    fieldsAndProps = GetFieldsAndProps(targetType).ToArray();

                    for (int i = 0, counti = fieldsAndProps.Length; i < counti; i++)
                    {
                        MemberInfo member = fieldsAndProps[i];
                        Type memberType = member is FieldInfo ? (member as FieldInfo).FieldType : (member as PropertyInfo).PropertyType;

                        memberNames.Add(member.Name + $" ({memberType.Name})");

                        if (index == 0 || uvar.name == member.Name)
                        {
                            memberIndex = i;
                            uvar.name = string.IsNullOrEmpty(uvar.name) ? member.Name : uvar.name;
                            index = memberType.Equals(typeof(int)) ? op_AssetInt
                                : (memberType.Equals(typeof(float)) ? op_AssetFloat : op_AssetString);
                        }
                    }
                }
                else
                {
                    index = op_AssetFloat;
                }

                if (memberIndex < 0)
                {
                    memberIndex = 0;
                    uvar.name = fieldsAndProps.Length > 0 ? memberNames[0] : "";
                }

                if (fieldsAndProps.Length == 0)
                {
                    memberNames.Add(targetType != null ? "(none)" : "(missing)");
                }

                int oldMember = memberIndex;
                memberIndex = EditorGUI.Popup(GetSection(parts, 1), memberIndex, memberNames.ToArray());
                if (memberIndex != oldMember)
                {
                    changed = true;
                    if (memberIndex < fieldsAndProps.Length)
                    {
                        // Set to field
                        uvar.name = fieldsAndProps[memberIndex].Name;
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

                List<string> dynOptions = new List<string> { };
                // Fields and properties indexed by parameters
                List<MemberInfo[]> fieldsAndProps = new List<MemberInfo[]>(factory.parameters.Length);
                int[] numMembers = new int[factory.parameters.Length];
                int paramIndex = -1;
                int memberIndex = -1;
                bool foundMatch = false;
                int oldArgIndex = -1;
                int memberCount = 0;
                for (int i = 0, counti = factory.parameters.Length; i < counti; i++)
                {
                    IContext.Parameter parameter = factory.parameters[i];

                    // Find all fields & properties matching type and access requirements
                    // Bonus: Support custom types
                    fieldsAndProps.Add(GetFieldsAndProps(parameter.type).ToArray());

                    // Now step through members and locate selected, if any
                    numMembers[i] = fieldsAndProps[i].Length;
                    for (int j = 0, countj = fieldsAndProps[i].Length; j < countj; j++)
                    {
                        MemberInfo member = fieldsAndProps[i][j];
                        Type type = member is FieldInfo ? (member as FieldInfo).FieldType : (member as PropertyInfo).PropertyType;

                        dynOptions.Add($"{i}: {parameter.name}/{member.Name} ({type.Name})");

                        foundMatch = dynVar.name == member.Name && dynVar.index == i;
                        if (index == 0 || foundMatch)
                        {
                            // Found selected member by name, or just the first member as a fallback
                            paramIndex = i;
                            memberIndex = j;
                            oldArgIndex = memberCount + memberIndex;
                            dynVar.name = string.IsNullOrEmpty(dynVar.name) ? member.Name : dynVar.name;
                            index = type.Equals(typeof(int)) ? op_DynInt
                                : (type.Equals(typeof(float)) ? op_DynFloat : op_DynString);
                        }
                    }
                    memberCount += numMembers[i];
                }

                if (fieldsAndProps.Count == 0)
                {
                    dynOptions.Add("(none)");
                }

                int argIndex = EditorGUI.Popup(GetSection(parts), oldArgIndex, dynOptions.ToArray());
                if (oldArgIndex != argIndex)
                {
                    changed = true;

                    paramIndex = -1;
                    int totalOptions = 0;
                    // Step through parameters to find the chosen field
                    for (int counti = fieldsAndProps.Count; paramIndex < counti;)
                    {
                        paramIndex++;
                        int oldTotal = totalOptions;
                        totalOptions += fieldsAndProps[paramIndex].Length;
                        if (argIndex >= oldTotal && argIndex < totalOptions)
                        {
                            // Found field corresponding to the selected option
                            memberIndex = argIndex - oldTotal;
                            break;
                        }
                    }

                    if (paramIndex >= 0 && memberIndex >= 0 && paramIndex < fieldsAndProps.Count && memberIndex < fieldsAndProps[paramIndex].Length)
                    {
                        // Set to field
                        dynVar.index = paramIndex;
                        dynVar.name = fieldsAndProps[paramIndex][memberIndex].Name;
                    }
                    else
                    {
                        // Clear
                        dynVar = new DynamicVariable(0, "");
                    }
                }

                if (memberIndex < 0 || paramIndex < 0)
                {
                    memberIndex = 0;
                    paramIndex = 0;
                    dynVar.name = fieldsAndProps.Count > 0 && fieldsAndProps[0].Length > 0 ? fieldsAndProps[0][0].Name : "";
                }

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
            index = EditorGUI.Popup(GetSection(parts, parts - 1), index, options.ToArray());
            if (index != oldIndex)
            {
                changed = true;
                if (index == op_ConstInt)
                {
                    data = new Constant(factory.Create(
                        Mathf.FloorToInt(oldIndex == op_ConstFloat ? (float)data.Evaluate(factory)?.value : (oldIndex == op_ConstString && float.TryParse(data.Evaluate(factory)?.value as string, out float oldFloat) ? oldFloat : 0f))
                    ));
                }
                else if (index == op_ConstFloat)
                {
                    data = new Constant(factory.Create(
                        oldIndex == op_ConstInt ? (int)data.Evaluate(factory)?.value : (op_ConstString == 3 && float.TryParse(data.Evaluate(factory)?.value as string, out float oldValue) ? oldValue : 0f)
                    ));
                }
                else if (index == op_ConstString)
                {
                    data = new Constant(factory.Create(data is Constant ? data.Evaluate(factory)?.value?.ToString() : ""));
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
                    data = new DynamicVariable(0, "");
                }
                else if (index == op_DynFloat)
                {
                    data = new DynamicVariable(0, "");
                }
                else if (index == op_DynString)
                {
                    data = new DynamicVariable(0, "");
                }
                else if (index == op_BinOpAdd)
                {
                    inner = new BinOpAdd(data, new Constant(new VariantFloat(0f)));
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

            StepLines();

            return data;
        }

        public BinaryOperator BinaryOperatorField(BinaryOperator op, VariantFactory factory, Expression.Mutability mutability, ref bool changed, int depth = 0) {
            Expression a = ExpressionField(op.a, factory, ref changed, mutability, depth: depth + 1);

            // Limit assignment to a root variable
            bool assignable = a is DynamicVariable && depth <= 1;
            List<string> options = new List<string> { "+", "-", "*", "\\" };
            if (assignable)
            {
                options.Add("=");
            }

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
            else if (op is BinOpAssign && assignable)
            {
                selected = 4;
            }

            int old = selected;
            selected = EditorGUI.Popup(GetSection(), selected, options.ToArray());

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
                    case 4:
                        if (assignable)
                        {
                            op = new BinOpAssign(a as DynamicVariable ?? new DynamicVariable(0, ""), op.b);
                        }
                        break;
                }
            }
            else
            {
                op.a = a;
            }

            StepLines();

            op.b = ExpressionField(op.b, factory, ref changed, mutability, depth: depth + 1);
            return op;
        }

    }

}
