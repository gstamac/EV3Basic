using Microsoft.SmallBasic.Expressions;
using System;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class MethodCallExpressionCompiler : ExpressionCompiler<MethodCallExpression>
    {
        public MethodCallExpressionCompiler(MethodCallExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            string methodName = ParentExpression.FullName().ToUpper();
            EV3SubDefinition sub = Context.FindSubroutine(methodName);
            if (sub != null)
                type = sub.ReturnType;
            else
                AddError($"Unknown method call to {ParentExpression.FullName()}");
        }

        protected override void CalculateValue()
        {
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            throw new NotImplementedException();
        }
    }
}
