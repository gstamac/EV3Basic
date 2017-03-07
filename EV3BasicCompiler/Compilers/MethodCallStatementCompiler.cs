using Microsoft.SmallBasic.Statements;
using System;
using System.Linq;
using System.IO;
using EV3BasicCompiler.Compilers;

namespace EV3BasicCompiler.Compilers
{
    public class MethodCallStatementCompiler : StatementCompiler<MethodCallStatement>
    {
        public MethodCallStatementCompiler(MethodCallStatement statement, EV3CompilerContext context) : base(statement, context)
        {
        }

        public override void Compile(TextWriter writer, bool isRootStatement)
        {
            IExpressionCompiler expressionCompiler = ParentStatement.MethodCallExpression.Compiler();
            using (var tempVariables = Context.UseTempVariables())
            {
                if (expressionCompiler.Type == EV3Type.Void)
                    expressionCompiler.Compile(writer, null);
                else
                    expressionCompiler.Compile(writer, tempVariables.CreateVariable(expressionCompiler.Type));
            }
        }
    }
}
