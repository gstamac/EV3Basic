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
            foreach (Statement statement in parser.GetStatements())
                statement.AttachCompiler(context);
            foreach (SBExpression expression in parser.GetExpressions())
                expression.AttachCompiler(context);
        }

        private static void AttachCompiler(this Statement statement, EV3CompilerContext context)
        {
            if (statement is AssignmentStatement)
            {
                statement.AttachCompiler(new AssignmentStatementCompiler());
            }
            else if (statement is ElseIfStatement)
            {
            }
            else if (statement is EmptyStatement)
            {
            }
            else if (statement is ForStatement)
            {
            }
            else if (statement is GotoStatement)
            {
            }
            else if (statement is IfStatement)
            {
            }
            else if (statement is IllegalStatement)
            {
            }
            else if (statement is MethodCallStatement)
            {
            }
            else if (statement is SubroutineCallStatement)
            {
            }
            else if (statement is SubroutineStatement)
            {
            }
            else if (statement is WhileStatement)
            {
            }
            else
                throw new Exception("Unknown statement " + statement.GetType().Name);
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

        public static void Compile(this Parser parser, TextWriter writer)
        {
        }

        public static IEnumerable<T> GetStatements<T>(this Parser parser) where T : Statement
        {
            return parser.GetStatements().OfType<T>();
        }

        public static IEnumerable<Statement> GetStatements(this Parser parser)
        {
            if (parser.ParseTree == null) return new Statement[0];

            return parser.ParseTree.Concat(parser.ParseTree.GetSubStatements());
        }

        private static IEnumerable<Statement> GetSubStatements(this List<Statement> statements)
        {
            return statements.OfType<SubroutineStatement>().GetSubStatements()
                .Concat(statements.OfType<IfStatement>().GetSubStatements())
                .Concat(statements.OfType<ElseIfStatement>().GetSubStatements())
                .Concat(statements.OfType<ForStatement>().GetSubStatements())
                .Concat(statements.OfType<WhileStatement>().GetSubStatements());
        }

        private static IEnumerable<Statement> GetSubStatements(this IEnumerable<IfStatement> statements)
        {
            return statements.SelectMany(s => s.ThenStatements.GetSubStatements())
                .Concat(statements.SelectMany(s => s.ElseIfStatements.GetSubStatements()))
                .Concat(statements.SelectMany(s => s.ElseStatements.GetSubStatements()));
        }

        private static IEnumerable<Statement> GetSubStatements(this IEnumerable<ElseIfStatement> statements)
        {
            return statements.SelectMany(s => s.ThenStatements.GetSubStatements());
        }

        private static IEnumerable<Statement> GetSubStatements(this IEnumerable<ForStatement> statements)
        {
            return statements.SelectMany(s => s.ForBody.GetSubStatements());
        }

        private static IEnumerable<Statement> GetSubStatements(this IEnumerable<WhileStatement> statements)
        {
            return statements.SelectMany(s => s.WhileBody.GetSubStatements());
        }

        private static IEnumerable<Statement> GetSubStatements(this IEnumerable<SubroutineStatement> statements)
        {
            return statements.SelectMany(s => s.SubroutineBody.GetSubStatements());
        }

        public static IEnumerable<SBExpression> GetExpressions(this Parser parser)
        {
            IEnumerable<SBExpression> expressions = parser.GetStatements<AssignmentStatement>().SelectMany(s => new SBExpression[] { s.LeftValue, s.RightValue });
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
                return new SBExpression[] { binaryExpression.LeftHandSide }
                    .Concat(binaryExpression.LeftHandSide.GetSubExpressions())
                    .Concat(new SBExpression[] { binaryExpression.RightHandSide })
                    .Concat(binaryExpression.RightHandSide.GetSubExpressions());
            }
            else if (expression is ArrayExpression)
            {
                ArrayExpression arrayExpression = (ArrayExpression)expression;
                return new SBExpression[] { arrayExpression.LeftHand }
                    .Concat(arrayExpression.LeftHand.GetSubExpressions())
                    .Concat(new SBExpression[] { arrayExpression.Indexer })
                    .Concat(arrayExpression.Indexer.GetSubExpressions());
            }
            else
                return new SBExpression[0];
        }

        public static IEnumerable<T> GetExpressions<T>(this Parser parser) where T : SBExpression
        {
            return parser.GetExpressions().OfType<T>();
        }

        public static SBExpression SimplifyExpression(this SBExpression expression)
        {
            SBExpression simplifiedExpression = expression;
            if (simplifiedExpression is BinaryExpression)
            {
                BinaryExpression binaryExpression = (BinaryExpression)simplifiedExpression;
                SBExpression leftExpression = binaryExpression.LeftHandSide.SimplifyExpression();
                SBExpression rightExpression = binaryExpression.RightHandSide.SimplifyExpression();
                if (leftExpression.IsLiteral() && rightExpression.IsLiteral())
                {
                    TokenType tokenType = TokenType.Illegal;
                    Token token = Token.Illegal;
                    string text = string.Empty;

                    if (leftExpression.IsNumericLiteral() && rightExpression.IsNumericLiteral())
                    {
                        tokenType = TokenType.NumericLiteral;
                        token = Token.NumericLiteral;
                        switch (binaryExpression.Operator.Token)
                        {
                            case Token.Addition:
                                text = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(leftExpression.ToString()) + SmallBasicExtensions.ParseFloat(rightExpression.ToString()));
                                break;
                            case Token.Subtraction:
                                text = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(leftExpression.ToString()) - SmallBasicExtensions.ParseFloat(rightExpression.ToString()));
                                break;
                            case Token.Division:
                                text = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(leftExpression.ToString()) / SmallBasicExtensions.ParseFloat(rightExpression.ToString()));
                                break;
                            case Token.Multiplication:
                                text = SmallBasicExtensions.FormatFloat(SmallBasicExtensions.ParseFloat(leftExpression.ToString()) * SmallBasicExtensions.ParseFloat(rightExpression.ToString()));
                                break;
                            //case Token.And:
                            //case Token.Or:
                            //case Token.LessThan:
                            //case Token.LessThanEqualTo:
                            //case Token.GreaterThan:
                            //case Token.GreaterThanEqualTo:
                            //case Token.NotEqualTo:
                            default:
                                tokenType = TokenType.Illegal;
                                token = Token.Illegal;
                                break;
                        }
                    }
                    else if (!leftExpression.IsNumericLiteral() && !rightExpression.IsNumericLiteral())
                    {
                        if (binaryExpression.Operator.Token == Token.Addition)
                        {
                            tokenType = TokenType.StringLiteral;
                            token = Token.StringLiteral;
                            text = '"' + leftExpression.ToString().Trim('"') + rightExpression.ToString().Trim('"') + '"';
                        }
                    }
                    TokenInfo tokenInfo = new TokenInfo
                    {
                        Line = binaryExpression.LeftHandSide.StartToken.Line,
                        Column = binaryExpression.LeftHandSide.StartToken.Column,
                        TokenType = tokenType,
                        Token = token,
                        Text = text
                    };
                    if (tokenType != TokenType.Illegal)
                        simplifiedExpression = new LiteralExpression
                        {
                            StartToken = tokenInfo,
                            EndToken = tokenInfo,
                            Precedence = binaryExpression.Precedence,
                            Literal = tokenInfo
                        };
                }
                else if (leftExpression != binaryExpression.LeftHandSide || rightExpression != binaryExpression.RightHandSide)
                {
                    binaryExpression.LeftHandSide = leftExpression;
                    binaryExpression.RightHandSide = rightExpression;
                }
            }
            if (simplifiedExpression != expression)
                simplifiedExpression.AttachCompiler(expression.Compiler().Context);
            return simplifiedExpression;
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
                s.WriteLine($"{statement.GetType().Name}:{statement.ToString().Trim('\n', '\r')}");
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
