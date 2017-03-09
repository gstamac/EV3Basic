using Microsoft.SmallBasic.Expressions;
using System;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class PropertyExpressionCompiler : ExpressionCompiler<PropertyExpression>, IBooleanExpressionCompiler
    {
        public PropertyExpressionCompiler(PropertyExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override EV3Type CalculateType()
        {
            string propertyName = ParentExpression.FullName().ToUpper();
            EV3SubDefinitionBase sub = Context.FindMethod(propertyName);
            if (sub != null)
                return sub.ReturnType;

            return EV3Type.Unknown;
        }

        protected override string CalculateValue()
        {
            return ParentExpression.FullName().ToUpper();
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            EV3SubDefinitionBase sub = Context.FindMethod(Value);
            if (sub != null)
            {
                using (var tempVariables = Context.UseTempVariables())
                {
                    if (variable.Type.IsArray() && !sub.ReturnType.IsArray())
                        variable = tempVariables.CreateVariable(variable.Type.BaseType());
                    return sub.Compile(writer, Context, new string[0], variable.Ev3Name);
                }
            }
            else
                AddError($"Unknown property {ParentExpression.FullName()}");

            return variable.Ev3Name;
        }

        public void CompileBranch(TextWriter writer, string label)
        {
            CompileBranchForStringVariable(writer, this, label, false);
        }

        public void CompileBranchNegated(TextWriter writer, string label)
        {
            CompileBranchForStringVariable(writer, this, label, true);
        }
    }
}
