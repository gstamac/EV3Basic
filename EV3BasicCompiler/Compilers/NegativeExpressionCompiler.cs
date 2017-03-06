using Microsoft.SmallBasic.Expressions;
using System;
using EV3BasicCompiler.Compilers;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class NegativeExpressionCompiler : ExpressionCompiler<NegativeExpression>
    {
        public NegativeExpressionCompiler(NegativeExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            type = ParentExpression.Expression.Compiler().Type;
        }

        protected override void CalculateValue()
        {
            if (Type.IsNumber())
            {
                if (IsLiteral)
                    value = SmallBasicExtensions.FormatFloat(-SmallBasicExtensions.ParseFloat(ParentExpression.Expression.Compiler().Value));
            }
            else
                AddError("Need number after minus");
        }

        protected override void CalculateIsLiteral()
        {
            isLiteral = ParentExpression.Expression.Compiler().IsLiteral;
        }

        protected override void CalculateCanCompileBoolean() => canCompileBoolean = false;

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            if (IsLiteral)
                return Value;
            else
            {
                string expressionValue = ParentExpression.Expression.Compiler().Compile(writer, variable);
                writer.WriteLine($"    MATH NEGATE {expressionValue} {variable.Ev3Name}");
                return variable.Ev3Name;
            }
        }
    }
}
