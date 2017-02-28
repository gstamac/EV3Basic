using Microsoft.SmallBasic.Statements;
using System;
using System.Linq;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class EmptyStatementCompiler : StatementCompiler<EmptyStatement>
    {
        public EmptyStatementCompiler(EmptyStatement statement, EV3CompilerContext context) : base(statement, context)
        {
        }

        public override void Compile(TextWriter writer)
        {
        }
    }
}
