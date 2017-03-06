using Microsoft.SmallBasic.Statements;
using System;
using System.IO;
using System.Linq;

namespace EV3BasicCompiler.Compilers
{
    public class SubroutineStatementCompiler : StatementCompiler<SubroutineStatement>
    {
        public SubroutineStatementCompiler(SubroutineStatement statement, EV3CompilerContext context) : base(statement, context)
        {
            Ev3Name = statement.SubroutineName.NormalizedText.ToUpper();
        }

        public override void Compile(TextWriter writer)
        {
            ParentStatement.SubroutineBody.Compile(writer);
            writer.WriteLine("    SUB8 STACKPOINTER 1 STACKPOINTER");
            writer.WriteLine("    READ32 RETURNSTACK STACKPOINTER INDEX");
            writer.WriteLine("    JR_DYNAMIC INDEX");
        }

        public string Ev3Name { get; private set; }
    }
}
