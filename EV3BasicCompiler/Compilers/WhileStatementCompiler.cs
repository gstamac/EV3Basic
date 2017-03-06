using System.IO;
using Microsoft.SmallBasic.Statements;

namespace EV3BasicCompiler.Compilers
{
    public class WhileStatementCompiler : StatementCompiler<WhileStatement>
    {
        public WhileStatementCompiler(WhileStatement statement, EV3CompilerContext context) : base(statement, context)
        {
        }

        public override void Compile(TextWriter writer)
        {
            string label = Context.GetNextLabelNumber().ToString();
            writer.WriteLine($"  while{label}:");
            ConditionCompiler.CompileBranchNegated(writer, $"endwhile{label}");
            //JR_LTEQF VI 0.0 endwhile0
            writer.WriteLine($"  whilebody{label}:");
            ParentStatement.WhileBody.Compile(writer);
            //writer.WriteLine($"    JR whilebody{label}");
            // JR_GTF VI 0.0 whilebody0
            ConditionCompiler.CompileBranch(writer, $"whilebody{label}");
            writer.WriteLine($"  endwhile{label}:");
        }

        private IBooleanExpressionCompiler conditionCompiler;
        private IBooleanExpressionCompiler ConditionCompiler
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
