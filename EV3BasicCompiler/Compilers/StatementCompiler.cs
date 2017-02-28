using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Statements;
using System;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public abstract class StatementCompiler<T> : IStatementCompiler where T : Statement
    {
        public EV3CompilerContext Context { get; }
        protected T ParentStatement { get; }

        public StatementCompiler(T statement, EV3CompilerContext context)
        {
            ParentStatement = statement;
            Context = context;
        }

        public abstract void Compile(TextWriter writer);

        protected void AddError(string message) => AddError(message, ParentStatement.StartToken);
        protected void AddError(string message, TokenInfo tokenInfo) => Context.AddError(message, tokenInfo);

        //public override string ToString() => $"{base.ToString()}[Type = {type}, IsLiteral = {isLiteral}, Value = {value}]";
    }
}
