using Microsoft.SmallBasic;
using System;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;

namespace EV3BasicCompiler.Compilers
{
    public abstract class ExpressionCompiler<T> : IExpressionCompiler where T : SBExpression
    {
        protected EV3Type type;
        protected bool isLiteral;
        protected string value;

        public EV3CompilerContext Context { get; }
        protected T Expression { get; }

        public ExpressionCompiler(T expression, EV3CompilerContext context)
        {
            Expression = expression;
            Context = context;
            isLiteral = false;
            type = EV3Type.Unknown;
            value = null;
        }

        public EV3Type Type
        {
            get
            {
                EnsureType();
                return type;
            }
        }

        public bool IsLiteral
        {
            get
            {
                EnsureValue();
                return isLiteral;
            }
        }

        public string Value
        {
            get
            {
                EnsureValue();
                return value;
            }
        }

        protected void EnsureType()
        {
            if (type == EV3Type.Unknown) CalculateType();
        }

        protected abstract void CalculateType();

        protected void EnsureValue()
        {
            if (value == null) CalculateValue();
        }

        protected abstract void CalculateValue();

        protected void AddError(string message) => AddError(message, Expression.StartToken);
        protected void AddError(string message, TokenInfo tokenInfo) => Context.AddError(message, tokenInfo);

        public override string ToString() => $"{base.ToString()}[Type = {type}, IsLiteral = {isLiteral}, Value = {value}]";
    }
}
