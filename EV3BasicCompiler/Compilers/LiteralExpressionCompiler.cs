using Microsoft.SmallBasic.Expressions;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler.Compilers
{
    public class LiteralExpressionCompiler : ExpressionCompiler<LiteralExpression>, IBooleanExpressionCompiler
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
            if (Type == EV3Type.StringArray && variable.Type == EV3Type.StringArray)
            {
                foreach (Match match in Regex.Matches(Value, @"([\d]+)=([^;']*)"))
                {
                    writer.WriteLine($"    CALL ARRAYSTORE_STRING {match.Groups[1].Value}.0 '{match.Groups[2].Value}' {variable.Ev3Name}");
                }
                return "";
            }

            return Value;
        }

        protected override void CalculateBooleanValue()
        {
            booleanValue = "'true'".Equals(Value, StringComparison.InvariantCultureIgnoreCase);
        }

        public void CompileBranch(TextWriter writer, string label)
        {
            if (BooleanValue)
            {
                writer.WriteLine($"    JR {label}");
            }
        }

        public void CompileBranchNegated(TextWriter writer, string label)
        {
            if (!BooleanValue)
            {
                writer.WriteLine($"    JR {label}");
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
