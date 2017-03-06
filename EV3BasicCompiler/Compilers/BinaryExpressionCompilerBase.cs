using Microsoft.SmallBasic.Expressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using Microsoft.SmallBasic;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public abstract class BinaryExpressionCompilerBase : ExpressionCompiler<BinaryExpression> 
    {
        public BinaryExpressionCompilerBase(BinaryExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateIsLiteral()
        {
            EnsureValue();
        }

        protected EV3Type CalculateCommonType(EV3Type type1, EV3Type type2)
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

        protected SBExpression LeftExpression
        {
            get { return ParentExpression.LeftHandSide; }
        }

        protected SBExpression RightExpression
        {
            get { return ParentExpression.RightHandSide; }
        }

        private IExpressionCompiler _leftCompiler = null;
        protected IExpressionCompiler LeftCompiler
        {
            get
            {
                if (_leftCompiler == null)
                    _leftCompiler = LeftExpression.Compiler();
                return _leftCompiler;
            }
        }

        private IExpressionCompiler _rightCompiler = null;
        protected IExpressionCompiler RightCompiler
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
