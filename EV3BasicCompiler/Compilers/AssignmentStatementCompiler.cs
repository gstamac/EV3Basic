using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.SmallBasic.Statements;
using Microsoft.SmallBasic.Expressions;
using EV3BasicCompiler.Compilers;

namespace EV3BasicCompiler.Compilers
{
    public class AssignmentStatementCompiler : StatementCompiler<AssignmentStatement>
    {
        public AssignmentStatementCompiler(AssignmentStatement statement, EV3CompilerContext context) : base(statement, context)
        {
        }

        public override void Compile(TextWriter writer)
        {
            IAssignmentExpressionCompiler variableCompiler = ParentStatement.LeftValue.Compiler<IAssignmentExpressionCompiler>();
            if (variableCompiler != null)
            {
                EV3Variable variable = Context.FindVariable(variableCompiler.VariableName);
                IExpressionCompiler valueCompiler = ParentStatement.RightValue.Compiler();
                string compiledValue = valueCompiler.Compile(writer, variable);

                if (variable.Type.IsArray())
                {
                    if (valueCompiler is IAssignmentExpressionCompiler)
                        variable.UpdateMaxIndex(((IAssignmentExpressionCompiler)valueCompiler).Index);
                    else if (valueCompiler is LiteralExpressionCompiler)
                        variable.UpdateMaxIndex(((LiteralExpressionCompiler)valueCompiler).MaxIndex);
                }

                if (string.IsNullOrEmpty(compiledValue)) return;

                //Console.WriteLine($"valueCompiler.Type = {valueCompiler.Type}, variable.Type = {variable.Type}, variableCompiler.Type = {variableCompiler.Type}");

                if (variableCompiler.Type.IsArray() && (valueCompiler.Type != variableCompiler.Type))
                {
                    AddError($"Cannot assign value to array variable '{variableCompiler.VariableName}' without index");
                }
                else if (!variableCompiler.Type.IsArray() && valueCompiler.Type.IsArray())
                {
                    AddError($"Cannot assign array value to non-array variable '{variableCompiler.VariableName}'");
                }
                else if (valueCompiler.Type == EV3Type.String && variableCompiler.Type.IsNumber())
                {
                    AddError($"Cannot assign {valueCompiler.Type} value to {variableCompiler.Type} variable '{variableCompiler.VariableName}'");
                }
                else if (valueCompiler.Type.IsNumber() && variableCompiler.Type == EV3Type.String)
                {
                    writer.WriteLine($"    STRINGS VALUE_FORMATTED {compiledValue} '%g' 99 {variable.Ev3Name}");
                }
                else
                {
                    using (var tempVariables = Context.UseTempVariables())
                    {
                        if (valueCompiler.Type.IsNumber() && variable.Type == EV3Type.StringArray)
                        {
                            IEV3Variable tempVariable = tempVariables.CreateVariable(EV3Type.String);
                            writer.WriteLine($"    STRINGS VALUE_FORMATTED {compiledValue} '%g' 99 {tempVariable.Ev3Name}");
                            compiledValue = tempVariable.Ev3Name;
                        }
                        variableCompiler.CompileAssignment(writer, compiledValue, variable.Ev3Name);
                    }
                }
            }
            else
                AddError($"Cannot assign value to this expression {ParentStatement.LeftValue}");
        }
    }
}
