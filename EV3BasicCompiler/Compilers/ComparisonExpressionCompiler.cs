using Microsoft.SmallBasic.Expressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using Microsoft.SmallBasic;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class ComparisonExpressionCompiler : BinaryExpressionCompilerBase, IBooleanExpressionCompiler
    {
        public ComparisonExpressionCompiler(BinaryExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            if (!CanCompileBoolean)
            {
                type = EV3Type.Void;
            }
            else
                type = EV3Type.Boolean;
        }

        protected override void CalculateValue()
        {
            if (Type != EV3Type.Boolean) return;

            if (LeftCompiler.IsLiteral && RightCompiler.IsLiteral)
            {
                isLiteral = true;

                if (LeftCompiler.Type.IsNumber())
                {
                    float leftValue = SmallBasicExtensions.ParseFloat(LeftCompiler.Value);
                    float rightValue = SmallBasicExtensions.ParseFloat(RightCompiler.Value);

                    switch (ParentExpression.Operator.Token)
                    {
                        case Token.Equals:
                            value = SmallBasicExtensions.FormatBoolean(leftValue == rightValue);
                            break;
                        case Token.NotEqualTo:
                            value = SmallBasicExtensions.FormatBoolean(leftValue != rightValue);
                            break;
                        case Token.LessThan:
                            value = SmallBasicExtensions.FormatBoolean(leftValue < rightValue);
                            break;
                        case Token.LessThanEqualTo:
                            value = SmallBasicExtensions.FormatBoolean(leftValue <= rightValue);
                            break;
                        case Token.GreaterThan:
                            value = SmallBasicExtensions.FormatBoolean(leftValue > rightValue);
                            break;
                        case Token.GreaterThanEqualTo:
                            value = SmallBasicExtensions.FormatBoolean(leftValue >= rightValue);
                            break;
                    }
                }
                else
                {
                    switch (ParentExpression.Operator.Token)
                    {
                        case Token.Equals:
                            value = SmallBasicExtensions.FormatBoolean(LeftCompiler.Value == RightCompiler.Value);
                            break;
                        case Token.NotEqualTo:
                            value = SmallBasicExtensions.FormatBoolean(LeftCompiler.Value != RightCompiler.Value);
                            break;
                    }
                }
            }
        }

        protected override void CalculateCanCompileBoolean()
        {
            canCompileBoolean = false;
            if (LeftCompiler.Type != RightCompiler.Type)
                AddError("Boolean operations on unrelated types are not permited");
            else if (LeftCompiler.Type.IsArray() || RightCompiler.Type.IsArray())
                AddError("Boolean operations on arrays are not permited");
            else if (LeftCompiler.Type == EV3Type.String && ParentExpression.Operator.Token != Token.Equals && ParentExpression.Operator.Token != Token.NotEqualTo)
                AddError("Only (non)equality comparison is permited on strings");
            else
                canCompileBoolean = true;
        }

        protected override void CalculateBooleanValue()
        {
            booleanValue = "'true'".Equals(Value, StringComparison.InvariantCultureIgnoreCase);
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            return "";
        }

        public void CompileBranch(TextWriter writer, string label)
        {
            CompileBranch(writer, label, true);
        }

        public void CompileBranchNegated(TextWriter writer, string label)
        {
            CompileBranch(writer, label, false);
        }

        private void CompileBranch(TextWriter writer, string label, bool branchIfTrue)
        {
            if (Type != EV3Type.Boolean) return;

            if (IsLiteral)
                CompileBranchForLiteral(writer, label, branchIfTrue);
            else if (LeftCompiler.Type.IsNumber())
                CompileBranchWithNumericCompare(writer, label, branchIfTrue);
            else
                CompileBranchWithStringCompare(writer, label, branchIfTrue);
        }

        private void CompileBranchForLiteral(TextWriter writer, string label, bool branchIfTrue)
        {
            if (BooleanValue && branchIfTrue)
                writer.WriteLine($"    JR {label}");
            else if (!branchIfTrue)
                writer.WriteLine($"    JR {label}");
        }

        private void CompileBranchWithNumericCompare(TextWriter writer, string label, bool branchIfTrue)
        {
            using (var tempVariables = Context.UseTempVariables())
            {
                string leftValue = LeftCompiler.Compile(writer, tempVariables.CreateVariable(LeftCompiler.Type));
                string rightValue = RightCompiler.Compile(writer, tempVariables.CreateVariable(RightCompiler.Type));

                writer.WriteLine($"    JR_{GetOperationForJump(!branchIfTrue)}F {leftValue} {rightValue} {label}");
            }
        }

        private void CompileBranchWithStringCompare(TextWriter writer, string label, bool branchIfTrue)
        {
            if (LeftCompiler.IsLiteral && LeftCompiler.CanCompileBoolean && IsBooleanValue(LeftCompiler.Value))
            {
                CompileBranchForStringVariable(writer, RightCompiler, label, LeftCompiler.BooleanValue ^ (ParentExpression.Operator.Token == Token.NotEqualTo));
            }
            else if (RightCompiler.IsLiteral && RightCompiler.CanCompileBoolean && IsBooleanValue(RightCompiler.Value))
            {
                CompileBranchForStringVariable(writer, LeftCompiler, label, RightCompiler.BooleanValue ^ (ParentExpression.Operator.Token == Token.NotEqualTo));
            }
            else
            {
                using (var tempVariables = Context.UseTempVariables())
                {
                    string leftValue = LeftCompiler.Compile(writer, tempVariables.CreateVariable(LeftCompiler.Type));
                    string rightValue = RightCompiler.Compile(writer, tempVariables.CreateVariable(RightCompiler.Type));

                    //IEV3Variable tempResultVariable = tempVariables.CreateVariable(EV3Type.String);
                    //writer.WriteLine($"    CALL EQ_STRING {leftValue} {rightValue} {tempResultVariable.Ev3Name}");
                    //writer.WriteLine($"    AND8888_32 {tempResultVariable.Ev3Name} -538976289 {tempResultVariable.Ev3Name}");
                    //writer.WriteLine($"    STRINGS COMPARE {tempResultVariable.Ev3Name} 'TRUE' {tempResultVariable.Ev3Name}");
                    //writer.WriteLine($"    JR_{GetOperationForJump(branchIfTrue)}8 {tempResultVariable.Ev3Name} 0 {label}");
                    IEV3Variable tempResultVariable = tempVariables.CreateVariable(EV3Type.Float);
                    writer.WriteLine($"    CALL EQ_STRING8 {leftValue} {rightValue} {tempResultVariable.Ev3Name}");
                    writer.WriteLine($"    JR_{GetOperationForJump(branchIfTrue)}8 {tempResultVariable.Ev3Name} 0 {label}");
                }
            }
        }

        private bool IsBooleanValue(string stringValue)
        {
            return "'true'".Equals(stringValue, StringComparison.InvariantCultureIgnoreCase)
                || "'false'".Equals(stringValue, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetOperationForJump(bool negate)
        {
            switch (ParentExpression.Operator.Token)
            {
                case Token.Equals:
                    return negate ? "NEQ" : "EQ";
                case Token.NotEqualTo:
                    return negate ? "EQ" : "NEQ";
                case Token.LessThan:
                    return negate ? "GTEQ" : "LT";
                case Token.LessThanEqualTo:
                    return negate ? "GT" : "LTEQ";
                case Token.GreaterThan:
                    return negate ? "LTEQ" : "GT";
                case Token.GreaterThanEqualTo:
                    return negate ? "LT" : "GTEQ";
            }

            return "";
        }

        private string GetOperationForCall(bool negate)
        {
            switch (ParentExpression.Operator.Token)
            {
                case Token.Equals:
                    return negate ? "NE" : "EQ";
                case Token.NotEqualTo:
                    return negate ? "EQ" : "NE";
                case Token.LessThan:
                    return negate ? "GTEQ" : "LT";
                case Token.LessThanEqualTo:
                    return negate ? "GT" : "LTEQ";
                case Token.GreaterThan:
                    return negate ? "LTEQ" : "GT";
                case Token.GreaterThanEqualTo:
                    return negate ? "LT" : "GTEQ";
            }

            return "";
        }
    }
}
