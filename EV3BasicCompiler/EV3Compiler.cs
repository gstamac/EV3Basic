using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Expressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using SBError = Microsoft.SmallBasic.Error;
using Microsoft.SmallBasic.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EV3BasicCompiler;

namespace EV3BasicCompiler
{
    public class EV3Compiler
    {
        readonly Parser parser;
        readonly List<SBError> SBErrors;
        readonly Dictionary<string, EV3Variable> variables;

        public List<Error> Errors { get; private set; }

        public EV3Compiler()
        {
            Errors = new List<Error>();

            SBErrors = new List<SBError>();
            parser = new Parser(SBErrors);
            variables = new Dictionary<string, EV3Variable>();
        }

        public void Parse(TextReader reader)
        {
            parser.Parse(reader);
            foreach (SBError error in SBErrors) AddError(error);
            if (Errors.Count == 0)
            {
                ProcessVariables();
            }
        }

        private void ProcessVariables()
        {
            variables.Clear();
            foreach (var variable in parser.SymbolTable.InitializedVariables)
            {
                variables.Add(variable.Key, new EV3Variable(variable.Key, variable.Value));
            }
            foreach (EV3Variable variable in variables.Values)
            {
                if (variable.Type == EV3VariableType.Unknown)
                    ProcessVariable(variable, new EV3Variable[0]);
                if (variable.Type == EV3VariableType.Unknown)
                    AddError($"Variable type couldn't be determined for '{variable.Name}'", variable.TokenInfo);
            }
            foreach (EV3Variable variable in variables.Values.Where(v => v.Type.IsArray()))
            {
                UpdateVariableIndex(variable);
            }
        }

        private void ProcessVariable(EV3Variable variable, EV3Variable[] trace)
        {
            if (trace.Contains(variable)) return;
            if (variable.Type != EV3VariableType.Unknown) return;

            EV3Variable[] newTrace = trace.Concat(new EV3Variable[] { variable }).ToArray();

            foreach (AssignmentStatement statement in parser.GetStatements<AssignmentStatement>().Where(s => s.IsVariable(variable)))
            {
                ProcessAssignmentStatement(variable, statement, newTrace);
            }
            foreach (ForStatement statement in parser.GetStatements<ForStatement>().Where(s => s.IsVariable(variable)))
            {
                ProcessForStatement(variable, statement, newTrace);
            }
        }

        void ProcessAssignmentStatement(EV3Variable variable, AssignmentStatement statement, EV3Variable[] trace)
        {
            EV3VariableTypeWithIndex type = GetVariableType(statement.RightValue, trace);
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
            SetVariableType(variable, statement, EV3VariableType.Float);
        }

