using Microsoft.SmallBasic.Expressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using Microsoft.SmallBasic;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class BooleanExpressionCompiler : BinaryExpressionCompilerBase, IBooleanExpressionCompiler
    {
        public BooleanExpressionCompiler(BinaryExpression expression, EV3CompilerContext context) : base(expression, context)
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
                if (ParentExpression.Operator.Token == Token.And)
                    value = SmallBasicExtensions.FormatBoolean(BooleanValue);
                else
                    value = SmallBasicExtensions.FormatBoolean(BooleanValue);
            }
        }

        protected override void CalculateCanCompileBoolean()
        {
            canCompileBoolean = LeftCompiler.CanCompileBoolean && RightCompiler.CanCompileBoolean;
            if (!canCompileBoolean.Value)
                AddError("Can only operate boolean operations on boolean values.");
        }

        protected override void CalculateBooleanValue()
        {
            if (ParentExpression.Operator.Token == Token.And)
                booleanValue = LeftCompiler.BooleanValue && RightCompiler.BooleanValue;
            else
                booleanValue = LeftCompiler.BooleanValue || RightCompiler.BooleanValue;
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            return "";
        }

        public void CompileBranch(TextWriter writer, string label)
        {
            if (Type != EV3Type.Boolean) return;

            IBooleanExpressionCompiler leftBooleanCompiler = LeftCompiler as IBooleanExpressionCompiler;
            IBooleanExpressionCompiler rightBooleanCompiler = RightCompiler as IBooleanExpressionCompiler;

            if (leftBooleanCompiler != null && rightBooleanCompiler != null)
            {
                if (ParentExpression.Operator.Token == Token.And)
                {
                    string andLabel = $"and{Context.GetNextLabelNumber()}";
                    leftBooleanCompiler.CompileBranchNegated(writer, andLabel);
                    rightBooleanCompiler.CompileBranch(writer, label);
                    writer.WriteLine($"  {andLabel}:");
                }
                else
                {
                    leftBooleanCompiler.CompileBranch(writer, label);
                    rightBooleanCompiler.CompileBranch(writer, label);
                }
            }
            else
                AddError("Cannot use boolean operator on non-boolean expressions");
        }

        public void CompileBranchNegated(TextWriter writer, string label)
        {
            if (Type != EV3Type.Boolean) return;

            IBooleanExpressionCompiler leftBooleanCompiler = LeftCompiler as IBooleanExpressionCompiler;
            IBooleanExpressionCompiler rightBooleanCompiler = RightCompiler as IBooleanExpressionCompiler;

            if (leftBooleanCompiler != null && rightBooleanCompiler != null)
            {
                if (ParentExpression.Operator.Token == Token.And)
                {
                    leftBooleanCompiler.CompileBranchNegated(writer, label);
                    rightBooleanCompiler.CompileBranchNegated(writer, label);
                }
                else
                {
                    string orLabel = $"or{Context.GetNextLabelNumber()}";
                    leftBooleanCompiler.CompileBranch(writer, orLabel);
                    rightBooleanCompiler.CompileBranchNegated(writer, label);
                    writer.WriteLine($"  {orLabel}:");
                }
            }
            else
                AddError("Cannot use boolean operator on non-boolean expressions");
        }
    }
}
