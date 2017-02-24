using Microsoft.SmallBasic.Expressions;
using System;

namespace EV3BasicCompiler.Compilers
{
    public class MethodCallExpressionCompiler : ExpressionCompiler<MethodCallExpression>
    {
        public MethodCallExpressionCompiler(MethodCallExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            string methodName = Expression.FullName().ToUpper();
            EV3SubDefinition sub = Context.FindSubroutine(methodName);
            if (sub != null)
                type = sub.ReturnType;
            else
                AddError($"Unknown method call to {Expression.FullName()}");
        }

        protected override void CalculateValue()
        {
        }
    }
}
