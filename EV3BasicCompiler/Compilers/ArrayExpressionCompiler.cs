using Microsoft.SmallBasic.Expressions;
using System;
using System.IO;

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
            EV3Variable reference = Context.FindVariable(ParentExpression.VariableName());
            type = reference.Type.BaseType();
            index = ParentExpression.Index();
            //ProcessVariable(reference, trace, subResultTypes);
            //return new EV3VariableTypeWithIndexAndValue(reference.Type.BaseType(), reference.MaxIndex);
        }

        protected override void CalculateValue()
        {
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            EV3Variable reference = Context.FindVariable(ParentExpression.VariableName());

            if (reference != null)
            {
                if (variable.Type == EV3Type.String)
                    writer.WriteLine($"    CALL ARRAYGET_STRING {ParentExpression.Index()}.0 {variable.Ev3Name} {reference.Ev3Name}");
                else
                    writer.WriteLine($"    CALL ARRAYGET_FLOAT {ParentExpression.Index()}.0 {variable.Ev3Name} {reference.Ev3Name}");
                return variable.Ev3Name;
            }
            return "";
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
