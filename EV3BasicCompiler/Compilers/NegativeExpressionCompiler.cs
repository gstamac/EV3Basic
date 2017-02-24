using Microsoft.SmallBasic.Expressions;
using System;
using EV3BasicCompiler.Compilers;

namespace EV3BasicCompiler.Compilers
{
    public class NegativeExpressionCompiler : ExpressionCompiler<NegativeExpression>
    {
        public NegativeExpressionCompiler(NegativeExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            IExpressionCompiler subCompiler = Expression.Expression.Compiler();
            type = subCompiler.Type;
            isLiteral = subCompiler.IsLiteral;
        }

        protected override void CalculateValue()
        {
            if (Expression.IsNumericLiteral())
                value = SmallBasicExtensions.FormatFloat(Expression.ToString());
        }
    }
}
