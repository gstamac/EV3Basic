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

        protected override EV3Type CalculateType()
        {
            if (!CanCompileBoolean)
            {
                return EV3Type.Void;
            }
            else
                return EV3Type.Boolean;
        }

        protected override string CalculateValue()
        {
            if (Type != EV3Type.Boolean) return null;

            if (LeftCompiler.IsLiteral && RightCompiler.IsLiteral)
            {
                if (ParentExpression.Operator.Token == Token.And)
                    return SmallBasicExtensions.FormatBoolean(BooleanValue);
                else
                    return SmallBasicExtensions.FormatBoolean(BooleanValue);
            }
            return null;
        }

        protected override bool CalculateCanCompileBoolean()
        {
            if (LeftCompiler.CanCompileBoolean && RightCompiler.CanCompileBoolean)
                return true;

            AddError("Can only operate boolean operations on boolean values.");
            return false;
        }

        protected override bool CalculateBooleanValue()
        {
            if (ParentExpression.Operator.Token == Token.And)
                return LeftCompiler.BooleanValue && RightCompiler.BooleanValue;
            else
                return LeftCompiler.BooleanValue || RightCompiler.BooleanValue;
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
