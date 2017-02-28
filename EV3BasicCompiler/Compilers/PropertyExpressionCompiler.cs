using Microsoft.SmallBasic.Expressions;
using System;
using System.IO;

namespace EV3BasicCompiler.Compilers
{
    public class PropertyExpressionCompiler : ExpressionCompiler<PropertyExpression>//, IAssignmentExpressionCompiler
    {
        public PropertyExpressionCompiler(PropertyExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            string propertyName = ParentExpression.FullName().ToUpper();
            EV3SubDefinition sub = Context.FindSubroutine(propertyName);
            if (sub != null)
                type = sub.ReturnType;
        }

        protected override void CalculateValue()
        {
            value = ParentExpression.FullName().ToUpper(); 
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            EV3SubDefinition sub = Context.FindSubroutine(Value);
            if (sub != null)
            {
                writer.WriteLine($"    CALL {Value} {variable.Ev3Name}");
            }
            else
                AddError($"Unknown property {ParentExpression.FullName()}");

            return variable.Ev3Name;
        }

        public void CompileAssignment(TextWriter writer, string compiledValue, string outputName)
        {
        }
    }
}
