using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Expressions;
using Microsoft.SmallBasic.Statements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using System.Globalization;
using EV3BasicCompiler.Compilers;

namespace EV3BasicCompiler
{
    public class EV3Variables
    {
        private const int MAX_TEMP_VARIABLE_INDEX = 255;

        private Parser parser;
        private readonly Dictionary<string, EV3Variable> variables;
        private readonly Dictionary<EV3Type, List<int>> tempVariablesCurrent;
        private readonly Dictionary<EV3Type, int> tempVariablesMax;

        public bool DoDivisionCheck { get; set; }
        public bool DoBoundsCheck { get; set; }
        public List<Error> Errors { get; private set; }
        public List<Error> CompileErrors { get; private set; }

        public EV3Variables(Parser parser)
        {
            this.parser = parser;

            variables = new Dictionary<string, EV3Variable>();
            Errors = new List<Error>();
            CompileErrors = new List<Error>();

            tempVariablesCurrent = new Dictionary<EV3Type, List<int>>();
            tempVariablesMax = new Dictionary<EV3Type, int>();
            Clear();
        }

        public void Clear()
        {
            variables.Clear();
            Errors.Clear();
            CompileErrors.Clear();

            DoDivisionCheck = true;
            DoBoundsCheck = true;

            tempVariablesCurrent.Clear();
            tempVariablesMax.Clear();
            foreach (EV3Type type in Enum.GetValues(typeof(EV3Type)))
            {
                tempVariablesCurrent[type] = new List<int>();
                tempVariablesMax[type] = -1;
            }
        }

        public void Process(Dictionary<string, EV3Type> subResultTypes)
        {
            foreach (var variable in parser.SymbolTable.InitializedVariables)
            {
                variables.Add(variable.Key, new EV3Variable(variable.Key, variable.Value));
            }
            foreach (EV3Variable variable in variables.Values)
            {
                if (variable.Type == EV3Type.Unknown)
                    ProcessVariable(variable, new EV3Variable[0], subResultTypes);
                if (variable.Type == EV3Type.Unknown)
                    AddError($"Variable type couldn't be determined for '{variable.Name}'", variable.TokenInfo);
            }
            foreach (EV3Variable variable in variables.Values.Where(v => v.Type.IsArray()))
            {
                UpdateVariableIndex(variable);
            }

        }

        private void ProcessVariable(EV3Variable variable, EV3Variable[] trace, Dictionary<string, EV3Type> subResultTypes)
        {
            if (trace.Contains(variable)) return;
            if (variable.Type != EV3Type.Unknown) return;

            EV3Variable[] newTrace = trace.Concat(new EV3Variable[] { variable }).ToArray();

            foreach (AssignmentStatement statement in parser.GetStatements<AssignmentStatement>().Where(s => s.LeftValue.IsVariable(variable)))
            {
                ProcessAssignmentStatement(variable, statement, newTrace, subResultTypes);
            }
            foreach (ForStatement statement in parser.GetStatements<ForStatement>().Where(s => s.IsVariable(variable)))
            {
                ProcessForStatement(variable, statement, newTrace);
            }
        }

        void ProcessAssignmentStatement(EV3Variable variable, AssignmentStatement statement, EV3Variable[] trace, Dictionary<string, EV3Type> subResultTypes)
        {
            EV3VariableTypeWithIndexAndValue type = GetExpressionType(statement.RightValue, trace, subResultTypes);
            if (statement.LeftValue is ArrayExpression)
            {
                variable.UpdateMaxIndex(((ArrayExpression)statement.LeftValue).Compiler<ArrayExpressionCompiler>().Index);
                type.Type = type.Type.ConvertToArray();
            }
            else if (type.Type.IsArray())
            {
                if (type.Index == -1)
                {
                    EV3Variable referencedVariable = FindVariable(statement.RightValue);
                    if (referencedVariable != null)
                        variable.UpdateMaxIndex(referencedVariable.MaxIndex);
                }
                else
                    variable.UpdateMaxIndex(type.Index);
            }

            SetVariableType(variable, statement, type.Type);
        }