        private void SetVariableType(EV3Variable variable, Statement statement, EV3VariableType type)
        {
            if (variable.Type == EV3VariableType.Unknown)
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

        private EV3VariableTypeWithIndex GetVariableType(SBExpression value, EV3Variable[] trace)
        {
            if (value is LiteralExpression || value is NegativeExpression)
            {
                string valueString = value.ToString();
                if (Regex.IsMatch(valueString, @"^[\d\.\-]+$"))
                    return new EV3VariableTypeWithIndex(EV3VariableType.Float, -1);
                else if (Regex.IsMatch(valueString, @"^""([\d]+=[^;]*;)*([\d]+=[^;]*)""$"))
                {
                    Match match = Regex.Match(valueString, @"^""(?:[\d]+=[^;]*;)*([\d]+)=[^;]*""$");
                    return new EV3VariableTypeWithIndex(EV3VariableType.StringArray, int.Parse(match.Groups[1].Value));
                }
                else
                    return new EV3VariableTypeWithIndex(EV3VariableType.String, -1);
            }
            else if (value is IdentifierExpression)
            {
                EV3Variable reference = FindVariable(((IdentifierExpression)value).VariableName());
                ProcessVariable(reference, trace);
                return new EV3VariableTypeWithIndex(reference.Type, reference.MaxIndex);
            }
            else if (value is BinaryExpression)
            {
                BinaryExpression binaryExpression = (BinaryExpression)value;
                EV3VariableTypeWithIndex leftType = GetVariableType(binaryExpression.LeftHandSide, trace);
                EV3VariableTypeWithIndex rightType = GetVariableType(binaryExpression.RightHandSide, trace);
                if ((int)leftType.Type > (int)rightType.Type)
                    return leftType;
                else
                    return rightType;
            }
            else if (value is ArrayExpression)
            {
                EV3Variable reference = FindVariable(((ArrayExpression)value).VariableName());
                ProcessVariable(reference, trace);
                return new EV3VariableTypeWithIndex(reference.Type.BaseType(), reference.MaxIndex);
            }
            else if (value is MethodCallExpression)
            {
                AddError("Method calls are not supported yet", value.StartToken);
            }
            else if (value is PropertyExpression)
            {
                AddError("Property calls are not supported yet", value.StartToken);
            }
            return new EV3VariableTypeWithIndex(EV3VariableType.Unknown, -1);
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

        public void GenerateEV3Code(TextWriter writer)
        {
            GenerateInitialization(writer);
            GenerateEV3CodeForVariableDeclarations(writer);

            writer.WriteLine("");

            writer.WriteLine("vmthread MAIN");
            writer.WriteLine("{");
            GenerateMainThreadInitialization(writer);
            GenerateEV3CodeForVariableInitializations(writer);
            GenerateMainThreadStart(writer);
            writer.WriteLine("}");

            writer.WriteLine("");

            writer.WriteLine("subcall PROGRAM_MAIN");
            writer.WriteLine("{");
            GenerateMainProgramInitialization(writer);
            GenerateMainProgramFinalization(writer);
            writer.WriteLine("}");
        }

        private void GenerateInitialization(TextWriter writer)
        {
            writer.WriteLine("DATA16 FD_NATIVECODECOMMAND");
            writer.WriteLine("DATA16 FD_NATIVECODERESPONSE");
            writer.WriteLine("DATA32 STOPLCDUPDATE");
            writer.WriteLine("DATA32 NUMMAILBOXES");
            writer.WriteLine("ARRAY16 LOCKS 2");
        }

        private void GenerateMainThreadInitialization(TextWriter writer)
        {
            writer.WriteLine("    MOVE32_32 0 STOPLCDUPDATE");
            writer.WriteLine("    MOVE32_32 0 NUMMAILBOXES");
            writer.WriteLine("    OUTPUT_RESET 0 15");
            writer.WriteLine("    INPUT_DEVICE CLR_ALL -1");
            writer.WriteLine("    ARRAY CREATE8 0 LOCKS");
        }

        private void GenerateMainThreadStart(TextWriter writer)
        {
            writer.WriteLine("    ARRAY CREATE8 1 LOCKS");
            writer.WriteLine("    CALL PROGRAM_MAIN -1");
            writer.WriteLine("    PROGRAM_STOP -1");
        }

        private void GenerateMainProgramInitialization(TextWriter writer)
        {
            writer.WriteLine("    IN_32 SUBPROGRAM");
            writer.WriteLine("    DATA32 INDEX");
            writer.WriteLine("    ARRAY8 STACKPOINTER 4");
            writer.WriteLine("    ARRAY32 RETURNSTACK2 128");
            writer.WriteLine("    ARRAY32 RETURNSTACK 128");
            writer.WriteLine("    MOVE8_8 0 STACKPOINTER");
        }

        private void GenerateMainProgramFinalization(TextWriter writer)
        {
            writer.WriteLine("    ENDTHREAD:");
            writer.WriteLine("        RETURN");
        }

        private void GenerateEV3CodeForVariableDeclarations(TextWriter writer)
        {
            foreach (var variable in variables.Values)
            {
                writer.WriteLine(variable.GenerateDeclarationCode());
            }
        }

        private void GenerateEV3CodeForVariableInitializations(TextWriter writer)
        {
            foreach (var variable in variables.Values)
            {
                writer.WriteLine("    " + variable.GenerateInitializationCode());
            }
        }

        private void AddError(SBError error)
        {
            Errors.Add(new Error(error.Description, error.Line + 1, error.Column + 1));
        }

        private void AddError(string message, TokenInfo tokenInfo)
        {
            Errors.Add(new Error(message, tokenInfo.Line + 1, tokenInfo.Column + 1));
        }

        class EV3VariableTypeWithIndex
        {
            public EV3VariableType Type { get; set; }
            public int Index { get; set; }

            public EV3VariableTypeWithIndex(EV3VariableType type, int index)
            {
                Type = type;
                Index = index;
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage()]
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
