using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Statements;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SmallBasic.Expressions;
using EV3BasicCompiler.Compilers;
using System.IO;

namespace EV3BasicCompiler
{
    public static class SmallBasicParserExtensions
    {
        private static Dictionary<Statement, IStatementCompiler> statementCompilers = new Dictionary<Statement, IStatementCompiler>();
        private static Dictionary<SBExpression, IExpressionCompiler> expressionCompilers = new Dictionary<SBExpression, IExpressionCompiler>();

        public static void AttachCompilers(this Parser parser, EV3CompilerContext context)
        {
            foreach (SBExpression expression in parser.GetExpressions())
                expression.AttachCompiler(context);
            foreach (Statement statement in parser.GetStatements())
                statement.AttachCompiler(context);
        }

        private static void AttachCompiler(this Statement statement, EV3CompilerContext context)
        {
            if (statement is AssignmentStatement)
            {
                AssignmentStatement assignmentStatement = (AssignmentStatement)statement;

                PropertyExpressionCompiler propertyExpressionCompiler = assignmentStatement.LeftValue.Compiler<PropertyExpressionCompiler>();
                if (propertyExpressionCompiler != null && propertyExpressionCompiler.Value.Equals("Thread.Run", StringComparison.InvariantCultureIgnoreCase))
                    statement.AttachCompiler(new ThreadStatementCompiler(assignmentStatement, context));
                else
                    statement.AttachCompiler(new AssignmentStatementCompiler(assignmentStatement, context));
            }
            else if (statement is ElseIfStatement)
                statement.AttachCompiler(new DummyStatementCompiler(statement, context));
            else if (statement is EmptyStatement)
                statement.AttachCompiler(new EmptyStatementCompiler((EmptyStatement)statement, context));
            else if (statement is ForStatement)
                statement.AttachCompiler(new DummyStatementCompiler(statement, context));
            else if (statement is GotoStatement)
                statement.AttachCompiler(new DummyStatementCompiler(statement, context));
            else if (statement is IfStatement)
                statement.AttachCompiler(new DummyStatementCompiler(statement, context));
            else if (statement is IllegalStatement)
                statement.AttachCompiler(new DummyStatementCompiler(statement, context));
            else if (statement is MethodCallStatement)
                statement.AttachCompiler(new MethodCallStatementCompiler((MethodCallStatement)statement, context));
            else if (statement is SubroutineCallStatement)
                statement.AttachCompiler(new DummyStatementCompiler(statement, context));
            else if (statement is SubroutineStatement)
                statement.AttachCompiler(new DummyStatementCompiler(statement, context));
            else if (statement is WhileStatement)
                statement.AttachCompiler(new DummyStatementCompiler(statement, context));
            else
                throw new Exception("Unknown statement " + statement.GetType().Name);
        }

        class DummyStatementCompiler : StatementCompiler<Statement>  // TODO: remove this
        {
            public DummyStatementCompiler(Statement statement, EV3CompilerContext context) : base(statement, context)
            {
            }

            public override void Compile(TextWriter writer)
            {
            }
        }

        private static void AttachCompiler(this Statement statement, IStatementCompiler compiler)
        {
            statementCompilers.Add(statement, compiler);
        }

        private static void AttachCompiler(this SBExpression expression, EV3CompilerContext context)
        {
            if (expression is ArrayExpression)
                expression.AttachCompiler(new ArrayExpressionCompiler((ArrayExpression)expression, context));
            else if (expression is BinaryExpression)
                expression.AttachCompiler(new BinaryExpressionCompiler((BinaryExpression)expression, context));
            else if (expression is IdentifierExpression)
                expression.AttachCompiler(new IdentifierExpressionCompiler((IdentifierExpression)expression, context));
            else if (expression is LiteralExpression)
                expression.AttachCompiler(new LiteralExpressionCompiler((LiteralExpression)expression, context));
            else if (expression is MethodCallExpression)
                expression.AttachCompiler(new MethodCallExpressionCompiler((MethodCallExpression)expression, context));
            else if (expression is NegativeExpression)
                expression.AttachCompiler(new NegativeExpressionCompiler((NegativeExpression)expression, context));
            else if (expression is PropertyExpression)
                expression.AttachCompiler(new PropertyExpressionCompiler((PropertyExpression)expression, context));
            else
                throw new Exception("Unknown expression " + expression.GetType().Name);
        }

        private static void AttachCompiler(this SBExpression expression, IExpressionCompiler compiler)
        {
            expressionCompilers.Add(expression, compiler);
        }

        public static void RemoveCompilers(this Parser parser)
        {
            foreach (Statement statement in parser.GetStatements())
                statementCompilers.Remove(statement);
            foreach (SBExpression expression in parser.GetExpressions())
                expressionCompilers.Remove(expression);
        }

        public static bool HasCompiler(this Statement statement)
        {
            return statementCompilers.ContainsKey(statement);
        }