        private EV3VariableTypeWithIndexAndValue GetExpressionType(SBExpression expression, EV3Variable[] trace, Dictionary<string, EV3Type> subResultTypes)
        {
            if (expression.IsLiteral())
            {
                //return EV3VariableTypeWithIndexAndValue.CreateFromLiteral(expression);
            }
            else if (expression is NegativeExpression)
            {
                //return GetExpressionType(((NegativeExpression)expression).Expression, trace, subResultTypes);
            }
            else if (expression is IdentifierExpression)
            {
                //EV3Variable reference = FindVariable(((IdentifierExpression)expression).VariableName());
                //ProcessVariable(reference, trace, subResultTypes);
                //return new EV3VariableTypeWithIndexAndValue(reference.Type, reference.MaxIndex);
            }
            else if (expression is BinaryExpression)
            {
                //BinaryExpression binaryExpression = (BinaryExpression)expression;
                //EV3VariableTypeWithIndexAndValue leftType = GetExpressionType(binaryExpression.LeftHandSide, trace, subResultTypes);
                //EV3VariableTypeWithIndexAndValue rightType = GetExpressionType(binaryExpression.RightHandSide, trace, subResultTypes);
                //if (leftType.Type.IsArray() || rightType.Type.IsArray())
                //{
                //    AddError("Operations on arrays are not permited", expression.StartToken);
                //    return new EV3VariableTypeWithIndexAndValue(EV3Type.Unknown);
                //}
                //else if ((int)leftType.Type > (int)rightType.Type)
                //    return leftType;
                //else
                //    return rightType;
            }
            else if (expression is ArrayExpression)
            {
                //EV3Variable reference = FindVariable(((ArrayExpression)expression).VariableName());
                //ProcessVariable(reference, trace, subResultTypes);
                //return new EV3VariableTypeWithIndexAndValue(reference.Type.BaseType(), reference.MaxIndex);
            }
            else if (expression is MethodCallExpression)
            {
                //MethodCallExpression callExpression = (MethodCallExpression)expression;
                //string methodName = callExpression.FullName().ToUpper();
                //if (subResultTypes.ContainsKey(methodName))
                //    return new EV3VariableTypeWithIndexAndValue(subResultTypes[methodName]);
                //else
                //{
                //    AddError($"Unknown method call to {callExpression.FullName()}", expression.StartToken);
                //    return new EV3VariableTypeWithIndexAndValue(EV3Type.Unknown);
                //}
            }
            else if (expression is PropertyExpression)
            {
                //PropertyExpression propertyExpression = (PropertyExpression)expression;
                //string propertyName = propertyExpression.FullName().ToUpper();
                //if (subResultTypes.ContainsKey(propertyName))
                //    return new EV3VariableTypeWithIndexAndValue(subResultTypes[propertyName]);
                //else
                //{
                //    AddError($"Unknown property {propertyExpression.FullName()}", expression.StartToken);
                //    return new EV3VariableTypeWithIndexAndValue(EV3Type.Unknown);
                //}
            }
            return EV3VariableTypeWithIndexAndValue.CreateFromCompiler(expression.Compiler());
            //return new EV3VariableTypeWithIndexAndValue(EV3Type.Unknown);
        }

        private void ProcessForStatement(EV3Variable variable, ForStatement statement, EV3Variable[] eV3Variable)
        {
            SetVariableType(variable, statement, EV3Type.Float);
        }

        private void SetVariableType(EV3Variable variable, Statement statement, EV3Type type)
        {
            if (variable.Type == EV3Type.Unknown)
            {
                variable.Type = type;
            }
            else if (!variable.Type.IsArray() && type.IsArray())
                AddError($"Cannot use index on non-array variable '{variable.Name}'", statement.StartToken);
            else if (variable.Type.IsArray() && !type.IsArray())
                AddError($"Cannot assign value to array variable '{variable.Name}' without index", statement.StartToken);
            else if (variable.Type != type)
                AddError($"Cannot assign different types to '{variable.Name}'", statement.StartToken);
        }

