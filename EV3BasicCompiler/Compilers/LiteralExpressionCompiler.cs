using Microsoft.SmallBasic.Expressions;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler.Compilers
{
    public class LiteralExpressionCompiler : ExpressionCompiler<LiteralExpression>
    {
        private int maxIndex;

        public LiteralExpressionCompiler(LiteralExpression expression, EV3CompilerContext context) : base(expression, context)
        {
            maxIndex = -1;
        }

        protected override void CalculateType()
        {
            string valueString = ParentExpression.ToString();
            if (ParentExpression.IsNumericLiteral())
            {
                type = EV3Type.Float;
            }
            else
            {
                Match match = Regex.Match(valueString, @"^""(?:[\d]+=[^;]*;)*([\d]+)=[^;]*[;]*""$");
                if (match.Success)
                {
                    type = EV3Type.StringArray;
                    maxIndex = int.Parse(match.Groups[1].Value);
                }
                else
                {
                    type = EV3Type.String;
                }
            }
        }

        protected override void CalculateValue()
        {
            string valueString = ParentExpression.ToString();
            if (ParentExpression.IsNumericLiteral())
            {
                value = SmallBasicExtensions.FormatFloat(valueString);
                //value = valueString;
            }
            else
            {
                value = valueString.Replace('"', '\'');
            }
        }

        protected override void CalculateIsLiteral()
        {
            isLiteral = true;
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            //List<EV3VariableTypeWithIndexAndValue> values = new List<EV3VariableTypeWithIndexAndValue>();

            //EV3VariableTypeWithIndexAndValue expressionType = EV3VariableTypeWithIndexAndValue.CreateFromLiteral(expression);
            //if (expressionType.Type == EV3Type.StringArray && variable.Type == EV3Type.StringArray)
            //{
            //    foreach (Match match in Regex.Matches(expression.ToString(), @"([\d]+)=([^;""]*)"))
            //        values.Add(ConvertIfNeeded(writer, expression, variable.Type,
            //            new EV3VariableTypeWithIndexAndValue(EV3Type.StringArray, int.Parse(match.Groups[1].Value), $"'{match.Groups[2].Value}'")));
            //}
            //else
            //{
            //    values.Add(ConvertIfNeeded(writer, expression, variable.Type, expressionType));
            //}
            return Value;
        }

        public int MaxIndex
        {
            get
            {
                EnsureType();
                return maxIndex;
            }
        }
    }
}