        public static IStatementCompiler Compiler(this Statement statement)
        {
            return statementCompilers[statement];
        }

        public static T Compiler<T>(this Statement statement) where T : class, IStatementCompiler
        {
            return statementCompilers[statement] as T;
        }

        public static bool HasCompiler(this SBExpression expression)
        {
            return expressionCompilers.ContainsKey(expression);
        }

        public static IExpressionCompiler Compiler(this SBExpression expression)
        {
            return expressionCompilers[expression];
        }

        public static T Compiler<T>(this SBExpression expression) where T : class, IExpressionCompiler
        {
            return expressionCompilers[expression] as T;
        }

        public static IEnumerable<T> GetStatements<T>(this Parser parser) where T : Statement
        {
            return parser.GetStatements().OfType<T>();
        }

        public static IEnumerable<Statement> GetStatements(this Parser parser)
        {
            if (parser.ParseTree == null) return new Statement[0];

            return parser.ParseTree.GetStatements();
        }

        public static IEnumerable<Statement> GetStatements(this IEnumerable<Statement> statements)
        {
            foreach (Statement statement in statements)
            {
                yield return statement;
                foreach (Statement subStatement in statement.GetSubStatements())
                {
                    yield return subStatement;
                }
            }
        }

        private static IEnumerable<Statement> GetSubStatements(this Statement statement)
        {
            if (statement is IfStatement)
            {
                IfStatement ifStatement = (IfStatement)statement;
                return ifStatement.ThenStatements.GetStatements()
                    .Concat(ifStatement.ElseIfStatements.GetStatements())
                    .Concat(ifStatement.ElseStatements.GetStatements());
            }
            else if (statement is ElseIfStatement)
            {
                ElseIfStatement elseIfStatement = (ElseIfStatement)statement;
                return elseIfStatement.ThenStatements.GetStatements();
            }
            else if (statement is ForStatement)
            {
                ForStatement forStatement = (ForStatement)statement;
                return forStatement.ForBody.GetStatements();
            }
            else if (statement is WhileStatement)
            {
                WhileStatement whileStatement = (WhileStatement)statement;
                return whileStatement.WhileBody.GetStatements();
            }
            else if (statement is SubroutineStatement)
            {
                SubroutineStatement subroutineStatement = (SubroutineStatement)statement;
                return subroutineStatement.SubroutineBody.GetStatements();
            }
            return new Statement[0];
        }

        public static IEnumerable<SBExpression> GetExpressions(this Parser parser)
        {
            IEnumerable<SBExpression> expressions = parser
                .GetStatements<AssignmentStatement>().SelectMany(s => new SBExpression[] { s.LeftValue, s.RightValue })
                .Concat(parser.GetStatements<MethodCallStatement>().Select(s => s.MethodCallExpression));
            return expressions.Concat(expressions.GetSubExpressions());
        }

        public static IEnumerable<SBExpression> GetSubExpressions(this IEnumerable<SBExpression> expressions)
        {
            return expressions.SelectMany(e => e.GetSubExpressions());
        }

        public static IEnumerable<SBExpression> GetSubExpressions(this SBExpression expression)
        {
            if (expression is NegativeExpression)
            {
                NegativeExpression negativeExpression = (NegativeExpression)expression;
                return new SBExpression[] { negativeExpression.Expression }.Concat(negativeExpression.Expression.GetSubExpressions());
            }
            else if (expression is BinaryExpression)
            {
                BinaryExpression binaryExpression = (BinaryExpression)expression;
                return new SBExpression[] { binaryExpression.LeftHandSide, binaryExpression.RightHandSide }
                    .Concat(binaryExpression.LeftHandSide.GetSubExpressions())
                    .Concat(binaryExpression.RightHandSide.GetSubExpressions());
            }
            else if (expression is ArrayExpression)
            {
                ArrayExpression arrayExpression = (ArrayExpression)expression;
                return new SBExpression[] { arrayExpression.LeftHand, arrayExpression.Indexer }
                    .Concat(arrayExpression.LeftHand.GetSubExpressions())
                    .Concat(arrayExpression.Indexer.GetSubExpressions());
            }
            else if (expression is MethodCallExpression)
            {
                MethodCallExpression methodCallExpression = (MethodCallExpression)expression;
                return methodCallExpression.Arguments
                    .Concat(methodCallExpression.Arguments.SelectMany(a => a.GetSubExpressions()));
            }
            else
                return new SBExpression[0];
        }

        public static IEnumerable<T> GetExpressions<T>(this Parser parser) where T : SBExpression
        {
            return parser.GetExpressions().OfType<T>();
        }

