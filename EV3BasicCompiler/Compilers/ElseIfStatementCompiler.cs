using Microsoft.SmallBasic.Statements;
using System;
using System.IO;
using System.Linq;

namespace EV3BasicCompiler.Compilers
{
    public class ElseIfStatementCompiler : StatementCompiler<ElseIfStatement>, IConditionStatementCompiler
    {
        public ElseIfStatementCompiler(ElseIfStatement statement, EV3CompilerContext context) : base(statement, context)
        {
        }

        public override void Compile(TextWriter writer)
        {
        }

        public void CompileStatements(TextWriter writer)
        {
            ParentStatement.ThenStatements.Compile(writer);
        }

        public bool IsAlwaysFalse { get { return IsLiteralCondition() && !ConditionCompiler.BooleanValue; } }
        public bool IsAlwaysTrue { get { return IsLiteralCondition() && ConditionCompiler.BooleanValue; } }

        private bool IsLiteralCondition()
        {
            return (ConditionCompiler != null) && ConditionCompiler.CanCompileBoolean && ConditionCompiler.IsLiteral;
        }

        private IBooleanExpressionCompiler conditionCompiler;
        public IBooleanExpressionCompiler ConditionCompiler
        {
            get
            {
                if (conditionCompiler == null)
                    conditionCompiler = ParentStatement.Condition.Compiler<IBooleanExpressionCompiler>();
                return conditionCompiler;
            }
        }
    }
}
