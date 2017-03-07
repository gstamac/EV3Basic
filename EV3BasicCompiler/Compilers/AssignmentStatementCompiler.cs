using System.IO;
using Microsoft.SmallBasic.Statements;

namespace EV3BasicCompiler.Compilers
{
    public class AssignmentStatementCompiler : StatementCompiler<AssignmentStatement>
    {
        public AssignmentStatementCompiler(AssignmentStatement statement, EV3CompilerContext context) : base(statement, context)
        {
        }

        public override void Compile(TextWriter writer, bool isRootStatement)
        {
            IAssignmentExpressionCompiler variableCompiler = ParentStatement.LeftValue.Compiler<IAssignmentExpressionCompiler>();
            if (variableCompiler != null)
            {
                EV3Variable variable = Context.FindVariable(variableCompiler.VariableName);
                IExpressionCompiler valueCompiler = ParentStatement.RightValue.Compiler();

                if (variable.Type == EV3Type.Unknown)
                {
                    variable.Type = valueCompiler.Type;
                    if (variableCompiler.IsArray)
                        variable.Type = variable.Type.ConvertToArray();
                }
                variable.IsConstant &= isRootStatement && Context.DoOptimization && valueCompiler.IsLiteral && !variable.Type.IsArray();

                if (variable.Type.IsArray())
                {
                    if (valueCompiler is IAssignmentExpressionCompiler)
                        variable.UpdateMaxIndex(((IAssignmentExpressionCompiler)valueCompiler).Index);
                    else if (valueCompiler is LiteralExpressionCompiler)
                        variable.UpdateMaxIndex(((LiteralExpressionCompiler)valueCompiler).MaxIndex);
                }

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
                else if (valueCompiler.Type.IsNumber() && variableCompiler.Type == EV3Type.String && !variable.Type.IsArray())
                {
                    if (variable.IsConstant)
                        variable.Value = valueCompiler.Value;
                    else
                    {
                        using (var tempVariables = Context.UseTempVariables())
                        {
                            IEV3Variable tempVariable = tempVariables.CreateVariable(valueCompiler.Type);
                            string compiledValue = valueCompiler.Compile(writer, tempVariable);
                            if (string.IsNullOrEmpty(compiledValue)) return;

                            writer.WriteLine($"    STRINGS VALUE_FORMATTED {compiledValue} '%g' 99 {variable.Ev3Name}");
                        }
                    }
                }
                else
                {
                    if (variable.IsConstant)
                        variable.Value = valueCompiler.Value;
                    else
                    {
                        string compiledValue = valueCompiler.Compile(writer, variable);
                        if (string.IsNullOrEmpty(compiledValue)) return;

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
            }
            else
                AddError($"Cannot assign value to this expression {ParentStatement.LeftValue}");
        }
    }
}
