using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Expressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using Microsoft.SmallBasic.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler
{
    public class EV3Parser
    {
        readonly ParserEx parser;
        readonly Dictionary<string, EV3Variable> variables;

        public EV3Parser()
        {
            parser = new ParserEx();
            variables = new Dictionary<string, EV3Variable>();
        }

        public void Parse(TextReader reader)
        {
            parser.Parse(reader);
            ProcessVariables();
            ProcessReferences();
        }

        private void ProcessVariables()
        {
            variables.Clear();
            foreach (string key in parser.SymbolTable.InitializedVariables.Keys)
            {
                variables.Add(key, new EV3Variable($"V{key.ToUpper()}"));
            }
            foreach (AssignmentStatement statement in parser.GetStatements<AssignmentStatement>())
            {
                ProcessAssignmentStatement(statement);
            }
            foreach (ForStatement statement in parser.GetStatements<ForStatement>())
            {
                ProcessForStatement(statement);
            }
        }

        void ProcessAssignmentStatement(AssignmentStatement statement)
        {
            if (statement.LeftValue is IdentifierExpression)
            {
                IdentifierExpression identifierExpression = (IdentifierExpression)statement.LeftValue;
                ProcessIdentifierExpressionValue(identifierExpression.Identifier.NormalizedText.ToLower(), statement.RightValue);
            }
            else if (statement.LeftValue is ArrayExpression)
            {
                ArrayExpression arrayExpression = (ArrayExpression)statement.LeftValue;
                if (arrayExpression.LeftHand is IdentifierExpression)
                {
                    IdentifierExpression identifierExpression = (IdentifierExpression)arrayExpression.LeftHand;
                    ProcessIdentifierExpressionValue(identifierExpression.Identifier.NormalizedText.ToLower(), statement.RightValue, true, Int32.Parse(arrayExpression.Indexer.ToString()));
                }
            }
        }

        private void ProcessIdentifierExpressionValue(string variableName, SBExpression value, bool isArray = false, int index = -1)
        {
            EV3Variable variable = FindVariable(variableName);
            if (variable == null) return;

            variable.IsArray = isArray;
            variable.UpdateMaxIndex(index);

            variable.Comment += $" @{value.StartToken.Line} = {value} ({value.GetType()})";
            ProcessIdentifierExpressionValue(variable, value);
        }

        private void ProcessIdentifierExpressionValue(EV3Variable variable, SBExpression value)
        {
            if (value is LiteralExpression || value is NegativeExpression)
            {
                if (Regex.IsMatch(value.ToString(), @"^[\d\.\-]+$"))
                    variable.UpdateVariableType(EV3VariableType.Float);
                else
                    variable.UpdateVariableType(EV3VariableType.String);
            }
            else if (value is IdentifierExpression)
            {
                EV3Variable reference = FindVariable(((IdentifierExpression)value).Identifier.NormalizedText.ToLower());
                if (reference != null) variable.References.Add(reference);
            }
            else if (value is BinaryExpression)
            {
                BinaryExpression binaryExpression = (BinaryExpression)value;
                ProcessIdentifierExpressionValue(variable, binaryExpression.LeftHandSide);
                ProcessIdentifierExpressionValue(variable, binaryExpression.RightHandSide);
            }
        }

        private void ProcessForStatement(ForStatement statement)
        {
            EV3Variable variable = FindVariable(statement.Iterator.NormalizedText.ToLower());
            if (variable == null) return;

            variable.UpdateVariableType(EV3VariableType.Float);
        }

        private EV3Variable FindVariable(string variableName)
        {
            if (!variables.ContainsKey(variableName)) return null;

            return variables[variableName];
        }

        void ProcessReferences()
        {
            foreach (EV3Variable variable in variables.Values)
            {
                variable.UpdateVariableTypeFromReferences();
            }
        }

        public void GenerateEV3Code(TextWriter writer)
        {
            foreach (var variable in variables.Values)
            {
                writer.WriteLine(variable.GenerateCode() + " // " + variable.Comment);
            }
        }

        public string Dump()
        {
            StringWriter s = new StringWriter();

            s.WriteLine("VARIABLES:");
            foreach (var item in parser.SymbolTable.Variables)
                s.WriteLine(item.Key + " = " + item.Value);
            s.WriteLine("INITIALIZED VARIABLES:");
            foreach (var item in parser.SymbolTable.InitializedVariables)
                s.WriteLine($"{item.Key} = {item.Value}");
            s.WriteLine("SUBROUTINES:");
            foreach (var item in parser.SymbolTable.Subroutines)
                s.WriteLine($"{item.Key} = {item.Value}");
            s.WriteLine("LABELS:");
            foreach (var item in parser.SymbolTable.Labels)
                s.WriteLine($"{item.Key} = {item.Value}");
            s.WriteLine("ERRORS:");
            foreach (var item in parser.SymbolTable.Errors)
                s.WriteLine($"{item.Line}:{item.Column}: {item.Description}");

            s.WriteLine("TREE:");
            foreach (Statement statement in parser.ParseTree)
            {
                s.WriteLine($"{statement.GetType().Name}:{statement}");
                if (statement is AssignmentStatement)
                {
                    var leftValue = ((AssignmentStatement)statement).LeftValue;
                    var rightValue = ((AssignmentStatement)statement).RightValue;
                    s.WriteLine($"-----> LEFT: {leftValue.GetType().Name}:{leftValue.ToString()}");
                    if (leftValue is ArrayExpression)
                    {
                        ArrayExpression arrayExpression = (ArrayExpression)leftValue;
                        s.WriteLine("----->   ARRAY LeftHand: (" + arrayExpression.LeftHand.GetType() + ") " + arrayExpression.LeftHand);
                        s.WriteLine("----->   ARRAY Indexer: (" + arrayExpression.Indexer.GetType() + ") " + arrayExpression.Indexer);
                    }
                    s.WriteLine($"-----> RIGHT: {rightValue.GetType().Name}:{rightValue.ToString()}");
                }
                else if (statement is ForStatement)
                {
                    s.WriteLine($"-----> Iterator: {((ForStatement)statement).Iterator}");
                }
                else if (statement is SubroutineStatement)
                {
                    s.WriteLine($"-----> SubroutineName: {((SubroutineStatement)statement).SubroutineName}");
                }
            }

            return s.ToString();
        }
    }
}
