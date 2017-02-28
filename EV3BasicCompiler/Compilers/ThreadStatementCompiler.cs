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

        public override void Compile(TextWriter writer)
        {
            IExpressionCompiler valueCompiler = ParentStatement.RightValue.Compiler();
            int label = Context.GetNextLabelNumber();
            string thread = valueCompiler.Value.ToUpper();
            writer.WriteLine($"    DATA32 tmp{label}");
            writer.WriteLine($"    CALL GETANDINC32 RUNCOUNTER_{thread} 1  RUNCOUNTER_{thread} tmp{label}");
            writer.WriteLine($"    JR_NEQ32 0 tmp{label} alreadylaunched{label}");
            writer.WriteLine($"    OBJECT_START T{thread}");
            writer.WriteLine($"  alreadylaunched{label}:");
            return;
        }

        public void CompileDispatchTable(TextWriter writer)
        {
            IExpressionCompiler valueCompiler = ParentStatement.RightValue.Compiler();
            int label = Context.GetNextLabelNumber();
            string thread = valueCompiler.Value.ToUpper();
            writer.WriteLine($"    JR_NEQ32 SUBPROGRAM {threadNumber} dispatch{label}");
            // put initial return address on stack

            writer.WriteLine($"    WRITE32 ENDSUB_{thread}:ENDTHREAD STACKPOINTER RETURNSTACK");
            writer.WriteLine("    ADD8 STACKPOINTER 1 STACKPOINTER");
            writer.WriteLine($"    JR SUB_{thread}");
            writer.WriteLine($"  dispatch{label}:");
            return;
        }
    }
}
