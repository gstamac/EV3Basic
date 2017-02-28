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
        private Parser parser;
        private EV3Variables variables;
        private List<ThreadStatementCompiler> threadCompilers;
        private readonly List<EV3SubDefinition> subroutines;

        public List<Error> Errors { get; private set; }

        public EV3MainProgram(Parser parser, EV3Variables variables)
        {
            this.parser = parser;
            this.variables = variables;

            subroutines = new List<EV3SubDefinition>();

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
            foreach (SubroutineStatement statement in parser.GetStatements<SubroutineStatement>())
            {
                string name = statement.SubroutineName.NormalizedText.ToUpper();
                StringWriter code = new StringWriter();
                CompileCodeForSubroutine(code, name, statement);

                EV3SubDefinition sub = new EV3SubDefinition(name, name, code.ToString());
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
                CompileCodeForStatements(mainProgram, parser.ParseTree.Where(s => !(s is SubroutineStatement)));
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
            foreach (EV3SubDefinition sub in subroutines)
            {
                writer.WriteLine();
                writer.WriteLine($"SUB_{sub.Name}:");
                writer.WriteLine(sub.Code);
                writer.WriteLine($"ENDSUB_{sub.Name}:");
            }
        }

        private void CompileCodeForStatements(TextWriter writer, IEnumerable<Statement> statements)
        {
            foreach (Statement statement in statements)
            {
                statement.Compiler().Compile(writer);
            }
        }

        private void CompileCodeForSubroutine(TextWriter writer, string name, SubroutineStatement statement)
        {
            CompileCodeForStatements(writer, statement.SubroutineBody);
            writer.WriteLine("    SUB8 STACKPOINTER 1 STACKPOINTER");
            writer.WriteLine("    READ32 RETURNSTACK STACKPOINTER INDEX");
            writer.WriteLine("    JR_DYNAMIC INDEX");
        }
    }
}