        private void UpdateVariableIndex(EV3Variable variable)
        {
            foreach (ArrayExpression arrayExpression in parser.GetExpressions<ArrayExpression>().Where(ae => ae.IsVariable(variable)))
            {
                variable.UpdateMaxIndex(arrayExpression.Index());
            }
        }

        public void CompileCodeForVariableDeclarations(TextWriter writer)
        {
            foreach (EV3Variable variable in variables.Values)
            {
                variable.CompileCodeForDeclaration(writer);
            }
        }

        public void CompileCodeForVariableInitializations(TextWriter writer)
        {
            foreach (EV3Variable variable in variables.Values)
            {
                variable.CompileCodeForInitialization(writer);
            }
        }

        public void CompileCodeForAssignmentStatement(TextWriter writer, AssignmentStatement assignmentStatement)
        {
            int index = -1;
            if (assignmentStatement.LeftValue is ArrayExpression)
            {
                index = ((ArrayExpression)assignmentStatement.LeftValue).Index();
            }
            EV3Variable variable = FindVariable(assignmentStatement.LeftValue);
            if (variable != null)
            {
                foreach (EV3VariableTypeWithIndexAndValue value in CompileCodeForExpression(writer, variable, assignmentStatement.RightValue.SimplifyExpression()))
                {
                    if (value.Type.BaseType() != variable.Type.BaseType())
                        AddCompileError($"Value incompatible with variable type {variable.Type} in {assignmentStatement}", assignmentStatement.StartToken);

                    variable.CompileCodeForAssignment(writer, value.Index == -1 ? index : value.Index, value.Value, DoBoundsCheck);
                }
            }
        }

        private List<EV3VariableTypeWithIndexAndValue> CompileCodeForExpression(TextWriter writer, IEV3Variable variable, SBExpression expression)
        {
            List<EV3VariableTypeWithIndexAndValue> values = new List<EV3VariableTypeWithIndexAndValue>();
            if (expression.IsLiteral())
            {
                return CompileCodeForLiteralExpression(writer, variable, expression);
            }
            else if (expression is NegativeExpression)
            {
                return CompileCodeForNegativeExpression(writer, variable, (NegativeExpression)expression);
            }
            else if (expression is IdentifierExpression)
            {
                values = CompileCodeForIdentifierExpression(writer, variable, (IdentifierExpression)expression);
            }
            else if (expression is BinaryExpression)
            {
                return CompileCodeForBinaryExpression(writer, variable, (BinaryExpression)expression);
            }
            else if (expression is ArrayExpression)
            {
                return CompileCodeForArrayExpression(writer, variable, (ArrayExpression)expression);
            }
            else if (expression is MethodCallExpression)
            {
                MethodCallExpression callExpression = (MethodCallExpression)expression;
                //string methodName = callExpression.FullName().ToUpper();
                //if (subResultTypes.ContainsKey(methodName))
                //    return new EV3VariableTypeWithIndex(subResultTypes[methodName], -1);
                //else
                //    AddError($"Unknown method call to {callExpression.FullName()}", expression.StartToken);
            }
            else if (expression is PropertyExpression)
            {
                PropertyExpression propertyExpression = (PropertyExpression)expression;
                //string propertyName = propertyExpression.FullName().ToUpper();
                //if (subResultTypes.ContainsKey(propertyName))
                //    return new EV3VariableTypeWithIndex(subResultTypes[propertyName], -1);
                //else
                //    AddError($"Unknown property {propertyExpression.FullName()}", expression.StartToken);
            }
            else
            {
                AddCompileError($"Unknown expression in {expression}", expression.StartToken);
            }
            return values;
        }

