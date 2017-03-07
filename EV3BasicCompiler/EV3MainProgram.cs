using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Statements;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.SmallBasic.Expressions;
using EV3BasicCompiler.Compilers;
using EV3BasicCompiler;

namespace EV3BasicCompiler
{
    public class EV3MainProgram
    {
        private readonly Parser parser;
        private readonly EV3Variables variables;
        private readonly EV3CompilerContext context;
        private List<ThreadStatementCompiler> threadCompilers;
        private readonly List<EV3SubDefinitionBase> subroutines;

        public List<Error> Errors { get; private set; }

        public EV3MainProgram(Parser parser, EV3Variables variables, EV3CompilerContext context)
        {
            this.parser = parser;
            this.variables = variables;
            this.context = context;

            subroutines = new List<EV3SubDefinitionBase>();

            Errors = new List<Error>();

            Clear();
        }

        public void Clear()
        {
            subroutines.Clear();
            Errors.Clear();
        }

        public void Process()
        {
            ProcessThreads();
            ProcessSubroutines();
        }

        private void ProcessThreads()
        {
            threadCompilers = parser.GetStatements<AssignmentStatement>().Select(s => s.Compiler()).OfType<ThreadStatementCompiler>().ToList();
        }

        private void ProcessSubroutines()
        {
            foreach (SubroutineStatementCompiler compiler in parser.GetStatements<SubroutineStatement>().Select(s => s.Compiler<SubroutineStatementCompiler>()))
            {
                StringWriter code = new StringWriter();
                compiler.Compile(code, true);

                EV3SubDefinition sub = new EV3SubDefinition(compiler.Ev3Name, compiler.Ev3Name, code.ToString());
                subroutines.Add(sub);
            }
        }

        public void CompileCodeForRunCounterStorageDeclarations(TextWriter writer)
        {
            foreach (ThreadStatementCompiler threadStatementCompiler in threadCompilers)
                threadStatementCompiler.CompileDeclaration(writer);
        }

        public void CompileCodeForRunCounterStorageInitialization(TextWriter writer)
        {
            foreach (ThreadStatementCompiler threadStatementCompiler in threadCompilers)
                threadStatementCompiler.CompileInitialization(writer);
        }

        public void CompileCodeForThreads(TextWriter writer)
        {
            foreach (ThreadStatementCompiler threadStatementCompiler in threadCompilers)
                threadStatementCompiler.CompileThreadCode(writer);
        }

        public void CompileCodeForThreadPrograms(TextWriter writer)
        {
            foreach (ThreadStatementCompiler threadStatementCompiler in threadCompilers)
                threadStatementCompiler.CompileProgram(writer);
        }

        public void CompileCodeForMainProgram(TextWriter writer)
        {
            using (StringWriter mainProgram = new StringWriter())
            {
                parser.ParseTree.Where(s => !(s is SubroutineStatement)).Compile(mainProgram, true);
                CompileCodeForThreadDispatchTable(writer);
                writer.Write(mainProgram);
            }
        }

        private void CompileCodeForThreadDispatchTable(TextWriter writer)
        {
            foreach (ThreadStatementCompiler threadStatementCompiler in threadCompilers)
                threadStatementCompiler.CompileDispatchTable(writer);
        }

        public void CompileCodeForSubroutines(TextWriter writer)
        {
            foreach (EV3SubDefinitionBase sub in subroutines)
            {
                writer.WriteLine();
                sub.Compile(writer, context, new string[0], null);
            }
        }
    }
}
