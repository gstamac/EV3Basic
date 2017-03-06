using System.IO;
using Microsoft.SmallBasic.Statements;
using EV3BasicCompiler.Compilers;
using EV3BasicCompiler;

namespace EV3BasicCompiler.Compilers
{
    public class ForStatementCompiler : StatementCompiler<ForStatement>
    {
        public ForStatementCompiler(ForStatement statement, EV3CompilerContext context) : base(statement, context)
        {
        }

        public override void Compile(TextWriter writer)
        {
            string label = Context.GetNextLabelNumber().ToString();
            EV3Variable variable = Context.FindVariable(ParentStatement);
            IExpressionCompiler stepCompiler = ParentStatement.StepValue == null ? null : ParentStatement.StepValue.Compiler();
            using (var tempVariables = Context.UseTempVariables())
            {
                string initialValue = ParentStatement.InitialValue.Compiler().Compile(writer, variable);
                if (initialValue != variable.Ev3Name)
                    writer.WriteLine($"    MOVEF_F {initialValue} {variable.Ev3Name}");
                writer.WriteLine($"  for{label}:");
                string finalValue = ParentStatement.FinalValue.Compiler().Compile(writer, tempVariables.CreateVariable(EV3Type.Float));
                if (stepCompiler == null || stepCompiler.IsLiteral)
                {
                    if (stepCompiler is NegativeExpressionCompiler)
                        writer.WriteLine($"    JR_LTF {variable.Ev3Name} {finalValue} endfor{label}");
                    else
                        writer.WriteLine($"    JR_GTF {variable.Ev3Name} {finalValue} endfor{label}");
                }
                else
                {
                    string stepValue = stepCompiler.Compile(writer, tempVariables.CreateVariable(EV3Type.Float));
                    IEV3Variable tempStepResultVariable = tempVariables.CreateVariable(EV3Type.Float);
                    writer.WriteLine($"    CALL LE_STEP8 {variable.Ev3Name} {finalValue} {stepValue} {tempStepResultVariable.Ev3Name}");
                    writer.WriteLine($"    JR_EQ8 {tempStepResultVariable.Ev3Name} 0 endfor{label}");
                }
            }
            using (var tempVariables = Context.UseTempVariables())
            {
                writer.WriteLine($"  forbody{label}:");
                ParentStatement.ForBody.Compile(writer);
                string stepValue = stepCompiler == null ? "1.0" : stepCompiler.Compile(writer, tempVariables.CreateVariable(EV3Type.Float));
                writer.WriteLine($"    ADDF {variable.Ev3Name} {stepValue} {variable.Ev3Name}");
                string finalValue2 = ParentStatement.FinalValue.Compiler().Compile(writer, tempVariables.CreateVariable(EV3Type.Float));
                if (stepCompiler == null || stepCompiler.IsLiteral)
                {
                    if (stepCompiler is NegativeExpressionCompiler)
                        writer.WriteLine($"    JR_GTEQF {variable.Ev3Name} {finalValue2} forbody{label}");
                    else
                        writer.WriteLine($"    JR_LTEQF {variable.Ev3Name} {finalValue2} forbody{label}");
                }
                else
                {
                    IEV3Variable tempStepResultVariable = tempVariables.CreateVariable(EV3Type.Float);
                    writer.WriteLine($"    CALL LE_STEP8 {variable.Ev3Name} {finalValue2} {stepValue} {tempStepResultVariable.Ev3Name}");
                    writer.WriteLine($"    JR_NEQ8 {tempStepResultVariable.Ev3Name} 0 forbody{label}");
                }
                writer.WriteLine($"  endfor{label}:");
            }
        }
    }
}
