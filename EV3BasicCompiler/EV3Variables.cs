using EV3BasicCompiler;
using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Expressions;
using Microsoft.SmallBasic.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;

namespace EV3BasicCompiler
{
    public class EV3Variables
    {
        private Parser parser;
        private readonly Dictionary<string, EV3Variable> variables;

        public EV3Variables(Parser parser)
        {
            this.parser = parser;
            variables = new Dictionary<string, EV3Variable>();
            Errors = new List<Error>();
        }

        public List<Error> Errors { get; private set; }

        public void Process(Dictionary<string, EV3Type> subResultTypes)
        {
            variables.Clear();
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

            foreach (AssignmentStatement statement in parser.GetStatements<AssignmentStatement>().Where(s => s.IsVariable(variable)))
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
            EV3VariableTypeWithIndex type = GetVariableType(statement.RightValue, trace, subResultTypes);
            if (statement.LeftValue is ArrayExpression)
            {
                variable.UpdateMaxIndex(((ArrayExpression)statement.LeftValue).Index());
                type.Type = type.Type.ConvertToArray();
            }
            else
            {
                variable.UpdateMaxIndex(type.Index);
            }

            SetVariableType(variable, statement, type.Type);
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

        private EV3VariableTypeWithIndex GetVariableType(SBExpression value, EV3Variable[] trace, Dictionary<string, EV3Type> subResultTypes)
        {
            if (value is LiteralExpression || value is NegativeExpression)
            {
                string valueString = value.ToString();
                if (Regex.IsMatch(valueString, @"^[\d\.\-]+$"))
                    return new EV3VariableTypeWithIndex(EV3Type.Float, -1);
                else if (Regex.IsMatch(valueString, @"^""([\d]+=[^;]*;)*([\d]+=[^;]*)""$"))
                {
                    Match match = Regex.Match(valueString, @"^""(?:[\d]+=[^;]*;)*([\d]+)=[^;]*""$");
                    return new EV3VariableTypeWithIndex(EV3Type.StringArray, int.Parse(match.Groups[1].Value));
                }
                else
                    return new EV3VariableTypeWithIndex(EV3Type.String, -1);
            }
            else if (value is IdentifierExpression)
            {
                EV3Variable reference = FindVariable(((IdentifierExpression)value).VariableName());
                ProcessVariable(reference, trace, subResultTypes);
                return new EV3VariableTypeWithIndex(reference.Type, reference.MaxIndex);
            }
            else if (value is BinaryExpression)
            {
                BinaryExpression binaryExpression = (BinaryExpression)value;
                EV3VariableTypeWithIndex leftType = GetVariableType(binaryExpression.LeftHandSide, trace, subResultTypes);
                EV3VariableTypeWithIndex rightType = GetVariableType(binaryExpression.RightHandSide, trace, subResultTypes);
                if ((int)leftType.Type > (int)rightType.Type)
                    return leftType;
                else
                    return rightType;
            }
            else if (value is ArrayExpression)
            {
                EV3Variable reference = FindVariable(((ArrayExpression)value).VariableName());
                ProcessVariable(reference, trace, subResultTypes);
                return new EV3VariableTypeWithIndex(reference.Type.BaseType(), reference.MaxIndex);
            }
            else if (value is MethodCallExpression)
            {
                MethodCallExpression callExpression = (MethodCallExpression)value;
                string methodName = callExpression.FullName().ToUpper();
                if (subResultTypes.ContainsKey(methodName))
                    return new EV3VariableTypeWithIndex(subResultTypes[methodName], -1);
                else
                    AddError($"Unknown method call to {callExpression.FullName()}", value.StartToken);
            }
            else if (value is PropertyExpression)
            {
                PropertyExpression propertyExpression = (PropertyExpression)value;
                string propertyName = propertyExpression.FullName().ToUpper();
                if (subResultTypes.ContainsKey(propertyName))
                    return new EV3VariableTypeWithIndex(subResultTypes[propertyName], -1);
                else
                    AddError($"Unknown property {propertyExpression.FullName()}", value.StartToken);
            }
            return new EV3VariableTypeWithIndex(EV3Type.Unknown, -1);
        }

        private void UpdateVariableIndex(EV3Variable variable)
        {
            foreach (ArrayExpression arrayExpression in parser.GetExpressions<ArrayExpression>().Where(ae => ae.IsVariable(variable)))
            {
                variable.UpdateMaxIndex(arrayExpression.Index());
            }
        }

        private EV3Variable FindVariable(string variableName)
        {
            if (!variables.ContainsKey(variableName)) return null;

            return variables[variableName];
        }

        public void GenerateCodeForVariableDeclarations(TextWriter writer)
        {
            foreach (EV3Variable variable in variables.Values)
            {
                writer.WriteLine(variable.GenerateDeclarationCode());
            }
        }

        public void GenerateCodeForVariableInitializations(TextWriter writer)
        {
            foreach (EV3Variable variable in variables.Values)
            {
                writer.WriteLine("    " + variable.GenerateInitializationCode());
            }
        }

        private void AddError(string message, TokenInfo tokenInfo)
        {
            Errors.Add(new Error(message, tokenInfo.Line + 1, tokenInfo.Column + 1));
        }

        class EV3VariableTypeWithIndex
        {
            public EV3Type Type { get; set; }
            public int Index { get; set; }

            public EV3VariableTypeWithIndex(EV3Type type, int index)
            {
                Type = type;
                Index = index;
            }
        }
    }
}