        private List<EV3VariableTypeWithIndexAndValue> CompileCodeForLiteralExpression(TextWriter writer, IEV3Variable variable, SBExpression expression)
        {
            List<EV3VariableTypeWithIndexAndValue> values = new List<EV3VariableTypeWithIndexAndValue>();

            EV3VariableTypeWithIndexAndValue expressionType = EV3VariableTypeWithIndexAndValue.CreateFromLiteral(expression);
            if (expressionType.Type == EV3Type.StringArray && variable.Type == EV3Type.StringArray)
            {
                foreach (Match match in Regex.Matches(expression.ToString(), @"([\d]+)=([^;""]*)"))
                    values.Add(ConvertIfNeeded(writer, expression, variable.Type,
                        new EV3VariableTypeWithIndexAndValue(EV3Type.StringArray, int.Parse(match.Groups[1].Value), $"'{match.Groups[2].Value}'")));
            }
            else
            {
                values.Add(ConvertIfNeeded(writer, expression, variable.Type, expressionType));
            }
            //if ((!variable.Type.IsArray() || !expressionType.IsArray()) && (expressionType == variable.Type || variable.Type.IsArrayOf(expressionType)))
            //{
            //    if (expressionType == EV3Type.String)
            //        values.Add(new EV3VariableTypeWithIndexAndValue(expressionType, expression.ToString().Replace('"', '\'')));
            //    else
            //        values.Add(new EV3VariableTypeWithIndexAndValue(expressionType, SmallBasicExtensions.FormatFloat(expression.ToString())));
            //}
            //else if (expressionType == EV3Type.StringArray && variable.Type == EV3Type.StringArray)
            //{
            //    foreach (Match match in Regex.Matches(expression.ToString(), @"([\d]+)=([^;""]*)"))
            //        values.Add(new EV3VariableTypeWithIndexAndValue(expressionType, int.Parse(match.Groups[1].Value), $"'{match.Groups[2].Value}'"));
            //}
            //else if (expressionType.IsNumber() && variable.Type.BaseType() == EV3Type.String)
            //{
            //    TempEV3Variable outputVariable = CreateTempVariable(EV3Type.String, expression.StartToken);
            //    values.Add(new EV3VariableTypeWithIndexAndValue(expressionType, outputVariable.Ev3Name));

            //    writer.WriteLine($"    STRINGS VALUE_FORMATTED {SmallBasicExtensions.FormatFloat(expression.ToString())} '%g' 99 {outputVariable.Ev3Name}");

            //    RemoveTempVariable(outputVariable);
            //}
            //else
            //    AddCompileError($"Value incompatible with variable type {variable.Type} in {expression}", expression.StartToken);

            return values;
        }

        private List<EV3VariableTypeWithIndexAndValue> CompileCodeForNegativeExpression(TextWriter writer, IEV3Variable variable, NegativeExpression expression)
        {
            List<EV3VariableTypeWithIndexAndValue> values = new List<EV3VariableTypeWithIndexAndValue>();

            values.Add(new EV3VariableTypeWithIndexAndValue(variable.Type, variable.Ev3Name));

            string expressionValue = CompileCodeForExpression(writer, variable, expression.Expression).First().Value;
            writer.WriteLine($"    MATH NEGATE {expressionValue} {variable.Ev3Name}");

            return values;
        }

        private List<EV3VariableTypeWithIndexAndValue> CompileCodeForIdentifierExpression(TextWriter writer, IEV3Variable variable, IdentifierExpression identifierExpression)
        {
            List<EV3VariableTypeWithIndexAndValue> values = new List<EV3VariableTypeWithIndexAndValue>();

            EV3Variable reference = FindVariable(identifierExpression.VariableName());

            if (reference != null)
            {
                values.Add(new EV3VariableTypeWithIndexAndValue(reference.Type, reference.Ev3Name));
            }

            return values;
        }

