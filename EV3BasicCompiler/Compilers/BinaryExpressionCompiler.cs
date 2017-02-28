using Microsoft.SmallBasic.Expressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using Microsoft.SmallBasic;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class BinaryExpressionCompiler : ExpressionCompiler<BinaryExpression>
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
                    switch (ParentExpression.Operator.Token)
                    {
                        case Token.Addition:
                            value = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(LeftCompiler.Value) + SmallBasicExtensions.ParseFloat(RightCompiler.Value));
                            isLiteral = true;
                            break;
                        case Token.Subtraction:
                            value = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(LeftCompiler.Value) - SmallBasicExtensions.ParseFloat(RightCompiler.Value));
                            isLiteral = true;
                            break;
                        case Token.Division:
                            value = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(LeftCompiler.Value) / SmallBasicExtensions.ParseFloat(RightCompiler.Value));
                            isLiteral = true;
                            break;
                        case Token.Multiplication:
                            value = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(LeftCompiler.Value) * SmallBasicExtensions.ParseFloat(RightCompiler.Value));
                            isLiteral = true;
                            break;
                            //case Token.And:
                            //case Token.Or:
                            //case Token.LessThan:
                            //case Token.LessThanEqualTo:
                            //case Token.GreaterThan:
                            //case Token.GreaterThanEqualTo:
                            //case Token.NotEqualTo:
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

        protected override void CalculateIsLiteral()
        {
            EnsureValue();
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
                    string leftValue = LeftCompiler.Compile(writer, tempVariables.CreateVariable(LeftCompiler.Type));
                    string rightValue = RightCompiler.Compile(writer, tempVariables.CreateVariable(RightCompiler.Type));

                    leftValue = ConvertIfNeeded(writer, leftValue, LeftCompiler, commonType);
                    rightValue = ConvertIfNeeded(writer, rightValue, RightCompiler, commonType);

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
                                    writer.WriteLine("<NOT IMPLEMENTED YET>");
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

        private string ConvertIfNeeded(TextWriter writer, string value, IExpressionCompiler compiler, EV3Type outputType)
        {
            if (compiler.Type.BaseType().IsNumber() && outputType.BaseType() == EV3Type.String)
            {
                using (var tempVariables = Context.UseTempVariables())
                {
                    IEV3Variable outputVariable = tempVariables.CreateVariable(EV3Type.String);

                    writer.WriteLine($"    STRINGS VALUE_FORMATTED {value} '%g' 99 {outputVariable.Ev3Name}");

                    return outputVariable.Ev3Name;
                }
            }
            else
                return value;

        }

        private EV3Type CalculateCommonType(EV3Type type1, EV3Type type2)
        {
            if (type1.BaseType() == type2.BaseType())
            {
                if (type1.IsArray())
                    return type1;
                return type2;
            }
            if (type1.BaseType().IsNumber() && type2.BaseType().IsNumber())
                return EV3Type.Float;
            if ((type1.BaseType().IsNumber() && type2.BaseType() == EV3Type.String) || (type1.BaseType() == EV3Type.String && type2.BaseType().IsNumber()))
                return EV3Type.String;

            return EV3Type.Unknown;
        }

        public SBExpression LeftExpression
        {
            get { return ParentExpression.LeftHandSide; }
        }
        public SBExpression RightExpression
        {
            get { return ParentExpression.RightHandSide; }
        }

        private IExpressionCompiler _leftCompiler = null;
        public IExpressionCompiler LeftCompiler
        {
            get
            {
                if (_leftCompiler == null)
                    _leftCompiler = LeftExpression.Compiler();
                return _leftCompiler;
            }
        }

        private IExpressionCompiler _rightCompiler = null;
        public IExpressionCompiler RightCompiler
        {
            get
            {
                if (_rightCompiler == null)
                    _rightCompiler = RightExpression.Compiler();
                return _rightCompiler;
            }
        }
    }
}
