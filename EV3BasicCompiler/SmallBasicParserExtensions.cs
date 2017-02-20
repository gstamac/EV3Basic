using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Statements;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SmallBasic.Expressions;

namespace EV3BasicCompiler
{
    public static class SmallBasicParserExtensions
    {
        public static IEnumerable<T> GetStatements<T>(this Parser parser) where T : Statement
        {
            return parser.ParseTree.GetStatements<T>();
        }

        private static IEnumerable<T> GetStatements<T>(this List<Statement> statements) where T : Statement
        {
            return statements.OfType<T>()
                .Concat(statements.OfType<SubroutineStatement>().GetStatements<T>())
                .Concat(statements.OfType<IfStatement>().GetStatements<T>())
                .Concat(statements.OfType<ElseIfStatement>().GetStatements<T>())
                .Concat(statements.OfType<ForStatement>().GetStatements<T>())
                .Concat(statements.OfType<WhileStatement>().GetStatements<T>());
        }

        private static IEnumerable<T> GetStatements<T>(this IEnumerable<IfStatement> statements) where T : Statement
        {
            return statements.SelectMany(s => s.ThenStatements.GetStatements<T>())
                .Concat(statements.SelectMany(s => s.ElseIfStatements.GetStatements<T>()))
                .Concat(statements.SelectMany(s => s.ElseStatements.GetStatements<T>()));
        }

        private static IEnumerable<T> GetStatements<T>(this IEnumerable<ElseIfStatement> statements) where T : Statement
        {
            return statements.SelectMany(s => s.ThenStatements.GetStatements<T>());
        }

        private static IEnumerable<T> GetStatements<T>(this IEnumerable<ForStatement> statements) where T : Statement
        {
            return statements.SelectMany(s => s.ForBody.GetStatements<T>());
        }

        private static IEnumerable<T> GetStatements<T>(this IEnumerable<WhileStatement> statements) where T : Statement
        {
            return statements.SelectMany(s => s.WhileBody.GetStatements<T>());
        }

        private static IEnumerable<T> GetStatements<T>(this IEnumerable<SubroutineStatement> statements) where T : Statement
        {
            return statements.SelectMany(s => s.SubroutineBody.GetStatements<T>());
        }

        public static IEnumerable<T> GetExpressions<T>(this Parser parser) where T : SBExpression
        {
            var expressions = parser.ParseTree.GetStatements<AssignmentStatement>().Select(s => s.RightValue);
            return expressions.OfType<T>()
                .Concat(expressions.OfType<NegativeExpression>().Select(ne => ne.Expression).OfType<T>())
                .Concat(expressions.OfType<BinaryExpression>().Select(be => new SBExpression[] { be.LeftHandSide, be.RightHandSide }).SelectMany(e => e).OfType<T>());
        }

    }
}