        public static string Dump(this Parser parser)
        {
            StringWriter s = new StringWriter();

            s.WriteLine("VARIABLES:");
            foreach (var item in parser.SymbolTable.Variables)
                s.WriteLine(item.Key + " = " + item.Value);
            s.WriteLine("INITIALIZED VARIABLES:");
            foreach (var item in parser.SymbolTable.InitializedVariables)
                s.WriteLine($"{item.Key} = {item.Value}");
            s.WriteLine("SUBROUTINES:");
            foreach (var item in parser.SymbolTable.Subroutines)
                s.WriteLine($"{item.Key} = {item.Value}");
            s.WriteLine("LABELS:");
            foreach (var item in parser.SymbolTable.Labels)
                s.WriteLine($"{item.Key} = {item.Value}");
            s.WriteLine("PARSER ERRORS:");
            foreach (var item in parser.SymbolTable.Errors)
                s.WriteLine($"{item.Line}:{item.Column}: {item.Description}");

            s.WriteLine("TREE:");
            foreach (Statement statement in parser.ParseTree)
            {
                s.WriteLine($"{statement.GetType().Name}:{statement.ToString().Trim('\n', '\r')} --> {(statement.HasCompiler() ? statement.Compiler().ToString() : "NO COMPILER")}");
                if (statement is EmptyStatement)
                {
                    s.WriteLine($"-----> Comment: {((EmptyStatement)statement).EndingComment.Dump()}");
                }
                else if (statement is AssignmentStatement)
                {
                    var leftValue = ((AssignmentStatement)statement).LeftValue;
                    var rightValue = ((AssignmentStatement)statement).RightValue;
                    s.WriteLine($"-----> LEFT: {leftValue.Dump()}");
                    if (leftValue is ArrayExpression)
                    {
                        ArrayExpression arrayExpression = (ArrayExpression)leftValue;
                        s.WriteLine($"----->   ARRAY LeftHand: ({arrayExpression.LeftHand.Dump()}");
                        s.WriteLine($"----->   ARRAY Indexer: ({arrayExpression.Indexer.Dump()}");
                        DumpValueExpression(s, arrayExpression.Indexer);
                    }
                    s.WriteLine($"-----> RIGHT: {rightValue.Dump()}");
                    DumpValueExpression(s, rightValue);
                }
                else if (statement is MethodCallStatement)
                {
                    s.WriteLine($"-----> Expression: {((MethodCallStatement)statement).MethodCallExpression.Dump()}");
                    DumpValueExpression(s, ((MethodCallStatement)statement).MethodCallExpression);
                }
                else if (statement is ForStatement)
                {
                    s.WriteLine($"-----> Iterator: {((ForStatement)statement).Iterator}");
                }
                else if (statement is SubroutineStatement)
                {
                    s.WriteLine($"-----> SubroutineName: {((SubroutineStatement)statement).SubroutineName}");
                }
            }

            return s.ToString();
        }

        public static string Dump(this SBExpression expression)
        {
            return $"{expression.GetType().Name}[{expression.StartToken},{expression.EndToken}]:{expression} --> {(expression.HasCompiler() ? expression.Compiler().ToString() : "NO COMPILER")}";
        }

        public static string Dump(this TokenInfo tokenInfo)
        {
            return $"{tokenInfo.Token}:{tokenInfo}";
        }

        private static void DumpValueExpression(StringWriter s, SBExpression expression, string ident = "  ")
        {
            if (expression is NegativeExpression)
            {
                NegativeExpression negativeExpression = (NegativeExpression)expression;
                s.WriteLine(ident + $"-----> Expression: {negativeExpression.Expression.Dump()}");
                DumpValueExpression(s, negativeExpression.Expression, ident + "  ");
            }
            else if (expression is BinaryExpression)
            {
                BinaryExpression binaryExpression = (BinaryExpression)expression;
                s.WriteLine(ident + $"-----> LeftHandSide: {binaryExpression.LeftHandSide.Dump()}");
                DumpValueExpression(s, binaryExpression.LeftHandSide, ident + "  ");
                s.WriteLine(ident + $"-----> Operator: {binaryExpression.Operator.Dump()}");
                s.WriteLine(ident + $"-----> RightHandSide: {binaryExpression.RightHandSide.Dump()}");
                DumpValueExpression(s, binaryExpression.RightHandSide, ident + "  ");
            }
            else if (expression is MethodCallExpression)
            {
                MethodCallExpression methodCallExpression = (MethodCallExpression)expression;
                s.WriteLine($"{ident}-----> MethodName: {methodCallExpression.MethodName.Dump()}");
                s.WriteLine($"{ident}-----> TypeName: {methodCallExpression.TypeName.Dump()}");
                s.WriteLine($"{ident}-----> Arguments: {methodCallExpression.Arguments}");
            }
            else if (expression is PropertyExpression)
            {
                PropertyExpression propertyExpression = (PropertyExpression)expression;
                s.WriteLine($"{ident}-----> PropertyName: {propertyExpression.PropertyName.Dump()}");
                s.WriteLine($"{ident}-----> TypeName: {propertyExpression.TypeName.Dump()}");
            }
        }
    }
}
