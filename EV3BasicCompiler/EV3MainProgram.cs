using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Statements;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.SmallBasic.Expressions;

namespace EV3BasicCompiler
{
    public class EV3MainProgram
    {
        private Parser parser;
        private EV3Variables variables;
        int nextLabel;
        private readonly List<string> threads;
        private readonly List<EV3SubDefinition> subroutines;

        public List<Error> Errors { get; private set; }
        public List<Error> CompileErrors { get; private set; }

        public EV3MainProgram(Parser parser, EV3Variables variables)
        {
            this.parser = parser;
            this.variables = variables;

            threads = new List<string>();
            subroutines = new List<EV3SubDefinition>();

            Errors = new List<Error>();
            CompileErrors = new List<Error>();

            Clear();
        }

        public void Clear()
        {
            threads.Clear();
            subroutines.Clear();
            Errors.Clear();
            CompileErrors.Clear();
            nextLabel = 0;
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
                CompileCodeForThreadDispatchTable(writer);
                writer.Write(mainProgram);
            }
        }

        private void CompileCodeForThreadDispatchTable(TextWriter writer)
        {
            // add dispatch table to call proper sub-program first, if this is requested
            for (int i = 0; i < threads.Count; i++)
            {
                string thread = threads[i];
                int label = GetLabelNumber();
                writer.WriteLine("    JR_NEQ32 SUBPROGRAM " + i + " dispatch" + label);
                // put initial return address on stack

                writer.WriteLine("    WRITE32 ENDSUB_" + thread + ":ENDTHREAD STACKPOINTER RETURNSTACK");
                writer.WriteLine("    ADD8 STACKPOINTER 1 STACKPOINTER");
                writer.WriteLine("    JR SUB_" + thread);
                writer.WriteLine("  dispatch" + label + ":");
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
                CompileCodeForStatement(writer, statement);
            }
        }

        private void CompileCodeForStatement(TextWriter writer, Statement statement)
        {
            if (statement is AssignmentStatement)
            {
                CompileCodeForAssignmentStatement(writer, (AssignmentStatement)statement);
            }
        }

        private void CompileCodeForAssignmentStatement(TextWriter writer, AssignmentStatement assignmentStatement)
        {
            if (assignmentStatement.LeftValue is PropertyExpression)
            {
                PropertyExpression propertyExpression = (PropertyExpression)assignmentStatement.LeftValue;
                if (propertyExpression.FullName().Equals("Thread.Run", StringComparison.InvariantCultureIgnoreCase))
                {
                    int label = GetLabelNumber();
                    string thread = assignmentStatement.RightValue.ToString().ToUpper();
                    writer.WriteLine("    DATA32 tmp" + label);
                    writer.WriteLine("    CALL GETANDINC32 RUNCOUNTER_" + thread + " 1  RUNCOUNTER_" + thread + " tmp" + label);
                    writer.WriteLine("    JR_NEQ32 0 tmp" + label + " alreadylaunched" + label);
                    writer.WriteLine("    OBJECT_START T" + thread);
                    writer.WriteLine("  alreadylaunched" + label + ":");
                }
                else
                {

                }
            }
            else
            {
                variables.CompileCodeForAssignmentStatement(writer, assignmentStatement);
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

        private int GetLabelNumber()
        {
            return nextLabel++;
        }
    }
}
