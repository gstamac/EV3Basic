using Microsoft.SmallBasic.Expressions;
using System;

namespace EV3BasicCompiler.Compilers
{
    public class ArrayExpressionCompiler : ExpressionCompiler<ArrayExpression>
    {
        private int index;

        public ArrayExpressionCompiler(ArrayExpression expression, EV3CompilerContext context) : base(expression, context)
        {
            index = -1;
        }

        protected override void CalculateType()
        {
            EV3Variable reference = Context.FindVariable(Expression.VariableName());
            type = reference.Type.BaseType();
            index = Expression.Index();
            //ProcessVariable(reference, trace, subResultTypes);
            //return new EV3VariableTypeWithIndexAndValue(reference.Type.BaseType(), reference.MaxIndex);
        }

        protected override void CalculateValue()
        {
        }

        public int Index
        {
            get
            {
                EnsureType();
                return index;
            }
        }
    }
}
