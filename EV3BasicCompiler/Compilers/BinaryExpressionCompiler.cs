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

        protected override void CalculateType()
        {
            if (LeftCompiler.Type.IsArray() || RightCompiler.Type.IsArray())
            {
                AddError("Operations on arrays are not permited");
                type = EV3Type.Void;
            }
            else if ((int)LeftCompiler.Type > (int)RightCompiler.Type)
                type = LeftCompiler.Type;
            else
                type = RightCompiler.Type;
        }

        protected override void CalculateValue()
        {
            if (LeftCompiler.IsLiteral && RightCompiler.IsLiteral)
            {
                if (LeftCompiler.Type.IsNumber() && RightCompiler.Type.IsNumber())
                {
                    float leftValue = SmallBasicExtensions.ParseFloat(LeftCompiler.Value);
                    float rightValue = SmallBasicExtensions.ParseFloat(RightCompiler.Value);
                    switch (ParentExpression.Operator.Token)
                    {
                        case Token.Addition:
                            value = SmallBasicExtensions.FormatFloat(leftValue + rightValue);
                            isLiteral = true;
                            break;
                        case Token.Subtraction:
                            value = SmallBasicExtensions.FormatFloat(leftValue - rightValue);
                            isLiteral = true;
                            break;
                        case Token.Division:
                            value = SmallBasicExtensions.FormatFloat(leftValue / rightValue);
                            isLiteral = true;
                            break;
                        case Token.Multiplication:
                            value = SmallBasicExtensions.FormatFloat(leftValue * rightValue);
                            isLiteral = true;
                            break;
                    }
                }
                else if (!LeftCompiler.Type.IsNumber() && !RightCompiler.Type.IsNumber())
                {
                    if (ParentExpression.Operator.Token == Token.Addition)
                    {
                        value = '\'' + LeftCompiler.Value.Trim('\'') + RightCompiler.Value.Trim('\'') + '\'';
                        isLiteral = true;
                    }
                }
            }
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
                                    var sub = Context.FindSubroutine("Math.DivCheck");
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
