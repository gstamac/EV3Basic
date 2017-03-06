using Microsoft.SmallBasic.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler.Compilers
{
    public class MethodCallExpressionCompiler : ExpressionCompiler<MethodCallExpression>, IBooleanExpressionCompiler
    {
        public MethodCallExpressionCompiler(MethodCallExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            string methodName = ParentExpression.FullName().ToUpper();
            EV3SubDefinitionBase sub = Context.FindSubroutine(methodName);
            if (sub != null)
                type = sub.ReturnType;
        }

        protected override void CalculateValue()
        {
            value = ParentExpression.FullName().ToUpper();
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            EV3SubDefinitionBase sub = Context.FindSubroutine(Value);
            if (sub != null)
            {
                return CompileMethodCall(writer, variable, sub);
            }

            AddError($"Unknown method call to {ParentExpression.FullName()}");
            return "";
        }

        private string CompileMethodCall(TextWriter writer, IEV3Variable variable, EV3SubDefinitionBase sub)
        {
            if (sub.ParameterTypes.Count != ParentExpression.Arguments.Count)
            {
                AddError($"Incorrect number of parameters. Expected {sub.ParameterTypes.Count}.");
                return "";
            }

            using (var tempVariables = Context.UseTempVariables())
            {
                string[] arguments = ParentExpression.Arguments
                    .Select(a => a.Compiler())
                    .Zip(sub.ParameterTypes, (c, t) => new { Compiler = c, Type = t })
                    .Select(c => CompileWithConvert(writer, c.Compiler, c.Type, tempVariables)).ToArray();

                if (variable != null && variable.Type.IsArray() && !sub.ReturnType.IsArray())
                    variable = tempVariables.CreateVariable(variable.Type.BaseType());

                return sub.Compile(writer, Context, arguments, variable?.Ev3Name);
            }
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
