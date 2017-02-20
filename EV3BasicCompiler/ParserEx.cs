using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Statements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EV3BasicCompiler
{
    public class ParserEx : Parser
    {
        public IEnumerable<T> GetStatements<T>() where T : Statement
        {
            return GetStatements<T>(ParseTree);
        }

        private IEnumerable<T> GetStatements<T>(List<Statement> statements) where T : Statement
        {
            return statements.OfType<T>()
                .Concat(GetStatements<T>(statements.OfType<SubroutineStatement>()))
                .Concat(GetStatements<T>(statements.OfType<IfStatement>()))
                .Concat(GetStatements<T>(statements.OfType<ElseIfStatement>()))
                .Concat(GetStatements<T>(statements.OfType<ForStatement>()))
                .Concat(GetStatements<T>(statements.OfType<WhileStatement>()));
        }

        private IEnumerable<T> GetStatements<T>(IEnumerable<IfStatement> statements) where T : Statement
        {
            return statements.SelectMany(s => GetStatements<T>(s.ThenStatements))
                .Concat(statements.SelectMany(s => GetStatements<T>(s.ElseIfStatements)))
                .Concat(statements.SelectMany(s => GetStatements<T>(s.ElseStatements)));
        }

        private IEnumerable<T> GetStatements<T>(IEnumerable<ElseIfStatement> statements) where T : Statement
        {
            return statements.SelectMany(s => GetStatements<T>(s.ThenStatements));
        }

        private IEnumerable<T> GetStatements<T>(IEnumerable<ForStatement> statements) where T : Statement
        {
            return statements.SelectMany(s => GetStatements<T>(s.ForBody));
        }

        private IEnumerable<T> GetStatements<T>(IEnumerable<WhileStatement> statements) where T : Statement
        {
            return statements.SelectMany(s => GetStatements<T>(s.WhileBody));
        }

        private IEnumerable<T> GetStatements<T>(IEnumerable<SubroutineStatement> statements) where T : Statement
        {
            return statements.SelectMany(s => GetStatements<T>(s.SubroutineBody));
        }
    }
}
