using Microsoft.SmallBasic.Expressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using Microsoft.SmallBasic.Statements;
using System;
using System.Linq;
using Microsoft.SmallBasic;
using System.Globalization;
using EV3BasicCompiler.Compilers;

namespace EV3BasicCompiler
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage()]
    public static class SmallBasicExtensions
    {
        public static string VariableName(this IdentifierExpression identifierExpression)
        {
            return identifierExpression.Identifier.NormalizedText.ToLower();
        }

        public static string VariableName(this ArrayExpression arrayExpression)
        {
            if (arrayExpression.LeftHand is IdentifierExpression)
            {
                IdentifierExpression identifierExpression = (IdentifierExpression)arrayExpression.LeftHand;
                return identifierExpression.Identifier.NormalizedText.ToLower();
            }
            return "";
        }

        public static int Index(this ArrayExpression arrayExpression)
        {
            int index;
            IExpressionCompiler expressionCompiler = arrayExpression.Indexer.Compiler();
            string value = expressionCompiler.Value;
            if (value != null)
            {
                if (value.EndsWith(".0")) value = value.Remove(value.Length - 2);
                if (expressionCompiler.IsLiteral && int.TryParse(value, out index))
                    return index;
            }
            return - 1;
        }

        public static bool IsLiteral(this SBExpression expression)
        {
            return expression is LiteralExpression || (expression is NegativeExpression && ((NegativeExpression)expression).Expression.IsLiteral());
        }

        public static bool IsNumericLiteral(this SBExpression expression)
        {
            return expression.StartToken.TokenType == TokenType.NumericLiteral || expression.EndToken.TokenType == TokenType.NumericLiteral;
        }

        public static string FormatFloat(string value)
        {
            return FormatFloat(ParseFloat(value));
        }

        public static string FormatFloat(float value)
        {
            return value.ToString("0.0#########", CultureInfo.InvariantCulture);
        }

        public static float ParseFloat(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        public static bool IsVariable(this SBExpression expression, EV3Variable variable)
        {
            if (expression is IdentifierExpression)
            {
                return variable.Name.Equals(((IdentifierExpression)expression).VariableName(), StringComparison.InvariantCultureIgnoreCase);
            }
            else if (expression is ArrayExpression)
            {
                return variable.Name.Equals(((ArrayExpression)expression).VariableName(), StringComparison.InvariantCultureIgnoreCase);
            }
            return false;
        }

        public static bool IsVariable(this ForStatement statement, EV3Variable variable)
        {
            return variable.Name.Equals(statement.Iterator.NormalizedText, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsVariable(this ArrayExpression arrayExpression, EV3Variable variable)
        {
            return variable.Name.Equals(arrayExpression.VariableName(), StringComparison.InvariantCultureIgnoreCase);
        }

        public static string FullName(this PropertyExpression propertyExpression)
        {
            return propertyExpression.TypeName.NormalizedText + "." + propertyExpression.PropertyName.NormalizedText;
        }

        public static string FullName(this MethodCallExpression methodCallExpression)
        {
            return methodCallExpression.TypeName.NormalizedText + "." + methodCallExpression.MethodName.NormalizedText;
        }
    }
}
