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
        private readonly List<string> threads;
        private readonly List<EV3SubDefinition> subroutines;

        public List<Error> Errors { get; private set; }

        public EV3MainProgram(Parser parser, EV3Variables variables)
        {
            this.parser = parser;
            this.variables = variables;

            threads = new List<string>();
            subroutines = new List<EV3SubDefinition>();

            Errors = new List<Error>();

            Clear();
        }

        public void Clear()
        {
            threads.Clear();
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
            foreach (AssignmentStatement statement in parser.GetStatements<AssignmentStatement>().Where(s => s.LeftValue.ToString().Equals("Thread.Run")))
            {
                threads.Add(statement.RightValue.ToString().ToUpper());
            }
        }

        private void ProcessSubroutines()
        {
            foreach (SubroutineStatement statement in parser.GetStatements<SubroutineStatement>())
            {
                string name = statement.SubroutineName.NormalizedText.ToUpper();
                StringWriter code = new StringWriter();
                CompileCodeForSubroutine(code, name, statement);

                EV3SubDefinition sub = new EV3SubDefinition(name, code.ToString());
                subroutines.Add(sub);
            }
        }

        public void CompileCodeForRunCounterStorageDeclarations(TextWriter writer)
        {
            foreach (string thread in threads)
            {
                writer.WriteLine("DATA32 RUNCOUNTER_" + thread);
            }
        }

        public void CompileCodeForRunCounterStorageInitialization(TextWriter writer)
        {
            foreach (string thread in threads)
            {
                writer.WriteLine("    MOVE32_32 0 RUNCOUNTER_" + thread);
            }
        }

        public void CompileCodeForThreads(TextWriter writer)
        {
            for (int i = 0; i < threads.Count; i++)
            {
                string thread = threads[i];
                writer.WriteLine("vmthread " + "T" + thread);
                writer.WriteLine("{");
                // launch program with proper subprogram selector and correct local data area
                writer.WriteLine("    DATA32 tmp");
                writer.WriteLine("  launch:");
                writer.WriteLine("    CALL PROGRAM_" + thread + " " + i);
                writer.WriteLine("    CALL GETANDINC32 RUNCOUNTER_" + thread + " -1 RUNCOUNTER_" + thread + " tmp");
                // after this position in the code, the flag could be 0 and another thread could 
                // newly trigger this thread. this causes no problems, because this thread was 
                // in process of terminating anyway and would not have called the worker method
                // again. if it is now instead re-activated, it will immediately start at the
                // begining.
                writer.WriteLine("    JR_GT32 tmp 1 launch");
                writer.WriteLine("}");
                //memorize_reference("GETANDINC32");
            }
        }

        public void CompileCodeForThreadPrograms(TextWriter writer)
        {
            foreach (String thread in threads)
            {
                writer.WriteLine("subcall PROGRAM_" + thread);
            }
        }

        public void CompileCodeForMainProgram(TextWriter writer)
        {
            using (StringWriter mainProgram = new StringWriter())
            {
                CompileCodeForStatements(mainProgram, parser.ParseTree.Where(s => !(s is SubroutineStatement)));
                CompileCodeForThreadDispatchTable(writer, parser.ParseTree.OfType<AssignmentStatement>());
                writer.Write(mainProgram);
            }
        }

        private void CompileCodeForThreadDispatchTable(TextWriter writer, IEnumerable<AssignmentStatement> statements)
        {
            foreach (AssignmentStatement assignmentStatement in statements)
            {
                ThreadStatementCompiler threadStatementCompiler = assignmentStatement.Compiler<ThreadStatementCompiler>();
                if (threadStatementCompiler != null)
                    threadStatementCompiler.CompileDispatchTable(writer);
            }
        }

        public void CompileCodeForSubroutines(TextWriter writer)
        {
            foreach (EV3SubDefinition sub in subroutines)
            {
                writer.WriteLine();
                writer.WriteLine(sub.Code);
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
            writer.WriteLine($"SUB_{name}:");
            CompileCodeForStatements(writer, statement.SubroutineBody);
            writer.WriteLine("    SUB8 STACKPOINTER 1 STACKPOINTER");
            writer.WriteLine("    READ32 RETURNSTACK STACKPOINTER INDEX");
            writer.WriteLine("    JR_DYNAMIC INDEX");
            writer.WriteLine($"ENDSUB_{name}:");
        }
    }
}
