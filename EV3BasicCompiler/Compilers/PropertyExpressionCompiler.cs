using Microsoft.SmallBasic.Expressions;
using System;

namespace EV3BasicCompiler.Compilers
{
    public class PropertyExpressionCompiler : ExpressionCompiler<PropertyExpression>
    {
        public PropertyExpressionCompiler(PropertyExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            string propertyName = Expression.FullName().ToUpper();
            EV3SubDefinition sub = Context.FindSubroutine(propertyName);
            if (sub != null)
                type = sub.ReturnType;
            else
                AddError($"Unknown property {Expression.FullName()}");
        }

        protected override void CalculateValue()
        {
        }
    }
}
