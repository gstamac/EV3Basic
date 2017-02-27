using Microsoft.SmallBasic.Expressions;
using System;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class IdentifierExpressionCompiler : ExpressionCompiler<IdentifierExpression>
    {
        public IdentifierExpressionCompiler(IdentifierExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            EV3Variable reference = Context.FindVariable(VariableName);
            type = reference.Type;
        }

        protected override void CalculateValue()
        {
            EV3Variable reference = Context.FindVariable(VariableName);

            if (reference != null)
                value = reference.Ev3Name;
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            return Value;
        }

        private string _variableName = null;
        private string VariableName
        {
            get
            {
                if (_variableName == null)
                    _variableName = ParentExpression.VariableName();
                return _variableName;
            }
        }
    }
}
