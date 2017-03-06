using Microsoft.SmallBasic.Expressions;
using System;
using System.IO;
using EV3BasicCompiler.Compilers;

namespace EV3BasicCompiler.Compilers
{
    public class ArrayExpressionCompiler : ExpressionCompiler<ArrayExpression>, IAssignmentExpressionCompiler
    {
        public ArrayExpressionCompiler(ArrayExpression expression, EV3CompilerContext context) : base(expression, context)
        {
            index = -1;
        }

        protected override void CalculateType()
        {
            EV3Variable reference = Context.FindVariable(VariableName);
            if (reference.Type != EV3Type.Unknown && !reference.Type.IsArray())
                AddError($"Cannot use index on non-array variable '{VariableName}'");

            type = reference.Type.BaseType();
            index = ParentExpression.Index();

            reference.UpdateMaxIndex(index);
        }

        protected override void CalculateValue()
        {
        }

        public override string Compile(TextWriter writer, IEV3Variable variable)
        {
            IEV3Variable reference = Context.FindVariable(VariableName);

            if (reference != null)
            {
                using (var tempVariables = Context.UseTempVariables())
                {
                    IExpressionCompiler indexCompiler = ParentExpression.Indexer.Compiler();
                    string indexValue = indexCompiler.Compile(writer, tempVariables.CreateVariable(EV3Type.Float));
                    if (variable.Ev3Name.Equals(reference.Ev3Name, StringComparison.InvariantCultureIgnoreCase) ||
                        variable.Type != Type.BaseType())
                    {
                        variable = tempVariables.CreateVariable(Type.BaseType());
                    }
                    if (variable.Type == EV3Type.String)
                        writer.WriteLine($"    CALL ARRAYGET_STRING {indexValue} {variable.Ev3Name} {reference.Ev3Name}");
                    else
                    {
                        if (Context.DoBoundsCheck)
                            writer.WriteLine($"    CALL ARRAYGET_FLOAT {indexValue} {variable.Ev3Name} {reference.Ev3Name}");
                        else
                        {
                            if (indexCompiler.IsLiteral)
                            {
                                if (indexValue.EndsWith(".0")) indexValue = indexValue.Remove(indexValue.Length - 2);
                            }
                            else
                            {
                                writer.WriteLine($"    MOVEF_32 {indexValue} INDEX");
                                indexValue = "INDEX";
                            }
                            writer.WriteLine($"    ARRAY_READ {reference.Ev3Name} {indexValue} {variable.Ev3Name}");
                        }
                    }
                    return variable.Ev3Name;
                }
            }
            return "";
        }

        public void CompileAssignment(TextWriter writer, string compiledValue, string outputName)
        {
            using (var tempVariables = Context.UseTempVariables())
            {
                IExpressionCompiler indexCompiler = ParentExpression.Indexer.Compiler();
                string indexValue = indexCompiler.Compile(writer, tempVariables.CreateVariable(EV3Type.Float));

                switch (Type.BaseType())
                {
                    case EV3Type.Int8:
                    case EV3Type.Int16:
                    case EV3Type.Int32:
                    case EV3Type.Float:
                        if (Context.DoBoundsCheck)
                            writer.WriteLine($"    CALL ARRAYSTORE_FLOAT {indexValue} {compiledValue} {outputName}");
                        else
                        {
                            if (indexCompiler.IsLiteral)
                            {
                                if (indexValue.EndsWith(".0")) indexValue = indexValue.Remove(indexValue.Length - 2);
                            }
                            else
                            {
                                writer.WriteLine($"    MOVEF_32 {indexValue} INDEX");
                                indexValue = "INDEX";
                            }
                            writer.WriteLine($"    ARRAY_WRITE {outputName} {indexValue} {compiledValue}");
                        }
                        break;
                    case EV3Type.String:
                        writer.WriteLine($"    CALL ARRAYSTORE_STRING {indexValue} {compiledValue} {outputName}");
                        break;
                }

            }
        }

        private int index;
        public int Index
        {
            get
            {
                EnsureType();
                return index;
            }
        }

        private string _variableName = null;
        public string VariableName
        {
            get
            {
                if (_variableName == null)
                    _variableName = ParentExpression.VariableName();
                return _variableName;
            }
        }
    }
}