        private List<EV3VariableTypeWithIndexAndValue> CompileCodeForBinaryExpression(TextWriter writer, IEV3Variable variable, BinaryExpression binaryExpression)
        {
            List<EV3VariableTypeWithIndexAndValue> values = new List<EV3VariableTypeWithIndexAndValue>();

            TempEV3Variable leftTempVariable = CreateTempVariable(variable.Type, binaryExpression.StartToken);
            TempEV3Variable rightTempVariable = CreateTempVariable(variable.Type, binaryExpression.StartToken);

            List<EV3VariableTypeWithIndexAndValue> leftValues = CompileCodeForExpression(writer, leftTempVariable, binaryExpression.LeftHandSide);
            List<EV3VariableTypeWithIndexAndValue> rightValues = CompileCodeForExpression(writer, rightTempVariable, binaryExpression.RightHandSide);

            RemoveTempVariable(leftTempVariable);
            RemoveTempVariable(rightTempVariable);

            if (leftValues.Count == 1 && rightValues.Count == 1)
            {
                string leftValue = ConvertIfNeeded(writer, binaryExpression, variable.Type, leftValues[0]).Value;
                string rightValue = ConvertIfNeeded(writer, binaryExpression, variable.Type, rightValues[0]).Value;

                values.Add(new EV3VariableTypeWithIndexAndValue(variable.Type, variable.Ev3Name));

                if (variable.Type.IsNumber())
                {
                    switch (binaryExpression.Operator.Token)
                    {
                        case Token.Addition:
                            writer.WriteLine($"    ADDF {leftValue} {rightValue} {variable.Ev3Name}");
                            break;
                        case Token.Subtraction:
                            writer.WriteLine($"    SUBF {leftValue} {rightValue} {variable.Ev3Name}");
                            break;
                        case Token.Division:
                            if (DoDivisionCheck)
                                writer.WriteLine("<NOT IMPLEMENTED YET>");
                            else
                                writer.WriteLine($"    DIVF {leftValue} {rightValue} {variable.Ev3Name}");
                            break;
                        case Token.Multiplication:
                            writer.WriteLine($"    MULF {leftValue} {rightValue} {variable.Ev3Name}");
                            break;
                    }
                }
                else if (binaryExpression.Operator.Token == Token.Addition)
                {
                    writer.WriteLine($"    CALL TEXT.APPEND {leftValue} {rightValue} {variable.Ev3Name}");
                }
            }
            else if (leftValues.Count == 0 || rightValues.Count == 0)
                AddCompileError("Subexpression didn't return enough values", binaryExpression.StartToken);
            else
                AddCompileError("Subexpression didn't return correct number of values", binaryExpression.StartToken);

            return values;
        }

        private List<EV3VariableTypeWithIndexAndValue> CompileCodeForArrayExpression(TextWriter writer, IEV3Variable variable, ArrayExpression arrayExpression)
        {
            List<EV3VariableTypeWithIndexAndValue> values = new List<EV3VariableTypeWithIndexAndValue>();

            EV3Variable reference = FindVariable(arrayExpression.VariableName());

            if (reference != null)
            {
                if (variable.Type == EV3Type.String)
                    writer.WriteLine($"    CALL ARRAYGET_STRING {arrayExpression.Index()}.0 {variable.Ev3Name} {reference.Ev3Name}");
                else
                    writer.WriteLine($"    CALL ARRAYGET_FLOAT {arrayExpression.Index()}.0 {variable.Ev3Name} {reference.Ev3Name}");
                values.Add(new EV3VariableTypeWithIndexAndValue(variable.Type, variable.Ev3Name));
            }

            return values;

        }

        private EV3VariableTypeWithIndexAndValue ConvertIfNeeded(TextWriter writer, SBExpression expression, EV3Type variableType, EV3VariableTypeWithIndexAndValue value)
        {
            if (value.Type.IsNumber() && variableType.BaseType() == EV3Type.String)
            {
                TempEV3Variable outputVariable = CreateTempVariable(EV3Type.String, expression.StartToken);
                EV3VariableTypeWithIndexAndValue newValue = new EV3VariableTypeWithIndexAndValue(EV3Type.String, outputVariable.Ev3Name);

                writer.WriteLine($"    STRINGS VALUE_FORMATTED {value.Value} '%g' 99 {newValue.Value}");

                RemoveTempVariable(outputVariable);

                return newValue;
            }
            else
                return value;
        }

        private TempEV3Variable CreateTempVariable(EV3Type type, TokenInfo tokenInfo)
        {
            return new TempEV3Variable(type, () => GetTempVariableId(type, tokenInfo));
        }

        class TempEV3Variable : IEV3Variable
        {
            private Func<int> tempVariableGenerator;
            private string ev3Name = null;

            public TempEV3Variable(EV3Type type, Func<int> tempVariableGenerator)
            {
                Type = type;
                this.tempVariableGenerator = tempVariableGenerator;
            }

