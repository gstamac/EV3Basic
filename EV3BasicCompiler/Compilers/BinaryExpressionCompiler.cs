using Microsoft.SmallBasic.Expressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using EV3BasicCompiler.Compilers;
using Microsoft.SmallBasic;

namespace EV3BasicCompiler.Compilers
{
    public class BinaryExpressionCompiler : ExpressionCompiler<BinaryExpression>
    {
        SBExpression simplifiedExpression = null;

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
                if (LeftExpression.IsNumericLiteral() && RightExpression.IsNumericLiteral())
                {
                    switch (Expression.Operator.Token)
                    {
                        case Token.Addition:
                            value = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(LeftExpression.ToString()) + SmallBasicExtensions.ParseFloat(RightExpression.ToString()));
                            isLiteral = true;
                            break;
                        case Token.Subtraction:
                            value = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(LeftExpression.ToString()) - SmallBasicExtensions.ParseFloat(RightExpression.ToString()));
                            isLiteral = true;
                            break;
                        case Token.Division:
                            value = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(LeftExpression.ToString()) / SmallBasicExtensions.ParseFloat(RightExpression.ToString()));
                            isLiteral = true;
                            break;
                        case Token.Multiplication:
                            value = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(LeftExpression.ToString()) * SmallBasicExtensions.ParseFloat(RightExpression.ToString()));
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
                else if (!LeftExpression.IsNumericLiteral() && !RightExpression.IsNumericLiteral())
                {
                    if (Expression.Operator.Token == Token.Addition)
                    {
                        value = '"' + LeftExpression.ToString().Trim('"') + RightExpression.ToString().Trim('"') + '"';
                        isLiteral = true;
                    }
                }
            }
        }

        public SBExpression LeftExpression
        {
            get { return Expression.LeftHandSide; }
        }
        public SBExpression RightExpression
        {
            get { return Expression.RightHandSide; }
        }

        private IExpressionCompiler _leftCompiler = null;
        public IExpressionCompiler LeftCompiler
        {
            get
            {
                EnsureCompilers();
                return _leftCompiler;
            }
        }

        private IExpressionCompiler _rightCompiler = null;
        public IExpressionCompiler RightCompiler
        {
            get
            {
                EnsureCompilers();
                return _rightCompiler;
            }
        }

        private void EnsureCompilers()
        {
            if (_leftCompiler == null)
                _leftCompiler = LeftExpression.Compiler();
            if (_rightCompiler == null)
                _rightCompiler = RightExpression.Compiler();
        }
    }
}
