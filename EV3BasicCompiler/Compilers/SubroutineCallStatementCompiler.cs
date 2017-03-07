using Microsoft.SmallBasic.Statements;
using System;
using System.IO;
using System.Linq;

namespace EV3BasicCompiler.Compilers
{
    public class SubroutineCallStatementCompiler : StatementCompiler<SubroutineCallStatement>
    {
        public SubroutineCallStatementCompiler(SubroutineCallStatement statement, EV3CompilerContext context) : base(statement, context)
        {
            Ev3Name = statement.SubroutineName.NormalizedText.ToUpper();
        }

        public override void Compile(TextWriter writer, bool isRootStatement)
        {
            int label = Context.GetNextLabelNumber();
            writer.WriteLine($"    WRITE32 ENDSUB_{Ev3Name}:CALLSUB{label} STACKPOINTER RETURNSTACK");
            writer.WriteLine($"    ADD8 STACKPOINTER 1 STACKPOINTER");
            writer.WriteLine($"    JR SUB_{Ev3Name}");
            writer.WriteLine($"CALLSUB{label}:");
        }

        public string Ev3Name { get; private set; }
    }
}
