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

        protected override EV3Type CalculateType()
        {
            string valueString = ParentExpression.ToString();
            if (ParentExpression.IsNumericLiteral())
            {
                return EV3Type.Float;
            }
            else
            {
                Match match = Regex.Match(valueString, @"^""(?:[\d]+=[^;]*;)*([\d]+)=[^;]*[;]*""$");
                if (match.Success)
                {
                    maxIndex = int.Parse(match.Groups[1].Value);
                    return EV3Type.StringArray;
                }
                else
                {
                    return EV3Type.String;
                }
            }
        }

        protected override string CalculateValue()
        {
            string valueString = ParentExpression.ToString();
            if (ParentExpression.IsNumericLiteral())
            {
                return SmallBasicExtensions.FormatFloat(valueString);
            }
            else
            {
                return valueString.Replace('"', '\'');
            }
        }

        protected override bool CalculateIsLiteral() => true;

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

        protected override bool CalculateBooleanValue()
        {
            return "'true'".Equals(Value, StringComparison.InvariantCultureIgnoreCase);
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
