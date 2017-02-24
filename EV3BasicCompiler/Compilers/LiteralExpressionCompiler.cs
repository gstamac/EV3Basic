using Microsoft.SmallBasic.Expressions;
using System;
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
            string valueString = Expression.ToString();
            if (Expression.IsNumericLiteral())
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
            isLiteral = true;
            string valueString = Expression.ToString();
            if (Expression.IsNumericLiteral())
            {
                //value = SmallBasicExtensions.FormatFloat(valueString);
                value = valueString;
            }
            else
            {
                value = valueString.Replace('"', '\'');
            }
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
