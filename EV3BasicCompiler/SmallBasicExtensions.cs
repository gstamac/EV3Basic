using Microsoft.SmallBasic.Expressions;
using Microsoft.SmallBasic.Statements;
using System;
using System.Linq;

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
            return int.Parse(arrayExpression.Indexer.ToString());
        }

        public static bool IsVariable(this AssignmentStatement statement, EV3Variable variable)
        {
            if (statement.LeftValue is IdentifierExpression)
            {
                return variable.Name.Equals(((IdentifierExpression)statement.LeftValue).VariableName(), StringComparison.InvariantCultureIgnoreCase);
            }
            else if (statement.LeftValue is ArrayExpression)
            {
                return variable.Name.Equals(((ArrayExpression)statement.LeftValue).VariableName(), StringComparison.InvariantCultureIgnoreCase);
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
