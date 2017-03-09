using Microsoft.SmallBasic.Statements;
using System;
using System.IO;
using System.Linq;

namespace EV3BasicCompiler.Compilers
{
    public class ThreadStatementCompiler : StatementCompiler<AssignmentStatement>
    {
        private int threadNumber;

        public ThreadStatementCompiler(AssignmentStatement statement, EV3CompilerContext context) : base(statement, context)
        {
            threadNumber = context.GetNextThreadNumber();
        }

        public override void Compile(TextWriter writer, bool isRootStatement)
        {
            Context.FindSubroutine(ThreadName).IsReferenced = true;

            int label = Context.GetNextLabelNumber();
            writer.WriteLine($"    DATA32 tmp{label}");
            writer.WriteLine($"    CALL GETANDINC32 RUNCOUNTER_{ThreadName} 1  RUNCOUNTER_{ThreadName} tmp{label}");
            writer.WriteLine($"    JR_NEQ32 0 tmp{label} alreadylaunched{label}");
            writer.WriteLine($"    OBJECT_START T{ThreadName}");
            writer.WriteLine($"  alreadylaunched{label}:");
            return;
        }

        public void CompileDeclaration(TextWriter writer)
        {
            writer.WriteLine("DATA32 RUNCOUNTER_" + ThreadName);
        }

        public void CompileInitialization(TextWriter writer)
        {
            writer.WriteLine("    MOVE32_32 0 RUNCOUNTER_" + ThreadName);
        }

        public void CompileProgram(TextWriter writer)
        {
            writer.WriteLine("subcall PROGRAM_" + ThreadName);
        }

        public void CompileThreadCode(TextWriter writer)
        {
            writer.WriteLine("vmthread " + "T" + ThreadName);
            writer.WriteLine("{");
            // launch program with proper subprogram selector and correct local data area
            writer.WriteLine("    DATA32 tmp");
            writer.WriteLine("  launch:");
            writer.WriteLine("    CALL PROGRAM_" + ThreadName + " " + threadNumber);
            writer.WriteLine("    CALL GETANDINC32 RUNCOUNTER_" + ThreadName + " -1 RUNCOUNTER_" + ThreadName + " tmp");
            // after this position in the code, the flag could be 0 and another thread could 
            // newly trigger this thread. this causes no problems, because this thread was 
            // in process of terminating anyway and would not have called the worker method
            // again. if it is now instead re-activated, it will immediately start at the
            // begining.
            writer.WriteLine("    JR_GT32 tmp 1 launch");
            writer.WriteLine("}");
            //memorize_reference("GETANDINC32");
        }

        public void CompileDispatchTable(TextWriter writer)
        {
            int label = Context.GetNextLabelNumber();
            writer.WriteLine($"    JR_NEQ32 SUBPROGRAM {threadNumber} dispatch{label}");
            // put initial return address on stack

            writer.WriteLine($"    WRITE32 ENDSUB_{ThreadName}:ENDTHREAD STACKPOINTER RETURNSTACK");
            writer.WriteLine("    ADD8 STACKPOINTER 1 STACKPOINTER");
            writer.WriteLine($"    JR SUB_{ThreadName}");
            writer.WriteLine($"  dispatch{label}:");
            return;
        }

        private string threadName;
        public string ThreadName
        {
            get
            {
                if (threadName == null)
                    threadName = ParentStatement.RightValue.Compiler().Value.ToUpper();
                return threadName;
            }
        }

    }
}
