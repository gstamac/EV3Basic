using Microsoft.SmallBasic.Expressions;
using System;

namespace EV3BasicCompiler.Compilers
{
    public class IdentifierExpressionCompiler : ExpressionCompiler<IdentifierExpression>
    {
        public IdentifierExpressionCompiler(IdentifierExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            EV3Variable reference = Context.FindVariable(Expression.VariableName());
            type = reference.Type;
            //ProcessVariable(reference, trace, subResultTypes);
            //return new EV3VariableTypeWithIndexAndValue(reference.Type, reference.MaxIndex);
        }

        protected override void CalculateValue()
        {
        }
    }
}
