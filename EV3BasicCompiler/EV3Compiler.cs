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
using EV3BasicCompiler.Properties;
using System.Text;
using EV3BasicCompiler.Compilers;

namespace EV3BasicCompiler
{
    public class EV3Compiler : IDisposable
    {
        private readonly Parser parser;
        private EV3CompilerContext context;
        private readonly List<SBError> SBErrors;
        private readonly EV3Library library;
        private readonly EV3Variables variables;
        private readonly EV3MainProgram mainProgram;

        public EV3Compiler()
        {
            SBErrors = new List<SBError>();
            parser = new Parser(SBErrors);

            library = new EV3Library();

            variables = new EV3Variables(parser);
            context = new EV3CompilerContext(variables, library);
            mainProgram = new EV3MainProgram(parser, variables, context);
        }

        public List<Error> Errors { get { return context.Errors; } }

        public void Compile(TextReader reader, TextWriter writer)
        {
            Clear();

            Parse(reader);
            if (context.Errors.Count != 0) return;

            LoadLibrary();
            ProcessPragmas();
            LoadVariables();
            ProcessCode();

            string mainProgramCode = GenerateMainProgramCode();

            GenerateInitialization(writer);
            GenerateMainThread(writer);
            GenerateThreads(writer);
            GeneratePrograms(writer, mainProgramCode);
            GenerateReferences(writer);

            context.Errors.AddRange(mainProgram.Errors);
            context.Errors.AddRange(library.Errors);
        }

        private void Parse(TextReader reader)
        {
            parser.Parse(reader);
            parser.AttachCompilers(context);
            foreach (SBError error in SBErrors) AddError(error);
        }

        private void Clear()
        {
            if (parser != null)
                parser.RemoveCompilers();
            SBErrors.Clear();
            library.Clear();
            variables.Clear();
            mainProgram.Clear();
        }

        private void ProcessPragmas()
        {
            foreach (EmptyStatement statement in parser.GetStatements<EmptyStatement>()
                .Where(s => s.EndingComment.TokenType == TokenType.Comment && s.EndingComment.Text.StartsWith("'PRAGMA")))
            {
                string setting = statement.EndingComment.Text.Substring(7).Trim();
                if (setting.Equals("NOBOUNDSCHECK"))
                    context.DoBoundsCheck = false;
                else if (setting.Equals("BOUNDSCHECK"))
                    context.DoBoundsCheck = true;
                else if (setting.Equals("NODIVISIONCHECK"))
                    context.DoDivisionCheck = false;
                else if (setting.Equals("DIVISIONCHECK"))
                    context.DoDivisionCheck = true;
                else if (setting.Equals("NOOPTIMIZATION"))
                    context.DoOptimization = false;
                else if (setting.Equals("OPTIMIZATION"))
                    context.DoOptimization = true;
                else
                    AddError("Unknown PRAGMA: " + setting, statement.StartToken);
            }
        }

        private void LoadLibrary()
        {
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
        }

        private void LoadVariables()
        {
            variables.Process();
        }

        private void ProcessCode()
        {
            mainProgram.Process();
        }

        private string GenerateMainProgramCode()
        {
            using (StringWriter writer = new StringWriter())
            {
                mainProgram.CompileCodeForMainProgram(writer);
                return writer.ToString();
            }
        }

        private void GenerateInitialization(TextWriter writer)
        {
            library.CompileCodeForGlobals(writer);
            variables.CompileCodeForVariableDeclarations(writer);
            mainProgram.CompileCodeForRunCounterStorageDeclarations(writer);
        }

        private void GenerateMainThread(TextWriter writer)
        {
            writer.WriteLine("");

            writer.WriteLine("vmthread MAIN");
            writer.WriteLine("{");
            library.CompileCodeForRuntimeInit(writer);
            variables.CompileCodeForVariableInitializations(writer);
            mainProgram.CompileCodeForRunCounterStorageInitialization(writer);
            writer.WriteLine("    ARRAY CREATE8 1 LOCKS");
            // copy native code to brick if needed
            writer.WriteLine("    CALL PROGRAM_MAIN -1");       // launch main program
            writer.WriteLine("    PROGRAM_STOP -1");
            writer.WriteLine("}");
        }

        private void GenerateThreads(TextWriter writer)
        {
            mainProgram.CompileCodeForThreads(writer);
        }

        private void GeneratePrograms(TextWriter writer, string mainProgramCode)
        {
            // create the code for the basic program (will be called from various threads)
            // multiple VM subcall objects will be created that share the same implementation, but
            // have a seperate local storage

            writer.WriteLine("");

            writer.WriteLine("subcall PROGRAM_MAIN");
            mainProgram.CompileCodeForThreadPrograms(writer);
            writer.WriteLine("{");
            // the call parameter that decides, which subroutine to start
            writer.WriteLine("    IN_32 SUBPROGRAM");
            // storage for variables for compiler use
            writer.WriteLine("    DATA32 INDEX");
            writer.WriteLine("    ARRAY8 STACKPOINTER 4");  // waste 4 bytes, but keep alignment
            variables.CompileCodeForTempVariableDeclarationsFloat(writer);
            writer.WriteLine("    ARRAY32 RETURNSTACK2 128");   // addressing the stack is done with an 8bit int.
            writer.WriteLine("    ARRAY32 RETURNSTACK 128");    // when it wrapps over from 127 to -128, the second 
            writer.WriteLine();                                 // part of the stack will be used (256 entries total)
            variables.CompileCodeForTempVariableDeclarationsString(writer);

            // initialize the stack pointer
            writer.WriteLine("    MOVE8_8 0 STACKPOINTER");

            writer.Write(mainProgramCode);

            writer.WriteLine("ENDTHREAD:");
            writer.WriteLine("    RETURN");
            mainProgram.CompileCodeForSubroutines(writer);
            writer.WriteLine("}");
        }

        private void GenerateReferences(TextWriter writer)
        {
            library.CompileCodeForReferences(writer);
        }

        private void AddError(SBError error)
        {
            context.AddError(error.Description, error.Line + 1, error.Column + 1);
        }

        private void AddError(string message, TokenInfo tokenInfo)
        {
            context.AddError(message, tokenInfo);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage()]
        public string Dump()
        {
            StringWriter s = new StringWriter();

            s.WriteLine("COMPILER ERRORS:");
            foreach (var item in context.Errors)
                s.WriteLine($"{item.Line}:{item.Column}: {item.Message}");

            s.WriteLine(parser.Dump());

            return s.ToString();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
