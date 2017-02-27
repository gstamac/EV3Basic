using Microsoft.SmallBasic;
using System;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public abstract class ExpressionCompiler<T> : IExpressionCompiler where T : SBExpression
    {
        protected EV3Type type;
        protected bool? isLiteral;
        protected string value;

        public EV3CompilerContext Context { get; }
        protected T ParentExpression { get; }

        public ExpressionCompiler(T expression, EV3CompilerContext context)
        {
            ParentExpression = expression;
            Context = context;
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
                EnsureIsLiteral();
                return isLiteral.GetValueOrDefault();
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

        protected void EnsureIsLiteral()
        {
            if (!isLiteral.HasValue) CalculateIsLiteral();
        }

        protected virtual void CalculateIsLiteral()
        {
            isLiteral = false;
        }

        protected void EnsureValue()
        {
            if (value == null) CalculateValue();
        }

        protected abstract void CalculateValue();

        public abstract string Compile(TextWriter writer, IEV3Variable variable);

        protected void AddError(string message) => AddError(message, ParentExpression.StartToken);
        protected void AddError(string message, TokenInfo tokenInfo) => Context.AddError(message, tokenInfo);

        public override string ToString() => $"{base.ToString()}[Type = {type}, IsLiteral = {isLiteral}, Value = {value}]";
    }
}
