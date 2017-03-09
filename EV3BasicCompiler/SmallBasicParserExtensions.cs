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
            else if (statement is ForStatement)
                statement.AttachCompiler(new ForStatementCompiler((ForStatement)statement, context));
            else if (statement is WhileStatement)
                statement.AttachCompiler(new WhileStatementCompiler((WhileStatement)statement, context));
            else if (statement is GotoStatement)
                statement.AttachCompiler(new DummyStatementCompiler((GotoStatement)statement, context));
            else if (statement is IfStatement)
                statement.AttachCompiler(new IfStatementCompiler((IfStatement)statement, context));
            else if (statement is ElseIfStatement)
                statement.AttachCompiler(new ElseIfStatementCompiler((ElseIfStatement)statement, context));
            else if (statement is MethodCallStatement)
                statement.AttachCompiler(new MethodCallStatementCompiler((MethodCallStatement)statement, context));
            else if (statement is SubroutineCallStatement)
                statement.AttachCompiler(new SubroutineCallStatementCompiler((SubroutineCallStatement)statement, context));
            else if (statement is SubroutineStatement)
                statement.AttachCompiler(new SubroutineStatementCompiler((SubroutineStatement)statement, context));
            else if (statement is EmptyStatement)
                statement.AttachCompiler(new DummyStatementCompiler((EmptyStatement)statement, context));
            else if (statement is IllegalStatement)
                statement.AttachCompiler(new DummyStatementCompiler((IllegalStatement)statement, context));
            else
                throw new Exception("Unknown statement " + statement.GetType().Name);
        }

        class DummyStatementCompiler : StatementCompiler<Statement>  // TODO: remove this
        {
            public DummyStatementCompiler(Statement statement, EV3CompilerContext context) : base(statement, context)
            {
            }

            public override void Compile(TextWriter writer, bool isRootStatement)
            {
            }
        }

        private static void AttachCompiler(this Statement statement, IStatementCompiler compiler)
        {
            statementCompilers.Add(statement, compiler);
        }

        private static void AttachCompiler(this SBExpression expression, EV3CompilerContext context)
        {
            if (expression == null) return;

            if (expression is ArrayExpression)
                expression.AttachCompiler(new ArrayExpressionCompiler((ArrayExpression)expression, context));
            else if (expression is BinaryExpression)
            {
                BinaryExpression binaryExpression = (BinaryExpression)expression;
                if (binaryExpression.IsBooleanOperator())
                    expression.AttachCompiler(new BooleanExpressionCompiler(binaryExpression, context));
                else if (binaryExpression.IsBooleanCompareOperator())
                    expression.AttachCompiler(new ComparisonExpressionCompiler(binaryExpression, context));
                else
                    expression.AttachCompiler(new BinaryExpressionCompiler(binaryExpression, context));
            }
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
            foreach (SBExpression expression in parser.GetExpressions().Where(e => e != null))
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

        public static void Compile(this IEnumerable<Statement> statements, TextWriter writer, bool isRootStatement)
        {
            foreach (Statement statement in statements)
                statement.Compiler().Compile(writer, isRootStatement);
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

        public static IEnumerable<T> GetStatements<T>(this IEnumerable<Statement> statements) where T : Statement
        {
            return statements.GetStatements().OfType<T>();
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

        public static IEnumerable<Statement> GetSubStatements(this Statement statement)
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
                .Concat(parser.GetStatements<IfStatement>().Select(s => s.Condition))
                .Concat(parser.GetStatements<ElseIfStatement>().Select(s => s.Condition))
                .Concat(parser.GetStatements<WhileStatement>().Select(s => s.Condition))
                .Concat(parser.GetStatements<ForStatement>().SelectMany(s => new SBExpression[] { s.InitialValue, s.FinalValue, s.StepValue }))
                .Concat(parser.GetStatements<MethodCallStatement>().Select(s => s.MethodCallExpression));
            return expressions.Concat(expressions.GetSubExpressions());
        }

        public static IEnumerable<SBExpression> GetExpressions(this IEnumerable<Statement> statements)
        {
            IEnumerable<SBExpression> expressions = statements
                .GetStatements<AssignmentStatement>().SelectMany(s => new SBExpression[] { s.LeftValue, s.RightValue })
                .Concat(statements.GetStatements<IfStatement>().Select(s => s.Condition))
                .Concat(statements.GetStatements<ElseIfStatement>().Select(s => s.Condition))
                .Concat(statements.GetStatements<WhileStatement>().Select(s => s.Condition))
                .Concat(statements.GetStatements<ForStatement>().SelectMany(s => new SBExpression[] { s.InitialValue, s.FinalValue, s.StepValue }))
                .Concat(statements.GetStatements<MethodCallStatement>().Select(s => s.MethodCallExpression));
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
            StringWriter writer = new StringWriter();

            writer.WriteLine("VARIABLES:");
            foreach (var item in parser.SymbolTable.Variables)
                writer.WriteLine(item.Key + " = " + item.Value);
            writer.WriteLine("INITIALIZED VARIABLES:");
            foreach (var item in parser.SymbolTable.InitializedVariables)
                writer.WriteLine($"{item.Key} = {item.Value}");
            writer.WriteLine("SUBROUTINES:");
            foreach (var item in parser.SymbolTable.Subroutines)
                writer.WriteLine($"{item.Key} = {item.Value}");
            writer.WriteLine("LABELS:");
            foreach (var item in parser.SymbolTable.Labels)
                writer.WriteLine($"{item.Key} = {item.Value}");
            writer.WriteLine("PARSER ERRORS:");
            foreach (var item in parser.SymbolTable.Errors)
                writer.WriteLine($"{item.Line}:{item.Column}: {item.Description}");

            writer.WriteLine("TREE:");
            writer.WriteLine(parser.ParseTree.Dump());

            return writer.ToString();
        }

        private static string Dump(this IEnumerable<Statement> statements, string ident = "")
        {
            StringWriter writer = new StringWriter();

            foreach (Statement statement in statements)
            {
                writer.WriteLine($"{ident}{statement.GetType().Name} --> {(statement.HasCompiler() ? statement.Compiler().ToString() : "NO COMPILER")}");
                if (statement is EmptyStatement)
                {
                    writer.WriteLine($"{ident}-----> Comment: {((EmptyStatement)statement).EndingComment.Dump()}");
                }
                else if (statement is AssignmentStatement)
                {
                    var leftValue = ((AssignmentStatement)statement).LeftValue;
                    var rightValue = ((AssignmentStatement)statement).RightValue;
                    writer.WriteLine($"{ident}-----> LEFT: {leftValue.Dump()}");
                    if (leftValue is ArrayExpression)
                    {
                        ArrayExpression arrayExpression = (ArrayExpression)leftValue;
                        writer.WriteLine($"{ident}----->   ARRAY LeftHand: ({arrayExpression.LeftHand.Dump()}");
                        writer.WriteLine($"{ident}----->   ARRAY Indexer: ({arrayExpression.Indexer.Dump()}");
                        DumpValueExpression(writer, arrayExpression.Indexer, ident);
                    }
                    writer.WriteLine($"{ident}-----> RIGHT: {rightValue.Dump()}");
                    DumpValueExpression(writer, rightValue, ident);
                }
                else if (statement is MethodCallStatement)
                {
                    writer.WriteLine($"{ident}-----> Expression: {((MethodCallStatement)statement).MethodCallExpression.Dump()}");
                    DumpValueExpression(writer, ((MethodCallStatement)statement).MethodCallExpression, ident);
                }
                else if (statement is IfStatement)
                {
                    IfStatement ifStatement = (IfStatement)statement;
                    writer.WriteLine($"{ident}-----> Condition: {ifStatement.Condition} {ifStatement.Condition.Dump()}");
                    DumpValueExpression(writer, ifStatement.Condition, ident);
                    writer.WriteLine($"{ident}-----> ThenStatements <-----");
                    writer.Write(ifStatement.ThenStatements.Dump(ident + "    "));
                    writer.WriteLine($"{ident}-----> ElseIfStatements <-----");
                    writer.Write(ifStatement.ElseIfStatements.Dump(ident + "    "));
                    writer.WriteLine($"{ident}-----> ElseStatements <-----");
                    writer.Write(ifStatement.ElseStatements.Dump(ident + "    "));
                    writer.WriteLine($"{ident}-----> EndIf <-----");
                }
                else if (statement is ElseIfStatement)
                {
                    ElseIfStatement elseIfStatement = (ElseIfStatement)statement;
                    writer.WriteLine($"{ident}-----> Condition: {elseIfStatement.Condition} {elseIfStatement.Condition.Dump()}");
                    DumpValueExpression(writer, elseIfStatement.Condition, ident);
                    writer.WriteLine($"{ident}-----> ElseIfStatements <-----");
                    writer.Write(elseIfStatement.ThenStatements.Dump(ident + "    "));
                }
                else if (statement is ForStatement)
                {
                    ForStatement forStatement = (ForStatement)statement;
                    writer.WriteLine($"{ident}-----> Iterator: {forStatement.Iterator} {forStatement.Iterator.Dump()}");
                    writer.WriteLine($"{ident}-----> ForBody <-----");
                    writer.Write(forStatement.ForBody.Dump(ident + "    "));
                    writer.WriteLine($"{ident}-----> EndFor <-----");
                }
                else if (statement is WhileStatement)
                {
                    WhileStatement whileStatement = (WhileStatement)statement;
                    writer.WriteLine($"{ident}-----> Condition: {whileStatement.Condition} {whileStatement.Condition.Dump()}");
                    DumpValueExpression(writer, whileStatement.Condition, ident);
                    writer.WriteLine($"{ident}-----> WhileBody <-----");
                    writer.Write(whileStatement.WhileBody.Dump(ident + "    "));
                    writer.WriteLine($"{ident}-----> EndWhile <-----");
                }
                else if (statement is SubroutineStatement)
                {
                    SubroutineStatement subroutineStatement = (SubroutineStatement)statement;
                    writer.WriteLine($"{ident}-----> SubroutineName: {subroutineStatement.SubroutineName}");
                    writer.WriteLine($"{ident}-----> SubroutineBody <-----");
                    writer.Write(subroutineStatement.SubroutineBody.Dump(ident + "    "));
                    writer.WriteLine($"{ident}-----> EndSubroutine <-----");
                }
                else
                    writer.WriteLine($"{ident}-----> {statement.ToString().Trim('\n', '\r')}");
            }

            return writer.ToString();
        }

        public static string Dump(this SBExpression expression)
        {
            if (expression == null)
                return "NO EXPRESSION";
            else
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
                s.WriteLine(ident + $"-----> Operator: {binaryExpression.Operator.Dump()}");
                s.WriteLine(ident + $"-----> LeftHandSide: {binaryExpression.LeftHandSide.Dump()}");
                DumpValueExpression(s, binaryExpression.LeftHandSide, ident + "  ");
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
