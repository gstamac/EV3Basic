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

        protected override EV3Type CalculateType()
        {
            return ParentExpression.Expression.Compiler().Type;
        }

        protected override string CalculateValue()
        {
            if (Type.IsNumber())
            {
                if (IsLiteral)
                    return SmallBasicExtensions.FormatFloat(-SmallBasicExtensions.ParseFloat(ParentExpression.Expression.Compiler().Value));
            }
            else
                AddError("Need number after minus");
            return null;
        }

        protected override bool CalculateIsLiteral()
        {
            return ParentExpression.Expression.Compiler().IsLiteral;
        }

        protected override bool CalculateCanCompileBoolean() => false;

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
