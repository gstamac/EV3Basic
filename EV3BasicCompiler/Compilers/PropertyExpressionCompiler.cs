using Microsoft.SmallBasic.Expressions;
using System;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class PropertyExpressionCompiler : ExpressionCompiler<PropertyExpression>
    {
        public PropertyExpressionCompiler(PropertyExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            string propertyName = ParentExpression.FullName().ToUpper();
            EV3SubDefinition sub = Context.FindSubroutine(propertyName);
            if (sub != null)
                type = sub.ReturnType;
            else
                AddError($"Unknown property {ParentExpression.FullName()}");
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