            public string Ev3Name
            {
                get
                {
                    if (ev3Name == null)
                    {
                        Id = tempVariableGenerator();
                        if (Type == EV3Type.String)
                            ev3Name = $"S{Id}";
                        else
                            ev3Name = $"F{Id}";
                    }
                    return ev3Name;
                }
            }

            public EV3Type Type { get; private set; }
            public int Id { get; private set; } = -1;
        }

        private int GetTempVariableId(EV3Type type, TokenInfo tokenInfo)
        {
            int firstAvailable = -1;
            if (type.IsNumber() || type == EV3Type.String)
            {
                for (int i = 0; i <= MAX_TEMP_VARIABLE_INDEX; i++)
                {
                    if (!tempVariablesCurrent[type].Contains(i))
                    {
                        firstAvailable = i;
                        break;
                    }
                }
                tempVariablesCurrent[type].Add(firstAvailable);
                tempVariablesMax[type] = Math.Max(tempVariablesMax[type], firstAvailable);
            }
            else
                AddCompileError($"Incorrect type {type} for temporary variable", tokenInfo);

            return firstAvailable;
        }

        private void RemoveTempVariable(TempEV3Variable variable)
        {
            tempVariablesCurrent[variable.Type].Remove(variable.Id);
        }

        public EV3Variable FindVariable(string variableName)
        {
            if (!variables.ContainsKey(variableName)) return null;

            return variables[variableName];
        }

        private EV3Variable FindVariable(SBExpression expression)
        {
            return variables.Values.FirstOrDefault(v => expression.IsVariable(v));
        }

        private void AddError(string message, TokenInfo tokenInfo)
        {
            Errors.Add(new Error(message, tokenInfo.Line + 1, tokenInfo.Column + 1));
        }

        private void AddCompileError(string message, TokenInfo tokenInfo)
        {
            CompileErrors.Add(new Error(message, tokenInfo.Line + 1, tokenInfo.Column + 1));
        }

        class EV3VariableTypeWithIndexAndValue
        {
            public EV3Type Type { get; set; }
            public int Index { get; set; }
            public string Value { get; set; }

            public EV3VariableTypeWithIndexAndValue(EV3Type type)
                : this(type, -1)
            {
            }

            public EV3VariableTypeWithIndexAndValue(EV3Type type, int index)
                : this(type, index, "")
            {
            }

            public EV3VariableTypeWithIndexAndValue(EV3Type type, string value)
                : this(type, -1, value)
            {
            }

            public EV3VariableTypeWithIndexAndValue(EV3Type type, int index, string value)
            {
                Type = type;
                Index = index;
                Value = value;
            }

            public static EV3VariableTypeWithIndexAndValue CreateFromLiteral(SBExpression expression)
            {
                string valueString = expression.ToString();
                if (expression.IsNumericLiteral())
                    return new EV3VariableTypeWithIndexAndValue(EV3Type.Float, SmallBasicExtensions.FormatFloat(valueString));

                Match match = Regex.Match(valueString, @"^""(?:[\d]+=[^;]*;)*([\d]+)=[^;]*[;]*""$");
                if (match.Success)
                {
                    return new EV3VariableTypeWithIndexAndValue(EV3Type.StringArray, int.Parse(match.Groups[1].Value), expression.ToString().Replace('"', '\''));
                }
                else
                    return new EV3VariableTypeWithIndexAndValue(EV3Type.String, expression.ToString().Replace('"', '\''));
            }

            public static EV3VariableTypeWithIndexAndValue CreateFromCompiler(IExpressionCompiler compiler)
            {
                if (compiler is ArrayExpressionCompiler)
                    return new EV3VariableTypeWithIndexAndValue(compiler.Type, ((ArrayExpressionCompiler)compiler).Index, compiler.Value);
                else if (compiler is LiteralExpressionCompiler)
                    return new EV3VariableTypeWithIndexAndValue(compiler.Type, ((LiteralExpressionCompiler)compiler).MaxIndex, compiler.Value);
                else
                    return new EV3VariableTypeWithIndexAndValue(compiler.Type, -1, compiler.Value);
            }
        }
    }
}
