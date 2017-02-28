using Microsoft.SmallBasic.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler.Compilers
{
    public class MethodCallExpressionCompiler : ExpressionCompiler<MethodCallExpression>
    {
        public MethodCallExpressionCompiler(MethodCallExpression expression, EV3CompilerContext context) : base(expression, context)
        {
        }

        protected override void CalculateType()
        {
            string methodName = ParentExpression.FullName().ToUpper();
            EV3SubDefinition sub = Context.FindSubroutine(methodName);
            if (sub == null)
                sub = Context.FindInline(methodName);
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
                return CompileMethodCall(writer, variable, sub);
            }
            sub = Context.FindInline(Value);
            if (sub != null)
            {
                return CompileInlineCall(writer, variable, sub);
            }

            AddError($"Unknown method call to {ParentExpression.FullName()}");
            return "";
        }

        private string CompileMethodCall(TextWriter writer, IEV3Variable variable, EV3SubDefinition sub)
        {
            if (sub.ParameterTypes.Count != ParentExpression.Arguments.Count)
            {
                AddError($"Incorrect number of parameters. Expected {sub.ParameterTypes.Count}.");
                return "";
            }

            using (var tempVariables = Context.UseTempVariables())
            {
                string arguments = string.Join("", ParentExpression.Arguments
                    .Select(a => a.Compiler())
                    .Select(c => " " + c.Compile(writer, tempVariables.CreateVariable(c.Type))));
                if (variable == null)
                    writer.WriteLine($"    CALL {sub.Name}{arguments}");
                else
                    writer.WriteLine($"    CALL {sub.Name}{arguments} {variable.Ev3Name}");
            }
            return variable?.Ev3Name;
        }

        private string CompileInlineCall(TextWriter writer, IEV3Variable variable, EV3SubDefinition sub)
        {
            if (sub.ParameterTypes.Count != ParentExpression.Arguments.Count)
            {
                AddError($"Incorrect number of parameters. Expected {sub.ParameterTypes.Count}.");
                return "";
            }

            using (var tempVariables = Context.UseTempVariables())
            {
                string inlineCode = sub.Code;

                string[] arguments = ParentExpression.Arguments
                    .Select(a => a.Compiler())
                    .Select(c => c.Compile(writer, tempVariables.CreateVariable(c.Type))).ToArray();

                for (int i = 0; i < arguments.Length; i++)
                    inlineCode = inlineCode.Replace($":{i}", arguments[i]);
                if (variable != null)
                    inlineCode = inlineCode.Replace($":{arguments.Length}", variable.Ev3Name);
                if (inlineCode.Contains(":#"))
                    inlineCode = inlineCode.Replace(":#", Context.GetNextLabelNumber().ToString());

                writer.WriteLine(inlineCode);
            }
            return variable?.Ev3Name;
        }
    }
}
