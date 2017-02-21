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
using EV3BasicCompiler.Properties;

namespace EV3BasicCompiler
{
    public class EV3Compiler
    {
        private readonly Parser parser;
        private readonly List<SBError> SBErrors;
        private readonly EV3Variables variables;
        private readonly EV3Library library;

        public List<Error> Errors { get; private set; }

        public EV3Compiler()
        {
            Errors = new List<Error>();

            SBErrors = new List<SBError>();
            parser = new Parser(SBErrors);
            variables = new EV3Variables(parser);
            library = new EV3Library();
        }

        public void Parse(TextReader reader)
        {
            parser.Parse(reader);
            foreach (SBError error in SBErrors) AddError(error);
            if (Errors.Count == 0)
            {
                LoadLibrary();
                ProcessVariables();
                ProcessCode();
            }
        }

        private void LoadLibrary()
        {
            library.Clear();
            library.LoadModule(Resources.runtimelibrary);
            library.LoadModule(Resources.Assert);
            library.LoadModule(Resources.Buttons);
            library.LoadModule(Resources.EV3);
            library.LoadModule(Resources.EV3File);
            library.LoadModule(Resources.LCD);
            library.LoadModule(Resources.Mailbox);
            library.LoadModule(Resources.Math);
            library.LoadModule(Resources.Motor);
            library.LoadModule(Resources.Program);
            library.LoadModule(Resources.Sensor);
            library.LoadModule(Resources.Speaker);
            library.LoadModule(Resources.Text);
            library.LoadModule(Resources.Thread);
            library.LoadModule(Resources.Vector);
            Errors.AddRange(library.Errors);
        }

        private void ProcessVariables()
        {
            variables.Process(library.GetSubResultTypes());
            Errors.AddRange(variables.Errors);
        }

        private void ProcessCode()
        {
        }

        public void GenerateEV3Code(TextWriter writer)
        {
            GenerateInitialization(writer);

            writer.WriteLine("");

            writer.WriteLine("vmthread MAIN");
            writer.WriteLine("{");
            GenerateMainThreadInitialization(writer);
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
            library.GenerateCodeForGlobals(writer);
            variables.GenerateCodeForVariableDeclarations(writer);
        }

        private void GenerateMainThreadInitialization(TextWriter writer)
        {
            library.GenerateCodeForRuntimeInit(writer);
            variables.GenerateCodeForVariableInitializations(writer);
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

        private void AddError(SBError error)
        {
            Errors.Add(new Error(error.Description, error.Line + 1, error.Column + 1));
        }

        private void AddError(string message, TokenInfo tokenInfo)
        {
            Errors.Add(new Error(message, tokenInfo.Line + 1, tokenInfo.Column + 1));
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
                    if (rightValue is MethodCallExpression)
                    {
                        MethodCallExpression methodCallExpression = (MethodCallExpression)rightValue;
                        s.WriteLine("----->   SUBCALL MethodName: " + methodCallExpression.MethodName);
                        s.WriteLine("----->   SUBCALL TypeName: " + methodCallExpression.TypeName);
                        s.WriteLine("----->   SUBCALL Arguments: " + methodCallExpression.Arguments);
                    }
                    else if (rightValue is PropertyExpression)
                    {
                        PropertyExpression propertyExpression = (PropertyExpression)rightValue;
                        s.WriteLine("----->   SUBCALL PropertyName: " + propertyExpression.PropertyName);
                        s.WriteLine("----->   SUBCALL TypeName: " + propertyExpression.TypeName);
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
