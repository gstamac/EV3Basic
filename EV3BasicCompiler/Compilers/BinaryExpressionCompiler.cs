using Microsoft.SmallBasic.Expressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using Microsoft.SmallBasic;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class BinaryExpressionCompiler : BinaryExpressionCompilerBase
    {
        public BinaryExpressionCompiler(BinaryExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override EV3Type CalculateType()
        {
            if (LeftCompiler.Type.IsArray() || RightCompiler.Type.IsArray())
            {
                AddError("Operations on arrays are not permited");
                return EV3Type.Void;
            }
            else if ((int)LeftCompiler.Type > (int)RightCompiler.Type)
                return LeftCompiler.Type;
            else
                return RightCompiler.Type;
        }

        protected override string CalculateValue()
        {
            if (LeftCompiler.IsLiteral && RightCompiler.IsLiteral)
            {
                EV3Type commonType = CalculateCommonType(LeftCompiler.Type, RightCompiler.Type);

                if (commonType.IsArray() || commonType == EV3Type.Unknown) return null;

                if (commonType.IsNumber())
                {
                    float leftValue = SmallBasicExtensions.ParseFloat(LeftCompiler.Value);
                    float rightValue = SmallBasicExtensions.ParseFloat(RightCompiler.Value);
                    switch (ParentExpression.Operator.Token)
                    {
                        case Token.Addition:
                            return SmallBasicExtensions.FormatFloat(leftValue + rightValue);
                        case Token.Subtraction:
                            return SmallBasicExtensions.FormatFloat(leftValue - rightValue);
                        case Token.Division:
                            return SmallBasicExtensions.FormatFloat(leftValue / rightValue);
                        case Token.Multiplication:
                            return SmallBasicExtensions.FormatFloat(leftValue * rightValue);
                    }
                }
                else 
                {
                    if (ParentExpression.Operator.Token == Token.Addition)
                    {
                        string leftValue = LeftCompiler.Value.Trim('\'');
                        if (LeftCompiler.Type.IsNumber())
                            leftValue = SmallBasicExtensions.FormatFloat(leftValue);
                        string rightValue = RightCompiler.Value.Trim('\'');
                        if (RightCompiler.Type.IsNumber())
                            rightValue = SmallBasicExtensions.FormatFloat(rightValue);

                        return '\'' + leftValue + rightValue + '\'';
                    }
                }
            }
            return null;
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            if (IsLiteral)
            {
                return Value;
            }

            EV3Type commonType = CalculateCommonType(LeftCompiler.Type, RightCompiler.Type);
            if (commonType == EV3Type.Unknown)
                AddError("Types of left and right side of expression don't match");
            else
            {
                using (var tempVariables = Context.UseTempVariables())
                {
                    string leftValue = CompileWithConvert(writer, LeftCompiler, commonType, tempVariables);
                    string rightValue = CompileWithConvert(writer, RightCompiler, commonType, tempVariables);

                    if (variable.Type.IsArray())
                        variable = tempVariables.CreateVariable(variable.Type.BaseType());

                    if (Type.IsNumber())
                    {
                        switch (ParentExpression.Operator.Token)
                        {
                            case Token.Addition:
                                writer.WriteLine($"    ADDF {leftValue} {rightValue} {variable.Ev3Name}");
                                break;
                            case Token.Subtraction:
                                writer.WriteLine($"    SUBF {leftValue} {rightValue} {variable.Ev3Name}");
                                break;
                            case Token.Division:
                                if (Context.DoDivisionCheck)
                                {
                                    var sub = Context.FindMethod("Math.DivCheck");
                                    if (variable.Type.IsArray() && !sub.ReturnType.IsArray())
                                        variable = tempVariables.CreateVariable(variable.Type.BaseType());
                                    sub.Compile(writer, Context, new string[] { leftValue, rightValue }, variable.Ev3Name);
                                }
                                else
                                    writer.WriteLine($"    DIVF {leftValue} {rightValue} {variable.Ev3Name}");
                                break;
                            case Token.Multiplication:
                                writer.WriteLine($"    MULF {leftValue} {rightValue} {variable.Ev3Name}");
                                break;
                        }
                    }
                    else if (ParentExpression.Operator.Token == Token.Addition)
                    {
                        writer.WriteLine($"    CALL TEXT.APPEND {leftValue} {rightValue} {variable.Ev3Name}");
                    }

                    return variable.Ev3Name;
                }
            }
            return "";
        }
    }
}
